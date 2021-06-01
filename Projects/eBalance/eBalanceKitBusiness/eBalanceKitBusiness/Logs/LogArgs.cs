using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Interfaces;

namespace eBalanceKitBusiness.Logs {
    public class LogArgs : System.EventArgs {
        public LogArgs(string element, object oldValue, object newValue) {
            Element = element;
            OldValue = oldValue;
            NewValue = newValue;
        }
        public string Element { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }
    public delegate void LogHandler(ILoggableObject myObject, LogArgs myArgs);
}
