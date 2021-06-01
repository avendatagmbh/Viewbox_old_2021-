namespace ViewBuilderBusiness.Structures.Config
{
    /// <summary>
    ///   Enumeration of all avaliable report intervals.
    /// </summary>
    public enum DailyReportIntervall
    {
        /// <summary>
        ///   Report once per day at 08:30.
        /// </summary>
        ReportOncePerDay,

        /// <summary>
        ///   Report twice per day at 08:30 and 17:30.
        /// </summary>
        ReportTwicePerDay
    }

    /// <summary>
    ///   Configuration for e-mail notifications.
    /// </summary>
    public class MailConfig
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="MailConfig" /> class.
        /// </summary>
        public MailConfig()
        {
            SendMailOnError = true;
            SendMailOnViewFinished = true;
            SendFinalReport = true;
            SendDailyReport = true;
            DailyReportIntervall = DailyReportIntervall.ReportOncePerDay;
        }

        #region properties

        /// <summary>
        ///   Gets or sets a value indicating whether a status report should be sent when an error has occured.
        /// </summary>
        /// <value> <c>true</c> if a status report should be sent when an error has occured; otherwise, <c>false</c> . </value>
        public bool SendMailOnError { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether a status report should be sent when a view has been finished.
        /// </summary>
        /// <value> <c>true</c> if a status report should be sent when a view has been finished; otherwise, <c>false</c> . </value>
        public bool SendMailOnViewFinished { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether if a final report should be sent.
        /// </summary>
        /// <value> <c>true</c> if a final report should be sent; otherwise, <c>false</c> . </value>
        public bool SendFinalReport { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether a daily status report should be sent.
        /// </summary>
        /// <value> <c>true</c> if a daily status report shoul be sent; otherwise, <c>false</c> . </value>
        public bool SendDailyReport { get; set; }

        /// <summary>
        ///   Gets or sets the daily report intervall.
        /// </summary>
        /// <value> The daily report intervall. </value>
        public DailyReportIntervall DailyReportIntervall { get; set; }

        #endregion properties

        /*****************************************************************************************************/
    }
}