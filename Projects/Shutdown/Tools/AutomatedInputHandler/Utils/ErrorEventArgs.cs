using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils {    
    public class ErrorEventArgs : EventArgs {
        public ErrorEventArgs(string message) { this.Message = message; }
        public string Message { get; set; }
    }
}
