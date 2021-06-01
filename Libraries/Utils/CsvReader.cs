using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Utils
{
    /// <summary>
    ///   CSV Reader.
    /// </summary>
    public class CsvReader
    {
        #region Delegates

        public delegate TextReader CreateReaderDelegate(string filename, Encoding encoding);

        #endregion

        public CsvReader(string filename)
        {
            Filename = filename;
            Separator = ',';
            StringsOptionallyEnclosedBy = '"';
            HeadlineInFirstRow = true;
            CreateReader = delegate(string s, Encoding encoding)
                               {
                                   FileStream fs = new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                   return new StreamReader(fs, encoding);
                               };
        }

        /// <summary>
        ///   Gets or sets the filename.
        /// </summary>
        /// <value> The filename. </value>
        public string Filename { get; set; }

        /// <summary>
        ///   Gets or sets the field separator.
        /// </summary>
        /// <value> The separator. </value>
        public char Separator { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether [headline in first row].
        /// </summary>
        /// <value> <c>true</c> if [headline in first row]; otherwise, <c>false</c> . </value>
        public bool HeadlineInFirstRow { get; set; }

        /// <summary>
        ///   Gets or sets the character, which is used for string enclosion.
        /// </summary>
        /// <value> The strings optionally enclosed by. </value>
        public char StringsOptionallyEnclosedBy { get; set; }

        public string LastError { get; private set; }

        public CreateReaderDelegate CreateReader { get; set; }

        private string NormalizeHeader(string header)
        {
            return header.Trim()
                .Replace('/', '_')
                .Replace('.', '_');
        }

        public DataTable GetCsvData(int limit, Encoding encoding)
        {
            LastError = null;
            DataTable csvData = new DataTable();
            TextReader reader = null;
            StringBuilder value = new StringBuilder();
            int lines = 0;
            try
            {
                reader = CreateReader(Filename, encoding);
                int lastColumnCount = -1;
                string csvLine;
                while ((csvLine = reader.ReadLine()) != null && (limit == 0 || lines < limit))
                {
                    csvLine = csvLine.Trim();
                    lines++;
                    if (csvLine.Length == 0) continue;
                    int curColumnCount = 0;
                    if (lastColumnCount < 0)
                    {
                        List<string> values = new List<string>();
                        bool inTextMode = false;
                        value.Clear();
                        for (int i = 0; i < csvLine.Length; i++)
                        {
                            if (inTextMode)
                            {
                                if (csvLine[i].Equals(StringsOptionallyEnclosedBy))
                                {
                                    inTextMode = !inTextMode;
                                }
                                else
                                {
                                    value.Append(csvLine[i]);
                                }
                            }
                            else
                            {
                                if (csvLine[i].Equals(Separator))
                                {
                                    curColumnCount++;
                                    if (HeadlineInFirstRow)
                                    {
                                        if (csvData.Columns.Contains(NormalizeHeader(value.ToString())))
                                        {
                                            int n = 1;
                                            string name = NormalizeHeader(value.ToString()) + "(" + n + ")";
                                            while (csvData.Columns.Contains(name))
                                            {
                                                n++;
                                                name = NormalizeHeader(value.ToString()) + "(" + n + ")";
                                            }
                                            csvData.Columns.Add(name);
                                        }
                                        else
                                        {
                                            csvData.Columns.Add(NormalizeHeader(value.ToString()));
                                        }
                                    }
                                    else
                                    {
                                        csvData.Columns.Add("Spalte " + curColumnCount);
                                        values.Add(value.ToString());
                                    }
                                    value.Clear();
                                }
                                else if (csvLine[i].Equals(StringsOptionallyEnclosedBy))
                                {
                                    inTextMode = !inTextMode;
                                }
                                else
                                {
                                    value.Append(csvLine[i]);
                                }
                            }
                        }
                        if (value.Length > 0 || curColumnCount < lastColumnCount || csvLine.Last() == Separator)
                        {
                            curColumnCount++;
                            if (HeadlineInFirstRow)
                            {
                                if (csvData.Columns.Contains(NormalizeHeader(value.ToString())))
                                {
                                    int n = 1;
                                    string name = NormalizeHeader(value.ToString()) + "(" + n + ")";
                                    while (csvData.Columns.Contains(name))
                                    {
                                        n++;
                                        name = NormalizeHeader(value.ToString()) + "(" + n + ")";
                                    }
                                    csvData.Columns.Add(name);
                                }
                                else
                                {
                                    csvData.Columns.Add(NormalizeHeader(value.ToString()));
                                }
                            }
                            else
                            {
                                csvData.Columns.Add("Spalte " + curColumnCount);
                                values.Add(value.ToString());
                            }
                        }
                        if (!HeadlineInFirstRow)
                        {
                            csvData.Rows.Add(values.ToArray());
                        }
                        lastColumnCount = curColumnCount;
                    }
                    else
                    {
                        string[] values = new string[lastColumnCount];
                        bool inTextMode = false;
                        value.Clear();
                        for (int i = 0; i < csvLine.Length; i++)
                        {
                            //if (curColumnCount >= values.Length) break;
                            if (inTextMode)
                            {
                                if (csvLine[i].Equals(StringsOptionallyEnclosedBy))
                                {
                                    inTextMode = !inTextMode;
                                }
                                else
                                {
                                    value.Append(csvLine[i]);
                                }
                            }
                            else
                            {
                                if (csvLine[i].Equals(Separator))
                                {
                                    values[curColumnCount] = value.ToString();
                                    value.Clear();
                                    curColumnCount++;
                                }
                                else if (csvLine[i].Equals(StringsOptionallyEnclosedBy))
                                {
                                    inTextMode = !inTextMode;
                                }
                                else
                                {
                                    value.Append(csvLine[i]);
                                }
                            }
                        }
                        if (value.Length > 0 || curColumnCount < lastColumnCount)
                        {
                            values[curColumnCount] = value.ToString();
                            curColumnCount++;
                        }
                        if (curColumnCount == lastColumnCount)
                        {
                            csvData.Rows.Add(values);
                        }
                        else
                        {
                            throw new Exception("Invalid csv file!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                LastError = "Fehler beim Einlesen der CSV-Datei in Zeile " + lines;
                //throw new Exception("CSV-File could not be read:" + Environment.NewLine + ex.Message);
                csvData = new DataTable();
                lines = 0;
                if (reader != null) reader.Close();
                reader = CreateReader(Filename, Encoding.UTF7);
                csvData.Columns.Add("Datenfehler (falsches Trennzeichen?)");
                string csvLine;
                while ((csvLine = reader.ReadLine()) != null && (limit == 0 || lines < limit))
                {
                    lines++;
                    DataRow dr = csvData.Rows.Add(new object[] {csvLine});
                }
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return csvData;
        }

        public string DetectCommonFileEnd(int limit, Encoding encoding)
        {
            using (var reader = CreateReader(Filename, encoding))
            {
                StringBuilder line = new StringBuilder();
                int currentChar;
                List<string> lines = new List<string>();
                //Read a maximum of limit lines
                while ((currentChar = reader.Read()) != -1)
                {
                    line.Append((char) currentChar);
                    if (currentChar == '\n')
                    {
                        lines.Add(line.ToString());
                        if (lines.Count == limit)
                            break;
                        line.Clear();
                    }
                }
                //Analyse line end
                StringBuilder lineEndReversed = new StringBuilder();
                if (lines.Count <= 1 || lines[0].Length == 0)
                    return "";
                for (int reverseIndex = 0; reverseIndex < lines[0].Length; ++reverseIndex)
                {
                    char curChar = '\0';
                    foreach (var curLine in lines)
                    {
                        if (reverseIndex >= curLine.Length)
                            return string.Join("", lineEndReversed.ToString().Reverse());
                        if (curLine == lines[0])
                            curChar = curLine[curLine.Length - 1 - reverseIndex];
                        else
                        {
                            if (curChar != curLine[curLine.Length - 1 - reverseIndex])
                                return string.Join("", lineEndReversed.ToString().Reverse());
                        }
                    }
                    lineEndReversed.Append(curChar);
                }
                return string.Join("", lineEndReversed.ToString().Reverse());
            }
        }
    }
}