using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Interfaces;
using Utils;
using Business.Enums;
using Base.Localisation;
using Config.Interfaces.DbStructure;

namespace Business.Structures {
    internal class ImportDbStructureProgress : NotifyPropertyChangedBase, IImportDbStructureProgress {
        internal ImportDbStructureProgress() { ErrorMessages = new ObservableCollectionAsync<string>(); }

        public ObservableCollectionAsync<string> ErrorMessages { get; private set; }

        #region Step
        private ImportDbStructureSteps _step;

        public ImportDbStructureSteps Step {
            get { return _step; }
            set {
                _step = value;
                switch (value) {
                    case ImportDbStructureSteps.GeneratingMeta:
                        StepCaption = ResourcesCommon.ImportStepGeneratingMetaInformation;
                        break;
                    case ImportDbStructureSteps.CountingTables:
                        StepCaption = ResourcesCommon.ImportStepCountingTable;
                        break;
                    case ImportDbStructureSteps.SavingData:
                        StepCaption = ResourcesCommon.ImportStepSaving;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("value");
                }

                OnPropertyChanged("Step");
                OnPropertyChanged("StepCaption");
            }
        }
        #endregion

        public string StepCaption { get; internal set; }

        #region TablesTotal
        private int _tablesTotal;

        public int TablesTotal {
            get { return _tablesTotal; }
            set {
                _tablesTotal = value;
                OnPropertyChanged("TablesTotal");
            }
        }
        #endregion TablesTotal

        #region TablesFinished
        private int _tablesFinished;

        public int TablesFinished {
            get { return _tablesFinished; }
            internal set {
                _tablesFinished = value;
                OnPropertyChanged("TablesFinished");
            }
        }
        #endregion TablesFinished

        #region ActualProcessedTables
        private readonly ObservableCollectionAsync<IImportDbStructureTableProgressWrapper> _actualProcessedTables =
            new ObservableCollectionAsync<IImportDbStructureTableProgressWrapper>();

        public IEnumerable<IImportDbStructureTableProgressWrapper> ActualProcessedTables { get { return _actualProcessedTables; } }
        #endregion

        #region WorkingTasks
        private int _workingTasks;

        public int WorkingTasks {
            get { return _workingTasks; }
            set {
                _workingTasks = value;
                _actualProcessedTables.Clear();
                for (int i = 0; i < value; i++) _actualProcessedTables.Add(new ImportDbStructureTableProgressWrapper());
                OnPropertyChanged("ActualProcessedTables");
            }
        }
        #endregion WorkingTasks

        public void AddErrorMessage(string message) { ErrorMessages.Add(message); }

        public IImportDbStructureTableProgress AddProcessedTable(ITable table) {
            IImportDbStructureTableProgress result = null;
            lock (_actualProcessedTables) {
                foreach (var importDbStructureTableProgressWrapper in _actualProcessedTables) {
                    if (importDbStructureTableProgressWrapper.Progress == null) {
                        result = new ImportDbStructureTableProgress
                                 {TableName = table.Name, DatasetsTotal = table.Count};
                        importDbStructureTableProgressWrapper.Progress = result;

                        OnPropertyChanged("ActualProcessedTables");
                        return result;
                    }
                }
            }
            return null;
        }

        public void RemoveProcessedTable(ITable table) {
            TablesFinished++;
            lock (_actualProcessedTables) {
                foreach (var importDbStructureTableProgressWrapper in _actualProcessedTables) {
                    if (importDbStructureTableProgressWrapper.Progress != null && importDbStructureTableProgressWrapper.Progress.TableName == table.Name)
                        importDbStructureTableProgressWrapper.Progress = null;
                }    
            }
            OnPropertyChanged("ActualProcessedTables");
        }
    }
}
