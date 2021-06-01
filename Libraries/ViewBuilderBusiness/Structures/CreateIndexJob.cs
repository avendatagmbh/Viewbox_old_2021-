using System;
using System.Diagnostics;
using System.Threading;
using Utils;
using ViewBuilderCommon;
using ViewBuilderCommon.Interfaces;

namespace ViewBuilderBusiness.Structures
{
    public class CreateIndexJob : NotifyPropertyChangedBase, IJobInfo
    {
        #region Constructor

        #endregion Constructor

        #region Properties

        #region DurationHelper

        private readonly Stopwatch _sw = new Stopwatch();
        private DateTime _creationTimestamp;
        private TimeSpan _duration;

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

        public void StartStopwatch()
        {
            _sw.Reset();
            _sw.Start();
            new Thread(UpdateDuration).Start();
        }

        /// <summary>
        ///   Stops the stopwatch and updates the creation timestamp.
        /// </summary>
        public void StopStopwatch()
        {
            CreationTimestamp = DateTime.Now;
            _sw.Stop();
            Duration = _sw.Elapsed;
        }

        /// <summary>
        ///   Gets the duration display string.
        /// </summary>
        /// <value> The duration display string. </value>
        public string DurationDisplayString
        {
            get { return StringUtils.FormatTimeSpan(Duration); }
        }

        /// <summary>
        ///   Updates the duration.
        /// </summary>
        private void UpdateDuration()
        {
            while (_sw.IsRunning)
            {
                Duration = _sw.Elapsed;
                Thread.Sleep(500);
            }
        }

        #endregion DurationHelper

        private string _name;

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

        public ViewscriptStates State
        {
            get { return ViewscriptStates.CreatingIndex; }
        }

        #endregion Properties

        #region Methods

        #endregion Methods
    }
}