using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Utils;

namespace ViewboxBusiness.ViewBuilder
{
	public class ViewBuilderState : INotifyPropertyChanged, IDisposable
	{
		private readonly Stopwatch _sw;

		private readonly Thread _updateDurationThread;

		private TimeSpan _duration;

		private int _processedViews;

		private double _progress;

		private int _processedIndices;

		public int ProcessedViews
		{
			get
			{
				return _processedViews;
			}
			set
			{
				if (_processedViews != value)
				{
					_processedViews = value;
					OnPropertyChanged("ProcessedViews");
					UpdateProgress();
				}
			}
		}

		public int TotalViews { get; private set; }

		public int TotalIndices { get; private set; }

		public double Progress
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
					OnPropertyChanged("ProgressDisplayString");
					OnPropertyChanged("Percent");
				}
			}
		}

		public int Percent => (int)(100.0 * Progress);

		public string ProgressDisplayString => (100.0 * Progress).ToString("0.00") + "%";

		public TimeSpan Duration
		{
			get
			{
				return _duration;
			}
			set
			{
				if (_duration != value)
				{
					_duration = value;
					OnPropertyChanged("Duration");
					OnPropertyChanged("DurationDisplayString");
				}
			}
		}

		public string DurationDisplayString => StringUtils.FormatTimeSpan(Duration);

		public int ProcessedIndices
		{
			get
			{
				return _processedIndices;
			}
			set
			{
				if (_processedIndices != value)
				{
					_processedIndices = value;
					OnPropertyChanged("ProcessedIndices");
					UpdateProgress();
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		internal ViewBuilderState(int processedViews, int totalViews, int totalIndices)
		{
			_sw = new Stopwatch();
			_sw.Start();
			_updateDurationThread = new Thread(UpdateDuration);
			_updateDurationThread.Start();
			ProcessedViews = processedViews;
			TotalViews = totalViews;
			TotalIndices = totalIndices;
		}

		private void OnPropertyChanged(string property)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		private void UpdateProgress()
		{
			Progress = (double)(ProcessedViews + ProcessedIndices) / (double)(TotalViews + TotalIndices);
			if (ProcessedViews == TotalViews)
			{
				_sw.Stop();
			}
		}

		public void Dispose()
		{
			_updateDurationThread.Abort();
		}

		private void UpdateDuration()
		{
			try
			{
				while (_sw.IsRunning)
				{
					Duration = _sw.Elapsed;
					Thread.Sleep(500);
				}
			}
			catch
			{
			}
		}
	}
}
