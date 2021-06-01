namespace ViewboxAdmin_ViewModel.Structures.Misc {
    public class MessageEventArgs : global::System.EventArgs {

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MessageEventArgs(string message) {
            this.Message = message;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; private set; }
    }
}
