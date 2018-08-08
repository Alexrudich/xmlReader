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
    public class BelintXmlReader
    {
        private string _watchedFolder;
        private string _tableName;
        public BelintXmlReader(string watchedFolder, string tableName)
        {
            _watchedFolder = watchedFolder;
            _tableName = tableName;
        }

        private void RunWatcher( string configurationString)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(); ;
            watcher.Path = _watchedFolder;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.xml*";
            watcher.Created += new FileSystemEventHandler(RunXmlReader);
            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        public void RunReader()
        {
            string StepFolder = ConfigurationManager.AppSettings["StepFolder"];
            DirectoryInfo di = new DirectoryInfo(StepFolder);
            FileInfo[] files = di.GetFiles("*.xml"); // Read xml files from folder
            foreach (FileInfo file in files)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["StepCargo"].ConnectionString;
                SqlConnection con = new SqlConnection(connectionString);

                if (file != null)
                {
                    Console.WriteLine($"File: {file.FullName} is processed");

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
                        allNodeInfo.DeclarationDate = DeclNumb;

                        XmlNode AccountNode = doc.SelectSingleNode("//KOD_DOC[text()='04021']");
                        string AcNumb = AccountNode.NextSibling.InnerText;
                        string AcDate = AccountNode.NextSibling.NextSibling.InnerText;
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
                        #endregion
                        #region add to database
                        string querry = @"IF EXISTS(SELECT * FROM dbo.StepCargo WHERE TransportNumber=@trNum) 
                    UPDATE dbo.StepCargo 
                    SET TransportNumber = @trNum,
                        SMGSNumber = @SMGSnum,
                        SmgsDate = @SMGSdt,
                        DeclarationNumber = @DeclNumb,
                        DeclarationDate = @DeclNumb,
                        AccountNumber = @AcNumb,
                        AccountDate = @AcDate,
                        RegistrationNumber = @RegNum,
                        RegistrationDate = @RegDate,
                        TempDislocationNumber = @TempDisNum,
                        TempDislocationDate = @TempDisDate
                    WHERE TransportNumber=@trNum

                    ELSE INSERT INTO dbo.StepCargo(TransportNumber,SmgsNumber,SmgsDate,DeclarationNumber,DeclarationDate,AccountNumber,AccountDate,RegistrationNumber,RegistrationDate,TempDislocationNumber,TempDislocationDate) 
                    VALUES(@trNum,@SMGSnum,@SMGSdt,@DeclNumb,@DeclDate,@AcNumb,@AcDate,@RegNum,@RegDate,@TempDisNum,@TempDisDate);";

                        using (SqlCommand updSql = new SqlCommand(querry, con))
                        {
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

                            con.Open();
                            updSql.ExecuteNonQuery();
                            con.Close();
                        }
                        #region Moving processed file
                        //string ProcessedFileStr = string.Format($"C:\\Users\\a.rudich\\Desktop\\Test\\ProcessedStepFolder\\{file}");
                        //try
                        //{
                        //    File.Move(file.FullName, ProcessedFileStr);
                        //}
                        //catch (IOException) //if file already exist 
                        //{ }
                        #endregion
                    }

                    catch (System.NullReferenceException)
                    {

                    }
                }
            }
            #endregion

            RunWatcher(ConfigurationManager.AppSettings["StepFolder"]);
            RunWatcher(ConfigurationManager.AppSettings["StepFolder"]);
        }


        public void RunXmlReader(object source, FileSystemEventArgs e)
        {
            string StepFolder = ConfigurationManager.AppSettings["StepFolder"];
            DirectoryInfo di = new DirectoryInfo(StepFolder);
            FileInfo[] files = di.GetFiles("*.xml"); // Read xml files from folder
            foreach (FileInfo file in files)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["StepCargo"].ConnectionString;
                SqlConnection con = new SqlConnection(connectionString);

                if (file != null)
                {
                    Console.WriteLine($"File: {file.FullName} is processed");

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
                        allNodeInfo.DeclarationDate = DeclNumb;

                        XmlNode AccountNode = doc.SelectSingleNode("//KOD_DOC[text()='04021']");
                        string AcNumb = AccountNode.NextSibling.InnerText;
                        string AcDate = AccountNode.NextSibling.NextSibling.InnerText;
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
                        #endregion
                        #region add to database
                        string querry = @"IF EXISTS(SELECT * FROM dbo.StepCargo WHERE TransportNumber=@trNum) 
                    UPDATE dbo.StepCargo 
                    SET TransportNumber = @trNum,
                        SMGSNumber = @SMGSnum,
                        SmgsDate = @SMGSdt,
                        DeclarationNumber = @DeclNumb,
                        DeclarationDate = @DeclNumb,
                        AccountNumber = @AcNumb,
                        AccountDate = @AcDate,
                        RegistrationNumber = @RegNum,
                        RegistrationDate = @RegDate,
                        TempDislocationNumber = @TempDisNum,
                        TempDislocationDate = @TempDisDate
                    WHERE TransportNumber=@trNum

                    ELSE INSERT INTO dbo.StepCargo(TransportNumber,SmgsNumber,SmgsDate,DeclarationNumber,DeclarationDate,AccountNumber,AccountDate,RegistrationNumber,RegistrationDate,TempDislocationNumber,TempDislocationDate) 
                    VALUES(@trNum,@SMGSnum,@SMGSdt,@DeclNumb,@DeclDate,@AcNumb,@AcDate,@RegNum,@RegDate,@TempDisNum,@TempDisDate);";

                        using (SqlCommand updSql = new SqlCommand(querry, con))
                        {
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

                            con.Open();
                            updSql.ExecuteNonQuery();
                            con.Close();
                        }
                        #region Moving processed file
                        //string ProcessedFileStr = string.Format($"C:\\Users\\a.rudich\\Desktop\\Test\\ProcessedStepFolder\\{file}");
                        //try
                        //{
                        //    File.Move(file.FullName, ProcessedFileStr);
                        //}
                        //catch (IOException) //if file already exist 
                        //{ }
                        #endregion
                    }

                    catch (System.NullReferenceException)
                    {

                    }
                }
            }
            #endregion
        }

    }
}
