using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SimleXmlReader
{
    class ParsedInfo
    {
        public string TransportNumber { get; set; }
        public string SMGSNumber { get; set; }
        public string SMGSDate { get; set; }
        public string DeclaractionNumber { get; set; }
        public string DeclarationDate { get; set; }
        public string AccountNumber { get; set; }
        public string AccountDate { get; set; }
        public string RegistrationNumber { get; set; }
        public string RegistrationDate { get; set; }
        public string TemproraryNumber { get; set; }
        public string TemproraryDate { get; set; }

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
                if (file != null)
                {
                    XDocument xdoc = XDocument.Load(file.FullName);
                    var tempObj = xdoc.ToString();   //xml to string
                    XDocument doc = XDocument.Parse(tempObj);
                    List<ParsedInfo> NodeInfo = new List<ParsedInfo>();
                    #region TransportNumber
                    XElement TrNumber = doc.XPathSelectElement("descendant::G01_N[G01]"); // Path to parent node + [child node]
                    if (TrNumber != null)
                    {
                        IEnumerable<XElement> keyWords = TrNumber.Elements("G01");
                        NodeInfo = (from itm in keyWords
                                    where itm.Element("TR_NOMER") != null
                                       
                                    select new ParsedInfo()
                                    {
                                        TransportNumber = itm.Element("TR_NOMER").Value,
                                    }).ToList();
                    }
                    #endregion
                    #region SMGSNumber & SMGSDate
                    XElement SMGS = doc.XPathSelectElement("descendant::Declarant[G02]");
                    List<ParsedInfo> SMGList = new List<ParsedInfo>();
                    if (SMGS != null)
                    {
                        IEnumerable<XElement> keyWords = SMGS.Elements("G02");
                        SMGList = (from itm in keyWords
                                    where itm.Element("KOD_DOC") != null && itm.Element("KOD_DOC").Value == "02013"

                                   select new ParsedInfo()
                                    {
                                        SMGSNumber = itm.Element("NOM_DOC").Value,
                                        SMGSDate = itm.Element("DATE_DOC").Value,
                                    }).ToList();
                    }
                    #endregion
                    #region DeclaractionNumber & DeclarationDate
                    XElement TD = doc.XPathSelectElement("descendant::Declarant[G02]");
                    List<ParsedInfo> TDList = new List<ParsedInfo>();
                    if (TD != null)
                    {
                        IEnumerable<XElement> keyWords = TD.Elements("G02");
                        TDList = (from itm in keyWords
                                   where itm.Element("KOD_DOC") != null && itm.Element("KOD_DOC").Value == "09013"

                                  select new ParsedInfo()
                                   {
                                       DeclaractionNumber = itm.Element("NOM_DOC").Value,
                                       DeclarationDate = itm.Element("DATE_DOC").Value,
                                   }).ToList();
                    }
                    #endregion
                    #region AccountNumber & AccountDate
                    XElement account = doc.XPathSelectElement("descendant::Declarant[G02]");
                    List<ParsedInfo> accountList = new List<ParsedInfo>();
                    if (account != null)
                    {
                        IEnumerable<XElement> keyWords = account.Elements("G02");
                        SMGList = (from itm in keyWords
                                   where itm.Element("KOD_DOC") != null && itm.Element("KOD_DOC").Value == "04021"

                                   select new ParsedInfo()
                                   {
                                       AccountNumber = itm.Element("NOM_DOC").Value,
                                       AccountDate = itm.Element("DATE_DOC").Value,
                                   }).ToList();
                    }
                    #endregion
                    #region RegistrationNumber & RegistrationDate
                    XElement RegNumber = doc.XPathSelectElement("descendant::Custom[G_B/REGNUM_PTO]");
                    List<ParsedInfo> regList = new List<ParsedInfo>();
                    if (RegNumber != null)
                    {
                        IEnumerable<XElement> keyWords = RegNumber.Elements("G_B");
                        regList = (from itm in keyWords
                                   where itm.Element("REGNUM_PTO") != null && itm.Element("DATE_REG") != null

                                   select new ParsedInfo()
                                   {
                                       RegistrationNumber = itm.Element("REGNUM_PTO").Value,
                                       RegistrationDate = itm.Element("DATE_REG").Value,
                                   }).ToList();
                    }
                    #endregion
                    NodeInfo.AddRange(SMGList);
                    NodeInfo.AddRange(accountList);
                    NodeInfo.AddRange(TDList);
                    NodeInfo.AddRange(regList);
                    foreach (ParsedInfo p in NodeInfo)
                    {
                        Console.WriteLine("TransportNumber - {0}", p.TransportNumber);
                        Console.WriteLine("SMGSNumber - {0}", p.SMGSNumber);
                        Console.WriteLine("SMGSDate - {0}", p.SMGSDate);
                        Console.WriteLine("AccountNumber - {0}", p.AccountNumber);
                        Console.WriteLine("AccountDate - {0}", p.AccountDate);
                        Console.WriteLine("DeclaractionNumber - {0}", p.DeclaractionNumber);
                        Console.WriteLine("DeclarationDate - {0}", p.DeclarationDate);
                        Console.WriteLine("RegistrationNumber - {0}", p.RegistrationNumber);
                        Console.WriteLine("RegistrationDate - {0}", p.RegistrationDate);
                    }
                    Console.ReadLine();
                } 
            }
           
        }
    }
}