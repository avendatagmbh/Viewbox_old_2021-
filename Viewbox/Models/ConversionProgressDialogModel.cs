using Viewbox.Job;

namespace Viewbox.Models
{
	public class ConversionProgressDialogModel
	{
		public DownloadJob DownLoadJob { get; set; }

		public string Title { get; internal set; }

		public string Key { get; internal set; }

		public string Link { get; internal set; }
	}
}
