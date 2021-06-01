using System.Linq;
using System.Xml;
using Utils;
using System.Collections.Generic;

namespace eBalanceKitBusiness.HyperCubes.Import.Templates
{
    /// <summary>
    /// Base class for access to the xml (writing & reading) with the configuration for assignments CSV - HyperCube.
    /// </summary>
    public abstract class TemplateBase : NotifyPropertyChangedBase
    {
        protected XmlDocument XmlAssignmentDoc;

        protected XmlNode XmlRoot { get; private set; }
        protected XmlNode XmlColumnRoot { get; private set; }
        protected XmlNode XmlRowRoot { get; private set; }
        protected string XmlAttrName = "csv";
        protected string XmlIdPrefix = "id";
        private eBalanceKitBusiness.HyperCubes.Interfaces.Structure.IHyperCube _cube;

        protected eBalanceKitBusiness.HyperCubes.Interfaces.Structure.IHyperCube Cube {
            get { return _cube; }
            set {
                _cube = value;
                
            if (_cube.Dimensions.AllDimensionItems.Count() == 3) {
                var hyCu3d = _cube.Get3DCube(_cube.Dimensions.AllDimensionItems.ToList()[0],
                                             _cube.Dimensions.AllDimensionItems.ToList()[1],
                                             _cube.Dimensions.AllDimensionItems.ToList()[2]);
                Table = hyCu3d.Tables.First();
            } else {
                Table = value.GetTable(value.Dimensions.Primary, value.Dimensions.DimensionItems.Last());
            }

                InitHyperCubeHeaderDicts();
            }
        }

        protected TemplateBase() {
            InitXml();
            //_hyperCubeColumnHeader = new Dictionary<string, bool?>();
            //_hyperCubeRowHeader = new Dictionary<string, bool?>();
        }


        protected void InitXml()
        {
            XmlAssignmentDoc = new XmlDocument();

            XmlRoot = XmlAssignmentDoc.CreateElement("XML");

            XmlColumnRoot = XmlAssignmentDoc.CreateElement("Columns");

            XmlRowRoot = XmlAssignmentDoc.CreateElement("Rows");
        }

        protected eBalanceKitBusiness.HyperCubes.Interfaces.ViewModels.IHyperCubeTable Table;

        //#region HyperCubeColumnHeader
        //protected Dictionary<string, bool?> _hyperCubeColumnHeader;

        #region ColumnHeaders
        private HashSet<HyperCubeHeader> _columnHeaders;

        public HashSet<HyperCubeHeader> ColumnHeaders {
            get { return _columnHeaders; }
            set {
                if (_columnHeaders != value) {
                    _columnHeaders = value;
                    OnPropertyChanged("ColumnHeaders");
                }
            }
        }
        #endregion ColumnHeaders

        #region RowHeaders
        private HashSet<HyperCubeHeader> _rowHeaders;

        public HashSet<HyperCubeHeader> RowHeaders {
            get { return _rowHeaders; }
            set {
                if (_rowHeaders != value) {
                    _rowHeaders = value;
                    OnPropertyChanged("RowHeaders");
                }
            }
        }
        #endregion RowHeaders

        ///// <summary>
        ///// Contains the column header of the current cube plus assignment information (true = assigned, false = not assigned, null = assigned to the current selected entry)
        ///// </summary>
        //public Dictionary<string, bool?> HyperCubeColumnHeader {
        //    get {
        //        /*
        //        if (HyperCubeColumnHeader.Count == 0) {
        //            InitHyperCubeColumnHeaderDict();
        //        }
        //        */
        //        return _hyperCubeColumnHeader;
        //    }
        //    set {
        //        _hyperCubeColumnHeader = value;
        //    }

        //}
        //#endregion

        //#region HyperCubeRowHeader
        //protected Dictionary<string, bool?> _hyperCubeRowHeader;
        ///// <summary>
        ///// Contains the row header of the current cube plus assignment information (true = assigned, false = not assigned, null = assigned to the current selected entry)
        ///// </summary>
        //public Dictionary<string, bool?> HyperCubeRowHeader {
        //    get {
        //        /*
        //        if (HyperCubeRowHeader.Count == 0)
        //        {
        //            InitHyperCubeRowHeaderDict();
        //        }
        //        */
        //        return _hyperCubeRowHeader;
        //    }
        //    set {
        //        _hyperCubeRowHeader = value;
        //    }
        //}
        //#endregion

        ///// <summary>
        ///// Contains the assignments row (CSV data) - header (HyperCube row header or if inverse the HyperCube column header)
        ///// </summary>
        //public Dictionary<int, string> RowAssignmentDict { get; set; }
        ///// <summary>
        ///// Contains the assignments column (CSV data) - header (HyperCube colum header or if inverse the HyperCube row header)
        ///// </summary>
        //public Dictionary<int, string> ColumnAssignmentDict { get; set; }

        /// <summary>
        /// Initialize HyperCubeRowHeader and HyperCubeColumnHeader dictionaries
        /// </summary>
        protected void InitHyperCubeHeaderDicts() {

            //HyperCubeRowHeader = (from entry in table.AllRows select entry.Header).ToDictionary(header => header, header => false);
            //HyperCubeColumnHeader = (from entry in table.AllColumns select entry.Header).ToDictionary(header => header, header => false);
            if (_cube.Dimensions.AllDimensionItems.Count() == 3) {
                InitHyperCubeColumnHeaderDict3D();
            } else {
                InitHyperCubeColumnHeaderDict();
            }
            InitHyperCubeRowHeaderDict();
        }

        public class HyperCubeHeader : NotifyPropertyChangedBase {
            public HyperCubeHeader(long id, string header, bool? assigned) {
                Id = id;
                Header = header;
                AssignmentFlag = assigned;
            }

            public long Id { get; set; }
            public string Header { get; set; }


            #region AssignmentFlag
            private bool? _assignmentFlag;

            public bool? AssignmentFlag {
                get { return _assignmentFlag; }
                set {
                    if (_assignmentFlag != value) {
                        _assignmentFlag = value;
                        OnPropertyChanged("AssignmentFlag");
                    }
                }
            }
            #endregion AssignmentFlag

            #region CsvPosition
            private int _csvPosition;

            public int CsvPosition {
                get { return _csvPosition; }
                set {
                    if (_csvPosition != value) {
                        _csvPosition = value;
                        OnPropertyChanged("CsvPosition");
                    }
                }
            }
            #endregion CsvPosition

        }

        /// <summary>
        /// Loads the row header for the current cube plus the default value (false) in the HyperCubeRowHeader dictionary
        /// </summary>
        private void InitHyperCubeRowHeaderDict() {
            RowHeaders = new HashSet<HyperCubeHeader>();
            //_hyperCubeRowHeader = new Dictionary<string, bool?>();
            //foreach (var header in (from entry in Table.AllRows select entry.Header)) {
            //    _hyperCubeRowHeader.Add(header, false);
            //}

            foreach (var entry in Table.AllRows) {
                RowHeaders.Add(new HyperCubeHeader(entry.DimensionValue.ElementId, entry.Header, false));
            }


        }
        /// <summary>
        /// Loads the column header for the current cube plus the default value (false) in the HyperCubeColumnHeader dictionary
        /// </summary>
        private void InitHyperCubeColumnHeaderDict3D() {
            //_hyperCubeColumnHeader = new Dictionary<string, bool?>();
            ColumnHeaders = new HashSet<HyperCubeHeader>();
            /*
            foreach (var header in (from entry in Cube.Dimensions.Primary.Values select entry.Label)) {
                _hyperCubeColumnHeader.Add(header, false);
            }
            */
            
            foreach (var entry in Cube.Dimensions.Primary.Values) {
                ColumnHeaders.Add(new HyperCubeHeader(entry.ElementId, entry.Label, false));
            }
            
        }

        /// <summary>
        /// Loads the column header for the current cube plus the default value (false) in the HyperCubeColumnHeader dictionary
        /// </summary>
        private void InitHyperCubeColumnHeaderDict() {
            //_hyperCubeColumnHeader = new Dictionary<string, bool?>();
            ColumnHeaders = new HashSet<HyperCubeHeader>();
            //foreach (var header in (from entry in Table.AllColumns select entry.Header)) {
            //    _hyperCubeColumnHeader.Add(header, false);
            //}
            foreach (var entry in Table.AllColumns) {
                ColumnHeaders.Add(new HyperCubeHeader(entry.DimensionValue.ElementId, entry.Header, false));
            }
        }

        public void LoadDicts(Dictionary<int, long> columnDict, Dictionary<int, long> rowDict, bool isInverse = false) {
            InitHyperCubeHeaderDicts();

            InitHyperCubeHeaderDicts();
            foreach (KeyValuePair<int, long> keyValuePair in rowDict) {
                if (isInverse) {
                    var entry = ColumnHeaders.First(x => x.Id == keyValuePair.Value);
                    entry.CsvPosition = keyValuePair.Key;
                    entry.AssignmentFlag = true;
                }
                else {
                    var entry = RowHeaders.First(x => x.Id == keyValuePair.Value);
                    entry.CsvPosition = keyValuePair.Key;
                    entry.AssignmentFlag = true;
                }
            }

            foreach (KeyValuePair<int, long> keyValuePair in columnDict) {
                if (isInverse) {
                    var entry = RowHeaders.First(x => x.Id == keyValuePair.Value);
                    entry.CsvPosition = keyValuePair.Key;
                    entry.AssignmentFlag = true;
                }
                else {
                    var entry = ColumnHeaders.First(x => x.Id == keyValuePair.Value);
                    entry.CsvPosition = keyValuePair.Key;
                    entry.AssignmentFlag = true;
                }

            }
            
        }

    }
}
