using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics.Contracts;
using Ude;

namespace EasyDocExtraction
{
    public class EasyMetadataExtractor : IEasyMetadataExtractor
    {
        const string FIELD_DEFINITION_DELIMITER = "@FLDDSC";
        const string ITEM_DELIMITER = "@FOLDER";
        string[] _lines;
        string _fileName;

        public EasyMetadataExtractor(string fileName) 
        {
            // contract definition
            Contract.Requires(!string.IsNullOrWhiteSpace(fileName));
            Contract.Requires(File.Exists(fileName));
            Contract.Ensures(_lines.Length > 0);
            _fileName = fileName;

            //// used to find the encoding (1252 : seems no to change) 
            //using (FileStream fs = File.OpenRead(_fileName))
            //{
            //    CharsetDetector cdet = new CharsetDetector();
            //    cdet.Feed(fs);
            //    cdet.DataEnd();
            //    Console.WriteLine("{0}, {1}", cdet.Charset, cdet.Confidence);
            //}
            _lines = File.ReadAllLines(fileName, Encoding.GetEncoding(1252));
        }
        /// <summary>
        /// Test purposes only
        /// </summary>
        public EasyMetadataExtractor()  { }

        /// <summary>
        /// Returns the top of the file containing DB info
        /// </summary>
        /// <returns></returns>
        public string GetDatabaseInfo()
        {
            // is the line containing the DB name (if any)
            var folderName = _lines.ToList().Find(l => l.StartsWith("@DB"));

            // gets "DEMO\BELEGE" from "@DB,$(#DEMO)\BELEGE"
            if (!string.IsNullOrEmpty(folderName))
            {
                return Regex.Replace(folderName, @"[@,)(#$]", "");
            }
            else
            {
                // if folderName is null the DB name can also be found in the file name
                return Path.GetFileNameWithoutExtension(_fileName);
            }
        }
        public IEnumerable<string> GetMetadataHeaders()
        {
           return (from l in _lines where l.StartsWith(FIELD_DEFINITION_DELIMITER) select l + "\r\n");
        }
        /// <summary>
        /// Gets all data contained after @FOLDER
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetMetadataBodies()
        {
            int arraySize = _lines.Count(l => l.StartsWith("@FOLDER"));
            Dictionary<int, StringBuilder> items = new Dictionary<int, StringBuilder>(arraySize);
            var itemCount = -1;
            // creates a string item for each folder found and all subsequent lines are assigne to that string item
            foreach (var line in _lines)
            {
                if (line.StartsWith("@FOLDER")) items.Add(++itemCount, new StringBuilder());
                else if (line.StartsWith("@CHARSET")) continue; // ignore charset

                if (itemCount > -1)
                    items[itemCount].AppendLine(line);
            }
            return items.Values.Select(sb => sb.ToString());
        }
    }
}
