using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoP_Viewer.Source.Util
{
    internal class CsvHandler
    {
        public static List<String[]> getValuesFromCSV(String path)
        {
            string content = File.ReadAllText(path);

            var values = new List<String[]>();

            foreach (var line in content.Split("\r\n"))
            {
                if (line == "")
                {
                    continue;
                }

                values.Add(line.Split(";"));
            }

            return values;
        }
    }
}
