namespace ViewBuilderBusiness.EventArgs
{
    /// <summary>
    ///   Event class for script parse errors.
    /// </summary>
    public class ScriptParseErrorArgs : System.EventArgs
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="ScriptParseErrorArgs" /> class.
        /// </summary>
        /// <param name="scriptfile"> The scriptfile. </param>
        /// <param name="message"> The message. </param>
        public ScriptParseErrorArgs(string scriptfile, string message)
        {
            Scriptfile = scriptfile;
            Message = message;
        }

        /// <summary>
        ///   Gets or sets the scriptfile.
        /// </summary>
        /// <value> The scriptfile. </value>
        public string Scriptfile { get; set; }

        /// <summary>
        ///   Gets or sets the message.
        /// </summary>
        /// <value> The message. </value>
        public string Message { get; set; }
    }
}