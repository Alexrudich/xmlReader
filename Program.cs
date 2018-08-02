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

namespace SimleXmlReader
{
    class Program
    {
        static void Main(string[] args)
        {
            var first = new BelintXmlReader(pass from config);
            first.RunReader();
            var second = new BelintXmlReader(pass from config);
            first.RunReader();
            Console.WriteLine("Press \'q\' to quit the console.");
            while (Console.Read() != 'q') ;
        }
        //[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]

    }
}