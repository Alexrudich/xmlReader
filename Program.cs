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
            {
                StepXmlReader.RunStepReader();
                KolXmlReader.RunKolReader();
                //Console.WriteLine("Press \'q\' to quit the console.");
                //while (Console.Read() != 'q') ;
            }
          
        }

    }
}