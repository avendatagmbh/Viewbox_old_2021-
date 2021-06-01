using System.Collections.Generic;
using System.Linq;
using DbSearchLogic.SearchCore.KeySearch;
using Utils;

namespace DbSearch.Models {
    /// <summary>
    /// Represents a founded key selection list
    /// </summary>
    public class TableListModel : NotifyPropertyChangedBase{
        #region Public Properties
        /// <summary>
        /// List of tables
        /// </summary>
        public List<Table> Tables {  
            get { 
                return _tables; 
            } 
            set {
                _tables = value;
                OnPropertyChanged("Tables");
            } 
        }

        /// <summary>
        /// Represents a selected tables
        /// </summary>
        public List<Table> SelectedTables { 
            get { 
                return new List<Table>(Tables.Where(t => t.IsChecked)); 
            }
        }


        #endregion Public Properties        
 
        #region [ Private Fields ]
        
        /// <summary>
        /// List of tables
        /// </summary>
        private List<Table> _tables;
        
        #endregion [ Private Fields ]

        #region [ Constructors ]
            
        /// <summary>
        /// Constructor
        /// </summary>
        public TableListModel() {
            Init();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tables">The list of the tables</param>
        public TableListModel(List<Table> tables) {
            _tables = new List<Table>(tables);
        }

        #endregion [ Constructors ]
        
        #region [ Private Methods ]

        /// <summary>
        /// Initialize the object
        /// </summary>
        private void Init() {
            Tables = new List<Table>();
        }

        #endregion [ Private Methods ]
    }

    public class Table {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsChecked { get; set; }

        public Table(int tableId, string tableName) {
            Id = tableId;
            Name = tableName;
        }
    }
}
