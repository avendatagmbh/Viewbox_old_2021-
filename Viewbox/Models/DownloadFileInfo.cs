namespace Viewbox.Models
{
	public class DownloadFileInfo : IDownloadInfo
	{
		public string DownloadFilePath { get; set; }

		public byte[] DownloadFileBytesBuffer { get; set; }

		public string ConverterDirectoryPath { get; set; }

		public string OverlayFilePath { get; set; }

		public string ConversionType { get; set; }

		public string SaveFilePath { get; set; }

		public int Width { get; set; }

		public int Height { get; set; }

		public int PositionX { get; set; }

		public int PositionY { get; set; }

		public string Text { get; set; }

		public string TextPages { get; set; }

		public bool IsConvert { get; set; }

		public string Extension { get; set; }

		public string Type { get; set; }

		public string Mietvertrag { get; set; }

		public string Gjahr { get; set; }

		public string SourcePath { get; set; }

		public DownloadFileInfo(string downloadFilePath, string converterDirectory, string overlayFilePath, string conversionType, string saveFilePath, int width = 1250, int height = 900, int positionX = 300, int positionY = 85, string text = "", string textPages = "", bool isConvert = false, string type = "", string mietvertrag = "", string gjahr = "", string sourcePath = "")
		{
			DownloadFilePath = downloadFilePath;
			ConverterDirectoryPath = converterDirectory;
			OverlayFilePath = overlayFilePath;
			ConversionType = conversionType;
			SaveFilePath = saveFilePath;
			Width = width;
			Height = height;
			PositionX = positionX;
			PositionY = positionY;
			Text = text;
			TextPages = textPages;
			IsConvert = isConvert;
			Type = type;
			Mietvertrag = mietvertrag;
			Gjahr = gjahr;
			SourcePath = sourcePath;
		}

		public DownloadFileInfo(string downloadFilePath, byte[] bytesBuffer, string extension)
		{
			DownloadFilePath = downloadFilePath;
			DownloadFileBytesBuffer = bytesBuffer;
			Extension = extension;
		}
	}
}
