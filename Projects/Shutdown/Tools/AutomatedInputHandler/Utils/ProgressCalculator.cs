using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Utils {
    public class ProgressCalculator : BackgroundWorker,INotifyPropertyChanged{
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        #region Constructor
        public ProgressCalculator() {
            WorkerReportsProgress = true;
        }
        #endregion

        #region Properties
        private readonly List<ProgressCalculator> _children = new List<ProgressCalculator>();
        private ProgressCalculator _parentCalculator = null;
        private long _maxSteps = 0;
        private long _currentStep = 0;

        private int _progress;
        public int Progress { 
            get { return _progress; } 
            set {
                if (_progress != value) {
                    _progress = value;
                    OnPropertyChanged("Progress");
                }
            }
        }

        public int Minimum { get { return 0; } }
        public int Maximum { get { return 100; } }

        private string _description = "Arbeite...";
        public string Description {
            get { return _description; }
            set {
                if (_description != value) {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        private string _title = "";
        public string Title {
            get { return _title; }
            set {
                if (_title != value) {
                    _title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        public object UserState { get; set; }
        #endregion

        #region Methods
        public void SetWorkSteps(long num, bool hasChildren) {
            _maxSteps = num;
            if (hasChildren) {
                _children.Clear();
                for (int i = 0; i < num; ++i) {
                    ProgressCalculator child = new ProgressCalculator() {_parentCalculator = this};
                    _children.Add(child);
                    child.PropertyChanged += new PropertyChangedEventHandler(child_PropertyChanged);
                }
            }
        }

        void child_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Description") {
                if(((ProgressCalculator) sender).Description != Description)
                    Description = ((ProgressCalculator) sender).Description;
            }
        }

        public ProgressCalculator this[int index] {
            get { return _children[index]; }
        }


        public void SetStep(int step)
        {
            Progress = step - 1;
            _currentStep = step;
            ReportProgress(0, 0);
        }

        public void StepDone() {
            _currentStep++;
            ReportProgress(0, 0);
        }
        public void StepsDone(int count) {
            for (int i = 0; i < count; ++i )
                _currentStep++;
            ReportProgress(0, 0);
        }

        private void ReportProgress(long currentStepChild, long maxStepsChild) {
            if (_parentCalculator != null) _parentCalculator.ReportProgress(_currentStep + currentStepChild, _maxSteps + maxStepsChild);
            else {
                double progressPerChild = 100.0 / _maxSteps;
                double overallProgress = _currentStep * progressPerChild;
                if (maxStepsChild != 0) overallProgress += progressPerChild * currentStepChild / maxStepsChild;
                if ((int)overallProgress > Progress) {
                    ReportProgress(System.Math.Min((int)overallProgress, 100));
                    Progress = (int)overallProgress;
                }
            }
        }
        #endregion
    }
}