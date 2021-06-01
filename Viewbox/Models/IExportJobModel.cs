using System.Collections.Generic;
using Viewbox.Job;

namespace Viewbox.Models
{
	public interface IExportJobModel
	{
		IEnumerable<Export> ExportJobs { get; }

		string LabelRuntime(Export job);
	}
}
