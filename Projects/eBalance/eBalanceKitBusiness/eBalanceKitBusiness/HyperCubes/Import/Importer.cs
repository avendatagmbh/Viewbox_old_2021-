using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml;
using AvdCommon.DataGridHelper;
using AvdCommon.DataGridHelper.Interfaces;
using Taxonomy.Enums;
using Taxonomy.PresentationTree;
using Utils;
using eBalanceKitBusiness.HyperCubes.DbMapping;
using eBalanceKitBusiness.HyperCubes.Import.Templates;
using eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateGenerator;
using eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateLoader;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Structures;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;
using DataColumn = System.Data.DataColumn;
using DataGrid = System.Windows.Forms.DataGrid;
using DataRow = System.Data.DataRow;
using System.ComponentModel;


namespace eBalanceKitBusiness.HyperCubes.Import
{
    public class Importer : ImportBase
    {
        
        #region variable


        #region Entry
        private DbEntityHyperCubeImport _entry;

        public DbEntityHyperCubeImport Entry {
            get { return _entry; }
            set {
                if (_entry != value) {
                    _entry = value;
                    OnPropertyChanged("Entry");
                }
            }
        }
        #endregion Entry

        #region CsvSource
        private string _csvSource;

        public string CsvSource {
            get { return _csvSource; }
            set {
                if (_csvSource != value) {
                    _csvSource = value;
                    if(File.Exists(value))
                        ValidateCsvSource();
                    OnPropertyChanged("CsvSource");
                }
            }
        }
        #endregion CsvSource

        //public HashSet<> I { get; private set; }
        
        #region TemplateLoader
        private TemplateLoaderDB _templateLoader;

        public TemplateLoaderDB Templates
        {
            get { return _templateLoader; }
            set {
                if (_templateLoader != value) {
                    _templateLoader = value;
                    OnPropertyChanged("Templates");
                }
            }
        }
        #endregion TemplateLoader

        #region CurrentAssignmentIsColumn
        private bool _currentAssignmentIsColumn;

        public bool CurrentAssignmentIsColumn {
            get { return _currentAssignmentIsColumn; }
            set {
                if (_currentAssignmentIsColumn != value) {
                    _currentAssignmentIsColumn = value;
                    OnPropertyChanged("CurrentAssignmentIsColumn");
                }
            }
        }
        #endregion CurrentAssignmentIsColumn

        #region TemplateGenerator
        private TemplateGeneratorDb _templateGenerator;
        /// <summary>
        /// Object to access AddRow, AddColumn and more
        /// </summary>
        public TemplateGeneratorDb TemplateGenerator
        {
            get { return _templateGenerator; }
            set
            {
                if (_templateGenerator != value)
                {
                    _templateGenerator = value;
                    OnPropertyChanged("TemplateGenerator");
                }
            }
        }

        #endregion TemplateGenerator

        #region CsvSourceValidation
        private bool _csvSourceValidation;

        public bool CsvSourceValidation {
            get { return _csvSourceValidation; }
            set {
                if (_csvSourceValidation != value) {
                    _csvSourceValidation = value;
                    OnPropertyChanged("CsvSourceValidation");
                }
            }
        }
        #endregion CsvSourceValidation

        private void ValidateCsvSource() {
            if (!File.Exists(Config.CsvFileName)) {
                CsvSourceValidation = false;
                return;
            }

            try {
                LoadImportPreview();
                CsvSourceValidation = true;
            } catch (Exception) {
                // TODO log exception
                CsvSourceValidation = false;
                //throw;
            }
        }

        #region CsvData
        /// <summary>
        /// Gets or sets a data table, which could be used to show a preview of the imported data in the gui.
        /// </summary>
        public DataTable CsvData
        {
            get { return _csvData; }
            private set
            {
                _csvData = value;
                OnPropertyChanged("CsvData");
            }
        }
        private DataTable _csvData;
        #endregion

        #region PreviewData
        private System.Data.DataTable _previewData;
        /// <summary>
        /// Contains a preview of the CSV with only limited number of rows.
        /// </summary>
        public System.Data.DataTable PreviewData {
            get { return _previewData; }
            set {
                if (_previewData != value) {
                    _previewData = value;
                    OnPropertyChanged("PreviewData");
                }
            }
        }
        #endregion PreviewData

        #region CsvAssignment
        /// <summary>
        /// Gets or sets a data table, which could be used to show a preview of the imported data in the gui.
        /// </summary>
        public DataTable CsvAssignment
        {
            get { return _csvAssignment; }
            private set
            {
                _csvAssignment = value;
                OnPropertyChanged("CsvAssignment");
            }
        }
        private DataTable _csvAssignment;
        #endregion

        /// <summary>
        /// Store the assignment for the specified column.
        /// </summary>
        /// <param name="column">The column number in the CSV.</param>
        /// <param name="assignmentValue">The value of the HyperCubeColumn dict that will be assigned.</param>
        public void StoreColumnAssignment(int column, TemplateBase.HyperCubeHeader header) {
            if (column >= 0) {
                if (Config.Dimensions != null && Config.Dimensions.Count > 0) {
                    for (int i = 1; i < Config.Dimensions.Count; i++) {
                        if ((CsvData.Columns[column + i] as CsvColumn).AssignmentFlag == true) {
                            LastWarning = "Column can not assigned. Problem with configured cube dimensions.";
                            return;
                        }
                    }
                }
                

                TemplateGenerator.AssignColumn(column, header);
                
                foreach (CsvRow val in CsvData.Rows) {
                    if (Config.Dimensions != null && Config.Dimensions.Count > 0) {
                        for (int i = 0; i < Config.Dimensions.Count; i++) {
                            (val[column + i] as DataValue).Flag = true;
                        }
                    }
                    else {
                        (val[column] as DataValue).Flag = true;
                    }
                }

                if (Config.Dimensions != null && Config.Dimensions.Count > 0) {
                    for (int i = 0; i < Config.Dimensions.Count; i++) {
                        (CsvData.Columns[column + i] as CsvColumn).AssignmentFlag = true;
                        (CsvData.Columns[column + i] as CsvColumn).Name = header.Header + " (" + Config.Dimensions[i].DimensionName + ")";
                    }
                }
                else {
                    (CsvData.Columns[column] as CsvColumn).AssignmentFlag = true;
                    (CsvData.Columns[column] as CsvColumn).Name = header.Header;
                }
            }
        }

        #region LastWarning
        private string _lastWarning;

        public string LastWarning {
            get { return _lastWarning; }
            set {
                if (_lastWarning != value) {
                    _lastWarning = value;
                    OnPropertyChanged("LastWarning");
                }
            }
        }
        #endregion LastWarning



        public void StoreRowAssignment(int row, HyperCubes.Import.Templates.TemplateBase.HyperCubeHeader hyperCubeHeader) {
            if (row >= 0) {
                
                var currentRow = CsvData.Rows[row] as CsvRow;

                    //hyperCubeHeader.AssignmentFlag = true;
                    //hyperCubeHeader.CsvPosition = row;
                    TemplateGenerator.AssignRow(row, hyperCubeHeader);
                
                currentRow.AssignmentFlag = true;

                currentRow.Header = hyperCubeHeader.Header;
            }
        }

        /// <summary>
        /// Removes an assignment for the specified column.
        /// </summary>
        /// <param name="columnNumber">Column number in the CsvData.</param>
        public void RemoveColumnAssignment(int columnNumber) {

            if (TemplateGenerator.ColumnHeaders.Count(x => x.CsvPosition == columnNumber) > 0) {
                foreach (CsvRow row in CsvData.Rows)
                {
                    if (Config.Dimensions != null) {
                        for (int i = 0; i < Config.Dimensions.Count; i++) {
                            (row[columnNumber + i] as DataValue).Flag = false;
                        }
                    }
                    else {
                        (row[columnNumber] as DataValue).Flag = false;
                    }
                }
                
                if (Config.Dimensions != null) {
                    for (int i = 0; i < Config.Dimensions.Count; i++) {
                        (CsvData.Columns[columnNumber + i] as CsvColumn).AssignmentFlag = false;
                        (CsvData.Columns[columnNumber + i] as CsvColumn).Name = string.Empty;
                    }
                }
                else {
                    (CsvData.Columns[columnNumber] as CsvColumn).AssignmentFlag = false;
                    (CsvData.Columns[columnNumber] as CsvColumn).Name = string.Empty;
                }

                TemplateGenerator.RemoveColumnAssignment(columnNumber);
            }
        }

        /// <summary>
        /// Removes an assignment for the specified row.
        /// </summary>
        /// <param name="rowNumber">Row number in the CsvData.</param>
        public void RemoveRowAssignment(int rowNumber) {
            if (TemplateGenerator.RowHeaders.Count(x => x.CsvPosition == rowNumber) > 0) {
                (CsvData.Rows[rowNumber] as CsvRow).AssignmentFlag = false;
                (CsvData.Rows[rowNumber] as CsvRow).Header = string.Empty;
                foreach (DataValue entry in CsvData.Rows[rowNumber].RowEntries)
                {
                    entry.Flag = false;
                }
                TemplateGenerator.RemoveRowAssignment(rowNumber);
            }
        }

        #region Properties

        /// <summary>
        /// Contains the done entries.
        /// </summary>
        public HashSet<string> RightEntries { get; private set; }
        /// <summary>
        /// Object of the ImportConfig
        /// </summary>
        public ImportConfig Config { get; set; }

        #region WrongEntries
        private HashSet<ImportConfig.FailedEntry> _wrongEntries;
        /// <summary>
        /// Contains the failed entries. 
        /// (DimensionKey|csv value|cube value|row caption|column caption) 
        /// </summary>
        public HashSet<ImportConfig.FailedEntry> WrongEntries
        {
            get { return _wrongEntries; }
            set
            {
                if (_wrongEntries != value)
                {
                    _wrongEntries = value;
                    Config.OnPropertyChanged("WrongEntries");
                }
            }
        }
        #endregion WrongEntries

        /// <summary>
        /// HyperCubeDimensionKey, Value from CSV
        /// </summary>
        private Dictionary<long, string> _csvDict;
        /// <summary>
        /// ColumnId (their position), DimensionId
        /// </summary>
        private Dictionary<int, long> _columnDict;
        /// <summary>
        /// RowId (their Position), DimensionId
        /// </summary>
        private Dictionary<int, long> _rowDict;
        
        #endregion
        
        #endregion


        public Utils.CsvReader CsvReader { get; set; }

        #region Constructor
        public Importer(Document document) : base(document) { Init(); }
        public Importer(Document document, ImportConfig.PossibleHyperCubes hyperCubeName) : base(document, hyperCubeName) { Init(); }
        public Importer(Document document, string elementId) : base(document, elementId) { Init(); }
        public Importer(IHyperCube cube)
            : base(cube) {

            Config = new ImportConfig();
            Init();
            Config.Initialize();
            // To_Do only test -- sev
            //_templateGenerator = new TemplateGeneratorDb(cube);
        }


        public void ModifyTemplate(DbMapping.DbEntityHyperCubeImport dbEntry) {
            TemplateGenerator = new TemplateGeneratorDb(cube, dbEntry);
            Templates.Load(dbEntry);
            //Init();
            InitDictonaries();
            TemplateGenerator.LoadDictsForEntryModification(_columnDict, _rowDict, dbEntry.IsInverse);

            this.TemplateGenerator.Entry.PropertyChanged += new PropertyChangedEventHandler(Config_PropertyChanged);
            this.Entry = TemplateGenerator.Entry;
            this.Entry.PropertyChanged += new PropertyChangedEventHandler(Entry_PropertyChanged);
            //var table = cube.GetTable();
            /*
            foreach (KeyValuePair<long, long> pair in _columnDict) {
                TemplateGenerator.AssignColumn((int) pair.Value, );
            }
            */
        }

        void Entry_PropertyChanged(object sender, PropertyChangedEventArgs e) {

            if (e.PropertyName != "Dimension") {
                return;
            }

            foreach (string dimension in Entry.DimensionOrder.Split('|')) {
                long dimId;
                long.TryParse(dimension, out dimId);
                var dim = cube.Dimensions.GetDimensionValueById(dimId);
                Config.Dimensions.Add(new ImportConfig.DimensionInfo { HyperCubeDimensionValue = dim, UniquIdentifier = dimId, DimensionName = dim.Label });
            }
        }

        private void Init() {
            this.CsvReader = new Utils.CsvReader(string.Empty);
            CsvReader.HeadlineInFirstRow = false;

            //Config = new ImportConfig();

            _csvDict = new Dictionary<long, string>();
            _columnDict = new Dictionary<int, long>(); 
            _rowDict = new Dictionary<int, long>();
            RightEntries = new HashSet<string>();
            WrongEntries = new HashSet<ImportConfig.FailedEntry>();
            CsvSourceValidation = false;
            Templates = new TemplateLoaderDB(cube);
            this.Templates.PropertyChanged += new PropertyChangedEventHandler(SelectedTemplate_PropertyChanged);

            //this.CsvReader = new Utils.CsvReader(string.Empty);
            //CsvReader.HeadlineInFirstRow = false;

            this.Config.PropertyChanged += new PropertyChangedEventHandler(FileName_PropertyChanged);

            if (cube.Dimensions.AllDimensionItems.Count() == 3) {
                int dimensionCounter = 0;
                foreach (IHyperCubeDimensionValue value in cube.Dimensions.AllDimensionItems.First().Values) {
                    Config.Dimensions.Add(new ImportConfig.DimensionInfo {Index = dimensionCounter, DimensionName = value.Label, HyperCubeDimensionValue = value, UniquIdentifier = value.ElementId});
                    dimensionCounter++;
                }
            }


        }
        
        #endregion

        #region eventHandler
        void SelectedTemplate_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != "SelectedTemplate") {
                return;
            }

            Entry = _templateLoader.SelectedTemplate;
        }

        void FileName_PropertyChanged(object sender, PropertyChangedEventArgs e) {

            if (e.PropertyName == "Dimensions") {
                //Entry.DimensionOrder = string.Join("|", from dim in Config.Dimensions select dim.UniquIdentifier);
            }

            if (e.PropertyName != "CsvFileName") {
                return;
            }
            if (!File.Exists(Config.CsvFileName)) {
                Config.Warning = "Die angegebene Datei existiert nicht.";
                return;
            }


            this.CsvReader.Filename = this.Config.CsvFileName;

            //Check which is the most likely seperator
            StreamReader reader = null;
            try {
                if ((Entry != null) && (Entry.Encoding != null)) {
                    reader = new StreamReader(this.Config.CsvFileName, Entry.Encoding);
                }
                else {
                    reader = new StreamReader(this.Config.CsvFileName);
                }

                if (!reader.EndOfStream) {
                    string csvLine = reader.ReadLine().Trim();
                    if (Entry != null) {
                        Entry.Seperator = FindMostOftenCharacter(csvLine, ",;|");
                        Entry.Delimiter = FindMostOftenCharacter(csvLine, "\"'");
                    }
                }
            }
            catch (IOException ioex) {
                Config.Warning = ioex.Message;
            }
            catch (Exception ex) {
                Config.Warning = ex.Message;
            }
            finally {
                if (reader != null)
                    reader.Close();
            }

            if (reader != null)
                LoadImportPreview();

        }

        void Config_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            
            if (e.PropertyName != "Warning")
                Config.Warning = string.Empty;
            
            try {
                switch (e.PropertyName) {
                    case "Encoding":
                    case "Seperator":
                    case "TextDelimiter":
                        if (!string.IsNullOrEmpty(Entry.Seperator)) {
                            LoadImportPreview();
                        }
                        break;

                    case "CsvFileName": {
                            
                            //else
                            //Config.Warning = "Zugriff verweigert";
                            break;
                        }
                }
            }
            catch (Exception) {
                //MessageBox.Show(this.Owner, ex.Message);
            }
        }

        #region FindMostOftenCharacter
        string FindMostOftenCharacter(string line, string characters) {
            int max = -1; string maxSeperator = Convert.ToString(characters[0]);
            foreach (char seperator in characters) {
                int occurences = line.Split(seperator).Length - 1;
                if (occurences > max) {
                    max = occurences;
                    maxSeperator = Convert.ToString(seperator);
                }
            }
            return maxSeperator;
        }
        #endregion
        #endregion eventHandler


        /// <summary>
        /// Imports the given CSV file.
        /// </summary>
        public void Import() {

            // someone forgot to load the dictionaries (StartTemplateLoading)
            if (_columnDict.Count == 0 || _rowDict.Count == 0) {
                //Entry.XmlAssignment;
                if (Templates.SelectedTemplate == null) 
                    Templates.SelectedTemplate = Entry;
                InitDictonaries();
            }

            // Remove all old data
            cube.Clear();

            // If no "preview" was loaded
            if (_csvData.Columns.Count == 0) {
                // Load CSV data
                LoadCsv();
            }

            if (cube.Dimensions.AllDimensionItems.Count() == 3) {

                Config.Dimensions = new ObservableCollection<ImportConfig.DimensionInfo>();
                foreach (string dimension in Entry.DimensionOrder.Split('|')) {
                    long dimId;
                    long.TryParse(dimension, out dimId);
                    var dim = cube.Dimensions.GetDimensionValueById(dimId);
                    Config.Dimensions.Add(new ImportConfig.DimensionInfo {
                        HyperCubeDimensionValue = dim,
                        UniquIdentifier = dimId,
                        DimensionName = dim.Label
                    });
                }
            }

            // Create an internal dictionary
            LoadCsvDict();

            // write the new Infos
            if (cube.Dimensions.AllDimensionItems.Count() < 3) {
                UpdateValuesWithDict();
            } else {
                UpdateValuesWithDict3d();
            }
        }

        /// <summary>
        /// Removes all entries but will store the old values in the log.
        /// ToDo: LOG
        /// </summary>
        private void ClearCube() {
            
            cube.Clear();

        }


        public void GoToRowAssignment() {
            int rowCounter = 0;

            foreach (CsvRow row in CsvData.Rows) {
                foreach (CsvColumn column in CsvData.Columns)
                    column.AssignmentFlag = false;


                var rowEntry = from e in TemplateGenerator.RowHeaders where e.CsvPosition == rowCounter select e;
                if (rowEntry.Any()) {
                    row.Header = rowEntry.First().Header;
                }

                var headerEntry = TemplateGenerator.RowHeaders.Where(x => x.CsvPosition == rowCounter);
                if (headerEntry.Any()) {
                    row.AssignmentFlag = true;
                    row.Header = TemplateGenerator.RowHeaders.First(x => x.CsvPosition == rowCounter).Header;

                    foreach (DataValue entry in row.RowEntries) {
                        entry.Flag = true;
                    }
                }
                else {
                    row.AssignmentFlag = false;

                    foreach (DataValue entry in row.RowEntries) {
                        entry.Flag = false;
                    }
                }
                rowCounter++;
            }
            CurrentAssignmentIsColumn = false;
        }

        public void GoToSummaryAssignment() {

            // someone forgot to load the dictionaries (StartTemplateLoading)
            if (_columnDict.Count == 0 || _rowDict.Count == 0) {
                InitDictonaries();
            }
            
            int columnCounter = 0;
            int rowCounter = 0;
            HashSet<TemplateBase.HyperCubeHeader> dict;
            if (TemplateGenerator == null) {
                if (Templates.ColumnHeaders == null || Templates.ColumnHeaders.Count == 0) {
                    Templates.LoadDicts(Templates.LoadColumnDict(), Templates.LoadRowDict()); 
                }
                dict = Templates.ColumnHeaders;
            } else {
                dict = TemplateGenerator.ColumnHeaders;
            }

            foreach (CsvColumn column in CsvData.Columns) {
                var entry = dict.First(x => x.CsvPosition == columnCounter);
                if (entry != null) {
                    column.Name = entry.Header;
                    column.AssignmentFlag = true;
                }
                else {
                    column.AssignmentFlag = false;
                }
                columnCounter++;
            }

            if (TemplateGenerator == null) {
                dict = Templates.RowHeaders;
            } else {
                dict = TemplateGenerator.RowHeaders;
            }

            foreach (CsvRow row in CsvData.Rows) {
                var entry = dict.First(x => x.CsvPosition == rowCounter);
                if (entry != null) {
                    row.AssignmentFlag = true;
                    row.Header = entry.Header;
                    /*
                    foreach (DataValue entry in row.RowEntries) {
                        entry.Flag = true;
                    }
                    */
                }
                else {
                    row.AssignmentFlag = false;
                    /*
                    foreach (DataValue entry in row.RowEntries) {
                        entry.Flag = false;
                    }
                    */
                }

                foreach (DataValue value in row.RowEntries) {
                    value.Flag = false;
                }

                rowCounter++;
            }
        }

        public void GoToColumnAssignment() {
            int columnCounter = 0;
            int rowCounter = 0;

            foreach (CsvColumn column in CsvData.Columns) {

                    var entry = from e in TemplateGenerator.RowHeaders where e.CsvPosition == columnCounter select e;
                    if (entry.Any()) {
                        column.Name = entry.First().Header;
                        column.AssignmentFlag = true;
                    } else {
                        column.AssignmentFlag = false;
                    }
                foreach (CsvRow row in CsvData.Rows) {
                    row.AssignmentFlag = false;

                    if (entry.Any()) {
                        (row.RowEntries[columnCounter] as DataValue).Flag = true;
                    } else {
                        (row.RowEntries[columnCounter] as DataValue).Flag = false;
                    }

                    var rowEntry = from e in TemplateGenerator.RowHeaders where e.CsvPosition == rowCounter select e;
                    if (rowEntry.Any()) {
                        row.Header = rowEntry.First().Header;
                    }
                    rowCounter++;
                }
                columnCounter++;
            }
            CurrentAssignmentIsColumn = true;

        }

        
        /// <summary>
        /// Imports the given CSV file.
        /// </summary>
        /// <param name="csvSource">Path (with filename) to the csv file.</param>
        public void Import(string csvSource) {

            // someone forgot to load the dictionaries (StartTemplateLoading)
            if (_columnDict.Count == 0 || _rowDict.Count == 0) {
                InitDictonaries();
            }

            Config.CsvFileName = csvSource;
            // Remove all old data but log the old data
            ClearCube();

            // If no "preview" was loaded
            if (_csvData.Columns.Count == 0) {
                // Load CSV data
                LoadCsv();
            }
            // Create an internal dictionary
            LoadCsvDict();
            // write the new Infos
            UpdateValuesWithDict();
        }
        
        public void LoadImportPreview(string csvSource) {
            Config.CsvFileName = csvSource;
            // Load CSV data
            LoadImportPreview();
        }
        
        public void LoadImportPreview() {
            // Load CSV data
            //InitCsv();
            if (CsvReader != null && !string.IsNullOrEmpty(Config.CsvFileName)) {
                //this.CsvReader.Separator = this.Config.Seperator[0];
                if (Entry != null) {

                    CsvReader.Filename = Config.CsvFileName;
                    CsvReader.Separator = Entry.Seperator[0];
                    CsvReader.StringsOptionallyEnclosedBy = !string.IsNullOrEmpty(Entry.Delimiter) ? Entry.Delimiter[0] : '\0';
                    PreviewData = CsvReader.GetCsvData(50, Entry.Encoding);
                }
                //else
                //    PreviewData = CsvReader.GetCsvData(50, Encoding.Default);
            }
        }

        /// <summary>
        /// Methode to init a new TemplateGenerator
        /// </summary>
        public void CreateNewTemplate(string templateName = null, string comment = null) {
            TemplateGenerator = new TemplateGeneratorDb(cube, Config.CsvFileName, templateName, comment);
            this.TemplateGenerator.Entry.PropertyChanged += new PropertyChangedEventHandler(Config_PropertyChanged);
            Entry = TemplateGenerator.Entry;
            //this.Entry.PropertyChanged += new PropertyChangedEventHandler(Config_PropertyChanged);
        }

        /// <summary>
        /// Initializes the TemplateGenerator
        /// </summary>
        public void LoadTemplateOverview() {
            Templates = new TemplateLoaderDB(cube);
        }

        /// <summary>
        /// Initializes the TemplateGenerator. Automatically called from the constructor.
        /// </summary>
        public void LoadTemplateOverview(IHyperCube cube) {
            Templates = new TemplateLoaderDB(cube);
        }


        #region private

        /// <summary>
        /// Load the dicts for columns and rows after loading the xml assignment from the database.
        /// </summary>
        private void InitDictonaries() {
            _columnDict = Templates.LoadColumnDict();
            _rowDict = Templates.LoadRowDict();
        }

        /// <summary>
        /// Read the CSV file
        /// </summary>
        public void LoadCsv(string filename = null, DataTable csvTable = null) {
            
            if (filename != null) {
                CsvReader.Filename = filename;
            } else {
                CsvReader.Filename = Config.CsvFileName;
            }

            CsvReader.Separator = Entry.Seperator[0];
            CsvReader.StringsOptionallyEnclosedBy = !string.IsNullOrEmpty(Entry.Delimiter) ? Entry.Delimiter[0] : '\0';
            if (csvTable == null) {
                CsvData = DataTable2AvdCommonDataTable(CsvReader.GetCsvData(0, Entry.Encoding));
            } else {
                csvTable = DataTable2AvdCommonDataTable(CsvReader.GetCsvData(0, Entry.Encoding));
            }
        }

        private DataTable DataTable2AvdCommonDataTable(System.Data.DataTable systemDataTable) {
            DataTable newDataTable = new DataTable();
            int columnCounter = 0;
            int rowCounter = 0;

            foreach (DataColumn column in systemDataTable.Columns) {
                newDataTable.AddColumn(new CsvColumn(false, string.Empty));//column.ColumnName == null ? "Spalte_" + columnCounter.ToString() : column.ColumnName));
                columnCounter++;
            }

            foreach (DataRow dataRow in systemDataTable.Rows) {
                newDataTable.AddRow(new CsvRow(string.Empty, (from item in dataRow.ItemArray select new DataValue(item, false))));
                rowCounter++;
            }

            return newDataTable;
        }

        /* //maybe needed to import 3d cubes from different files
        /// <summary>
        /// Load the dict for the csv (DimensionKey, Value)
        /// </summary>
        private Dictionary<long, string> LoadCsvDict(DataTable csvData) {
            Dictionary<long, string> result = new Dictionary<long, string>();
            foreach (KeyValuePair<int, long> rowEntry in _rowDict) {
                //rowEntry.Key
                foreach (KeyValuePair<int, long> columnEntry in _columnDict) {
                    long dimKey;
                    // Get the HypercubeDimensionKey (long)
                    if (Entry.IsInverse) {  
                        // If the assignment was inverse we have rowIds instead of columnIds
                        dimKey = cube.GetDimensionkey(rowEntry.Value, columnEntry.Value);
                    } else {
                        dimKey = cube.GetDimensionkey(columnEntry.Value, rowEntry.Value);
                    }
                    // The assignment of rows is still stored in the _rowDict
                    var val = csvData.Rows[rowEntry.Key][columnEntry.Key];

                    result.Add(dimKey, val.ToString());
                }
            }
            return result;
        }
        */
        /// <summary>
        /// Load the dict for the csv (DimensionKey, Value)
        /// </summary>
        private void LoadCsvDict() {
            //_csvDict = new Dictionary<long, string>();
            ImportDatas = new List<ImportData>();
            
            foreach (KeyValuePair<int, long> rowEntry in _rowDict) {

                if (rowEntry.Key >= CsvData.Rows.Count) {
                    WrongEntries.Add(new ImportConfig.FailedEntry {
                        Message = "zugewiesene Zeile nicht vorhanden, die CSV enthält zu wenige Zeilen " +
                                  rowEntry.Key
                    }
                        );
                    continue;
                }

                foreach (KeyValuePair<int, long> columnEntry in _columnDict) {

                    if (columnEntry.Key >= CsvData.Columns.Count) {
                        WrongEntries.Add(new ImportConfig.FailedEntry {
                            Message = "zugewiesene Spalte nicht vorhanden, die CSV enthält zu wenige Spalten " +
                                      columnEntry.Key
                        }
                            );
                    }

                    if (cube.Dimensions.AllDimensionItems.Count() == 3) {
                        try {
                            

                            int counter = 0;
                            foreach (var dimensionInfo in Config.Dimensions) {

                                var data = new ImportData {
                                    DimensionValues = new List<IHyperCubeDimensionValue>()
                                };

                                if (Entry.IsInverse) {
                                    data.Value = CsvData.Rows[rowEntry.Key + counter][columnEntry.Key].ToString();
                                }
                                else {
                                    data.Value = CsvData.Rows[rowEntry.Key][columnEntry.Key + counter].ToString();
                                }

                                counter++;

                                data.DimensionValues.Add(dimensionInfo.HyperCubeDimensionValue);
                                data.DimensionValues.Add(cube.Dimensions.GetDimensionValueById(rowEntry.Value));

                                var x = (from val in cube.Dimensions.Primary.Values
                                         where val.ElementId == columnEntry.Value
                                         select val).First();
                                data.DimensionValues.Add(x);

                                ImportDatas.Add(data);
                            }
                        }
                        catch (Exception ex) {
                            // ToDo log exception
                            System.Diagnostics.Debug.Print(ex.Message); //throw;
                            WrongEntries.Add(new ImportConfig.FailedEntry { Message = ex.Message });
                        }
                    }
                    else {
                        long dimKey;
                        // Get the HypercubeDimensionKey (long)
                        if (Entry.IsInverse) {
                            // If the assignment was inverse we have rowIds instead of columnIds
                            dimKey = cube.GetDimensionkey(rowEntry.Value, columnEntry.Value);
                        } else {
                            dimKey = cube.GetDimensionkey(columnEntry.Value, rowEntry.Value);
                        }
                        //The assignment of rows is still stored in the _rowDict
                        var val = _csvData.Rows[rowEntry.Key][columnEntry.Key];

                        _csvDict.Add(dimKey, val.ToString());
                    }
                }
            }
        }

        private List<ImportData> ImportDatas;

        struct ImportData {
            /*
            public ImportData() {
                Value = string.Empty;
                DimensionValues = new List<IHyperCubeDimensionValue>();
            }
            */
            public string Value;
            public List<IHyperCubeDimensionValue> DimensionValues;
        }

        /// <summary>
        /// Write the new Values to the cube.
        /// </summary>
        private void UpdateValuesWithDict3d()
        {
            Dictionary<IHyperCubeItem, string> itemDict = new Dictionary<IHyperCubeItem, string>();


                foreach (ImportData data in ImportDatas) {
                    // The Dimensions are sorted like specified by the user
                    var item = cube.Items.GetItem(data.DimensionValues);
                    itemDict.Add(item, data.Value);
                }

            // Sort the cube items by dimension key so that the "deepest entries" (child) is insert first
            foreach(KeyValuePair<IHyperCubeItem, string> entry in itemDict.OrderByDescending(k => cube.Items.GetItemDimensionKey(k.Key))) {

                    var item = entry.Key;
                    string value = entry.Value;

                    var entryKey = cube.Items.GetItemDimensionKey(item);
                    // If the item is enabled
                    if (item.IsEditable) {
                        // Everything is fine and we can write the new value.
                        try {

                            item.Value = value;
                            RightEntries.Add("key " + entryKey + "| value " + value);
                        } catch (Exception ex) {
                            var msg = ex.Message;
                            if (item.PrimaryDimensionValue.ValueType == XbrlElementValueTypes.Monetary)
                                msg += Environment.NewLine + ResourcesCommon.ValueMustBeNumeric;
                            WrongEntries.Add(new ImportConfig.FailedEntry(msg, entryKey,
                                                                          item.Context.Members.First().Member.Label,
                                                                          item.PrimaryDimensionValue.Label,
                                                                          value,
                                                                          item.Value.ToString()));
                        }
                    }
                        // If the item is locked but the CSV value is different to the existing (calculated) value
                    else {
                        decimal val;

                        Decimal.TryParse(value, out val);
                        if (item.Value != null && !item.Value.Equals(val.ToString("#,0.00"))) {
                            // Get all the important informations

                            // Add the information to our list of wrong entries.
                            WrongEntries.Add(new ImportConfig.FailedEntry(ResourcesCommon.ValueIsComputed,
                                                                          entryKey,
                                                                          item.Context.Members.First().Member.Label,
                                                                          item.PrimaryDimensionValue.Label,
                                                                          value,
                                                                          item.Value.ToString()));
                        }
                    }

                }
            
        }


        /// <summary>
        /// Write the new Values to the cube.
        /// </summary>
        private void UpdateValuesWithDict()
        {
            foreach (KeyValuePair<long, string> csvEntry in _csvDict.Reverse())
            {
                var item = cube.Items.GetItem(csvEntry.Key);
                // If the item is enabled
                if (item.IsEditable)
                {
                    // Everything is fine and we can write the new value.
                    try {
                        
                        item.Value = csvEntry.Value;
                    RightEntries.Add("key " + csvEntry.Key + "| value " + csvEntry.Value);
                    }
                    catch (Exception ex) {
                        //Logs.LogManager.Instance.BalanceListChange();
                        var msg = ex.Message;
                        if (item.PrimaryDimensionValue.ValueType == XbrlElementValueTypes.Monetary)
                            msg += ResourcesCommon.ValueMustBeNumeric;
                        WrongEntries.Add(new ImportConfig.FailedEntry(msg, csvEntry.Key,
                                                                      item.Context.Members.First().Member.Label,
                                                                      item.PrimaryDimensionValue.Label, csvEntry.Value,
                                                                      item.Value.ToString())); 
                    }
                }
                // If the item is locked but the CSV value is different to the existing (calculated) value
                else {
                    decimal val;

                    Decimal.TryParse(csvEntry.Value, out val);
                    if (item.Value != null && !item.Value.Equals(val.ToString("#,0.00"))) {
                        // Get all the important informations
                        
                        // Add the information to our list of wrong entries.
                        WrongEntries.Add(new ImportConfig.FailedEntry(ResourcesCommon.ValueIsComputed, csvEntry.Key,
                                                                      item.Context.Members.First().Member.Label,
                                                                      item.PrimaryDimensionValue.Label, csvEntry.Value,
                                                                      item.Value.ToString()));
                    }
                }

            }
        }


        #endregion





        public void RemoveAllAssignments() {
            foreach (KeyValuePair<int, long> keyValuePair in _columnDict) {
                RemoveColumnAssignment(keyValuePair.Key);
            }
            foreach (KeyValuePair<int, long> keyValuePair in _rowDict) {
                RemoveRowAssignment(keyValuePair.Key);
            }
            //throw new NotImplementedException();
        }

    }
}
