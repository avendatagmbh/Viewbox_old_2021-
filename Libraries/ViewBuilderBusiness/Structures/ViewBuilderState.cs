using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Utils;

namespace ViewBuilderBusiness.Structures
{
    /// <summary>
    /// </summary>
    public class ViewBuilderState : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="ViewBuilderState" /> class.
        /// </summary>
        /// <param name="processedViews"> The processed views. </param>
        /// <param name="totalViews"> The total views. </param>
        /// <param name="count"> </param>
        /// <param name="state"> The state. </param>
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

        #region events

        /// <summary>
        ///   Occurs when a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion events

        #region eventTriggers

        /// <summary>
        ///   Called when a property changed.
        /// </summary>
        /// <param name="property"> The property. </param>
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion eventTriggers

        #region fields

        /// <summary>
        ///   Stopwatch for runtime measurement.
        /// </summary>
        private readonly Stopwatch _sw;

        private readonly Thread _updateDurationThread;

        /// <summary>
        ///   See property Duration.
        /// </summary>
        private TimeSpan _duration;

        /// <summary>
        ///   See property ProcessedViews.
        /// </summary>
        private int _processedViews;

        /// <summary>
        ///   See property Progress.
        /// </summary>
        private double _progress;

        #endregion fields

        #region properties

        private int _processedIndices;

        /// <summary>
        ///   Gets or sets the processed views.
        /// </summary>
        /// <value> The processed views. </value>
        public int ProcessedViews
        {
            get { return _processedViews; }
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

        /// <summary>
        ///   Gets or sets the total views.
        /// </summary>
        /// <value> The total views. </value>
        public int TotalViews { get; private set; }

        public int TotalIndices { get; private set; }

        /// <summary>
        ///   Gets or sets the progress.
        /// </summary>
        /// <value> The progress. </value>
        public double Progress
        {
            get { return _progress; }
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

        /// <summary>
        ///   Gets the percent.
        /// </summary>
        /// <value> The percent. </value>
        public int Percent
        {
            get { return (int) (100*Progress); }
        }

        /// <summary>
        ///   Gets the progress display string.
        /// </summary>
        /// <value> The progress display string. </value>
        public string ProgressDisplayString
        {
            get { return (100*Progress).ToString("0.00") + "%"; }
        }

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
        ///   Gets the duration display string.
        /// </summary>
        /// <value> The duration display string. </value>
        public string DurationDisplayString
        {
            get { return StringUtils.FormatTimeSpan(Duration); }
        }

        public int ProcessedIndices
        {
            get { return _processedIndices; }
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

        private void UpdateProgress()
        {
            Progress = (ProcessedViews + ProcessedIndices)/(double) (TotalViews + TotalIndices);
            // stop stopwatch if all views has been processed
            if (ProcessedViews == TotalViews)
            {
                _sw.Stop();
            }
        }

        #endregion properties

        #region methods 

        /// <summary>
        ///   Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            _updateDurationThread.Abort();
        }

        /// <summary>
        ///   Updates the duration.
        /// </summary>
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
            catch (Exception)
            {
            }
        }

        #endregion methods

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/
    }
}