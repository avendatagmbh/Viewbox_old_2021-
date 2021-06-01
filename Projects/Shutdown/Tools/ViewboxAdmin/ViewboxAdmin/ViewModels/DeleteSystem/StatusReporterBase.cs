using System;
using ViewboxAdmin.CustomEventArgs;

namespace ViewboxAdmin.ViewModels {
    public abstract class StatusReporterBase {
        /// <summary>
        /// system deletion procedure started event
        /// </summary>
        public event EventHandler<EventArgs> Started;

        protected virtual void OnStart() {
            if (Started != null)
            {
                Started(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// System deletion completed event
        /// </summary>
        public event EventHandler<EventArgs> Completed;

        protected virtual void OnCompleted() {
            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        /// <summary>
        /// System deletion crashed event
        /// </summary>
        public event EventHandler<EventArgs> Crashed;

        protected virtual void OnCrashed() {
            if (Crashed != null)
                Crashed(this, EventArgs.Empty);
        }

        /// <summary>
        /// progress event 
        /// </summary>
        public event EventHandler<CustomEventArgs.DebugEventArgs> DebugEvent;

        protected virtual void OnDebug(string progressmessage) {
            if (DebugEvent != null)
                DebugEvent(this, new DebugEventArgs(progressmessage));
        }

        public event EventHandler<CustomEventArgs.ProgressEventArgs> ProgressEvent;
        protected virtual void OnProgress(int progresspercentage) {
            if (ProgressEvent != null)
                ProgressEvent(this, new ProgressEventArgs(progresspercentage));
        }

    }
}