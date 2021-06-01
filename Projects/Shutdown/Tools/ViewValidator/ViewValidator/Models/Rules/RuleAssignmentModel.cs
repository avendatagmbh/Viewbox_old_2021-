// -----------------------------------------------------------
// Created by Benjamin Held - 25.08.2011 11:03:57
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AvdCommon.Rules;
using AvdCommon.Rules.Gui.DragDrop;
using ViewValidatorLogic.Structures.InitialSetup;
using ViewValidatorLogic.Manager;
using System.Collections.Generic;

namespace ViewValidator.Models.Rules {
    public class RuleAssignmentModel : INotifyPropertyChanged{
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        #region Constructor
        public RuleAssignmentModel(MainWindowModel mainWindowModel, Window ownerWindow){
            this.ExtendedDataPreviewModel = new ExtendedDataPreviewModel(this);
            MainWindowModel = mainWindowModel;
            MainWindowModel.PropertyChanged += new PropertyChangedEventHandler(MainWindowModel_PropertyChanged);
            DragDropData = new RuleListDragDropData();
            RuleToColumnsAssignmentModelValidation = new RuleToColumnsAssignmentModel(0, ownerWindow, DragDropData);
            RuleToColumnsAssignmentModelView = new RuleToColumnsAssignmentModel(1, ownerWindow, DragDropData);
        }
        #endregion

        #region Properties
        public bool DoUpdate { get; set; }
        public RuleListDragDropData DragDropData { get; set; }

        public MainWindowModel MainWindowModel { get; private set; }
        public ExtendedDataPreviewModel ExtendedDataPreviewModel { get; set; }
        public RuleSet ProfileRules { get {
            return MainWindowModel.SelectedProfile == null ? null : MainWindowModel.SelectedProfile.CustomRules; /*RuleManager.Instance.ProfileRules;*/
            }
        }
        public RuleToColumnsAssignmentModel RuleToColumnsAssignmentModelValidation { get; private set; }
        public RuleToColumnsAssignmentModel RuleToColumnsAssignmentModelView { get; private set; }

        #region SelectedRule
        private Rule _selectedRule;
        public Rule SelectedRule {
            get { return _selectedRule; }
            set {
                if (_selectedRule != value) {
                    _selectedRule = value;
                    OnPropertyChanged("SelectedRule");
                }
            }
        }
        #endregion SelectedRule

        #region SelectedSortRule
        private Rule _selectedSortRule;
        public Rule SelectedSortRule {
            get { return _selectedSortRule; }
            set {
                if (_selectedSortRule != value) {
                    _selectedSortRule = value;
                    OnPropertyChanged("SelectedSortRule");
                }
            }
        }
        #endregion SelectedSortRule

        #region Tasks
        private Task LastFillDataTask { get; set; }



        CancellationTokenSource _lastTokenSource = new CancellationTokenSource();
        #endregion
        #endregion

        #region Eventhandler
        void MainWindowModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "SelectedTableMapping") {
                Update();
                if (MainWindowModel.SelectedTableMapping != null) {
                    MainWindowModel.SelectedTableMapping.ColumnMappings.CollectionChanged += ColumnMappings_CollectionChanged;
                }
            }
            if (e.PropertyName == "SelectedProfile") {
                OnPropertyChanged("ProfileRules");
            }
        }

        void ColumnMappings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            Update();
        }
        #endregion

        #region Methods
        public void Update() {
            if (!DoUpdate) return;
            TableMapping tableMapping = MainWindowModel.SelectedTableMapping;
            if (MainWindowModel.SelectedProfile == null || tableMapping == null) ExtendedDataPreviewModel.Init(null);
            else {
                ExtendedDataPreviewModel.Init(tableMapping);

                //ExtendedDataPreviewModel.FillData(true);
                if (LastFillDataTask != null && !LastFillDataTask.IsCompleted) {
                    _lastTokenSource.Cancel();
                    _lastTokenSource = new CancellationTokenSource();

                }
                LastFillDataTask = Task.Factory.StartNew(() => ExtendedDataPreviewModel.FillData(true), _lastTokenSource.Token).ContinueWith((task) => HandleFillDataError(task));

                RuleToColumnsAssignmentModelValidation.TableMapping = tableMapping;
                RuleToColumnsAssignmentModelView.TableMapping = tableMapping;

                OnPropertyChanged("ExtendedDataPreviewModel");
            }
        }

        private void HandleFillDataError(Task task) {
            if (task.Exception != null) {
                System.Diagnostics.Debug.WriteLine("Error: " + task.Exception.Message);
            }
        }

        public void AddRules(List<Rule> rules, string column, int which, int index ) {
            TableMapping tableMapping = MainWindowModel.SelectedTableMapping;
            if (tableMapping != null) {

                //Normally this should work, but the index is the displayindex of a column which may under some circumstances change
                if (index < tableMapping.ColumnMappings.Count && tableMapping.ColumnMappings[index].GetColumnName(which) == column) {
                    tableMapping.ColumnMappings[index].GetColumn(which).Rules.AddRules(rules);
                } else {
                    //Here the problem was that if columns have been mapped multiple times, the rules will be added to all columns
                    foreach (var columnMapping in tableMapping.ColumnMappings) {
                        if (columnMapping.GetColumnName(which) == column) {
                            columnMapping.GetColumn(which).Rules.AddRules(rules);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
