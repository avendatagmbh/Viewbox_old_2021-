namespace Viewbox.Models
{
	public interface IDownloadInfo
	{
		bool IsConvert { get; set; }

		string SourcePath { get; set; }
	}
}
