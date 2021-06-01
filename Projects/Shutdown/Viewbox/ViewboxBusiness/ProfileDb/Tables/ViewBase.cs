using System;
using System.Globalization;
using DbAccess.Attributes;

namespace ViewboxBusiness.ProfileDb.Tables
{
	public class ViewBase : TableBase, ICloneable
	{
		private string _agent;

		private DateTime _creationTimestamp;

		private string _description;

		private TimeSpan _duration;

		private string _error;

		private string _fileName;

		private string _name;

		private string _script;

		private ViewStates _state;

		private ViewscriptStates _viewScriptState;

		private string _warning;

		[DbColumn("name", AllowDbNull = false, Length = 256)]
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

		[DbColumn("fileName", AllowDbNull = false, Length = 2048)]
		public string FileName
		{
			get
			{
				return _fileName;
			}
			set
			{
				if (_fileName != value)
				{
					_fileName = value;
					OnPropertyChanged("FileName");
				}
			}
		}

		[DbColumn("description", AllowDbNull = false, Length = 128)]
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

		[DbColumn("agent", AllowDbNull = true, Length = 64)]
		public string Agent
		{
			get
			{
				return _agent;
			}
			set
			{
				if (_agent != value)
				{
					_agent = value;
					OnPropertyChanged("Agent");
				}
			}
		}

		[DbColumn("creationTimestamp", AllowDbNull = false)]
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

		[DbColumn("duration", AllowDbNull = false)]
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

		[DbColumn("script", AllowDbNull = false, Length = 100000)]
		public string Script
		{
			get
			{
				return _script;
			}
			set
			{
				if (_script != value)
				{
					_script = value;
					OnPropertyChanged("Script");
				}
			}
		}

		[DbColumn("state", AllowDbNull = false)]
		public ViewscriptStates ViewScriptState
		{
			get
			{
				return _viewScriptState;
			}
			set
			{
				if (_viewScriptState != value)
				{
					_viewScriptState = value;
					OnPropertyChanged("ViewScriptState");
				}
			}
		}

		[DbColumn("error", AllowDbNull = false, Length = 100000)]
		public string Error
		{
			get
			{
				return _error;
			}
			set
			{
				if (_error != value)
				{
					_error = value;
					OnPropertyChanged("Error");
				}
			}
		}

		[DbColumn("warnings", AllowDbNull = false, Length = 100000)]
		public string Warnings
		{
			get
			{
				return _warning;
			}
			set
			{
				if (_warning != value)
				{
					_warning = value;
					OnPropertyChanged("Warnings");
				}
			}
		}

		public ViewStates State
		{
			get
			{
				return _state;
			}
			set
			{
				if (_state != value)
				{
					_state = value;
					OnPropertyChanged("State");
				}
			}
		}

		public string CreationTimestampDisplayString => CreationTimestamp.ToString(CultureInfo.InvariantCulture);

		public string DurationDisplayString => $"{Duration.TotalHours:00}:{Duration.Minutes:00}:{Duration.Seconds:00}";

		public ViewBase()
		{
			Name = string.Empty;
			Description = string.Empty;
			Script = string.Empty;
			Agent = string.Empty;
			Duration = default(TimeSpan);
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
