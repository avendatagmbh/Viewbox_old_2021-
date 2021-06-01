// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Base.Localisation;
using Business.Enums;
using Business.Interfaces;
using Config.Interfaces.DbStructure;
using DbAccess.Interaction;
using Utils;
using System.Diagnostics;
using System.IO;

namespace Business.Structures {
    internal class TransferProgress : NotifyPropertyChangedBase, ITransferProgress {

        internal TransferProgress() {
            ErrorMessages = new ObservableCollectionAsync<string>();
            EntityFinishedList = new List<ITransferEntity>();
        }

        public ObservableCollectionAsync<string> ErrorMessages { get; private set; }

        #region Step
        private TransferProgressSteps _step;

        public TransferProgressSteps Step {
            get { return _step; }
            set {
                _step = value;
                switch (value)
                {
                    case TransferProgressSteps.ExportingTables:
                        StepCaption = ResourcesCommon.ExportStepExportingTables;
                        break;
                    case TransferProgressSteps.GeneratingIndexXml:
                        StepCaption = ResourcesCommon.ExportStepGeneratingIndexXml;
                        break;
                    case TransferProgressSteps.ExportingErrorTables:
                        StepCaption = ResourcesCommon.ExportStepExportingErrorTables;
                        break;
                    case TransferProgressSteps.CreateDocumentation:
                        StepCaption = ResourcesCommon.ExportStepExportingCreateDocumentation;
                        break;
                    case TransferProgressSteps.WaitForConnection:
                        StepCaption = ResourcesCommon.ExportStepExportingWaitForConnection;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("value");
                }

                OnPropertyChanged("Step");
                OnPropertyChanged("StepCaption");
            }
        }
        #endregion

        public string StepCaption { get; set; }

        #region EntitiesTotal
        private int _entitiesTotal;

        public int EntitiesTotal {
            get { return _entitiesTotal; }
            set {
                _entitiesTotal = value;
                OnPropertyChanged("EntitiesTotal");
            }
        }
        #endregion EntitiesTotal

        #region WorkingTasks
        private int _workingTasks;

        public int WorkingTasks {
            get { return _workingTasks; }
            set {
                _workingTasks = value;
                _actualProcessedTables.Clear();
                for(int i = 0; i < value; i++) _actualProcessedTables.Add(new TransferTableProgressWrapper());
                OnPropertyChanged("ActualProcessedTables");
            }
        }
        #endregion WorkingTasks

        #region EntitiesFinished
        private int _entitiesFinished;

        public int EntitiesFinished {
            get { return _entitiesFinished; }
            set {
                _entitiesFinished = value;
                OnPropertyChanged("EntitiesFinished");
            }
        }
        #endregion EntitiesFinished

        #region TablesFinishedList
        public List<ITransferEntity> EntityFinishedList { get; set; }
        #endregion

        #region ActualProcessedTables
        private readonly ObservableCollectionAsync<ITransferTableProgressWrapper> _actualProcessedTables =
            new ObservableCollectionAsync<ITransferTableProgressWrapper>();

        public IEnumerable<ITransferTableProgressWrapper> ActualProcessedTables { get { return _actualProcessedTables; } }
        #endregion

        #region ProgressIndexXmlGenerationValue
        private int _progressIndexXmlGenerationValue;

        public int ProgressIndexXmlGenerationValue {
            get { return _progressIndexXmlGenerationValue; }
            set {
                _progressIndexXmlGenerationValue = value;
                OnPropertyChanged("ProgressIndexXmlGenerationValue");
            }
        }
        #endregion ProgressIndexXmlGenerationValue

        #region ProgressIndexXmlGenerationMax
        private int _progressIndexXmlGenerationMax;

        public int ProgressIndexXmlGenerationMax {
            get { return _progressIndexXmlGenerationMax; }
            set {
                _progressIndexXmlGenerationMax = value;
                OnPropertyChanged("ProgressIndexXmlGenerationValue");
            }
        }
        #endregion ProgressIndexXmlGenerationMax

        public string CpuPerformanceLabel {
            get {
                string result = ((int)CpuPerformance).ToString();
                return result + " %";
            }
        }

        public string RamPerformanceLabel {
            get {
                string result = RamPerformance.ToString();
                return result + " MB";
            }
        }

        public string HDDPerformanceLabel {
            get {
                if (_driveInfo == null) return string.Empty;
                // convert value to GB
                return ((double)HDDPerformance / (1024 * 1024 * 1024)).ToString("0.0,0", CultureInfo.InvariantCulture) + " GB";
            }
        }

        public double CpuPerformance {
            get { return CpuPerformanceCounter.NextValue(); }
        }

        public double RamPerformance {
            get { return RamPerformanceCounter.NextValue(); }
        }

        public long HDDPerformance {
            get {
                if (_driveInfo == null) return 0;
                return _driveInfo.AvailableFreeSpace;
            }
        }

        private PerformanceCounter _cpuPerformanceCounter;
        private PerformanceCounter CpuPerformanceCounter {
            get {
                if (_cpuPerformanceCounter == null)
                    _cpuPerformanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                return _cpuPerformanceCounter;
            }
        }

        private PerformanceCounter _ramPerformanceCounter;
        private PerformanceCounter RamPerformanceCounter {
            get {
                if (_ramPerformanceCounter == null)
                    _ramPerformanceCounter = new PerformanceCounter("Memory", "Available MBytes");
                return _ramPerformanceCounter;
            }
        }

        private DriveInfo _driveInfo;

        public void AddErrorMessage(string message) { ErrorMessages.Add(message); }

        public ITransferTableProgress AddProcessedEntity(ITransferEntity entity) {
            ITransferTableProgress result = null;
            lock (_actualProcessedTables) {
                foreach (var transferTableProgressWrapper in _actualProcessedTables) {
                    if (transferTableProgressWrapper.Progress == null) {
                        result = new TransferTableProgress {
                            TableName = entity.Name,
                            DatasetsTotal = entity.Count,
                            FilesTotal= ((ITable)entity).FileNames.Count,
                            StartTime = System.DateTime.Now
                        };
                        transferTableProgressWrapper.Progress = result;

                        OnPropertyChanged("ActualProcessedTables");
                        return result;
                    }
                }    
            }
            return null;
        }

        public void RemoveProcessedEntity(ITransferEntity entity) {
            EntitiesFinished++;
            EntityFinishedList.Add(entity);
            lock (_actualProcessedTables) {
                foreach (var transferTableProgressWrapper in _actualProcessedTables) {
                    if (transferTableProgressWrapper.Progress != null && transferTableProgressWrapper.Progress.TableName == entity.Name)
                        transferTableProgressWrapper.Progress = null;
                }    
            }
            OnPropertyChanged("ActualProcessedTables");
        }

        public void UpdatePerformace() {
            OnPropertyChanged("CpuPerformanceLabel");
            OnPropertyChanged("RamPerformanceLabel");
            OnPropertyChanged("HDDPerformanceLabel");
        }

        public void InitHDDObservation(DirectoryInfo di) {
            foreach (DriveInfo drive in DriveInfo.GetDrives()) {
                if (drive.Name == di.Root.Name) {
                    _driveInfo = drive;
                }
            }
        }
    }
}