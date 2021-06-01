using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Utils
{
	public class ProgressCalculator : BackgroundWorker, INotifyPropertyChanged, IProgressCalculator
	{
		private static string _defaultProgressDescription = "Arbeite...";

		private readonly List<ProgressCalculator> _children = new List<ProgressCalculator>();

		private long _currentStep;

		private string _description = DefaultProgressDescription;

		private object _extInfo;

		private long _maxSteps;

		private ProgressCalculator _parentCalculator;

		private int _progress;

		private string _title = "";

		public static string DefaultProgressDescription
		{
			get
			{
				return _defaultProgressDescription;
			}
			set
			{
				_defaultProgressDescription = value;
			}
		}

		public object ExtInfo
		{
			get
			{
				return _extInfo;
			}
			set
			{
				if (_extInfo != value)
				{
					_extInfo = value;
					OnPropertyChanged("ExtInfo");
				}
			}
		}

		public int Progress
		{
			get
			{
				return _progress;
			}
			set
			{
				if (_progress != value)
				{
					_progress = value;
					OnPropertyChanged("Progress");
					OnPropertyChanged("ProgressDouble");
				}
			}
		}

		public double ProgressDouble => _progress;

		public int Minimum => 0;

		public int Maximum => 100;

		public double MinimumDouble => 0.0;

		public double MaximumDouble => 100.0;

		public string Title
		{
			get
			{
				return _title;
			}
			set
			{
				if (_title != value)
				{
					_title = value;
					OnPropertyChanged("Title");
				}
			}
		}

		public object UserState { get; set; }

		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				if (_description != value)
				{
					_description = value;
					OnPropertyChanged("Description");
				}
			}
		}

		public ProgressCalculator this[int index] => _children[index];

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
				if (propertyName == "IsBusy")
				{
					this.PropertyChanged(this, new PropertyChangedEventArgs("IsNotBusy"));
				}
			}
		}

		public ProgressCalculator()
		{
			base.WorkerReportsProgress = true;
		}

		public void SetWorkSteps(long num, bool hasChildren)
		{
			_maxSteps = num;
			if (hasChildren)
			{
				_children.Clear();
				for (int i = 0; i < num; i++)
				{
					ProgressCalculator child = new ProgressCalculator
					{
						_parentCalculator = this
					};
					_children.Add(child);
					child.PropertyChanged += ChildPropertyChanged;
				}
			}
		}

		public void StepDone()
		{
			_currentStep++;
			ReportProgress(0L, 0L);
		}

		private void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Description" && ((ProgressCalculator)sender).Description != Description)
			{
				Description = ((ProgressCalculator)sender).Description;
			}
		}

		public void SetStep(int step)
		{
			Progress = step - 1;
			_currentStep = step;
			ReportProgress(0L, 0L);
		}

		public void StepsDone(int count)
		{
			for (int i = 0; i < count; i++)
			{
				_currentStep++;
			}
			ReportProgress(0L, 0L);
		}

		private void ReportProgress(long currentStepChild, long maxStepsChild)
		{
			if (_parentCalculator != null)
			{
				_parentCalculator.ReportProgress(_currentStep + currentStepChild, _maxSteps + maxStepsChild);
				return;
			}
			double progressPerChild = 100.0 / (double)_maxSteps;
			double overallProgress = (double)_currentStep * progressPerChild;
			if (maxStepsChild != 0L)
			{
				overallProgress += progressPerChild * (double)currentStepChild / (double)maxStepsChild;
			}
			if ((int)overallProgress > Progress)
			{
				ReportProgress(Math.Min((int)overallProgress, 100));
				Progress = (int)overallProgress;
			}
		}
	}
}
