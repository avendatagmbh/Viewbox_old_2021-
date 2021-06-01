using System;
using System.Collections.Generic;
using System.Linq;
using SystemDb;
using SystemDb.Internal;
using Utils;
using System.Windows.Input;
using ViewAssistant.Controls;

namespace ViewAssistantBusiness.Models
{
    public class TableModel : NotifyPropertyChangedBase, IViewboxLocalisable, IRenameable
    {
        public MainModel MainModel { get; set; }

        public TableModel(MainModel model)
        {
            MainModel = model;
        }

        #region General Properties

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }


        #endregion General Properties

        #region SourceInfos

        public bool IsInSource { get; set; }

        private bool _sourceChecked;
        public bool SourceChecked
        {
            get { return _sourceChecked; }
            set
            {
                if (value != _sourceChecked)
                {
                    _sourceChecked = value;
                    OnPropertyChanged("SourceChecked");
                    MainModel.SetAllSelects();
                }
            }
        }

        private Int64 _sourceRowCount;
        public Int64 SourceRowCount
        {
            get { return _sourceRowCount; }
            set
            {
                if (value != _sourceRowCount)
                {
                    _sourceRowCount = value;
                    OnPropertyChanged("SourceRowCount");
                }
            }
        }


        ProgressCalculator _sourceProgress;
        public ProgressCalculator SourceProgress
        {
            get { return _sourceProgress; }
            set
            {
                if (value != _sourceProgress)
                {
                    _sourceProgress = value;
                    OnPropertyChanged("SourceProgress");
                }
            }
        }


        private string _sourceError;
        public string SourceError
        {
            get { return _sourceError; }
            set
            {
                if (value != _sourceError)
                {
                    _sourceError = value;
                    OnPropertyChanged("SourceError");
                    OnPropertyChanged("HasSourceError");
                }
            }
        }

        public bool HasSourceError { get { return !String.IsNullOrEmpty(SourceError); } }


        private string _sourceWarning;
        public string SourceWarning
        {
            get { return _sourceWarning; }
            set
            {
                if (value != _sourceWarning)
                {
                    _sourceWarning = value;
                    OnPropertyChanged("SourceWarning");
                    OnPropertyChanged("HasSourceWarning");
                }
            }
        }

        public bool HasSourceWarning { get { return !String.IsNullOrEmpty(SourceWarning); } }


        public void ClearSourceInfos()
        {
            SourceError = "";
            SourceWarning = "";
            IsInSource = false;
            SourceRowCount = 0;
            ClearSourceColumns();
        }

        #endregion SourceInfos

        #region ViewboxInfos

        public bool IsInViewbox { get; set; }

        private bool _viewboxChecked;
        public bool ViewboxChecked
        {
            get { return _viewboxChecked; }
            set
            {
                if (value != _viewboxChecked)
                {
                    _viewboxChecked = value;
                    OnPropertyChanged("ViewboxChecked");
                    MainModel.SetAllSelects();
                }
            }
        }


        private Table _viewboxTable;
        public Table ViewboxTable
        {
            get { return _viewboxTable; }
            set
            {
                if (value != _viewboxTable)
                {
                    _viewboxTable = value;
                    OnPropertyChanged("ViewboxTable");
                }
            }
        }

        private Int64 _viewboxRowCount;
        public Int64 ViewboxRowCount
        {
            get { return _viewboxRowCount; }
            set
            {
                if (value != _viewboxRowCount)
                {
                    _viewboxRowCount = value;
                    OnPropertyChanged("ViewboxRowCount");
                }
            }
        }


        ProgressCalculator _viewboxProgress;
        public ProgressCalculator ViewboxProgress
        {
            get { return _viewboxProgress; }
            set
            {
                if (value != _viewboxProgress)
                {
                    _viewboxProgress = value;
                    OnPropertyChanged("ViewboxProgress");
                }
            }
        }



        private string _viewboxError;
        public string ViewboxError
        {
            get { return _viewboxError; }
            set
            {
                if (value != _viewboxError)
                {
                    _viewboxError = value;
                    OnPropertyChanged("ViewboxError");
                    OnPropertyChanged("HasViewboxError");
                }
            }
        }

        public bool HasViewboxError { get { return !String.IsNullOrEmpty(ViewboxError); } }



        private string _viewboxWarning;
        public string ViewboxWarning
        {
            get { return _viewboxWarning; }
            set
            {
                if (value != _viewboxWarning)
                {
                    _viewboxWarning = value;
                    OnPropertyChanged("ViewboxWarning");
                    OnPropertyChanged("HasViewboxWarning");
                }
            }
        }

        public bool HasViewboxWarning { get { return !String.IsNullOrEmpty(ViewboxWarning); } }


        public void ClearViewboxInfos()
        {
            ViewboxError = "";
            ViewboxWarning = "";
            IsInViewbox = false;
            ViewboxRowCount = 0;
            ViewboxProgress = null;
            ViewboxTable = null;
            ClearViewboxColumns();
        }

        #endregion ViewboxInfos

        #region FinalInfos

        public bool IsInFinal { get; set; }

        private Int64 _finalRowCount;
        public Int64 FinalRowCount
        {
            get { return _finalRowCount; }
            set
            {
                if (value != _finalRowCount)
                {
                    _finalRowCount = value;
                    OnPropertyChanged("FinalRowCount");
                }
            }
        }

        private string _finalError;
        public string FinalError
        {
            get { return _finalError; }
            set
            {
                if (value != _finalError)
                {
                    _finalError = value;
                    OnPropertyChanged("FinalError");
                    OnPropertyChanged("HasFinalError");
                }
            }
        }

        public bool HasFinalError { get { return !String.IsNullOrEmpty(FinalError); } }

        public void ClearFinalInfos()
        {
            FinalError = "";
            IsInFinal = false;
            FinalRowCount = 0;
            ClearFinalColumns();
        }

        #endregion FinalInfos

        #region Columns

        public bool HasViewboxSpecialColumn
        {
            get { return _viewboxColumns.Values.Any(w => w.ViewboxInfo.Flag == 1); }
        }


        private readonly Dictionary<string, ColumnModel> _columns = new Dictionary<string, ColumnModel>();
        private Dictionary<string, ColumnModel> _sourceColumns = new Dictionary<string, ColumnModel>();
        private readonly Dictionary<string, ColumnModel> _viewboxColumns = new Dictionary<string, ColumnModel>();
        private readonly Dictionary<string, ColumnModel> _finalColumns = new Dictionary<string, ColumnModel>();

        public Dictionary<string, ColumnModel> SourceColumnDict { get { return _sourceColumns; } }
        public List<ColumnModel> SourceColumns { get { return _sourceColumns.Values.OrderByDescending(w => w.SourceOptimizationType).ToList(); } }
        public List<ColumnModel> ViewboxColumns { get { return _viewboxColumns.Values.Where(w => w.ViewboxInfo != null && w.ViewboxInfo.Flag != 1).OrderByDescending(w => w.ViewboxOptimizationType).ThenBy(w=>w.ViewboxOrdinal).ToList(); } }
        public List<ColumnModel> FinalColumns { get { return _finalColumns.Values.ToList(); } }
        public List<ColumnModel> ViewboxSpecialColumns { get { return _viewboxColumns.Values.Where(w => w.ViewboxInfo != null && w.ViewboxInfo.Flag == 1).OrderByDescending(w => w.ViewboxOptimizationType).ToList(); } }

        public void CopySourceColumns(TableModel tableModel)
        {
            _sourceColumns = tableModel._sourceColumns;
        }

        public ColumnModel GetColumn(string name)
        {
            lock (padlock)
            {
                var nameLower = name.ToLower();
                if (_columns.ContainsKey(nameLower))
                    return _columns[nameLower];
                var col = new ColumnModel(MainModel, this) { Name = nameLower };
                _columns.Add(nameLower, col);
                return col;
            }
        }

        public void RemoveColumn(ColumnModel column)
        {
            lock(padlock)
            {
                if (_columns.ContainsKey(column.Name))
                    _columns.Remove(column.Name);
                if (_viewboxColumns.ContainsKey(column.Name))
                    _viewboxColumns.Remove(column.Name);
                OnPropertyChanged("HasViewboxSpecialColumn");
                OnPropertyChanged("ViewboxSpecialColumns");
            }
            
        }

        public ColumnModel AddSpecial()
        {

            lock (padlock)
            {
                int i = 1;
                while (_columns.Any(w => w.Value.Name == "special_column_" + i))
                {
                    i++;
                }
                var name = "special_column_" + i;
                var column = GetColumn(name);
                
                AddViewboxColumn(column);

                using (var conn = MainModel.CurrentProfile.ViewboxConnection.CreateConnection())
                {
                    column.ViewboxInfo = new Column();
                    column.ViewboxInfo.Name = name;
                    column.ViewboxInfo.Ordinal = (_viewboxColumns.Any()
                                                      ? _viewboxColumns.Values.Max(w => w.ViewboxInfo.Ordinal)
                                                      : 0) + 1;
                    column.ViewboxInfo.Table = this.ViewboxTable;
                    column.ViewboxInfo.IsVisible = false;
                    column.ViewboxInfo.IsEmpty = false;
                    column.ViewboxInfo.UserDefined = false;
                    column.ViewboxInfo.OptimizationType = OptimizationType.NotSet;
                    column.ViewboxInfo.DataType = SqlType.String;
                    column.ViewboxInfo.MaxLength = 255;
                    column.ViewboxInfo.FromColumnFormat = "";
                    column.ViewboxInfo.Flag = 1;
                    conn.DbMapping.Save(column.ViewboxInfo);
                    this.ViewboxTable.Columns.Add(column.ViewboxInfo);
                }

                OnPropertyChanged("HasViewboxSpecialColumn");
                OnPropertyChanged("ViewboxSpecialColumns");
                return column;
            }
        }


        public void RemoveSpecial(ColumnModel column)
        {
            lock (padlock)
            {
                using (var conn = MainModel.CurrentProfile.ViewboxConnection.CreateConnection())
                {
                    conn.DbMapping.Delete(column.ViewboxInfo);

                    if (_viewboxColumns.ContainsKey(column.Name))
                        _viewboxColumns.Remove(column.Name);
                    if (_finalColumns.ContainsKey(column.Name))
                        _finalColumns.Remove(column.Name);
                    if (_sourceColumns.ContainsKey(column.Name))
                        _sourceColumns.Remove(column.Name);
                    if (_columns.ContainsKey(column.Name))
                        _columns.Remove(column.Name);
                }
                OnPropertyChanged("HasViewboxSpecialColumn");
                OnPropertyChanged("ViewboxSpecialColumns");
            }
        }

        private readonly object padlock = new object();

        public void AddSourceColumn(ColumnModel column)
        {
            lock (padlock)
            {
                if (!_sourceColumns.ContainsKey(column.Name))
                    _sourceColumns.Add(column.Name, column);
                column.IsInSource = true;
            }
        }

        public void AddViewboxColumn(ColumnModel column)
        {
            lock (padlock)
            {
                if (!_viewboxColumns.ContainsKey(column.Name))
                    _viewboxColumns.Add(column.Name, column);
                column.IsInViewbox = true;
            }
        }


        public void AddFinalColumn(ColumnModel column)
        {
            lock (padlock)
            {
                if (!_finalColumns.ContainsKey(column.Name))
                    _finalColumns.Add(column.Name, column);
                column.IsInFinal = true;
            }
        }

        public void ClearSourceColumns()
        {
            lock (padlock)
            {
                foreach (var column in _sourceColumns.Values)
                {
                    column.ClearSourceInfos();
                    if (!_finalColumns.ContainsKey(column.Name) && !_viewboxColumns.ContainsKey(column.Name))
                    {
                        _columns.Remove(column.Name);
                    }
                }
                _sourceColumns.Clear();
            }
        }


        public void ClearViewboxColumns()
        {
            lock (padlock)
            {
                foreach (var column in _viewboxColumns.Values)
                {
                    column.ClearViewboxInfos();
                    if (!_sourceColumns.ContainsKey(column.Name) && !_finalColumns.ContainsKey(column.Name))
                    {
                        _columns.Remove(column.Name);
                    }
                }
                _viewboxColumns.Clear();
            }
        }

        public void ClearFinalColumns()
        {
            lock (padlock)
            {
                foreach (var column in _finalColumns.Values)
                {
                    column.ClearFinalInfos();
                    if (!_sourceColumns.ContainsKey(column.Name) && !_viewboxColumns.ContainsKey(column.Name))
                    {
                        _columns.Remove(column.Name);
                    }
                }
                _finalColumns.Clear();
            }
        }

        public bool SourceViewboxDiff()
        {
            if(SourceRowCount != ViewboxRowCount || SourceColumns.Count != ViewboxColumns.Count || SourceColumns.Any(column => !column.IsInViewbox))
            {
                return true;
            }
            return SourceColumns.Any(column => SqlTypeHelper.ColumnTypeToSqlType(column.SourceInfo.Type) != column.ViewboxInfo.DataType);
        }

        public bool ViewboxFinalDiff()
        {
            if (FinalRowCount != ViewboxRowCount || FinalColumns.Count - 1 != ViewboxColumns.Count + ViewboxSpecialColumns.Count ||
                ViewboxColumns.Any(column => !column.IsInFinal) || ViewboxSpecialColumns.Any(column => !column.IsInFinal))
            {
                return true;
            }
            return FinalColumns.Where(w=>w.Name != "_row_no_").Any(column => SqlTypeHelper.ColumnTypeToSqlType(column.FinalInfo.Type) != column.ViewboxInfo.DataType);
        }


        #endregion Columns

        public ColumnModel MandtCol { get
        {
            return ViewboxColumns.FirstOrDefault(w => w.ViewboxInfo.OptimizationType == OptimizationType.IndexTable) ??
                 ViewboxSpecialColumns.FirstOrDefault(w => w.ViewboxInfo.OptimizationType == OptimizationType.IndexTable);
        } }
        public ColumnModel BukrsCol { get
        {
            return ViewboxColumns.FirstOrDefault(w => w.ViewboxInfo.OptimizationType == OptimizationType.SplitTable) ??
                ViewboxSpecialColumns.FirstOrDefault(w => w.ViewboxInfo.OptimizationType == OptimizationType.SplitTable);
        } }
        public ColumnModel GjahrCol { get
        {
            return ViewboxColumns.FirstOrDefault(w => w.ViewboxInfo.OptimizationType == OptimizationType.SortColumn) ??
                ViewboxSpecialColumns.FirstOrDefault(w => w.ViewboxInfo.OptimizationType == OptimizationType.SortColumn);
        } }

        public List<string> GetOptimizationColumns()
        {
            var ret = new List<string>();
            if (MandtCol != null)
                ret.Add(MandtCol.Name);
            if (BukrsCol != null)
                ret.Add(BukrsCol.Name);
            if (GjahrCol != null)
                ret.Add(GjahrCol.Name);
            return ret;
        }

        #region ConfigureTableTexts

        public ICommand ConfigureTableTextsCommand { get { return new RelayCommand(OnConfigureTableTextsClickedExecute); } }

        private void OnConfigureTableTextsClickedExecute(object sender)
        {
            OnConfigureLocalisationTextsClicked(sender, this);
        }

        public event ConfigureLocalisationTextsClicked ConfigureLocalisationTextsClicked;

        public void OnConfigureLocalisationTextsClicked(object sender, IViewboxLocalisable model)
        {
            if (ConfigureLocalisationTextsClicked != null)
            {
                ConfigureLocalisationTextsClicked(sender, model);
            }
        }

        public IDataObject Info
        {
            get { return ViewboxTable; }
        }

        #endregion

        #region Renamer

        public ICommand RenamerCommand { get { return new RelayCommand(OnRenamerCommandExecute); } }

        private void OnRenamerCommandExecute(object sender)
        {
            OnRenamerClicked(sender, this);
        }

        public event RenamerClicked RenamerClicked;

        public void OnRenamerClicked(object sender, IRenameable model)
        {
            if (RenamerClicked != null)
            {
                RenamerClicked(sender, model);
            }
        }

        #endregion
    }
}
