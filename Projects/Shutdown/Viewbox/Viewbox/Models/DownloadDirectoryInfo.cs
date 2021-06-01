namespace Viewbox.Models
{
	public class DownloadDirectoryInfo : IDownloadInfo
	{
		public string SourcePath { get; set; }

		public string Name { get; set; }

		public bool IsConvert { get; set; }

		public DownloadDirectoryInfo(string path, string name, bool isConvert = false)
		{
			SourcePath = path;
			Name = name;
			IsConvert = isConvert;
		}
	}
}
