using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using SystemDb;
using AFPConverter;
using Ionic.Zip;
using Ionic.Zlib;
using MerckConverter;
using PayRollConverter;
using Viewbox.Models;
using Viewbox.Properties;

namespace Viewbox.Job
{
	public class DownloadJob : Base
	{
		private static Dictionary<string, DownloadJob> _jobs = new Dictionary<string, DownloadJob>();

		public AFPConverter.Converter AfpConverter { get; set; }

		public global::MerckConverter.Converter MerckConverter { get; set; }

		public PayRollConverter.Converter PayrollConverter { get; set; }

		public int GetDocumentCount { get; set; }

		public int GetCurrentDocumentIndex { get; set; }

		public string ZipFilePath { get; set; }

		private DownloadJob(IEnumerable<IDownloadInfo> downloadFileInfos, IUser user)
		{
			_jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			base.JobFinished += delegate(Base j)
			{
				Janitor.RegisterTempObject(j.Key);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
			};
			StartJob(delegate(object obj, CancellationToken token)
			{
				DownloadFiles((IEnumerable<IDownloadInfo>)((object[])obj)[0], (IUser)((object[])obj)[1]);
			}, new object[2] { downloadFileInfos, user });
			string title = "Download";
			title += ((downloadFileInfos.FirstOrDefault((IDownloadInfo x) => x.IsConvert) != null) ? (" and " + Resources.Conversion) : string.Empty);
			foreach (ILanguage language in ViewboxApplication.Languages)
			{
				base.Descriptions.Add(language.CountryCode, title);
			}
		}

		public static DownloadJob Start(IEnumerable<IDownloadInfo> downloadFileInfos, IUser user)
		{
			return new DownloadJob(downloadFileInfos, user);
		}

		private void DownloadFiles(IEnumerable<IDownloadInfo> downloadFileInfos, IUser user)
		{
			GetDocumentCount = downloadFileInfos.Count((IDownloadInfo x) => x is DownloadFileInfo);
			FileInfo packageFileInfo = new FileInfo(Path.Combine(ViewboxApplication.TemporaryDirectory, user.UserName + "_belegs.zip"));
			if (packageFileInfo.Exists)
			{
				packageFileInfo.Delete();
			}
			using (ZipFile zip = new ZipFile())
			{
				zip.CompressionLevel = CompressionLevel.None;
				foreach (IDownloadInfo downloadInfo in downloadFileInfos)
				{
					if (downloadInfo is DownloadFileInfo)
					{
						DownloadFileInfo downloadFileInfo = downloadInfo as DownloadFileInfo;
						if (ViewboxApplication.PayRollConverter && downloadFileInfo.DownloadFilePath.Contains(".TXT"))
						{
							PayrollConverter = new PayRollConverter.Converter(downloadFileInfo.DownloadFilePath);
							zip.AddFile(PayrollConverter.Start(), "");
						}
						else if (ViewboxApplication.MerckFileMerge)
						{
							if (!File.Exists(downloadFileInfo.ConverterDirectoryPath + downloadFileInfo.Mietvertrag + "\\" + downloadFileInfo.Gjahr + "\\" + downloadFileInfo.DownloadFilePath))
							{
								MerckConverter = new global::MerckConverter.Converter(downloadFileInfo.SourcePath, downloadFileInfo.ConverterDirectoryPath + downloadFileInfo.Mietvertrag + "\\" + downloadFileInfo.Gjahr, downloadFileInfo.Type, downloadFileInfo.DownloadFilePath);
								zip.AddFile(MerckConverter.Start(), "");
							}
							else
							{
								zip.AddFile(downloadFileInfo.ConverterDirectoryPath + downloadFileInfo.Mietvertrag + "\\" + downloadFileInfo.Gjahr + "\\" + downloadFileInfo.DownloadFilePath, "");
							}
						}
						else if (downloadFileInfo.IsConvert)
						{
							AfpConverter = new AFPConverter.Converter(downloadFileInfo.DownloadFilePath, downloadFileInfo.ConverterDirectoryPath, downloadFileInfo.OverlayFilePath, downloadFileInfo.ConversionType, downloadFileInfo.SaveFilePath, downloadFileInfo.Width, downloadFileInfo.Height, downloadFileInfo.PositionX, downloadFileInfo.PositionY, downloadFileInfo.Text, downloadFileInfo.TextPages);
							zip.AddFile(AfpConverter.Start(), "");
						}
						else if (downloadFileInfo.DownloadFileBytesBuffer != null)
						{
							if (!string.IsNullOrEmpty(Path.GetExtension(downloadFileInfo.DownloadFilePath)) && string.IsNullOrEmpty(Path.GetDirectoryName(downloadFileInfo.DownloadFilePath)))
							{
								zip.AddEntry(downloadFileInfo.DownloadFilePath, downloadFileInfo.DownloadFileBytesBuffer);
							}
							else if (!string.IsNullOrEmpty(Path.GetExtension(downloadFileInfo.DownloadFilePath)) && !string.IsNullOrEmpty(Path.GetDirectoryName(downloadFileInfo.DownloadFilePath)))
							{
								zip.AddEntry(new FileInfo(downloadFileInfo.DownloadFilePath).Name, downloadFileInfo.DownloadFileBytesBuffer);
							}
							else if (string.IsNullOrEmpty(Path.GetExtension(downloadFileInfo.DownloadFilePath)) && !string.IsNullOrEmpty(Path.GetDirectoryName(downloadFileInfo.DownloadFilePath)))
							{
								zip.AddEntry(new FileInfo(downloadFileInfo.DownloadFilePath).Name + "." + downloadFileInfo.Extension, downloadFileInfo.DownloadFileBytesBuffer);
							}
							else
							{
								zip.AddEntry(downloadFileInfo.DownloadFilePath + "." + downloadFileInfo.Extension, downloadFileInfo.DownloadFileBytesBuffer);
							}
						}
						else
						{
							zip.AddFile(downloadFileInfo.DownloadFilePath, "");
						}
						GetCurrentDocumentIndex++;
					}
					else
					{
						DownloadDirectoryInfo downloadDirectoryInfo = downloadInfo as DownloadDirectoryInfo;
						zip.AddDirectory(downloadDirectoryInfo.SourcePath, downloadDirectoryInfo.Name);
					}
				}
				zip.Save(packageFileInfo.FullName);
			}
			ZipFilePath = packageFileInfo.FullName;
		}
	}
}
