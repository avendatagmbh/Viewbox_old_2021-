using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Ude;

namespace Utils {
    internal enum DelimiteredBy {
        DelimiterChar,
        FixLength
    }

    /// <summary>
    /// Read CSV-formatted data from a file or TextReader
    /// </summary>
    public class CsvReader : IDisposable {
        #region Constructors
        /// <summary>
        /// Read CSV-formatted data from a string
        /// </summary>
        /// <param name="filename">String containing CSV data</param>
        public CsvReader(string filename) {
            Filename = filename ?? "";
            // InitializeDelegate();
        }

        /// <summary>
        /// Creates the reader.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        private BinaryReader CreateReader(String filename, Encoding encoding) {
            var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            reader = new BinaryReader(fs, encoding);
            return reader;
        }

        /// <summary>
        /// Initializes a reader with a given encoding.
        /// </summary>
        /// <param name="encoding">The encoding used in the input file.</param>
        private void InitializeReader(Encoding encoding) { reader = CreateReader(Filename, encoding); }
        #endregion

        #region Properties
        /// <summary>
        /// The new line character.
        /// </summary>
        private const string NewLineCharacter = "\r\n";

        private bool _headlineInFirstRow = true;
        private char _stringsOptionallyEnclosedBy = '"';

        /// <summary>
        /// For reading rows.
        /// </summary>
        private string currentLine = "";

        /// <summary>
        /// Is the object disposed?
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// This reader will read all of the CSV data
        /// </summary>
        private BinaryReader reader;

        /// <summary>
        /// The file path that needed to read.
        /// </summary>
        public String Filename { get; set; }

        /// <summary>
        /// Does the headlines are in the first row?
        /// </summary>
        public bool HeadlineInFirstRow { get { return _headlineInFirstRow; } set { _headlineInFirstRow = value; } }

        /// <summary>
        /// The encoding of the input file.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// The name of the last error.
        /// </summary>
        public String LastError { get; private set; }

        /// <summary>
        /// The separator character.
        /// </summary>
        public char Separator { get; set; }

        /// <summary>
        /// Collection of colums lengths.
        /// </summary>
        public ObservableCollection<int> ColumnLengths { get; set; }

        /// <summary>
        /// The quote character.
        /// </summary>
        public char StringsOptionallyEnclosedBy { get { return _stringsOptionallyEnclosedBy; } set { _stringsOptionallyEnclosedBy = value; } }
        #endregion

        #region Methods
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            if (isDisposed) {
                return;
            }

            if (reader != null) {
                try {
                    reader.Close();
                } catch {
                }
            }

            isDisposed = true;
        }

        /// <summary>
        /// Read the next row from the CSV data
        /// </summary>
        /// <returns>A list of objects read from the row, or null if there is no next row</returns>
        private List<object> ReadRow(ObservableCollection<int> columLengths = null, object isDelimitered = null) {
            DelimiteredBy Delimitered = DelimiteredBy.DelimiterChar;
            if (isDelimitered != null) {
                Delimitered = (DelimiteredBy) isDelimitered;
            }

            // ReadLine() will return null if there's no next line
            if (reader.BaseStream.Position >= reader.BaseStream.Length) {
                return null;
            }

            var builder = new StringBuilder();
            int delimiteredBy = 0;
            if (isDelimitered != null) {
                delimiteredBy = (int) isDelimitered;
            }

            // Read the next line
            while ((reader.BaseStream.Position < reader.BaseStream.Length) &&
                   (!builder.ToString().EndsWith(NewLineCharacter))) {
                char c = reader.ReadChar();
                builder.Append(c);
            }

            currentLine = builder.ToString();
            if (currentLine.EndsWith(NewLineCharacter)) {
                currentLine = currentLine.Remove(currentLine.IndexOf(NewLineCharacter), NewLineCharacter.Length);
            }

            var objects = new List<object>();
            // Build the list of objects in the line
            if (Delimitered == DelimiteredBy.DelimiterChar) {
                while (currentLine != "") {
                    objects.Add(ReadNextObject());
                }
            } else {
                currentLine = builder.ToString();
                if (currentLine.EndsWith(NewLineCharacter)) {
                    currentLine = currentLine.Remove(currentLine.IndexOf(NewLineCharacter), NewLineCharacter.Length);
                }
                ColumnLengths = columLengths ?? new ObservableCollection<int> {5, 5, 5};
                objects = ReadObjectsFixLength();
            }

            return objects;
        }

        /// <summary>
        /// Read the next object from the currentLine string
        /// </summary>
        /// <returns>The next object in the currentLine string</returns>
        private object ReadNextObject() {
            if (currentLine == null) {
                return null;
            }

            // Check to see if the next value is quoted
            bool quoted = currentLine.StartsWith(StringsOptionallyEnclosedBy.ToString());

            // Find the end of the next value
            string nextObjectString = "";
            int i = 0;
            int len = currentLine.Length;
            bool foundEnd = false;

            while (!foundEnd && i <= len) {
                // Check if we've hit the end of the string
                if ((!quoted && i == len) // non-quoted strings end with a comma or end of line
                    || (!quoted && currentLine.Substring(i, 1) == Separator.ToString())
                    // quoted strings end with a quote followed by a comma or end of line
                    || (quoted && i == len - 1 && currentLine.EndsWith(StringsOptionallyEnclosedBy.ToString()))
                    ||
                    (quoted &&
                     currentLine.Substring(i, 2) == StringsOptionallyEnclosedBy.ToString() + Separator.ToString())) {
                    foundEnd = true;
                } else {
                    i++;
                }
            }
            if (quoted) {
                if (i > len || !currentLine.Substring(i, 1).StartsWith(StringsOptionallyEnclosedBy.ToString())) {
                    LastError = ("Invalid CSV format: " + currentLine.Substring(0, i));
                }
                i++;
            }
            nextObjectString = currentLine.Substring(0, i);

            currentLine = i < len ? currentLine.Substring(i + 1) : "";

            if (!quoted) {
                return nextObjectString;
            } else {
                if (nextObjectString.StartsWith(StringsOptionallyEnclosedBy.ToString())) {
                    nextObjectString = nextObjectString.Substring(1);
                }
                if (nextObjectString.EndsWith(StringsOptionallyEnclosedBy.ToString())) {
                    nextObjectString = nextObjectString.Substring(0, nextObjectString.Length - 1);
                }
                return nextObjectString;
            }
        }

        /// <summary>
        /// Reads the length of the objects fix.
        /// </summary>
        /// <returns></returns>
        private List<object> ReadObjectsFixLength() {
            var objects = new List<object>();
            int from = 0;

            foreach (int colLength in ColumnLengths) {
                int to = colLength;
                if (from + colLength > currentLine.Length) {
                    to = currentLine.Length - from;
                }
                string obj = string.Empty;
                obj = currentLine.Substring(from, to);
                objects.Add(obj);
                from += to;
            }

            return objects;
        }

        /// <summary>
        /// Read the row data read using repeated ReadRow() calls and build a DataColumnCollection with types and column names
        /// </summary>
        /// <returns>System.Data.DataTable object populated with the row data</returns>
        public DataTable GetCsvData(int limit, Encoding encoding, ObservableCollection<int> columnLengths = null,
                                    object isDelimitered = null) {
            if (limit == 0) {
                limit = int.MaxValue;
            }
            InitializeReader(encoding);

            // Read the CSV data into rows
            var rows = new List<List<object>>();
            List<object> readRow = null;

            int actualRow = 0;
            while ((readRow = ReadRow(columnLengths, isDelimitered)) != null && actualRow < limit) {
                rows.Add(readRow);
                actualRow++;
            }

            // The types and names (if HeadlineInFirstRow is true) will be stored in these lists
            var columnTypes = new List<Type>();
            var columnNames = new List<string>();

            // Read the column names from the header row (if there is one)
            if (HeadlineInFirstRow && rows.Count > 0) {
                columnNames.Clear();
                columnNames.AddRange(rows[0].Select(name => name.ToString()));
            }

            // Read the column types from each row in the list of rows
            bool headerRead = false;
            columnTypes.Clear();
            foreach (var row in rows) {
                if (headerRead || !HeadlineInFirstRow) {
                    for (int i = 0; i < row.Count; i++) {
                        // If we're adding a new column to the columnTypes list, use its type.
                        // Otherwise, find the common type between the one that's there and the new row.
                        if (columnTypes.Count < i + 1) {
                            columnTypes.Add(row[i].GetType());
                        } else {
                            columnTypes[i] = StringConverter.FindCommonType(columnTypes[i], row[i].GetType());
                        }
                    }
                } else {
                    headerRead = true;
                }
            }

            // Create the table and add the columns
            var table = new DataTable();
            table.Columns.Clear();
            for (int i = 0; i < columnTypes.Count; i++) {
                table.Columns.Add();
                table.Columns[i].DataType = columnTypes[i];
                if (i >= columnNames.Count || columnNames[i].Length <= 0) {
                    continue;
                }

                if (table.Columns.Contains(columnNames[i])) {
                    int count =
                        table.Columns.Cast<DataColumn>().Count(x => x.ColumnName.StartsWith(columnNames[i]));

                    table.Columns[i].ColumnName = columnNames[i] + " (" + count + ")";
                } else {
                    table.Columns[i].ColumnName = columnNames[i];
                }
            }

            // Add the data from the rows
            headerRead = false;
            foreach (var row in rows) {
                if (headerRead || !HeadlineInFirstRow) {
                    DataRow dataRow = table.NewRow();
                    for (int i = 0; i < row.Count; i++) {
                        dataRow[i] = row[i];
                    }
                    table.Rows.Add(dataRow);
                } else {
                    headerRead = true;
                }
            }

            reader.Dispose();
            return table;
        }

        /// <summary>
        /// Detects the common file end.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        public string DetectCommonFileEnd(int limit, Encoding encoding) {
            using (BinaryReader reader = CreateReader(Filename, encoding)) {
                var line = new StringBuilder();
                int currentChar;
                var lines = new List<string>();

                //Read a maximum of limit lines
                while ((currentChar = reader.Read()) != -1) {
                    line.Append((char) currentChar);
                    if (currentChar != '\n') {
                        continue;
                    }

                    lines.Add(line.ToString());
                    if (lines.Count == limit) {
                        break;
                    }
                    line.Clear();
                }

                //Analyse line end
                var lineEndReversed = new StringBuilder();
                if (lines.Count <= 1 || lines[0].Length == 0) {
                    return "";
                }
                for (int reverseIndex = 0; reverseIndex < lines[0].Length; ++reverseIndex) {
                    char curChar = '\0';
                    foreach (string curLine in lines) {
                        if (reverseIndex >= curLine.Length) {
                            return string.Join("", lineEndReversed.ToString().Reverse());
                        }
                        if (curLine == lines[0]) {
                            curChar = curLine[curLine.Length - 1 - reverseIndex];
                        } else {
                            if (curChar != curLine[curLine.Length - 1 - reverseIndex]) {
                                return string.Join("", lineEndReversed.ToString().Reverse());
                            }
                        }
                    }
                    lineEndReversed.Append(curChar);
                }
                return string.Join("", lineEndReversed.ToString().Reverse());
            }
        }


        /// <summary>
        /// Detects the encoding.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public Encoding DetectEncoding(int limit) {
            if (!File.Exists(Filename)) {
                return null;
            }

            using (FileStream fs = File.OpenRead(Filename)) {
                ICharsetDetector cdet = new CharsetDetector();
                if (limit > 0) {
                    cdet.Feed(fs, limit);
                } else {
                    cdet.Feed(fs);
                }

                cdet.DataEnd();
                return cdet.Charset == null ? Encoding.UTF8 : Encoding.GetEncoding(cdet.Charset);
            }
        }


        /// <summary>
        /// Finalizes an instance of the <see cref="CsvReader" /> class.
        /// </summary>
        ~CsvReader() { Dispose(); }
        #endregion
    }
}