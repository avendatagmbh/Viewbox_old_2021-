using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TransDATABusiness.Structures;
using System.ComponentModel;
using TransDATABusiness.ConfigDb.Tables;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using TransDATABusiness.EventArgs;
using TransDATABusiness.Manager;

namespace TransDATA.Models {

    internal class MainWindowModel : INotifyPropertyChanged {

        /// <summary>
        /// Gets the app config.
        /// </summary>
        /// <value>The app config.</value>
        public AppConfig AppConfig {
            get { return TransDATABusiness.Global.AppConfig; }
        }

        public MainWindowModel(Window window) {
            ColumnHeaderChecked = null;
            TableHeaderChecked = null;
            EmptyTableHeaderChecked = null;
            this._owner = window;
            
            //ProfileManager.ProfileNames[0]...
            this.CurrentProfile = new Profile("Default");
            this.CurrentProfile.TableAdded += new EventHandler<TransDATABusiness.EventArgs.TableAddedEventArgs>(CurrentProfile_TableAdded);
            
            this.AllTables = new ObservableCollection<Table>();
            this.DeselectedTables = new ObservableCollection<Table>();
            this.SelectedTables = new ObservableCollection<Table>();
            this.EmptyTables = new ObservableCollection<Table>();
            this.SourceTables = new ObservableCollection<Table>();
            this.Table = new Table();

            this.SelectedColumns = new ObservableCollection<Column>();
            this.DeselectedColumns = new ObservableCollection<Column>();
            this.SourceColumns = new ObservableCollection<Column>();
        }

        /*****************************************************************************************************/

        #region events

        /// <summary>
        /// Occurs when a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion events

        /*****************************************************************************************************/

        #region eventTrigger

        /// <summary>
        /// Called when a property changed.
        /// </summary>
        /// <param name="property">The property.</param>
        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion eventTrigger

        /*****************************************************************************************************/

        #region fields

        /// <summary>
        /// See property CurrentProfile.
        /// </summary>
        private Profile _currentProfile;

        private Window _owner;

        /// <summary>
        /// See property SourceTables.
        /// </summary>
        private ObservableCollection<Table> _sourceTables;
        //private ObservableCollection<Table> _searchTables;

        /// <summary>
        /// See property SourceColumns.
        /// </summary>
        private ObservableCollection<Column> _sourceColumns;

        /// <summary>
        /// See property Table.
        /// </summary>
        private Table _table;

        private bool _isTabChecked;
        private int _counter;

        #endregion fields

        /******************************************************************************************************/
      
        #region properties

        /// <summary>
        /// Gets or sets the tables.
        /// </summary>
        /// <value>The tables.</value>
        public ObservableCollection<Table> AllTables { get; private set; }

        /// <summary>
        /// Gets or sets the deselected tables.
        /// </summary>
        /// <value>The deselected tables.</value>
        public ObservableCollection<Table> DeselectedTables { get; private set; }


        /// <summary>
        /// Gets or sets the selected tables.
        /// </summary>
        /// <value>The selected tables.</value>
        public ObservableCollection<Table> SelectedTables { get; private set; }

        /// <summary>
        /// Gets or sets the empty tables.
        /// </summary>
        /// <value>The empty tables.</value>
        public ObservableCollection<Table> EmptyTables { get; private set; }

        /// <summary>
        /// Gets or sets the item source.
        /// </summary>
        /// <value>The item source.</value>
        public ObservableCollection<Table> SourceTables {
            get { return _sourceTables; }
            set {
                if (_sourceTables != value) {
                    _sourceTables = value;
                    OnPropertyChanged("SourceTables");
                }
            }
        }

        /// <summary>
        /// Gets or sets the table.
        /// </summary>
        /// <value>The table.</value>
        public Table Table {
            get { return _table; }
            set {
                if (_table != value) {
                    _table = value;
                    OnPropertyChanged("Table");
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected columns.
        /// </summary>
        /// <value>The selected columns.</value>
        public ObservableCollection<Column> SelectedColumns { get; private set; }

        /// <summary>
        /// Gets or sets the deselected columns.
        /// </summary>
        /// <value>The deselected columns.</value>
        public ObservableCollection<Column> DeselectedColumns { get; private set; }

        /// <summary>
        /// Gets or sets the source columns.
        /// </summary>
        /// <value>The source columns.</value>
        public ObservableCollection<Column> SourceColumns {
            get { return _sourceColumns; }
            set {
                if (_sourceColumns != value) {
                    _sourceColumns = value;
                    OnPropertyChanged("SourceColumns");
                }
            }
        }


        /// <summary>
        /// Gets or sets the current profile.
        /// </summary>
        /// <value>The current profile.</value>
        public Profile CurrentProfile {
            get { return _currentProfile; }
            set {
                if (_currentProfile != value) {
                    _currentProfile = value;
                    OnPropertyChanged("CurrentProfile");
                }
            }
        }

        /// <summary>
        /// Gets or sets the table header checked.
        /// </summary>
        /// <value>The table header checked.</value>
        bool? _TableHeaderChecked;
        public bool? TableHeaderChecked {
            get { return _TableHeaderChecked; }
            set { _TableHeaderChecked = value; }
        }

        /// <summary>
        /// Gets or sets the column header checked.
        /// </summary>
        /// <value>The column header checked.</value>
        public bool? ColumnHeaderChecked { get; set; }

        /// <summary>
        /// Gets or sets the empty table header checked.
        /// </summary>
        /// <value>The empty table header checked.</value>
        public bool? EmptyTableHeaderChecked { get; set; }



        public bool IsTabChecked {
            get { return _isTabChecked; }
            set {
                if (_isTabChecked != value) {
                    _isTabChecked = value;
                    OnPropertyChanged("IsTabChecked");
                }
            }
        }
        #endregion properties

        /*******************************************************************************************************/

        #region methods

        public void CreateProfile() {
            this.AllTables.Clear();
            this.EmptyTables.Clear();
            this.DeselectedTables.Clear();
            this.CurrentProfile.Tables.Clear();

            Thread thread = new Thread(new ParameterizedThreadStart(AppConfig.GetStructure));
            thread.Start(this.CurrentProfile);
        }

        void CurrentProfile_TableAdded(object sender, TransDATABusiness.EventArgs.TableAddedEventArgs e) {
            if (!_owner.Dispatcher.CheckAccess()) {
                _owner.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new EventHandler<TableAddedEventArgs>(CurrentProfile_TableAdded), sender, new object[] { e });
                return;
            }

            if (e.Table.TableInfo.Count != 0) {
                this.AllTables.Add(e.Table);
            } else { 
                this.EmptyTables.Add(e.Table); 
            }

        }

        public void GetListTables(string rdbTablesName) {
            switch (rdbTablesName) {
                case "rdbAllIsChecked":
                    this.SourceTables = this.AllTables;
                    _counter = 1;
                    break;
                case "rdbSelectedIsChecked":
                    this.SourceTables = this.SelectedTables;
                    _counter = 2;
                    break;
                case "rdbDeselectedIsChecked":
                    this.SourceTables = this.DeselectedTables;
                    _counter = 3;
                    break;
                default:
                    throw new Exception("DatenQuelle nicht definiert");
             }
        }

        public void GetListColumns(string rdbColumnsSelect) {
                    switch (rdbColumnsSelect){
                        case "rdbAllColumnsIsChecked":
                            if (this.Table == null) {
                                this.SourceColumns = null;
                                return;
                            }
                            this.SourceColumns = this.Table.Columns;
                            break;
                        case "rdbSelectedColumnsIsChecked":
                            this.SourceColumns = this.SelectedColumns;
                            break;
                        case "rdbDeselectedColumnsIsChecked":
                            this.SourceColumns = this.DeselectedColumns;
                            break;

                default:
                    throw new Exception("DatenQuelle nicht definiert");
            }
        }

        public void GetIsTabChecked(string tbi) {
            if (tbi == "tab") {
                if (this.IsTabChecked == true) {
                    return;
                } else {
                    this.Table = null;
                    this.SourceColumns = null;
                    this.IsTabChecked = true;
                }
            } else {
                if (this.IsTabChecked == true) {
                    this.Table = null;
                    this.SourceColumns = null;
                    this.IsTabChecked = false;
                } else {
                    return;
                }
            }
        }

        public void GetSearch(char[] ch,  int ch_length) {

            if (ch_length == 0) {
                switch (_counter) {
                    case 1:
                        this.SourceTables = this.AllTables;
                        break;
                    case 2:
                        this.SourceTables = this.SelectedTables;
                        break;
                    case 3:
                        this.SourceTables = this.DeselectedTables;
                        break;
                }
                return;
            }

            if (ch_length != 0 && ch_length == ch.Length) {
                switch (_counter) {
                    case 1:
                        this.SourceTables = this.AllTables;
                        break;
                    case 2:
                        this.SourceTables = this.SelectedTables;
                        break;
                    case 3:
                        this.SourceTables = this.DeselectedTables;
                        break;
                }
            }

            ObservableCollection<Table> _searchTables = new ObservableCollection<Table>();

            for (int i = 0; i < this.SourceTables.Count; i++) {
                _searchTables.Add(SourceTables.ElementAt(i));
            }

            int j=0;
                while(j<_searchTables.Count) {
                    if (_searchTables.ElementAt(j).TableInfo.Name[ch_length-ch.Length] != ch[0]) {
                        _searchTables.RemoveAt(j);
                    } else { j++; 
                    }
                }

                if ((ch.Length - 1) == 0) {
                    this.SourceTables = _searchTables; return; 
                }

                char[] chars = new char[ch.Length - 1];
                for (int i = 0; i < ch.Length-1; i++) {
                    chars[i] = ch[i + 1];
                }
                this.SourceTables = _searchTables;
                this.GetSearch(chars, ch_length);
        }
        #endregion methods

        /*******************************************************************************************************/
    }
}
