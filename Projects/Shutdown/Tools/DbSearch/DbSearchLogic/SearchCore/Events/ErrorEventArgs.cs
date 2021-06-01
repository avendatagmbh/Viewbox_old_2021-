using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchLogic.SearchCore.Events {
    public class ErrorEventArgs : EventArgs {
        public ErrorEventArgs(Exception exception) {
            if (exception is AggregateException) {
                AggregateException aggregateEx = (AggregateException) exception;
                Message = "Es sind folgende Fehler aufgetreten: " + Environment.NewLine;
                foreach (var ex in aggregateEx.InnerExceptions) {
                    Message += ex.Message + Environment.NewLine;
                }
            } else Message = exception.Message;
        }

        public string Message { get; private set; }
    }
}
