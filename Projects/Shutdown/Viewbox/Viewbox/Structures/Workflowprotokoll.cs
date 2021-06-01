using DbAccess.Attributes;

namespace Viewbox.Structures
{
	[DbTable("FI_DOCUMENT_WORKFLOW_DYN")]
	public class Workflowprotokoll
	{
		[DbColumn("MANDT")]
		public string MANDT { get; set; }

		[DbColumn("WI_ID_ORIG")]
		public string WI_ID_ORIG { get; set; }

		[DbColumn("WI_PARENT")]
		public string WI_PARENT { get; set; }

		[DbColumn("WI_ID")]
		public string WI_ID { get; set; }

		[DbColumn("WI_TEXT")]
		public string WI_TEXT { get; set; }

		[DbColumn("WI_STAT_TXT")]
		public string WI_STAT_TXT { get; set; }

		[DbColumn("RESULT_TXT")]
		public string RESULT_TXT { get; set; }

		[DbColumn("WI_CD")]
		public string WI_CD { get; set; }

		[DbColumn("WI_CT")]
		public string WI_CT { get; set; }
	}
}
