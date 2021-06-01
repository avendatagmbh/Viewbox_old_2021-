using System;
using System.Globalization;
using System.Xml.Serialization;
using DbSearchLogic.SearchCore.Structures;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearchLogic.SearchCore.SearchMatrix
{
    /// <summary>
    /// SearchValue stores the search value in all possible database types
    /// </summary>
    public class SearchValue {
        #region SearchValue

        public SearchValue(string value, CultureInfo language, Column column, int rowEntryId) {
            if (value == null) {
                value = string.Empty;
            }
            //Used for later references
            RowEntryId = rowEntryId;
            CreateTypes(value, language);
            Column = column;
            UseEntry = true;
            SearchParams = new SearchParamsEvaluated() {
                NumericPrecision = Column.SearchParams.NumericPrecision,
                UseCaseIgnore = Column.SearchParams.UseCaseIgnore,
                UseInStringSearch = Column.SearchParams.UseInStringSearch,
                UseSearchRoundedValues = Column.SearchParams.UseSearchRoundedValues,
                UseStringSearch = Column.SearchParams.UseSearchRoundedValues
            };
        }
        #endregion SearchValue

        #region Properties
        public int RowEntryId { get; set; }
        public bool UseEntry { get; set; }
        public IConfigSearchParams SearchParams { get; private set; }
        public Column Column { get; private set; }
        /// <summary>
        /// Gets or sets the string value
        /// </summary>
        [XmlElement("String")]
        public string String { get; set; }

        /// <summary>
        /// Gets or sets the numeric value
        /// </summary>
        [XmlElement("Numeric")]
        public decimal? Numeric { get; set; }

        /// <summary>
        /// Gets or sets the integer value
        /// </summary>
        [XmlElement("Integer")]
        public Int64? Integer { get; set; }

        /// <summary>
        /// Gets or sets the integer value
        /// </summary>
        [XmlElement("UnsignedInteger")]
        public UInt64? UnsignedInteger { get; set; }

        /// <summary>
        /// Gets or sets the dateTime value
        /// </summary>
        [XmlElement("DateTime")]
        public DateTime? DateTime { get; set; }

        #endregion Properties


        /// <summary>
        /// Create the different types of the search value based on the given string and primary language
        /// </summary>
        /// <param name="value"></param>
        /// <param name="language"></param>
        public void CreateTypes(string value, CultureInfo language) {
            // Initialize the variables
            String = value;
            Numeric = SearchValueParser.GetDecimal(String, language);
            Integer = SearchValueParser.GetInteger(String);
            UnsignedInteger = SearchValueParser.GetUnsignedInteger(String); 
            DateTime = SearchValueParser.GetDateTime(String, language);
        }

        public override string ToString() {
            return String;
        }
    }
}