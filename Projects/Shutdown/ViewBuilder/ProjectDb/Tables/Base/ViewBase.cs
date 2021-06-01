using System;
using System.ComponentModel;
using DbAccess;
using ProjectDb.Enums;
using ViewBuilderCommon;

namespace ProjectDb.Tables.Base
{
    /// <summary>
    ///   This class specifies a view.
    /// </summary>
    public class ViewBase : TableBase, INotifyPropertyChanged, ICloneable
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="ViewState" /> class.
        /// </summary>
        public ViewBase()
        {
            Name = string.Empty;
            Description = string.Empty;
            Script = string.Empty;
            Agent = string.Empty;
            Duration = new TimeSpan();
        }

        #region fields

        /// <summary>
        ///   See property Agent.
        /// </summary>
        private string _agent;

        /// <summary>
        ///   See property CreationTimespamp.
        /// </summary>
        private DateTime _creationTimestamp;

        /// <summary>
        ///   See property Description.
        /// </summary>
        private string _description;

        /// <summary>
        ///   See property Duration.
        /// </summary>
        private TimeSpan _duration;

        /// <summary>
        ///   See property Error.
        /// </summary>
        private string _error;

        /// <summary>
        ///   See property TableName.
        /// </summary>
        private string _fileName;

        /// <summary>
        ///   See property Name.
        /// </summary>
        private string _name;

        /// <summary>
        ///   See property Script.
        /// </summary>
        private string _script;

        /// <summary>
        ///   See property State.
        /// </summary>
        private ViewStates _state;

        /// <summary>
        ///   See property ViewScriptState.
        /// </summary>
        private ViewscriptStates _viewScriptState;

        /// <summary>
        ///   See property Warning.
        /// </summary>
        private string _warning;

        #endregion fields

        #region persistent properties

        /// <summary>
        ///   Gets or sets the name.
        /// </summary>
        /// <value> The name. </value>
        [DbColumn("name", AllowDbNull = false, Length = 256)]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the fileName, which includes the full path, the phisical file name and extension
        /// </summary>
        /// <value> The fileName. </value>
        [DbColumn("fileName", AllowDbNull = false, Length = 2048)]
        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged("FileName");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the description.
        /// </summary>
        /// <value> The description. </value>
        [DbColumn("description", AllowDbNull = false, Length = 128)]
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the agent.
        /// </summary>
        /// <value> The agent. </value>
        [DbColumn("agent", AllowDbNull = true, Length = 64)]
        public string Agent
        {
            get { return _agent; }
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
            get { return _creationTimestamp; }
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

        /// <summary>
        ///   Gets or sets the duration to implement the view (excluding the time for index creation).
        /// </summary>
        /// <value> The duration. </value>
        [DbColumn("duration", AllowDbNull = false)]
        public TimeSpan Duration
        {
            get { return _duration; }
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

        /// <summary>
        ///   Gets or sets the script.
        /// </summary>
        /// <value> The script. </value>
        [DbColumn("script", AllowDbNull = false, Length = 100000)]
        public string Script
        {
            get { return _script; }
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
            get { return _viewScriptState; }
            set
            {
                if (_viewScriptState != value)
                {
                    _viewScriptState = value;
                    OnPropertyChanged("ViewScriptState");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the last error message.
        /// </summary>
        /// <value> The last error message. </value>
        [DbColumn("error", AllowDbNull = false, Length = 100000)]
        public string Error
        {
            get { return _error; }
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
            get { return _warning; }
            set
            {
                if (_warning != value)
                {
                    _warning = value;
                    OnPropertyChanged("Warnings");
                }
            }
        }

        #endregion persistent properties

        #region non persistent properties

        /// <summary>
        ///   Gets or sets the state.
        /// </summary>
        /// <value> The state. </value>
        public ViewStates State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        /// <summary>
        ///   Gets the creation timestamp display string.
        /// </summary>
        /// <value> The creation timestamp display string. </value>
        public string CreationTimestampDisplayString
        {
            get { return CreationTimestamp.ToString(); }
        }

        /// <summary>
        ///   Gets the duration display string (format: hhh:mm:ss)
        /// </summary>
        /// <value> The duration display string. </value>
        public string DurationDisplayString
        {
            get { return string.Format("{0:00}:{1:00}:{2:00}", Duration.TotalHours, Duration.Minutes, Duration.Seconds); }
        }

        #endregion non persistent properties

        #region methods

        /// <summary>
        ///   Creates a copy of this instance.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion methods

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/
    }
}