using System;
using System.ComponentModel;
using DbAccess;
using ViewboxBusiness.Common;

namespace ViewboxBusiness.ViewBuilder
{
	public class WorkerState : EventArgs, INotifyPropertyChanged
	{
		private readonly object _lock = new object();

		private DatabaseBase _connection;

		private IJobInfo _jobInfo;

		private double _preparationProgress;

		private WorkerTask _task;

		public int ThreadIndex { get; private set; }

		public WorkerTask Task
		{
			get
			{
				return _task;
			}
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

		public string TaskName => Task switch
		{
			WorkerTask.Preparing => "Erstelle Temporäre Viewdatenbank...", 
			WorkerTask.Working => "Vieweinspielung...", 
			WorkerTask.Cleanup => "Deleting temporary data...", 
			WorkerTask.CreatingIndex => "Creating Indexes...", 
			WorkerTask.WaitingForNextViewscript => "Vaiting for next Viewscript...", 
			WorkerTask.CheckingReportParameters => "Checking report parameters...", 
			WorkerTask.CheckingProcedureParameters => "Checking procedure parameters...", 
			WorkerTask.CheckingWhereCondition => "Checking where condition...", 
			WorkerTask.CopyTable => "Copying table...", 
			WorkerTask.GetIndexInfo => "Getting index information for table...", 
			WorkerTask.DropExistingMetadata => "Dropping existing metadata...", 
			WorkerTask.GetColumnInfo => "Getting column information...", 
			WorkerTask.DeletingTableEntries => "Deleting existing table entries...", 
			WorkerTask.SavingIssue => "Saving view/report...", 
			WorkerTask.CalculatingOrderArea => "Calculating order areas...", 
			WorkerTask.GeneratingDistinctValues => "Generating distinct values for parameters...", 
			_ => throw new NotImplementedException(), 
		};

		public double PreparationProgress
		{
			get
			{
				return _preparationProgress;
			}
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

		public int Percent => (int)(100.0 * PreparationProgress);

		public string PreparationProgressText => (100.0 * PreparationProgress).ToString("0.00") + "%";

		public IJobInfo JobInfo
		{
			get
			{
				return _jobInfo;
			}
			set
			{
				if (_jobInfo != value)
				{
					_jobInfo = value;
					OnPropertyChanged("JobInfo");
				}
			}
		}

		public DatabaseBase Connection
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

		public event PropertyChangedEventHandler PropertyChanged;

		public WorkerState(int threadIndex)
		{
			ThreadIndex = threadIndex;
			_preparationProgress = 0.0;
		}

		private void OnPropertyChanged(string property)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}
	}
}
