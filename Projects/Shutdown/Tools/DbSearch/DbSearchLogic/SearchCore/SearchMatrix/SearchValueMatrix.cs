using System.Globalization;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System;
using DbSearchLogic.SearchCore.Structures;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearchLogic.SearchCore.SearchMatrix
{
    /// <summary>
    /// SearchValueMatrix stores a matrix of SearchValues
    /// </summary>
    public class SearchValueMatrix : IXmlSerializable
    {
        /*
         * Variables
         */

        [XmlElement("Matrix")]
        public SearchValue[,] Values;

        public SearchValueMatrixCellType[,] CellTypes;
        public int[] NumericCellsIndices;
        public int[] IntegerCellsIndices;
        public int[] UnsignedIntegerCellsIndices;
        public int[] DateTimeCellsIndices;
        public int[] TextCellsIndices;
        //public int[] AllIndices;

        public SearchValue[] NumericValues;
        public SearchValue[] IntegerValues;
        public SearchValue[] UnsignedIntegerValues;
        public SearchValue[] DateTimeValues;
        public SearchValue[] TextValues;
        public SearchValue[] AllValues;

        //public SearchValue[] NumericValues;

        //public ConfigSearchParams[] SearchParamsForValue;

        // buffer for results
        private List<decimal> _numericValues;
        private List<Int64> _integerValues;
        private List<UInt64> _unsignedIntegerValues;
        private List<DateTime> _dateTimeValues;
        
        [XmlElement("SpaltenNamen")]
        private string[] _columnNames;


        #region SearchValueMatrix
        /// <summary>
        /// Constructor of SearchValueMatrix (only for serialization)
        /// </summary>
        public SearchValueMatrix() {
            
        }

        /// <summary>
        /// Constructor of SearchValueMatrix
        /// </summary>
        /// <param name="nNumberOfRows">Number of rows</param>
        /// <param name="nNumberOfColumns">Number of columns</param>
        public SearchValueMatrix(int nNumberOfRows, int nNumberOfColumns)
        {
            // Initialize the variables
            this.Values = new SearchValue[nNumberOfRows, nNumberOfColumns];
            this._columnNames = new string[nNumberOfColumns];     
        }
        #endregion SearchValueMatrix

        /// <summary>
        /// Inits the types.
        /// </summary>
        public void InitTypes() {

            CellTypes = new SearchValueMatrixCellType[NumberOfRows, NumberOfColumns];

            for (int i = 0; i < NumberOfRows; i++) {
                for (int j = 0; j < NumberOfColumns; j++) {
                    //constructing the celltype array. For every type in the svm a different bit is used in the enum. Text is implicitly allways on
                    if (!Values[i, j].UseEntry) CellTypes[i, j] = SearchValueMatrixCellType.None;
                    else {
                        CellTypes[i, j] = SearchValueMatrixCellType.UseCell |
                            (Values[i, j].Numeric.HasValue ? SearchValueMatrixCellType.IsNumeric : SearchValueMatrixCellType.None) |
                            (Values[i, j].Integer.HasValue ? SearchValueMatrixCellType.IsInteger : SearchValueMatrixCellType.None) |
                            (Values[i, j].UnsignedInteger.HasValue ? SearchValueMatrixCellType.IsUnsignedInteger : SearchValueMatrixCellType.None) |
                            (Values[i, j].DateTime.HasValue ? SearchValueMatrixCellType.IsDateTime : SearchValueMatrixCellType.None) |
                            SearchValueMatrixCellType.IsText;
                    }
                }
            }
            InitSubArrays();
        }

        #region Properties
        private int _numberOfValues;
        public int NumberOfValues { get { return _numberOfValues; } }

        public int NumberOfRows
        {
            get { return this.Values.GetLength(0); }
        }

        public int NumberOfColumns
        {
            get { return this.Values.GetLength(1); }
        }

        public string[] ColumnNames {
            get { return this._columnNames; }
        }
        #endregion Properties


        #region Methods
        /// <summary>
        /// Gets the specific row
        /// </summary>
        /// <param name="nRowIndex">Row index</param>
        /// <returns>Array of SearchValues (row)</returns>
        public SearchValue[] GetRow(int nRowIndex)
        {
            SearchValue[] oResult = new SearchValue[this.NumberOfColumns];
            for (int i = 0; i < this.NumberOfColumns; i++)
            {
                oResult[i] = this.Values[nRowIndex, i];
            }

            return oResult;
        }

        /// <summary>
        /// Gets the specific column
        /// </summary>
        /// <param name="name">Column name</param>
        /// <returns>Array of SearchValues (row)</returns>
        public SearchValue[] GetColumn(string name) {
            SearchValue[] oResult = new SearchValue[this.NumberOfRows];

            // Get column index
            int nColumnIndex = -1;
            for (int i = 0; i < this._columnNames.Length; i++) {
                if (this._columnNames[i].ToLower() == name.ToLower()) {
                    nColumnIndex = i;
                }
            }

            // If column couldn't be found
            if (nColumnIndex == -1) {
                return null;
            }

            // Create the column
            for (int i = 0; i < this.NumberOfRows; i++) {
                oResult[i] = this.Values[i, nColumnIndex];
            }

            return oResult;
        }

        /// <summary>
        /// Gets the name of a column
        /// </summary>
        /// <param name="nColumnIndex">Column index</param>
        /// <returns>Name of the column</returns>
        public string GetColumnName(int nColumnIndex)
        {
            return this._columnNames[nColumnIndex];
        }

        /// <summary>
        /// Sets the name of a column
        /// </summary>
        /// <param name="nColumnIndex">Column index</param>
        /// <param name="sName">The column name</param>
        public void SetColumnName(int nColumnIndex, string sName)
        {
            this._columnNames[nColumnIndex] = sName;
        }

        /// <summary>
        /// Returns null
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema() {
            return null;
        }

        /// <summary>
        /// Custom Xml Serializition method to write
        /// </summary>
        /// <param name="w"></param>
        public void WriteXml(XmlWriter writer) {
            // Write dimensions
            writer.WriteAttributeString("Zeilen", this.NumberOfRows.ToString());
            writer.WriteAttributeString("Spalten", this.NumberOfColumns.ToString());

            // Write matrix
            for(int i=0; i<this.NumberOfRows; i++)
            {
                for (int j = 0; j < this.NumberOfColumns; j++) {
                    XmlSerializer s = new XmlSerializer(typeof(SearchValue));
                    s.Serialize(writer, this.Values[i, j]);
                }
            }

            // Write column names
            for (int i = 0; i < this._columnNames.Length; i++) {
                string value = this._columnNames[i];
                writer.WriteElementString("Spaltenname", value);
            }
        }

        /// <summary>
        /// Custom Xml Serializition method to read
        /// </summary>
        /// <param name="w"></param>
        public void ReadXml(XmlReader reader) {
            int numberOfRows = int.Parse(reader.GetAttribute("Zeilen"));
            int numberOfColumns = int.Parse(reader.GetAttribute("Spalten"));

            this.Values = new SearchValue[numberOfRows, numberOfColumns];
            this._columnNames = new string[numberOfColumns];

            reader.ReadStartElement();

            // Read matrix
            for (int i = 0; i < this.NumberOfRows; i++) {
                for (int j = 0; j < this.NumberOfColumns; j++) {
                    
                    XmlSerializer s = new XmlSerializer(typeof(SearchValue));
                    this.Values[i, j] = (SearchValue)s.Deserialize(reader);
                    
                }
            }

            // Read column names
            for (int i = 0; i < this._columnNames.Length; i++) {
                this._columnNames[i] = reader.ReadElementString("Spaltenname");
            }
        }

        /// <summary>
        /// Returns a List of all numeric values.
        /// </summary>
        /// <returns></returns>
        public List<decimal> GetDecimalValues() {
            if (_numericValues == null) {
                _numericValues = new List<decimal>();

                for (int i = 0; i < this.NumberOfRows; i++) {
                    for (int j = 0; j < this.NumberOfColumns; j++) {
                        if ( (CellTypes[i, j] & SearchValueMatrixCellType.IsNumeric) == SearchValueMatrixCellType.IsNumeric ) {
                            _numericValues.Add(Values[i, j].Numeric.Value);
                        }
                    }
                }

                _numericValues = _numericValues.Distinct().ToList();
            }

            return _numericValues;
        }

        /// <summary>
        /// Returns a List of all integer values.
        /// </summary>
        /// <returns></returns>
        public List<Int64> GetIntegerValues() {
            if (_integerValues == null) {
                _integerValues = new List<Int64>();

                for (int i = 0; i < this.NumberOfRows; i++) {
                    for (int j = 0; j < this.NumberOfColumns; j++) {
                        if ( (CellTypes[i, j] & SearchValueMatrixCellType.IsInteger) == SearchValueMatrixCellType.IsInteger) {
                            _integerValues.Add(Values[i, j].Integer.Value);
                        }
                    }
                }

                _integerValues = _integerValues.Distinct().ToList();
            }

            return _integerValues;
        }

        /// <summary>
        /// Returns a List of all unsigned integer values.
        /// </summary>
        /// <returns></returns>
        public List<UInt64> GetUnsignedIntegerValues() {
            if (_unsignedIntegerValues == null) {
                _unsignedIntegerValues = new List<UInt64>();

                for (int i = 0; i < this.NumberOfRows; i++) {
                    for (int j = 0; j < this.NumberOfColumns; j++) {
                        if ( (CellTypes[i, j] & SearchValueMatrixCellType.IsUnsignedInteger) == SearchValueMatrixCellType.IsUnsignedInteger ) {
                            _unsignedIntegerValues.Add(Values[i, j].UnsignedInteger.Value);
                        }
                    }
                }

                _unsignedIntegerValues = _unsignedIntegerValues.Distinct().ToList();
            }

            return _unsignedIntegerValues;
        }

        /// <summary>
        /// Returns a List of all datetime values.
        /// </summary>
        /// <returns></returns>
        public List<DateTime> GetDateTimeValues() {
            if (_dateTimeValues == null) {
                _dateTimeValues  = new List<DateTime>();

                for (int i = 0; i < this.NumberOfRows; i++) {
                    for (int j = 0; j < this.NumberOfColumns; j++) {
                        if ( (CellTypes[i, j] & SearchValueMatrixCellType.IsDateTime) == SearchValueMatrixCellType.IsDateTime ) {
                            _dateTimeValues.Add(Values[i, j].DateTime.Value);
                        }
                    }
                }

                _dateTimeValues = _dateTimeValues.Distinct().ToList();
            }

            return _dateTimeValues;
        }


        /// <summary>
        /// Disables a single Cell
        /// </summary>
        /// <param name="nRowIndex">Row Index of the Cell in the SVM</param>
        /// <param name="nColumnIndex">Column Index of the Cell in the SVM</param>
        public void DisableCell(int nRowIndex, int nColumnIndex)
        {
            if ((CellTypes[nRowIndex, nColumnIndex] & SearchValueMatrixCellType.UseCell) == SearchValueMatrixCellType.UseCell)
            {
                CellTypes[nRowIndex, nColumnIndex] ^= SearchValueMatrixCellType.UseCell;
            }
            InitSubArrays();
        }

        /// <summary>
        /// Enables a single Cell
        /// </summary>
        /// <param name="nRowIndex">Row Index of the Cell in the SVM</param>
        /// <param name="nColumnIndex">Column Index of the Cell in the SVM</param>
        public void EnableCell(int nRowIndex, int nColumnIndex)
        {
            if (!((CellTypes[nRowIndex, nColumnIndex] & SearchValueMatrixCellType.UseCell) == SearchValueMatrixCellType.UseCell))
            {
                CellTypes[nRowIndex, nColumnIndex] ^= SearchValueMatrixCellType.UseCell;
            }
            InitSubArrays();
        }

        #region Dis- & Enable Rows/Columns
        /// <summary>
        /// Disables a single Row
        /// </summary>
        /// <param name="nRowIndex">Row Index of the Row</param>
        public void DisableRow(int nRowIndex)
        {
            for (int i = 0; i < CellTypes.GetLength(1); i++)
            {
                if ((CellTypes[nRowIndex, i] & SearchValueMatrixCellType.UseCell) == SearchValueMatrixCellType.UseCell)
                {
                    CellTypes[nRowIndex, i] ^= SearchValueMatrixCellType.UseCell;
                }
            }
            InitSubArrays();
        }

        /// <summary>
        /// Enables a single Row
        /// </summary>
        /// <param name="nRowIndex">Row Index of the Row</param>
        public void EnableRow(int nRowIndex)
        {
            for (int i = 0; i < CellTypes.GetLength(1); i++)
            {
                if (!((CellTypes[nRowIndex, i] & SearchValueMatrixCellType.UseCell) == SearchValueMatrixCellType.UseCell))
                {
                    CellTypes[nRowIndex, i] ^= SearchValueMatrixCellType.UseCell;
                }
            }
            InitSubArrays();
        }

        /// <summary>
        /// Disables a single column
        /// </summary>
        /// <param name="nColumnindex">Column Index</param>
        public void DisableColumn(int nColumnindex)
        {
            for (int i = 0; i < CellTypes.GetLength(0); i++)
            {
                if ((CellTypes[i, nColumnindex] & SearchValueMatrixCellType.UseCell) == SearchValueMatrixCellType.UseCell)
                {
                    CellTypes[i, nColumnindex] ^= SearchValueMatrixCellType.UseCell;
                }
            }
            InitSubArrays();
        }

        /// <summary>
        /// Enables a single column
        /// </summary>
        /// <param name="nColumnindex">column index</param>
        public void EnableColumn(int nColumnindex)
        {
            for (int i = 0; i < CellTypes.GetLength(0); i++)
            {
                if (!((CellTypes[i, nColumnindex] & SearchValueMatrixCellType.UseCell) == SearchValueMatrixCellType.UseCell))
                {
                    CellTypes[i, nColumnindex] ^= SearchValueMatrixCellType.UseCell;
                }
            }
            InitSubArrays();
        }
        #endregion Dis- & Enable Rows/Columns

        #region InitSubArrays

        /// <summary>
        /// This method initialezes the subarrays for each datatype. During the search process so only the data is requested for the current matchting type
        /// </summary>
        private void InitSubArrays()
        {
            int lNumericValues = 0;
            int lIntegerValues = 0;
            int lUnsignedIntegerValues = 0;
            int lDateTimeValues = 0;
            int lTextValues = 0;
            int lAllValues = 0;


            //This block gets the required lengths for the different subarrays
            for (int i = 0; i < NumberOfRows; i++) {
                for (int j = 0; j < NumberOfColumns; j++) {
                    // Only use Cells, which are cleared for usage
                    if ((CellTypes[i, j] & SearchValueMatrixCellType.UseCell) == SearchValueMatrixCellType.UseCell) {
                        if ((CellTypes[i, j] & SearchValueMatrixCellType.IsNumeric) ==
                            SearchValueMatrixCellType.IsNumeric) {
                            lNumericValues++;
                        }
                        if ((CellTypes[i, j] & SearchValueMatrixCellType.IsInteger) ==
                            SearchValueMatrixCellType.IsInteger) {
                            lIntegerValues++;
                        }
                        if ((CellTypes[i, j] & SearchValueMatrixCellType.IsUnsignedInteger) ==
                            SearchValueMatrixCellType.IsUnsignedInteger) {
                            lUnsignedIntegerValues++;
                        }
                        if ((CellTypes[i, j] & SearchValueMatrixCellType.IsDateTime) ==
                            SearchValueMatrixCellType.IsDateTime) {
                            lDateTimeValues++;
                        }
                        if (((CellTypes[i, j] & SearchValueMatrixCellType.IsText) == SearchValueMatrixCellType.IsText) &&
                            Values[i, j].String.Trim().Length != 0) {
                            lTextValues++;
                        }
                        lAllValues++;
                    }
                    //AllValues[i*NumberOfColumns + j] = Values[i, j];
                    //AllIndices[i*NumberOfColumns + j] = new SearchValueMatrixOrdinals(i, j);
                }
            }
            //Allocate the subarrays
            NumericCellsIndices = new int[lNumericValues];
            IntegerCellsIndices = new int[lIntegerValues];
            UnsignedIntegerCellsIndices = new int[lUnsignedIntegerValues];
            DateTimeCellsIndices = new int[lDateTimeValues];
            TextCellsIndices = new int[lTextValues];
            //AllIndices = new int[lAllValues];

            NumericValues = new SearchValue[lNumericValues];
            IntegerValues = new SearchValue[lIntegerValues];
            UnsignedIntegerValues = new SearchValue[lUnsignedIntegerValues];
            DateTimeValues = new SearchValue[lDateTimeValues];
            TextValues = new SearchValue[lTextValues];
            AllValues = new SearchValue[lAllValues];

            //Reset the counters
            lNumericValues = 0;
            lIntegerValues = 0;
            lUnsignedIntegerValues = 0;
            lDateTimeValues = 0;
            lTextValues = 0;
            lAllValues = 0;

            //This Block assigns the ordinals to the allocated arrays for each search value type
            for (int i = 0; i < NumberOfRows; i++) {
                for (int j = 0; j < NumberOfColumns; j++) {
                    // Only use Cells, which are cleared for usage
                    if ((CellTypes[i, j] & SearchValueMatrixCellType.UseCell) == SearchValueMatrixCellType.UseCell) {
                        if ((CellTypes[i, j] & SearchValueMatrixCellType.IsNumeric) ==
                            SearchValueMatrixCellType.IsNumeric) {
                                NumericCellsIndices[lNumericValues] = lAllValues;
                                NumericValues[lNumericValues] = Values[i, j];
                                lNumericValues++;
                        }
                        if ((CellTypes[i, j] & SearchValueMatrixCellType.IsInteger) ==
                            SearchValueMatrixCellType.IsInteger) {
                                IntegerCellsIndices[lIntegerValues] = lAllValues;
                                IntegerValues[lIntegerValues] = Values[i, j];
                                lIntegerValues++;
                        }
                        if ((CellTypes[i, j] & SearchValueMatrixCellType.IsUnsignedInteger) ==
                            SearchValueMatrixCellType.IsUnsignedInteger) {
                                UnsignedIntegerCellsIndices[lUnsignedIntegerValues] = lAllValues;
                                UnsignedIntegerValues[lUnsignedIntegerValues] = Values[i, j];
                                lUnsignedIntegerValues++;
                        }
                        if ((CellTypes[i, j] & SearchValueMatrixCellType.IsDateTime) ==
                            SearchValueMatrixCellType.IsDateTime) {
                                DateTimeCellsIndices[lDateTimeValues] = lAllValues;
                                DateTimeValues[lDateTimeValues] = Values[i, j];
                                lDateTimeValues++;
                        }
                        if (((CellTypes[i, j] & SearchValueMatrixCellType.IsText) == SearchValueMatrixCellType.IsText) &&
                                Values[i, j].String.Trim().Length != 0) {
                                    TextCellsIndices[lTextValues] = lAllValues;
                                    TextValues[lTextValues] = Values[i, j];
                                    lTextValues++;
                        }
                        AllValues[lAllValues] = Values[i, j];
                        //AllIndices[lAllValues] = new SearchValueMatrixOrdinals(i, j);
                        lAllValues++;
                    }
                }
            }
            _numberOfValues = lAllValues;
        }
        #endregion InitSubArrays

        public static SearchValueMatrix CreateFromQuery(Query query) {
            //Count how many columns are used in the search
            int usedColumns = query.Columns.Count(column => column.IsUsedInSearch);

            SearchValueMatrix svm = new SearchValueMatrix(query.Rows.Count, usedColumns);
            Dictionary<RowEntry, int> rowEntryToId = query.RowEntryToId();
            int currentColIndex;
            for (int i = 0; i < query.Rows.Count; ++i) {
                currentColIndex = 0;
                for (int j = 0; j < query.Columns.Count; ++j) {
                    if (!query.Columns[j].IsUsedInSearch) continue;
                    svm.Values[i, currentColIndex] = new SearchValue(query.Rows[i].RowEntries[j].RuleDisplayString,
                                                        CultureInfo.InvariantCulture, query.Columns[j], rowEntryToId[query.Rows[i].RowEntries[j]]);
                    if (query.Rows[i].RowEntries[j].Status != RowEntryStatus.Used)
                        svm.Values[i, currentColIndex].UseEntry = false;
                    currentColIndex++;
                }
            }
            currentColIndex = 0;
            for (int j = 0; j < query.Columns.Count; ++j) {
                if (query.Columns[j].IsUsedInSearch) {
                    svm.SetColumnName(currentColIndex, query.Columns[j].Name);
                    currentColIndex++;
                }
            }
            svm.InitTypes();

            return svm;
        }
        #endregion Methods
    }
}