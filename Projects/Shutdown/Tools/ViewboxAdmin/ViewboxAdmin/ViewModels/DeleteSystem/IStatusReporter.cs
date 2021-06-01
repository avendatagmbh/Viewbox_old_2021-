using System;
using ViewboxAdmin.CustomEventArgs;

namespace ViewboxAdmin.ViewModels {
    public interface IStatusReporter {
        event EventHandler<EventArgs> Started;
        event EventHandler<EventArgs> Completed;
        event EventHandler<EventArgs> Crashed;
        event EventHandler<DebugEventArgs> DebugEvent;
        event EventHandler<ProgressEventArgs> ProgressEvent;
    }
}