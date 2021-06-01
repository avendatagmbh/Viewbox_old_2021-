namespace ViewboxBusiness.Structures.Config
{
	public class MailConfig
	{
		public bool SendMailOnError { get; set; }

		public bool SendMailOnViewFinished { get; set; }

		public bool SendFinalReport { get; set; }

		public bool SendDailyReport { get; set; }

		public DailyReportIntervall DailyReportIntervall { get; set; }

		public MailConfig()
		{
			SendMailOnError = true;
			SendMailOnViewFinished = true;
			SendFinalReport = true;
			SendDailyReport = true;
			DailyReportIntervall = DailyReportIntervall.ReportOncePerDay;
		}
	}
}
