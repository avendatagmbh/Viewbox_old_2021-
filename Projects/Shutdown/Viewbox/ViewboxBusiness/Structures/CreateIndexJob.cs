using System;
using System.Diagnostics;
using System.Threading;
using Utils;
using ViewboxBusiness.Common;
using ViewboxBusiness.ProfileDb;

namespace ViewboxBusiness.Structures
{
	public class CreateIndexJob : NotifyPropertyChangedBase, IJobInfo
	{
		private readonly Stopwatch _sw = new Stopwatch();

		private DateTime _creationTimestamp;

		private TimeSpan _duration;

		private string _name;

		public DateTime CreationTimestamp
		{
			get
			{
				return _creationTimestamp;
			}
			set
			{
				if (_creationTimestamp != value)
				{
					_creationTimestamp = value;
					OnPropertyChanged("CreationTimestamp");
					OnPropertyChanged("CreationTimestampDisplayString");
				}
			}
		}

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

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (_name != value)
				{
					_name = value;
					OnPropertyChanged("Name");
				}
			}
		}

		public ViewscriptStates State => ViewscriptStates.CreatingIndex;

		public void StartStopwatch()
		{
			_sw.Reset();
			_sw.Start();
			new Thread(UpdateDuration).Start();
		}

		public void StopStopwatch()
		{
			CreationTimestamp = DateTime.Now;
			_sw.Stop();
			Duration = _sw.Elapsed;
		}

		private void UpdateDuration()
		{
			while (_sw.IsRunning)
			{
				Duration = _sw.Elapsed;
				Thread.Sleep(500);
			}
		}
	}
}
