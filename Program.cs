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
                    XElement SMGS = doc.XPathSelectElement("descendant::Declarant[G02]");
                    List<ParsedInfo> SMGList = new List<ParsedInfo>();
                    if (SMGS != null)
                    {
                        IEnumerable<XElement> keyWords1 = SMGS.Elements("G02");
                        SMGList = (from itm in keyWords1
                                    where itm.Element("KOD_DOC") != null && itm.Element("KOD_DOC").Value == "02013"


                                   select new ParsedInfo()
                                    {
                                        SMGSNumber = itm.Element("NOM_DOC").Value,
                                        SMGSDate = itm.Element("DATE_DOC").Value,
                                    }).ToList();
                    }
                   
                    else
                    {
                        Console.WriteLine("Код по прежнему говно. Попробуй еще раз!");
                    }
                    NodeInfo.AddRange(SMGList);
                    foreach (ParsedInfo p in NodeInfo)
                    {
                        Console.WriteLine("TransportNumber - {0}", p.TransportNumber);
                        Console.WriteLine("SMGSNumber - {0}", p.SMGSNumber);
                        Console.WriteLine("SMGSDate - {0}", p.SMGSDate);
                    }
                    Console.ReadLine();
                } 
            }
           
        }
    }
}