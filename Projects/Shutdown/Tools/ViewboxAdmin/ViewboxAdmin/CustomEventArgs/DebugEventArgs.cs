using System;

namespace ViewboxAdmin.CustomEventArgs
{
    public class DebugEventArgs : EventArgs {
        public DebugEventArgs() {
                
        }
        public DebugEventArgs(string text) {
            this.DebugMessage = text;
        }

        public string DebugMessage { get; set; }

    }
}
