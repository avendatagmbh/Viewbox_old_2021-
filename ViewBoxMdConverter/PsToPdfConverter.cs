using System;
using System.IO;
using GhostscriptSharp;

namespace ViewboxMdConverter
{
	internal class PsToPdfConverter : GeneralConverter, IFileConverter
	{
		public bool Convert(string input, string output)
		{
			if (!File.Exists(input))
			{
				throw new Exception("File not exist: " + input);
			}
			GhostscriptWrapper.ConvertPsToPdf(input, output);
			return true;
		}
	}
}
