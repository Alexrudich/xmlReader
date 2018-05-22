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
            FileInfo[] files = di.GetFiles("*.xml"); // Read files xml from folder
            var temp = files.Length;
            foreach (FileInfo file in files)
            {
                if (file != null)
                {
                    XDocument Xdoc = XDocument.Load(file.FullName);
                    var tempObj = Xdoc.ToString();   //xml в стрингу
                    XDocument doc = XDocument.Parse(tempObj);
                    XElement result = doc.XPathSelectElement("descendant::G01_N[G01]");

                    List<ParsedInfo> NodeInfo = new List<ParsedInfo>();
                    if (result != null)
                    {
                        IEnumerable<XElement> keyWords = result.Elements("G01");
                        NodeInfo = (from itm in keyWords
                                    where itm.Element("TR_NOMER") != null
                                        //&& itm.Element("category").Value == "verb"
                                        //&& itm.Element("id") != null
                                        //&& itm.Element("base") != null
                                    select new ParsedInfo()
                                    {
                                        TransportNumber = itm.Element("TR_NOMER").Value,
                                        //Category = itm.Element("category").Value,
                                        //Id = itm.Element("id").Value,
                                    }).ToList();
                    }
                    else
                    {
                        Console.WriteLine("Код по прежнему говно. Попробуй еще раз!");
                    }
                    foreach (ParsedInfo p in NodeInfo)
                    {
                        Console.WriteLine(p.TransportNumber);
                    }
                    Console.ReadLine();
                } 
            }
           
        }
    }
}