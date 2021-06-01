namespace Viewbox.Models
{
	public struct LotusAttachment
	{
		public string FileName { get; set; }

		public string Path { get; set; }

		public LotusAttachment(string filename, string path)
		{
			this = default(LotusAttachment);
			FileName = filename;
			Path = path;
		}
	}
}
