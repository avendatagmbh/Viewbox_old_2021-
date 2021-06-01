using System;

namespace Utils
{
    public class ErrorEventArgs : EventArgs
    {
        public ErrorEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}