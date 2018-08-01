using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Linq;

namespace SimleXmlReader
{
    public class ParsedInfo
    {
        public string TransportNumber { get; set; }
        public string SMGSNumber { get; set; }
        public string SMGSDate { get; set; }
        public string DeclarationNumber { get; set; }
        public string DeclarationDate { get; set; }
        public string AccountNumber { get; set; }
        public string AccountDate { get; set; }
        public string RegistrationNumber { get; set; }
        public string RegistrationDate { get; set; }
        public string TempDislocationNumber { get; set; }
        public string TempDislocationDate { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string RootFolder = ConfigurationManager.AppSettings["RootFolder"];
            DirectoryInfo di = new DirectoryInfo(RootFolder);
            FileInfo[] files = di.GetFiles("*.xml"); // Read xml files from folder
            foreach (FileInfo file in files)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["KolCargo"].ConnectionString;
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
                    string querry = @"IF EXISTS(SELECT * FROM dbo.KolCargo WHERE TransportNumber=@trNum) 
                    UPDATE dbo.KolCargo 
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

                    ELSE INSERT INTO dbo.KolCargo(TransportNumber,SmgsNumber,SmgsDate,DeclarationNumber,DeclarationDate,AccountNumber,AccountDate,RegistrationNumber,RegistrationDate,TempDislocationNumber,TempDislocationDate) 
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
                    string ProcessedFileStr = string.Format($"C:\\Users\\a.rudich\\Desktop\\Test\\ProcessedFolder\\{file}");
                    try
                    {
                        File.Move(file.FullName, ProcessedFileStr);
                    }
                    catch (IOException) //if file already exist 
                    { }
                    #endregion
                }
            }
            #endregion
            {
                FileSystemWatcher watcher = new FileSystemWatcher(); ;
                watcher.Path = ConfigurationManager.AppSettings["RootFolder"];
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                   | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Filter = "*.xml*";
                watcher.Created += new FileSystemEventHandler(RunXmlReader);
                // Begin watching.
                watcher.EnableRaisingEvents = true;
            }
            Console.WriteLine("Press \'q\' to quit the console.");
            while (Console.Read() != 'q') ;
        }
        //[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]

        public static void RunXmlReader(object source, FileSystemEventArgs e)
        {    
            string RootFolder = ConfigurationManager.AppSettings["RootFolder"];
            DirectoryInfo di = new DirectoryInfo(RootFolder);
            FileInfo[] files = di.GetFiles("*.xml"); // Read xml files from folder
            foreach (FileInfo file in files)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["KolCargo"].ConnectionString;
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
                    string querry = @"IF EXISTS(SELECT * FROM dbo.KolCargo WHERE TransportNumber=@trNum) 
                    UPDATE dbo.KolCargo 
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

                    ELSE INSERT INTO dbo.KolCargo(TransportNumber,SmgsNumber,SmgsDate,DeclarationNumber,DeclarationDate,AccountNumber,AccountDate,RegistrationNumber,RegistrationDate,TempDislocationNumber,TempDislocationDate) 
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
                    string ProcessedFileStr = string.Format($"C:\\Users\\a.rudich\\Desktop\\Test\\ProcessedFolder\\{file}");
                    File.Move(file.FullName, ProcessedFileStr);
                    #endregion
                }
            }
            #endregion
        }

    }
}