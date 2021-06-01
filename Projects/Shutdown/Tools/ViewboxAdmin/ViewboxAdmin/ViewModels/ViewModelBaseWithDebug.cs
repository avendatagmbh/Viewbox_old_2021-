using System;
using AV.Log;
using ViewboxAdmin.CustomEventArgs;
using ViewboxAdmin_ViewModel;
using log4net;

namespace ViewboxAdmin.ViewModels {
    public class ViewModelBaseWithDebug : NotifyBase {
        private ILog _log = LogHelper.GetLogger();


        public ViewModelBaseWithDebug(IStatusReporter statusreporter) {
            //Init event listeners
            statusreporter.Started += OnDeleteProcessStarted;
            statusreporter.Crashed += OnDeleteProcessCrashed;
            statusreporter.Completed += OnDeleteProcessCompleted;
            statusreporter.DebugEvent += OnDebugEvent;
            statusreporter.ProgressEvent += OnProgressEvent;
        }
        private string _debugtext;
        private bool _isworking = false;

        public string DebugText {
            get { return _debugtext; }
            set {
                _debugtext = value;
                OnPropertyChanged("DebugText");
            }
        }

        public bool IsWorking {
            get {
                return _isworking;
            } 
            set {
                _isworking = value;
                OnPropertyChanged("IsWorking");
            }
        }

        #region Progress
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
        #endregion Progress

        protected void AddDebugInfo(string debugmessage) {
            _log.Info(debugmessage);

            DebugText = DateTime.Now.ToString() + " " + debugmessage + Environment.NewLine + DebugText;
        }
        protected virtual void OnDeleteProcessStarted(object sender, EventArgs e) {
            Progress = 0;  IsWorking = true; AddDebugInfo("The operation started..."); }

        protected virtual void OnDeleteProcessCompleted(object sender, EventArgs e) {
            IsWorking = false; 
            AddDebugInfo("The operation completed");
        }

        protected virtual void OnDeleteProcessCrashed(object sender, EventArgs e) { IsWorking = false; AddDebugInfo("The opearation crashed"); }

        protected virtual void OnDebugEvent(object sender, DebugEventArgs e) {
            AddDebugInfo(e.DebugMessage);
        }

        protected virtual void OnProgressEvent(object sender,ProgressEventArgs e) {
            Progress = e.ProgressPercentage;
        }
    }
}