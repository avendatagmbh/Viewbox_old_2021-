using System.Collections.Generic;
using System.ComponentModel;

namespace Utils
{
    public class ProgressCalculator : BackgroundWorker, INotifyPropertyChanged, IProgressCalculator
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                if (propertyName == "IsBusy")
                    PropertyChanged(this, new PropertyChangedEventArgs("IsNotBusy"));
            }
        }

        #endregion

        private static string _defaultProgressDescription = "Arbeite...";

        public static string DefaultProgressDescription
        {
            get { return _defaultProgressDescription; }
            set { _defaultProgressDescription = value; }
        }

        #region Constructor

        public ProgressCalculator()
        {
            WorkerReportsProgress = true;
        }

        #endregion

        #region Properties

        private readonly List<ProgressCalculator> _children = new List<ProgressCalculator>();
        private long _currentStep;
        private string _description = DefaultProgressDescription;
        private object _extInfo;
        private long _maxSteps;
        private ProgressCalculator _parentCalculator;

        private int _progress;
        private string _title = "";

        public object ExtInfo
        {
            get { return _extInfo; }
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
            get { return _progress; }
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

        public double ProgressDouble
        {
            get { return _progress; }
        }

        public int Minimum
        {
            get { return 0; }
        }

        public int Maximum
        {
            get { return 100; }
        }

        public double MinimumDouble
        {
            get { return 0; }
        }

        public double MaximumDouble
        {
            get { return 100; }
        }

        public string Title
        {
            get { return _title; }
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

        #endregion

        #region Methods

        public ProgressCalculator this[int index]
        {
            get { return _children[index]; }
        }

        public void SetWorkSteps(long num, bool hasChildren)
        {
            _maxSteps = num;
            if (hasChildren)
            {
                _children.Clear();
                for (int i = 0; i < num; ++i)
                {
                    ProgressCalculator child = new ProgressCalculator {_parentCalculator = this};
                    _children.Add(child);
                    child.PropertyChanged += child_PropertyChanged;
                }
            }
        }

        public void StepDone()
        {
            _currentStep++;
            ReportProgress(0, 0);
        }

        private void child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Description")
            {
                if (((ProgressCalculator) sender).Description != Description)
                    Description = ((ProgressCalculator) sender).Description;
            }
        }

        public void SetStep(int step)
        {
            Progress = step - 1;
            _currentStep = step;
            ReportProgress(0, 0);
        }

        public void StepsDone(int count)
        {
            for (int i = 0; i < count; ++i)
                _currentStep++;
            ReportProgress(0, 0);
        }

        private void ReportProgress(long currentStepChild, long maxStepsChild)
        {
            if (_parentCalculator != null)
                _parentCalculator.ReportProgress(_currentStep + currentStepChild, _maxSteps + maxStepsChild);
            else
            {
                double progressPerChild = 100.0/_maxSteps;
                double overallProgress = _currentStep*progressPerChild;
                if (maxStepsChild != 0) overallProgress += progressPerChild*currentStepChild/maxStepsChild;
                if ((int) overallProgress > Progress)
                {
                    ReportProgress(System.Math.Min((int) overallProgress, 100));
                    Progress = (int) overallProgress;
                }
            }
        }

        #endregion
    }
}