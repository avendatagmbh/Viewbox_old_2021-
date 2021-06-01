using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Utils
{
    public class EnhancedCsvReader
    {
        #region Constructor

        public EnhancedCsvReader(string filename)
        {
            Filename = filename;
            Separator = ",";
            EndOfLine = "\r\n";
            StringsOptionallyEnclosedBy = '"';
            HeadlineInFirstRow = true;
            CreateReader = (file, encoding) => new StreamReader(file, encoding);
        }

        #endregion Constructor

        #region Properties

        #region Delegates

        public delegate TextReader CreateReaderDelegate(string filename, Encoding encoding);

        #endregion

        /// <summary>
        ///   Gets or sets the filename.
        /// </summary>
        /// <value> The filename. </value>
        public string Filename { get; set; }

        /// <summary>
        ///   Gets or sets the field separator.
        /// </summary>
        /// <value> The separator. </value>
        public string Separator { get; set; }

        public string EndOfLine { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether [headline in first row].
        /// </summary>
        /// <value> <c>true</c> if [headline in first row]; otherwise, <c>false</c> . </value>
        public bool HeadlineInFirstRow { get; set; }

        /// <summary>
        ///   Gets or sets the character, which is used for string enclosion.
        /// </summary>
        /// <value> The strings optionally enclosed by. </value>
        public char? StringsOptionallyEnclosedBy { get; set; }

        public string LastError { get; private set; }

        public CreateReaderDelegate CreateReader { get; set; }

        private string NormalizeHeader(string header)
        {
            return header.Trim()
                .Replace('/', '_')
                .Replace('.', '_');
        }

        #endregion Properties

        #region Methods

        public DataTable GetCsvData(int limit, Encoding encoding)
        {
            //TODO optimize
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
                StringBuilder line = new StringBuilder();
                int currentChar;
                //while ((csvLine=reader.ReadLine()) != null && (limit == 0 || lines < limit)) {
                while ((currentChar = reader.Read()) != -1 && (limit == 0 || lines < limit))
                {
                    line.Append((char) currentChar);
                    if (!EndsWith(line, EndOfLine))
                        //if (!line.ToString().EndsWith(EndOfLine))
                        continue;
                    csvLine = line.ToString().Substring(0, line.Length - EndOfLine.Length);
                    line.Clear();
                    lines++;
                    //csvLine = csvLine.Trim();
                    //if (csvLine.Length == 0)
                    //    continue;
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
                                if (StringsOptionallyEnclosedBy.HasValue &&
                                    csvLine[i].Equals(StringsOptionallyEnclosedBy))
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
                                //TODO:
                                //if (csvLine[i].Equals(this.Separator)) {
                                if (i + Separator.Length <= csvLine.Length &&
                                    csvLine.Substring(i, Separator.Length) == Separator)
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
                                    i += Separator.Length - 1;
                                }
                                else if (StringsOptionallyEnclosedBy.HasValue &&
                                         csvLine[i].Equals(StringsOptionallyEnclosedBy))
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
                                if (StringsOptionallyEnclosedBy.HasValue &&
                                    csvLine[i].Equals(StringsOptionallyEnclosedBy))
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
                                if (i + Separator.Length <= csvLine.Length &&
                                    csvLine.Substring(i, Separator.Length) == Separator)
                                {
                                    //if (csvLine[i].Equals(this.Separator)) {
                                    values[curColumnCount] = value.ToString();
                                    value.Clear();
                                    i += Separator.Length - 1;
                                    curColumnCount++;
                                }
                                else if (StringsOptionallyEnclosedBy.HasValue &&
                                         csvLine[i].Equals(StringsOptionallyEnclosedBy))
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
                if (lines == 0 && line.Length != 0)
                    throw new InvalidOperationException("Falsches Trennzeichen");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                LastError = "Fehler beim Einlesen der CSV-Datei in Zeile " + lines;
                //throw new Exception("CSV-File could not be read:" + Environment.NewLine + ex.Message);
                csvData = new DataTable();
                lines = 0;
                if (reader != null)
                    reader.Close();
                reader = CreateReader(Filename, Encoding.UTF7);
                csvData.Columns.Add("Datenfehler (falsches Trennzeichen?)");

                StringBuilder line = new StringBuilder();
                int currentChar;
                while ((currentChar = reader.Read()) != -1 && (limit == 0 || lines < limit))
                {
                    line.Append((char) currentChar);
                    if (currentChar == '\n')
                    {
                        lines++;
                        csvData.Rows.Add(new object[] {line.Replace("\r", @"\r").Replace("\n", @"\n")});
                        line.Clear();
                    }
                }
                if (line.Length > 0)
                {
                    csvData.Rows.Add(new object[] {line.Replace("\r", @"\r").Replace("\n", @"\n")});
                }
                //while ((csvLine = reader.ReadLine()) != null && (limit == 0 || lines < limit)) {
                //    lines++;
                //    DataRow dr = csvData.Rows.Add(new object[] { csvLine.Replace("\r", @"\r").Replace("\n", @"\n") });
                //}
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return csvData;
        }

        public static bool EndsWith(StringBuilder line, string endOfLine)
        {
            if (line.Length < endOfLine.Length)
                return false;
            for (int i = 0; i < endOfLine.Length; ++i)
                if (line[line.Length - endOfLine.Length + i] != endOfLine[i])
                    return false;
            return true;
        }

        #endregion Methods
    }
}