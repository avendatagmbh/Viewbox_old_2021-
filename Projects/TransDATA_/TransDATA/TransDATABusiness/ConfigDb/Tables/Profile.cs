/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * hel                  2010-11-19      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using TransDATABusiness.ConfigDb.Tables.Base;
using System.Collections.ObjectModel;
using TransDATABusiness.Enums;
using TransDATABusiness.EventArgs;
using DbAccess;
using DbAccess.Attributes;
using DbAccess.Structures;

namespace TransDATABusiness.ConfigDb.Tables {

    /// <summary>
    /// This class represents a profile.
    /// </summary>
    [DbTable(TABLENAME)]
    public class Profile : TableBase, INotifyPropertyChanged {

        internal const string TABLENAME = "profile";
        internal const string NAME = "name";
        internal const string DESCRIPTION = "description";
        internal const string LOCATION = "location";
        internal const string EXPORTFOLDER = "exportFolder";
        /// <summary>
        /// Initializes a new instance of the <see cref="Profile"/> class.
        /// </summary>
        public Profile(string name) {
            Name = name;
            Description = "";
            Location = "";
            ExportFolder = "";
            this.Tables = new ObservableCollection<Table>(); 
            this.Tables.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Tables_CollectionChanged);
            this.ConfigDatabase = new DbConfig(ConnectionManager.GetDbName(typeof(DbAccess.DbSpecific.MySQL.Database))) {
                Hostname = string.Empty,
                Username = string.Empty,
                Password = string.Empty,
                Port = 3306,
                DSN = string.Empty
            }; 

        }

        void Tables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (Table table in e.NewItems) {
                    OnTableAdded(table);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Profile"/> class (used form load method).
        /// </summary>
        public Profile() {
        }

        /**************************************************************************************/

        #region events

        /// <summary>
        /// Occurs when [table added].
        /// </summary>
        public event EventHandler<TableAddedEventArgs> TableAdded;

        #endregion events

        /**************************************************************************************/

        #region evenTrigger

        /// <summary>
        /// Called when [table added].
        /// </summary>
        private void OnTableAdded(Table table) {
            if (TableAdded != null) {
                TableAdded(this, new TableAddedEventArgs { Table = table });
            }
        }

        #endregion evenTrigger

        /**************************************************************************************/

        #region fields

        /// <summary>
        /// See property Tables.
        /// </summary>       
        private ObservableCollection<Table> _tables;
       
        /// <summary>
        /// See property Name.
        /// </summary>
        private string _name;

        /// <summary>
        /// See property Description.
        /// </summary>
        private string _description;

        /// <summary>
        /// See property Location.
        /// </summary>
        private string _location;

        /// <summary>
        /// See property ExportFolder.
        /// </summary>
        private string _exportFolder;

        /// <summary>
        /// See property ConfigDatabase.
        /// </summary>
        private DbAccess.Structures.DbConfig _configDatabase;

        #endregion fields

        /**************************************************************************************/
    
        #region properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
      [DbColumn(NAME, AllowDbNull = false)]
        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// Gets or sets the table list.
        /// </summary>
        /// <value>The table list.</value>
        
        public ObservableCollection<Table> Tables {
            get { return _tables; }
            set {
                if (_tables != value) {
                    _tables = value;
                    OnPropertyChanged("Tables");
                }
            }
        }


        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        [DbColumn(DESCRIPTION, AllowDbNull = false)]
        public string Description {
            get { return _description; }
            set {
                if (_description != value) {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        [DbColumn(LOCATION, AllowDbNull = false)]
        public string Location {
            get {return _location;}
            set {
                if (_location != value) {
                    _location = value;
                    OnPropertyChanged("Location");
                }
            }
        }

        /// <summary>
        /// Gets or sets the export folder.
        /// </summary>
        /// <value>The export folder.</value>
        [DbColumn(EXPORTFOLDER, AllowDbNull = false)]
        public string ExportFolder {
            get { return _exportFolder; }
            set {
                if (_exportFolder != value) {
                    _exportFolder = value;
                    OnPropertyChanged("ExportFolder");
                }
            }
        }


        /// <summary>
        /// Gets or sets the config database.
        /// </summary>
        /// <value>The config database.</value>
        public DbConfig ConfigDatabase {
            get { return _configDatabase; }
            set {
                if (_configDatabase != value) {
                    _configDatabase = value;
                    OnPropertyChanged("ConfigDatabase");
                }
            }
        }

        #endregion properties

        /**************************************************************************************/
        #region methods

        public static Profile Load(IDatabase conn, string name) {
            List<Profile> profiles = conn.DbMapping.Load<Profile>(conn.Enquote(NAME) + "=" + name);
            if (profiles.Count == 0) return null;

            Profile profile = profiles[0];
            List<Table> tables = conn.DbMapping.Load<Table>(conn.Enquote("profileId") + "=" + profile.Id);
            foreach (Table tab in tables) {
                List<Column> column = conn.DbMapping.Load<Column>(conn.Enquote("tabeId") + "=" + tab.Id);
                foreach (Column col in column) {
                    tab.Columns.Add(col);
                }
                profile.Tables.Add(tab);
            }

            return profile;
        }




        public void Save() {
            try {
                using (IDatabase conn = Global.AppConfig.ConfigDb.ConnectionManager.GetConnection()) {
                    SaveOrUpdate(conn);
                }
            } catch (Exception ex) {
                throw ex;
            }
        }
    #endregion methods
    }      
}
