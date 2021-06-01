using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DbSearchLogic.SearchCore.Events;
using DbSearchLogic.SearchCore.KeySearch;
using Utils;

namespace DbSearchLogic.ProcessQueue {
    /// <summary>
    /// The base class of key search stat entity
    /// </summary>
    public abstract class KeySearchStatEntityBase : NotifyPropertyChangedBase, IKeySearchStatEntry, IDisposable {

        #region [ Events ]

        /// <inheritdoc />
        public event EventHandler NotifyCompleted;
        private void OnNotifyCompleted() { if (NotifyCompleted != null) NotifyCompleted(this, new EventArgs()); }

        #endregion [ Events ]

        #region [ Members ]

        /// <summary>
        /// The disposed flag
        /// </summary>
        private bool isDisposed = false;
        private static SemaphoreSlim _semaphoreAddKey = new SemaphoreSlim(1);
        private static SemaphoreSlim _semaphoreAddProcessedKey = new SemaphoreSlim(1);

        #endregion [ Members ]
        
        #region [ IDeleted members ] 

        private EventHandler _deleted;
        /// <inheritdoc />
        public event EventHandler Deleted
        {
            add { this._deleted += value; }
            remove { this._deleted -= value; }
        }

        protected void OnDeleted() {
            IsDeleted = true;
            if (_deleted != null) _deleted(this, new EventArgs());
        }

        /// <inheritdoc />
        public bool IsDeleted { get; internal set; }

        #endregion [ IDeleted members ]

        #region [ IPrimaryKeySearchEntry members ]

        /// <inheritdoc />
        public string ProfileName { get; internal set; }
        /// <inheritdoc />
        public abstract KeySearchTypeEnum SearchType { get; }
        /// <inheritdoc />
        public DateTime SearchStarted { get; internal set; }
        /// <inheritdoc />
        public bool IsSelected { get; set; }

        private float _progress;
        /// <inheritdoc />
        public float Progress { get { return _progress; } }

        public void SetProgress(float progress) {
            _progress = progress;
            OnPropertyChanged("Progress");
        }

        /// <inheritdoc />
        public string Info { get; internal set; }

        private string _statusDescription;
        /// <inheritdoc />
        public string StatusDescription {
            get { return _statusDescription; }
            set {
                _statusDescription = value;
                OnPropertyChanged("StatusDescription");
            }
        }

        /// <inheritdoc />
        public int KeyComplexity { get; internal set; }

        private int _numberOfKeyCandidates;
        /// <inheritdoc />
        public int NumberOfKeyCandidates {
            get { return _numberOfKeyCandidates; }
            set {
                _numberOfKeyCandidates = value;
                if (NumberOfKeysFound > 0)
                    _progress = ((float) NumberOfKeysProcessed/NumberOfKeyCandidates)*100;
                else
                    _progress = 0;
                OnPropertyChanged("Progress");
                OnPropertyChanged("NumberOfKeyCandidates");
            }
        }

        /// <inheritdoc />
        public int NumberOfKeysProcessed { get; internal set; }
        /// <inheritdoc />
        public int NumberOfKeysFound { get; internal set; }
        /// <inheritdoc />
        public void KeyFound() {
            _semaphoreAddKey.Wait();
            try {
                KeyProcessed();
                NumberOfKeysFound += 1;
                OnPropertyChanged("NumberOfKeysFound");
                OnPropertyChanged("Progress");
            } finally {
                _semaphoreAddKey.Release();
            }
        }
        /// <inheritdoc />
        public void KeyProcessed() {
            _semaphoreAddProcessedKey.Wait();
            try {
                NumberOfKeysProcessed += 1;
                _progress = ((float) NumberOfKeysProcessed/NumberOfKeyCandidates)*100;
                OnPropertyChanged("NumberOfKeysProcessed");
                OnPropertyChanged("Progress");
            } finally {
                _semaphoreAddProcessedKey.Release();
            }
        }

        private KeySearchResult _taskStatus;
        /// <inheritdoc />
        public KeySearchResult TaskStatus {
            get { return _taskStatus; } 
            set {
                if (_taskStatus != value) {
                    _taskStatus = value;
                    switch (_taskStatus) {
                        case KeySearchResult.FinishedAlreadyRun:
                        case KeySearchResult.Finished:
                        case KeySearchResult.IdxDbMissing:
                        case KeySearchResult.Canceled:
                            OnNotifyCompleted();
                            break;
                        case KeySearchResult.Running:
                        case KeySearchResult.NotStarted:
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    if (_taskStatus == KeySearchResult.Finished || _taskStatus == KeySearchResult.FinishedAlreadyRun) SetProgress(100);
                    OnPropertyChanged("TaskStatus");
                }
            } 
        }

        /// <inheritdoc />
        public bool IsCompleted {
            get {
                switch (TaskStatus) {
                    case KeySearchResult.FinishedAlreadyRun:
                    case KeySearchResult.Finished:
                    case KeySearchResult.IdxDbMissing:
                    case KeySearchResult.Canceled:
                        return true;
                    case KeySearchResult.Running:
                    case KeySearchResult.NotStarted:
                        return false;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        /// <inheritdoc />
        public bool IsRunning { get { return TaskStatus == KeySearchResult.Running; } }
        /// <inheritdoc />
        public bool IsWaiting { get { return TaskStatus == KeySearchResult.NotStarted; } }

        /// <inheritdoc />
        public CancellationTokenSource CancellationTokenSource { get; internal set; }
        /// <inheritdoc />
        public Task<KeySearchResult> KeySearchTask { get; set; }
        /// <inheritdoc />
        public void CancelTask() {
            // if already finished, then nothing to cancel
            if (TaskStatus == KeySearchResult.Finished || TaskStatus == KeySearchResult.FinishedAlreadyRun) return;
            if (!CancellationTokenSource.IsCancellationRequested) CancellationTokenSource.Cancel();
            TaskStatus = KeySearchResult.Canceled;
        }
        /// <inheritdoc />
        public void DeleteTask() {
            CancelTask();
            OnDeleted();
        }

        #endregion [ IPrimaryKeySearchEntry members ]

        #region [ IDisposable members ]

        public void Dispose() {
            if (!isDisposed) {
                if (TaskStatus == KeySearchResult.NotStarted || TaskStatus == KeySearchResult.Running || TaskStatus == KeySearchResult.Canceled)
                    if (!CancellationTokenSource.IsCancellationRequested) CancellationTokenSource.Cancel();
            }
        }
        
        #endregion [ IDisposable members ]

        #region [ Destructor ]

        ~KeySearchStatEntityBase() {
            Dispose();
        }

        #endregion [ Destructor ]
    }
}
