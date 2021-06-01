namespace ViewBuilderBusiness.Structures.Config
{
    /// <summary>
    ///   Config class for smtp-server configuration.
    /// </summary>
    public class SmtpServerConfig
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="SmtpServerConfig" /> class.
        /// </summary>
        public SmtpServerConfig()
        {
            Sender = "rsadata7@yahoo.de";
            Server = "smtp.mail.yahoo.de";
            Port = 25;
            User = "rsadata7@yahoo.de";
            Password = string.Empty;
        }

        #region properties

        /// <summary>
        ///   Gets or sets the sender.
        /// </summary>
        /// <value> The sender. </value>
        public string Sender { get; set; }

        /// <summary>
        ///   Gets or sets the server.
        /// </summary>
        /// <value> The server. </value>
        public string Server { get; set; }

        /// <summary>
        ///   Gets or sets the port.
        /// </summary>
        /// <value> The port. </value>
        public int Port { get; set; }

        /// <summary>
        ///   Gets or sets the user.
        /// </summary>
        /// <value> The user. </value>
        public string User { get; set; }

        /// <summary>
        ///   Gets or sets the password.
        /// </summary>
        /// <value> The password. </value>
        public string Password { get; set; }

        #endregion properties
    }
}