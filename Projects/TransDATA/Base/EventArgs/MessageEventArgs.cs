namespace Base.EventArgs {
    /// <summary>
    /// Event args for message events.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-08-27</since>
    public class MessageEventArgs : System.EventArgs {
        public MessageEventArgs(string message) {
            Message = message;
        }

        public string Message { get; private set; }
    }
}