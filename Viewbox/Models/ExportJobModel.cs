using System;
using System.Collections.Generic;
using Viewbox.Job;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class ExportJobModel : BaseModel, IExportJobModel
	{
		private IEnumerable<Export> _exportJobs;

		public IEnumerable<Export> ExportJobs
		{
			get
			{
				return (_exportJobs == null) ? Export.List : _exportJobs;
			}
			internal set
			{
				_exportJobs = value;
			}
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
			return $"> {job.Runtime.Days}";
		}
	}
}
