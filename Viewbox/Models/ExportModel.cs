using System;
using System.Collections.Generic;
using SystemDb;
using Viewbox.Job;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class ExportModel : ViewboxModel, IExportJobModel
	{
		public string Caption => Resources.Export;

		public bool Finished { get; private set; }

		public TableType CurrentType { get; private set; }

		public int ExpandedTableId { get; private set; }

		public ICategoryCollection Categories { get; private set; }

		public override string LabelCaption => Resources.Export;

		public bool ShowOtherList { get; private set; }

		public IEnumerable<Export> ExportJobs => Export.List;

		public ExportModel(TableType type, ICategoryCollection list, ICategoryCollection otherList, int table_id = -1, bool finished = false)
		{
			CurrentType = type;
			Categories = list;
			ExpandedTableId = table_id;
			Finished = finished;
			ShowOtherList = otherList.Count > 0;
		}

		public string LabelRuntime(Export job)
		{
			if (job.Runtime < TimeSpan.FromMinutes(1.0))
			{
				return $"{job.Runtime.Seconds}{Resources.Seconds}.";
			}
			if (job.Runtime < TimeSpan.FromHours(1.0))
			{
				return string.Format("{1}{3}. {0}{2}.", job.Runtime.Seconds, job.Runtime.Minutes, Resources.Seconds, Resources.Minutes);
			}
			if (job.Runtime < TimeSpan.FromDays(1.0))
			{
				return string.Format("{1}{3}. {0}{2}.", job.Runtime.Minutes, job.Runtime.Hours, Resources.Minutes, Resources.Hours);
			}
			return string.Format("> {0} {2}", job.Runtime.Days, Resources.Days);
		}
	}
}
