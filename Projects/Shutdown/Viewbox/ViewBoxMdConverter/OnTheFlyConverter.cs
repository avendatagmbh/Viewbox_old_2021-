using System;
using System.IO;

namespace ViewboxMdConverter
{
	public static class OnTheFlyConverter
	{
		public static void InitNReco()
		{
			ConverterFactory.Instance.GetMdToImageExtension(".html");
		}

		public static bool CreateThumbnail(string originalFilePath, string thumbnailPath, string originalFileExtension)
		{
			try
			{
				if (new FileInfo(originalFilePath).Length > 10485760)
				{
					return false;
				}
				IFileConverter tempFileConverter = ConverterFactory.Instance.GetMdToImageExtension(originalFileExtension.ToLower());
				if (tempFileConverter != null && File.Exists(originalFilePath))
				{
					string dirPath = new FileInfo(thumbnailPath).Directory.FullName;
					if (!Directory.Exists(dirPath))
					{
						Directory.CreateDirectory(dirPath);
					}
					tempFileConverter.Convert(originalFilePath, thumbnailPath);
					return true;
				}
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static bool CreateThumbnailFromMD(string VBDCD, string thumbnailPath, string filerelative)
		{
			if (!string.IsNullOrEmpty(VBDCD))
			{
				string filepath = Path.Combine(VBDCD, filerelative);
				FileInfo fi = new FileInfo(filepath);
				string mdfilepath = filepath.Substring(0, filepath.Length - fi.Extension.Length);
				string metamdfilepath = mdfilepath + ".md";
				if (File.Exists(metamdfilepath) && File.Exists(mdfilepath))
				{
					string mdFileType = GetFileTypeFromMd(metamdfilepath);
					IFileConverter mdToImageExtension = ConverterFactory.Instance.GetMdToImageExtension2(mdFileType);
					CreateRecursiveDirectory(thumbnailPath);
					return mdToImageExtension.Convert(mdfilepath, thumbnailPath);
				}
				if (!File.Exists(metamdfilepath) && File.Exists(mdfilepath))
				{
					IFileConverter mdToImageExtension2 = ConverterFactory.Instance.GetMdToImageExtension2(".pdf");
					CreateRecursiveDirectory(thumbnailPath);
					return mdToImageExtension2.Convert(mdfilepath, thumbnailPath);
				}
			}
			return false;
		}

		private static void CreateRecursiveDirectory(string thumbnailPath)
		{
			FileInfo fi = new FileInfo(thumbnailPath);
			if (fi != null)
			{
				string dirPath = fi.Directory.FullName;
				if (!Directory.Exists(dirPath))
				{
					Directory.CreateDirectory(dirPath);
				}
			}
		}

		public static bool CreateDocument(string originalFilePath, string documentPath, string originalFileExtension)
		{
			try
			{
				IFileConverter tempFileConverter = ConverterFactory.Instance.GetMdToPdfExtension(originalFileExtension.ToLower());
				if (tempFileConverter != null && File.Exists(originalFilePath))
				{
					string dirPath = new FileInfo(documentPath).Directory.FullName;
					if (!Directory.Exists(dirPath))
					{
						Directory.CreateDirectory(dirPath);
					}
					tempFileConverter.Convert(originalFilePath, documentPath);
					return true;
				}
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static string GetFileTypeFromMd(string filefullpath)
		{
			try
			{
				using FileStream file = File.OpenRead(filefullpath);
				using StreamReader stremReader = new StreamReader(file);
				string line;
				while ((line = stremReader.ReadLine()) != null)
				{
					string[] splitted = line.Split('=');
					if (splitted != null && splitted.Length > 1 && splitted[0] != null && splitted[0].ToLower() == "content_type" && splitted[1] != null)
					{
						return splitted[1].Trim();
					}
				}
			}
			catch
			{
			}
			return null;
		}
	}
}
