using System;
using System.ComponentModel;
using DbAccess;
using ViewBuilderCommon.Interfaces;

namespace ViewBuilderBusiness.EventArgs
{
    public enum WorkerTask
    {
        Preparing,
        Working,
        Cleanup,
        CreatingIndex,
        WaitingForNextViewscript,
        CheckingReportParameters,
        CheckingProcedureParameters,
        CheckingWhereCondition,
        CopyTable,
        GetIndexInfo,
        DropExistingMetadata,
        GetColumnInfo,
        DeletingTableEntries,
        SavingIssue,
        CalculatingOrderArea,
        GeneratingDistinctValues,
    }

    /// <summary>
    ///   State of a ViewBuilder worker thread.
    /// </summary>
    public class WorkerState : System.EventArgs, INotifyPropertyChanged
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="WorkerState" /> class.
        /// </summary>
        /// <param name="threadIndex"> Index of the thread. </param>
        public WorkerState(int threadIndex)
        {
            ThreadIndex = threadIndex;
            _preparationProgress = 0;
        }

        #region events

        /// <summary>
        ///   Occurs when a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion events

        #region eventTrigger

        /// <summary>
        ///   Called when a property changed.
        /// </summary>
        /// <param name="property"> The property. </param>
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion eventTrigger

        #region fields

        private readonly object _lock = new object();
        private IDatabase _connection;

        /// <summary>
        ///   Gets or sets the actually proceswsed view.
        /// </summary>
        /// <value> The view. </value>
        //public Viewscript View {
        //    get { return _view; }
        //    set {
        //        if (_view != value) {
        //            _view = value;
        //            OnPropertyChanged("View");
        //        }
        //    }
        //}
        private IJobInfo _jobInfo;

        /// <summary>
        ///   See property PreparationProgress.
        /// </summary>
        private double _preparationProgress;

        /// <summary>
        ///   See property Task.
        /// </summary>
        private WorkerTask _task;

        //private Viewscript _view;

        #endregion fields

        /*****************************************************************************************************/

        #region properties

        /// <summary>
        ///   See property View.
        /// </summary>
        /// <summary>
        ///   Gets or sets the index of the thread.
        /// </summary>
        /// <value> The index of the thread. </value>
        public int ThreadIndex { get; private set; }

        /// <summary>
        ///   Gets or sets the task.
        /// </summary>
        /// <value> The task. </value>
        public WorkerTask Task
        {
            get { return _task; }
            set
            {
                if (_task != value)
                {
                    _task = value;
                    OnPropertyChanged("Task");
                    OnPropertyChanged("TaskName");
                }
            }
        }

        /// <summary>
        ///   Gets the name of the task.
        /// </summary>
        /// <value> The name of the task. </value>
        public string TaskName
        {
            get
            {
                switch (Task)
                {
                    case WorkerTask.Preparing:
                        return "Erstelle Temporäre Viewdatenbank...";
                    case WorkerTask.Working:
                        return "Vieweinspielung...";
                    case WorkerTask.Cleanup:
                        return "Deleting temporary data...";
                    case WorkerTask.CreatingIndex:
                        return "Creating Indexes...";
                    case WorkerTask.WaitingForNextViewscript:
                        return "Vaiting for next Viewscript...";
                    case WorkerTask.CheckingReportParameters:
                        return "Checking report parameters...";
                    case WorkerTask.CheckingProcedureParameters:
                        return "Checking procedure parameters...";
                    case WorkerTask.CheckingWhereCondition:
                        return "Checking where condition...";
                    case WorkerTask.CopyTable:
                        return "Copying table...";
                    case WorkerTask.GetIndexInfo:
                        return "Getting index information for table...";
                    case WorkerTask.DropExistingMetadata:
                        return "Dropping existing metadata...";
                    case WorkerTask.GetColumnInfo:
                        return "Getting column information...";
                    case WorkerTask.DeletingTableEntries:
                        return "Deleting existing table entries...";
                    case WorkerTask.SavingIssue:
                        return "Saving view/report...";
                    case WorkerTask.CalculatingOrderArea:
                        return "Calculating order areas...";
                    case WorkerTask.GeneratingDistinctValues:
                        return "Generating distinct values for parameters...";
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        ///   Gets or sets the preparation progress.
        /// </summary>
        /// <value> The preparation progress. </value>
        public double PreparationProgress
        {
            get { return _preparationProgress; }
            set
            {
                if (_preparationProgress != value)
                {
                    _preparationProgress = value;
                    OnPropertyChanged("PreparationProgress");
                    OnPropertyChanged("PreparationProgressText");
                    OnPropertyChanged("Percent");
                }
            }
        }

        /// <summary>
        ///   Gets the percent.
        /// </summary>
        /// <value> The percent. </value>
        public int Percent
        {
            get { return (int) (100*PreparationProgress); }
        }

        /// <summary>
        ///   Gets the preparation progress text.
        /// </summary>
        /// <value> The preparation progress text. </value>
        public string PreparationProgressText
        {
            get { return (100*PreparationProgress).ToString("0.00") + "%"; }
        }

        public IJobInfo JobInfo
        {
            get { return _jobInfo; }
            set
            {
                if (_jobInfo != value)
                {
                    _jobInfo = value;
                    OnPropertyChanged("JobInfo");
                }
            }
        }

        public IDatabase Connection
        {
            get
            {
                lock (_lock)
                {
                    return _connection;
                }
            }
            set
            {
                lock (_lock)
                {
                    _connection = value;
                }
            }
        }

        #endregion properties

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/
    }
}