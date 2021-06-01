using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess.Structures;
using Utils;
using SystemDb;
using SystemDb.Internal;
using System.Windows.Input;
using ViewAssistant.Controls;

namespace ViewAssistantBusiness.Models
{
    public class ColumnModel : NotifyPropertyChangedBase, IViewboxLocalisable, IRenameable
    {
        public MainModel MainModel { get; set; }
        public TableModel TableModel { get; set; }
        
        public ColumnModel(MainModel model, TableModel tableModel)
        {
            MainModel = model;
            TableModel = tableModel;
        }

        public static Dictionary<string, OptimizationType> OptimizationTypes = new Dictionary<string, OptimizationType>
                                                                      {
                                                                          {"BUKRS", OptimizationType.SplitTable},
                                                                          {"MANDT", OptimizationType.IndexTable},
                                                                          {"GJAHR", OptimizationType.SortColumn},
                                                                          {"NOTSET", OptimizationType.NotSet},
                                                                      };
        public static Dictionary<OptimizationType, string> OptimizationTypesRev = new Dictionary<OptimizationType, string>
                                                                      {
                                                                          {OptimizationType.SplitTable, "BUKRS"},
                                                                          {OptimizationType.IndexTable, "MANDT"},
                                                                          {OptimizationType.SortColumn, "GJAHR"},
                                                                      };

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

        private DbColumnInfo _sourceInfo;
        public DbColumnInfo SourceInfo
        {
            get { return _sourceInfo; }
            set
            {
                if (value != _sourceInfo)
                {
                    _sourceInfo = value;
                    OnPropertyChanged("SourceInfo");
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
            SourceInfo = null;
            SourceOptimizationType = OptimizationType.NotSet;
        }

        private OptimizationType _sourceOptimizationType;
        public OptimizationType SourceOptimizationType
        {
            get { return _sourceOptimizationType; }
            set
            {
                if (value != _sourceOptimizationType)
                {
                    _sourceOptimizationType = value;
                    OnPropertyChanged("SourceOptimizationType");
                    OnPropertyChanged("SourceOptimizationTypeString");
                }
            }
        }

        public string SourceOptimizationTypeString
        {
            get { return OptimizationTypesRev.ContainsKey(SourceOptimizationType) ? OptimizationTypesRev[SourceOptimizationType] : ""; }
        }

        #endregion SourceInfos

        #region ViewboxInfos

        public bool IsInViewbox { get; set; }

        private Column _viewboxInfo;
        public Column ViewboxInfo
        {
            get { return _viewboxInfo; }
            set
            {
                if (value != _viewboxInfo)
                {
                    _viewboxInfo = value;
                    OnPropertyChanged("ViewboxInfo");
                }
            }
        }

        public int ViewboxOrdinal
        {
            get { return ViewboxInfo == null ? 0 : ViewboxInfo.Ordinal; }
        }
        public OptimizationType ViewboxOptimizationType
        {
            get { return ViewboxInfo == null ? OptimizationType.NotSet : ViewboxInfo.OptimizationType; }
            set
            {
                if (ViewboxInfo == null)
                    return;
                if (value != ViewboxInfo.OptimizationType)
                {
                    using (var conn = MainModel.CurrentProfile.ViewboxConnection.CreateConnection())
                    {
                        foreach (var col in TableModel.ViewboxColumns.Where(w=>w.ViewboxOptimizationType == value))
                        {
                            col.ViewboxOptimizationType = OptimizationType.NotSet;
                        }
                        foreach (var col in TableModel.ViewboxSpecialColumns.Where(w => w.ViewboxOptimizationType == value))
                        {
                            col.ViewboxOptimizationType = OptimizationType.NotSet;
                        }
                        ViewboxInfo.OptimizationType = value;
                        conn.DbMapping.Save(ViewboxInfo);
                    }
                    OnPropertyChanged("ViewboxOptimizationType");
                }
            }
        }


        public string FromColumn
        {
            get { return ViewboxInfo == null ? "" : ViewboxInfo.FromColumn; }
            set
            {
                if (ViewboxInfo == null)
                    return;
                if (ViewboxInfo.FromColumn == value) return;

                using (var conn = MainModel.CurrentProfile.ViewboxConnection.CreateConnection())
                {
                    ViewboxInfo.FromColumn = value;
                    conn.DbMapping.Save(ViewboxInfo);
                }

                OnPropertyChanged("FromColumn");
            }
        }

        public string FromColumnFormat
        {
            get { return ViewboxInfo == null ? "" : ViewboxInfo.FromColumnFormat; }
            set
            {
                if (ViewboxInfo == null)
                    return;
                if (ViewboxInfo.FromColumnFormat != value)
                {
                    using (var conn = MainModel.CurrentProfile.ViewboxConnection.CreateConnection())
                    {
                        ViewboxInfo.FromColumnFormat = value;
                        ViewboxInfo.DataType = IsSpecialFormat ? SqlType.String : SqlType.DateTime;
                        conn.DbMapping.Save(ViewboxInfo);
                    }
                    OnPropertyChanged("FromColumnFormat");
                }
            }
        }


        public bool IsSpecialFormat
        {
            get { return !String.IsNullOrEmpty(FromColumnFormat) && (FromColumnFormat.Contains('0') || FromColumnFormat.Contains('1')); }
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

        public ViewboxDb.RowNoFilters.Parser.OperatorsForAssistant ViewboxParamOperator
        {
            get { return ViewboxInfo == null ? ViewboxDb.RowNoFilters.Parser.OperatorsForAssistant.None : (ViewboxDb.RowNoFilters.Parser.OperatorsForAssistant)ViewboxInfo.ParamOperator; }
            set
            {
                if (ViewboxInfo == null)
                    return;
                if (value != (ViewboxDb.RowNoFilters.Parser.OperatorsForAssistant)ViewboxInfo.ParamOperator)
                {
                    using (var conn = MainModel.CurrentProfile.ViewboxConnection.CreateConnection())
                    {
                        ViewboxInfo.ParamOperator = (int)value;
                        conn.DbMapping.Save(ViewboxInfo);
                    }
                    OnPropertyChanged("ViewboxParamOperator");
                }
            }
        }

        public void ClearViewboxInfos()
        {
            ViewboxError = "";
            ViewboxWarning = "";
            ViewboxInfo = null;
            ViewboxParamOperator = ViewboxDb.RowNoFilters.Parser.OperatorsForAssistant.None;
            ViewboxOptimizationType = OptimizationType.NotSet;
            IsInViewbox = false;
        }

        #endregion ViewboxInfos

        #region FinalInfos

        public bool IsInFinal { get; set; }

        private DbColumnInfo _finalInfo;
        public DbColumnInfo FinalInfo
        {
            get { return _finalInfo; }
            set
            {
                if (value != _finalInfo)
                {
                    _finalInfo = value;
                    OnPropertyChanged("FinalInfo");
                }
            }
        }

        public void ClearFinalInfos()
        {
            IsInFinal = false;
            FinalInfo = null;
        }

        #endregion FinalInfos

        #region ConfigureTableTexts

        public ICommand ConfigureColumnTextsCommand { get { return new RelayCommand(OnConfigureColumnTextsCommandExecute); } }

        private void OnConfigureColumnTextsCommandExecute(object sender)
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
            get { return ViewboxInfo; }
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
