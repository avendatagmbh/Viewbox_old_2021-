namespace ViewboxBusiness.Common
{
	public class OptimizeCriterias
	{
		public bool DoClientSplit { get; set; }

		public string ClientField { get; set; }

		public bool DoCompCodeSplit { get; set; }

		public string CompCodeField { get; set; }

		public bool DoFYearSplit { get; set; }

		public string GJahrField { get; set; }

		public bool YearRequired { get; set; }

		public OptimizeCriterias()
		{
			DoClientSplit = false;
			ClientField = string.Empty;
			DoCompCodeSplit = false;
			CompCodeField = string.Empty;
			DoFYearSplit = false;
			GJahrField = string.Empty;
			YearRequired = false;
		}
	}
}
