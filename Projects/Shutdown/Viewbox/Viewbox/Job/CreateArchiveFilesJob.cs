using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using SystemDb;
using AV.Log;
using log4net;

namespace Viewbox.Job
{
	public class CreateArchiveFilesJob : Base
	{
		private static readonly Dictionary<string, CreateArchiveFilesJob> _jobs = new Dictionary<string, CreateArchiveFilesJob>();

		private CreateArchiveFilesJob(string system)
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
				DoCheckFilesAgainstDatabase((string)((object[])obj)[0]);
			}, new object[1] { system });
			foreach (ILanguage language in ViewboxApplication.Languages)
			{
				base.Descriptions.Add(language.CountryCode, "Copy archve files");
			}
		}

		private CreateArchiveFilesJob(int something, string system)
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
				DoCheckForFiles((string)((object[])obj)[0]);
			}, new object[1] { system });
			foreach (ILanguage language in ViewboxApplication.Languages)
			{
				base.Descriptions.Add(language.CountryCode, "Check archive files");
			}
		}

		private CreateArchiveFilesJob(int something, int other, string system)
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
				DoCheckThumnails((string)((object[])obj)[0]);
			}, new object[1] { system });
			foreach (ILanguage language in ViewboxApplication.Languages)
			{
				base.Descriptions.Add(language.CountryCode, "Check archive files");
			}
		}

		private CreateArchiveFilesJob(int something, int otherthing)
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
			StartJob(delegate
			{
				DoCreateThumbnails();
			}, new object());
			foreach (ILanguage language in ViewboxApplication.Languages)
			{
				base.Descriptions.Add(language.CountryCode, "Check archive files");
			}
		}

		public static CreateArchiveFilesJob Create(string system)
		{
			return new CreateArchiveFilesJob(system);
		}

		public static CreateArchiveFilesJob Create(int something, string system)
		{
			return new CreateArchiveFilesJob(something, system);
		}

		public static CreateArchiveFilesJob Create(int something, int other, string system)
		{
			return new CreateArchiveFilesJob(something, other, system);
		}

		public static CreateArchiveFilesJob Create(int something, int otherthing)
		{
			return new CreateArchiveFilesJob(something, otherthing);
		}

		private void DoCheckFilesAgainstDatabase(string system)
		{
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				try
				{
					DirectoryInfo di = new DirectoryInfo(ViewboxApplication.DocumentsDirectory);
					FileInfo[] files = di.GetFiles();
					foreach (FileInfo fileInfo in files)
					{
						if (!IsFileInArchiveList(system, fileInfo.Name, "path"))
						{
							_log.Info($"File not in database: {fileInfo.Name}");
						}
					}
					if (ViewboxApplication.DocumentsDirectory1 != null)
					{
						di = new DirectoryInfo(ViewboxApplication.DocumentsDirectory1);
						FileInfo[] files2 = di.GetFiles();
						foreach (FileInfo fileInfo4 in files2)
						{
							if (!IsFileInArchiveList(system, fileInfo4.Name, "path"))
							{
								_log.Info($"File not in database: {fileInfo4.Name}");
							}
						}
					}
					if (ViewboxApplication.DocumentsDirectory2 != null)
					{
						di = new DirectoryInfo(ViewboxApplication.DocumentsDirectory2);
						FileInfo[] files3 = di.GetFiles();
						foreach (FileInfo fileInfo3 in files3)
						{
							if (!IsFileInArchiveList(system, fileInfo3.Name, "path"))
							{
								_log.Info($"File not in database: {fileInfo3.Name}");
							}
						}
					}
					if (ViewboxApplication.DocumentsDirectory3 != null)
					{
						di = new DirectoryInfo(ViewboxApplication.DocumentsDirectory3);
						FileInfo[] files4 = di.GetFiles();
						foreach (FileInfo fileInfo2 in files4)
						{
							if (!IsFileInArchiveList(system, fileInfo2.Name, "path"))
							{
								_log.Info($"File not in database: {fileInfo2.Name}");
							}
						}
					}
					di = new DirectoryInfo(ViewboxApplication.ThumbnailsDirectory);
					FileInfo[] files5 = di.GetFiles();
					foreach (FileInfo thumbnail in files5)
					{
						if (!IsFileInArchiveList(system, thumbnail.Name, "thumbnail_path"))
						{
							_log.Info($"File not in database: {thumbnail.Name}");
						}
					}
					if (ViewboxApplication.ThumbnailsDirectory1 != null)
					{
						di = new DirectoryInfo(ViewboxApplication.ThumbnailsDirectory1);
						FileInfo[] files6 = di.GetFiles();
						foreach (FileInfo thumbnail4 in files6)
						{
							if (!IsFileInArchiveList(system, thumbnail4.Name, "thumbnail_path"))
							{
								_log.Info($"File not in database: {thumbnail4.Name}");
							}
						}
					}
					if (ViewboxApplication.ThumbnailsDirectory2 != null)
					{
						di = new DirectoryInfo(ViewboxApplication.ThumbnailsDirectory2);
						FileInfo[] files7 = di.GetFiles();
						foreach (FileInfo thumbnail3 in files7)
						{
							if (!IsFileInArchiveList(system, thumbnail3.Name, "thumbnail_path"))
							{
								_log.Info($"File not in database: {thumbnail3.Name}");
							}
						}
					}
					if (ViewboxApplication.ThumbnailsDirectory3 == null)
					{
						return;
					}
					di = new DirectoryInfo(ViewboxApplication.ThumbnailsDirectory3);
					FileInfo[] files8 = di.GetFiles();
					foreach (FileInfo thumbnail2 in files8)
					{
						if (!IsFileInArchiveList(system, thumbnail2.Name, "thumbnail_path"))
						{
							_log.Info($"File not in database: {thumbnail2.Name}");
						}
					}
				}
				catch (Exception ex)
				{
					_log.Error(ex.Message);
					throw;
				}
			}
		}

		private bool IsFileInArchiveList(string system, string fileName, string column)
		{
			return ViewboxApplication.Database.SystemDb.FileInArchiveData(system, fileName, column) > 0;
		}

		private void DoCheckForFiles(string system)
		{
			ViewboxApplication.Database.SystemDb.CheckFilesInArchiveData(system, ViewboxApplication.DocumentsDirectory, ViewboxApplication.ThumbnailsDirectory);
			if (ViewboxApplication.DocumentsDirectory1 != null && ViewboxApplication.ThumbnailsDirectory1 != null)
			{
				ViewboxApplication.Database.SystemDb.CheckFilesInArchiveData(system, ViewboxApplication.DocumentsDirectory1, ViewboxApplication.ThumbnailsDirectory1);
			}
			if (ViewboxApplication.DocumentsDirectory2 != null && ViewboxApplication.ThumbnailsDirectory2 != null)
			{
				ViewboxApplication.Database.SystemDb.CheckFilesInArchiveData(system, ViewboxApplication.DocumentsDirectory2, ViewboxApplication.ThumbnailsDirectory2);
			}
			if (ViewboxApplication.DocumentsDirectory3 != null && ViewboxApplication.ThumbnailsDirectory3 != null)
			{
				ViewboxApplication.Database.SystemDb.CheckFilesInArchiveData(system, ViewboxApplication.DocumentsDirectory3, ViewboxApplication.ThumbnailsDirectory3);
			}
		}

		private void DoCheckThumnails(string system)
		{
			ViewboxApplication.Database.SystemDb.RenameThumbnailFiles(system, ViewboxApplication.DocumentsDirectory, ViewboxApplication.ThumbnailsDirectory);
			if (ViewboxApplication.DocumentsDirectory1 != null && ViewboxApplication.ThumbnailsDirectory1 != null)
			{
				ViewboxApplication.Database.SystemDb.RenameThumbnailFiles(system, ViewboxApplication.DocumentsDirectory1, ViewboxApplication.ThumbnailsDirectory1);
			}
			if (ViewboxApplication.DocumentsDirectory2 != null && ViewboxApplication.ThumbnailsDirectory2 != null)
			{
				ViewboxApplication.Database.SystemDb.RenameThumbnailFiles(system, ViewboxApplication.DocumentsDirectory2, ViewboxApplication.ThumbnailsDirectory2);
			}
			if (ViewboxApplication.DocumentsDirectory3 != null && ViewboxApplication.ThumbnailsDirectory3 != null)
			{
				ViewboxApplication.Database.SystemDb.RenameThumbnailFiles(system, ViewboxApplication.DocumentsDirectory3, ViewboxApplication.ThumbnailsDirectory3);
			}
		}

		private void DoCreateThumbnails()
		{
			if (!Directory.Exists(ViewboxApplication.DocumentsDirectory))
			{
				return;
			}
			if (!Directory.Exists(ViewboxApplication.ThumbnailsDirectory))
			{
				Directory.CreateDirectory(ViewboxApplication.ThumbnailsDirectory);
			}
			if (ViewboxApplication.ThumbnailsDirectory1 != null && !Directory.Exists(ViewboxApplication.ThumbnailsDirectory1))
			{
				Directory.CreateDirectory(ViewboxApplication.ThumbnailsDirectory1);
			}
			if (ViewboxApplication.ThumbnailsDirectory2 != null && !Directory.Exists(ViewboxApplication.ThumbnailsDirectory2))
			{
				Directory.CreateDirectory(ViewboxApplication.ThumbnailsDirectory2);
			}
			if (ViewboxApplication.ThumbnailsDirectory3 != null && !Directory.Exists(ViewboxApplication.ThumbnailsDirectory3))
			{
				Directory.CreateDirectory(ViewboxApplication.ThumbnailsDirectory3);
			}
			DirectoryInfo documentsDirectory = new DirectoryInfo(ViewboxApplication.DocumentsDirectory);
			DirectoryInfo thumbnailsDirectory = new DirectoryInfo(ViewboxApplication.ThumbnailsDirectory);
			DirectoryInfo thumbnailsDirectory2 = null;
			DirectoryInfo thumbnailsDirectory3 = null;
			DirectoryInfo thumbnailsDirectory4 = null;
			List<DirectoryInfo> dis = new List<DirectoryInfo>();
			dis.Add(documentsDirectory);
			if (ViewboxApplication.DocumentsDirectory1 != null)
			{
				DirectoryInfo documentsDirectory2 = new DirectoryInfo(ViewboxApplication.DocumentsDirectory1);
				dis.Add(documentsDirectory2);
			}
			if (ViewboxApplication.DocumentsDirectory2 != null)
			{
				DirectoryInfo documentsDirectory3 = new DirectoryInfo(ViewboxApplication.DocumentsDirectory2);
				dis.Add(documentsDirectory3);
			}
			if (ViewboxApplication.DocumentsDirectory3 != null)
			{
				DirectoryInfo documentsDirectory4 = new DirectoryInfo(ViewboxApplication.DocumentsDirectory3);
				dis.Add(documentsDirectory4);
			}
			if (ViewboxApplication.ThumbnailsDirectory1 != null)
			{
				thumbnailsDirectory2 = new DirectoryInfo(ViewboxApplication.ThumbnailsDirectory1);
			}
			if (ViewboxApplication.ThumbnailsDirectory2 != null)
			{
				thumbnailsDirectory3 = new DirectoryInfo(ViewboxApplication.ThumbnailsDirectory2);
			}
			if (ViewboxApplication.ThumbnailsDirectory3 != null)
			{
				thumbnailsDirectory4 = new DirectoryInfo(ViewboxApplication.ThumbnailsDirectory3);
			}
			foreach (DirectoryInfo item in dis)
			{
				FileInfo[] files = item.GetFiles();
				foreach (FileInfo docFile in files)
				{
					if (string.CompareOrdinal(docFile.Extension.ToLower(), ".fax") == 0)
					{
						string newFileName = null;
						if (ViewboxApplication.DocumentsDirectory != null && item.FullName.Contains(ViewboxApplication.DocumentsDirectory))
						{
							newFileName = string.Concat(thumbnailsDirectory, docFile.Name.Replace(".fax", ".tif").Replace(".FAX", ".tif"));
						}
						else if (ViewboxApplication.DocumentsDirectory1 != null && item.FullName.Contains(ViewboxApplication.DocumentsDirectory1))
						{
							newFileName = string.Concat(thumbnailsDirectory2, docFile.Name.Replace(".fax", ".tif").Replace(".FAX", ".tif"));
						}
						else if (ViewboxApplication.DocumentsDirectory2 != null && item.FullName.Contains(ViewboxApplication.DocumentsDirectory2))
						{
							newFileName = string.Concat(thumbnailsDirectory3, docFile.Name.Replace(".fax", ".tif").Replace(".FAX", ".tif"));
						}
						else if (ViewboxApplication.DocumentsDirectory3 != null && item.FullName.Contains(ViewboxApplication.DocumentsDirectory3))
						{
							newFileName = string.Concat(thumbnailsDirectory4, docFile.Name.Replace(".fax", ".tif").Replace(".FAX", ".tif"));
						}
						if (File.Exists(newFileName))
						{
							File.Delete(newFileName);
						}
						FileInfo tifFile = docFile.CopyTo(newFileName);
						using Image jpeg = ImageConverter.ConvertToThumbnail(tifFile.FullName);
						jpeg.Save(newFileName.Replace(".tif", ".jpg"), ImageFormat.Jpeg);
						File.Delete(newFileName);
					}
					else if (string.CompareOrdinal(docFile.Extension.ToLower(), ".pdf") == 0)
					{
						_log.Info("Pdf file found, please use the command line utility.");
					}
				}
			}
		}
	}
}
