using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Linq;
using XmlReader;

namespace XmlReader
{
    public class StepXmlReader
    {
        public static void RunStepReader()
        {
            string StepFolder = ConfigurationManager.AppSettings["StepFolder"];
            DirectoryInfo di = new DirectoryInfo(StepFolder);
            FileInfo[] files = di.GetFiles("*.xml"); // Read xml files from folder
            int count = 0;
            foreach (FileInfo file in files)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["StepKolCargo"].ConnectionString;
                SqlConnection con = new SqlConnection(connectionString);
                var maxCount = files.Length; //total files in rootfolder
                if (file != null)
                {
                    count++;
                    try
                    {
                        XDocument xdoc = XDocument.Load(file.FullName);
                        var tempObj = xdoc.ToString();   //xml to string

                        ParsedInfo allNodeInfo = new ParsedInfo();
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(tempObj);
                        #region check specific xml data and add to class
                        try
                        {
                            XmlNode TrNumberNode = doc.DocumentElement.SelectSingleNode("descendant::G01_N");
                            string trNum = TrNumberNode.SelectSingleNode("G01").InnerText;
                            allNodeInfo.TransportNumber = trNum;

                            XmlNode SMGSNode = doc.SelectSingleNode("//KOD_DOC[text()='02013']");
                            string SMGSnum = SMGSNode.NextSibling.InnerText;
                            string SMGSdt = SMGSNode.NextSibling.NextSibling.InnerText;
                            allNodeInfo.SMGSNumber = SMGSnum;
                            allNodeInfo.SMGSDate = SMGSdt;

                            XmlNode DeclarationNode = doc.SelectSingleNode("//KOD_DOC[text()='09013']");
                            string DeclNumb = DeclarationNode.NextSibling.InnerText;
                            string DeclDate = DeclarationNode.NextSibling.NextSibling.InnerText;
                            allNodeInfo.DeclarationNumber = DeclNumb;
                            allNodeInfo.DeclarationDate = DeclDate;

                            //XmlNode AccountNode = doc.SelectSingleNode("//KOD_DOC[text()='04021']");
                            XmlNodeList TestNodeList = doc.SelectNodes("//G02[KOD_DOC=04021]/NOM_DOC"); //if we have several AccountNumber in doc

                            List<string> AccountList = new List<string>();
                            List<string> AccountDateList = new List<string>();
                            if (TestNodeList.Count > 0)
                            {
                                foreach (XmlNode item in TestNodeList)
                                {
                                    AccountList.Add(item.InnerText); //add multiple AccountNumber at List
                                    AccountDateList.Add(item.NextSibling.InnerText); //add multiple AccountDate at List
                                }
                            }
                            string AcNumb = String.Join(", ", AccountList.ToArray()); // convert List to string
                            string AcDate = String.Join(", ", AccountDateList.ToArray()); // convert List to string
                            allNodeInfo.AccountNumber = AcNumb;
                            allNodeInfo.AccountDate = AcDate;

                            XmlNode RegNumberNode = doc.SelectSingleNode("//G_B");
                            string RegNum = RegNumberNode.SelectSingleNode("REGNUM_PTO").InnerText; ;
                            string RegDate = RegNumberNode.SelectSingleNode("DATE_REG").InnerText;
                            allNodeInfo.RegistrationNumber = RegNum;
                            allNodeInfo.RegistrationDate = RegDate;

                            XmlNode TempNumberNode = doc.SelectSingleNode("//G04");
                            allNodeInfo.TempDislocationNumber = TempNumberNode.SelectSingleNode("NUM_RAZR").InnerText;
                            string TempDisNum = TempNumberNode.SelectSingleNode("NUM_RAZR").InnerText;
                            string TempDisDate = TempNumberNode.SelectSingleNode("DATE_RAZR").InnerText;
                            allNodeInfo.TempDislocationNumber = TempDisNum;
                            allNodeInfo.TempDislocationDate = TempDisDate;
                            int CargoStationID = 2;
                            string CurFileName = file.Name.ToString();
                            #endregion
                            string querry = sqlQuerry.sqlRequest;
                            using (SqlCommand updSql = new SqlCommand(querry, con))
                            {
                                updSql.Parameters.AddWithValue("@CurFileName", CurFileName);
                                updSql.Parameters.AddWithValue("@trNum", trNum);
                                updSql.Parameters.AddWithValue("@SMGSnum", SMGSnum);
                                updSql.Parameters.AddWithValue("@SMGSdt", SMGSdt);
                                updSql.Parameters.AddWithValue("@DeclNumb", DeclNumb);
                                updSql.Parameters.AddWithValue("@DeclDate", DeclDate);
                                updSql.Parameters.AddWithValue("@AcNumb", AcNumb);
                                updSql.Parameters.AddWithValue("@AcDate", AcDate);
                                updSql.Parameters.AddWithValue("@RegNum", RegNum);
                                updSql.Parameters.AddWithValue("@RegDate", RegDate);
                                updSql.Parameters.AddWithValue("@TempDisNum", TempDisNum);
                                updSql.Parameters.AddWithValue("@TempDisDate", TempDisDate);
                                updSql.Parameters.AddWithValue("@CargoStID", CargoStationID);

                                con.Open();
                                updSql.ExecuteNonQuery();
                                con.Close();
                            }
                        }
                        catch (NullReferenceException)
                        {
                        }
                        string delReq = @"set language british; DELETE FROM dbo.StepKolCargo WHERE DATEADD(WEEK, 6, RegistrationDate) < getdate()";
                        using (SqlCommand deleteOldData = new SqlCommand(delReq, con))
                        {
                            con.Open();
                            deleteOldData.ExecuteNonQuery();
                            con.Close();
                        }
                        #region Moving processed file
                        string ProcessedStepFolder = ConfigurationManager.AppSettings["ProcessedStepFolder"] + $"{file}";
                
                        try
                        {
                            File.Move(file.FullName, ProcessedStepFolder);
                        }
                        catch (IOException ex) //if file already exist 
                        {
                            Logger logger = LogManager.GetLogger("fileLogger");

                            // add custom message and pass in the exception
                            logger.Error(ex, "Whoops!");
                            File.Delete(ProcessedStepFolder);
                            File.Move(file.FullName, ProcessedStepFolder);
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine($"File: {file.Name} has been overwritten!");
                            Console.ForegroundColor = ConsoleColor.White;
                            //Console.WriteLine($"File: {file.FullName} is processed at {DateTime.Now.ToShortTimeString()}");
                        }
                        #endregion
                        Console.WriteLine($"File: {file.FullName} is processed at {DateTime.Now.ToShortTimeString()} {count}/{maxCount}");
                    }
                    catch (IOException ex)
                    {
                        Logger logger = LogManager.GetLogger("fileLogger");

                        // add custom message and pass in the exception
                        logger.Error(ex, "Whoops!");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"File: {file.Name} unavailable!");
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                }
       
            }
        }
    }
}
