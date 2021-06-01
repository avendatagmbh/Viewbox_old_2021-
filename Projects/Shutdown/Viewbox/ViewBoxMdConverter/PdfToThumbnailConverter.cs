using System;
using System.IO;
using GhostscriptSharp;

namespace ViewboxMdConverter
{
	internal class PdfToThumbnailConverter : GeneralConverter, IFileConverter
	{
		public bool Convert(string input, string output)
		{
			if (!File.Exists(input))
			{
				throw new Exception("File not exist: " + input);
			}
			GhostscriptWrapper.ConvertPdfToThumbs(input, output, 1, 700, 990);
			return false;
		}
	}
}
