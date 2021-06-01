using System;
using System.IO;
using System.Linq;
using System.Text;
using Utils.Localisation;

namespace Utils
{
    /// <summary>
    ///   CSV Writer.
    /// </summary>
    public class CsvWriter : IDisposable
    {
        private readonly char _alternativeSeparator = ',';
        private readonly TextWriter _writer;

        public CsvWriter(string filename, Encoding encode)
        {
            Separator = ';';
            //StringsOptionallyEnclosedBy = '"';
            //HeadlineInFirstRow = true;
            _writer = new StreamWriter(StringUtils.EscapeFileName(filename), append: false, encoding: encode);
            //CreateWriter = (file, encoding) => new StreamWriter(file, append:false, encoding:encoding);
        }

        /// <summary>
        ///   Gets or sets the field separator.
        /// </summary>
        /// <value> The separator. </value>
        public char Separator { get; set; }

        ///// <summary>
        ///// Gets or sets a value indicating whether [headline in first row].
        ///// </summary>
        ///// <value><c>true</c> if [headline in first row]; otherwise, <c>false</c>.</value>
        //public bool HeadlineInFirstRow { get; set; }
        ///// <summary>
        ///// Gets or sets the character, which is used for string enclosion.
        ///// </summary>
        ///// <value>The strings optionally enclosed by.</value>
        //public char StringsOptionallyEnclosedBy { get; set; }
        public string LastError { get; private set; }
        //public delegate TextWriter CreateWriterDelegate(string filename, Encoding encoding);
        //public CreateWriterDelegate CreateWriter { get; set; }
        /*private string NormalizeHeader(string header) {
            return header.Trim()
                .Replace('/', '_')
                .Replace('.', '_');
        }*/
        public int? FieldCount { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            _writer.Dispose();
        }

        #endregion

        public void WriteCsvData(params string[] rawRow)
        {
            try
            {
                if (!FieldCount.HasValue)
                {
                    if (rawRow.Length == 0)
                    {
                        throw new ArgumentException(ResourcesUtil.InvalidRow);
                    }
                    FieldCount = rawRow.Length;
                }
                if (rawRow.Length != FieldCount)
                {
                    throw new ArgumentException(ResourcesUtil.InvalidRow);
                }
                _writer.Write(SeparatorReplace(rawRow[0]));
                foreach (string element in rawRow.Skip(1))
                {
                    _writer.Write(Separator);
                    _writer.Write(SeparatorReplace(element));
                }
                _writer.WriteLine();
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
        }

        private string SeparatorReplace(string input)
        {
            return input.Replace(Separator, Separator == ';' ? _alternativeSeparator : ';');
        }
    }
}