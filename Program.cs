using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XmlReader
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(@"C:\Users\a.rudich\Documents\Projects-work\Metanit\XmlReader\products.xml");
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/Table/Product");
            string proID = "", proName = "", price = "";
            foreach (XmlNode node in nodeList)
            {
                proID = node.SelectSingleNode("Product_id").InnerText;
                proName = node.SelectSingleNode("Product_name").InnerText;
                price = node.SelectSingleNode("Product_price").InnerText;
                Console.WriteLine(proID + " " + proName + " " + price);
            }
            Console.ReadLine();
        }
    }
}
