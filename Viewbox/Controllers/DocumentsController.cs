using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using SystemDb;
using SystemDb.Internal;
using AFPConverter;
using ArchiveWebServiceInterface;
using AV.Log;
using DbAccess;
using Ionic.Zip;
using MerckConverter;
using PayRollConverter;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Properties;
using ViewboxDb;
using ViewboxDb.Filters;
using ViewboxMdConverter;

namespace Viewbox.Controllers
{
	[Authorize]
	[ViewboxFilter]
	public class DocumentsController : BaseController
	{
		public void FilterDelete()
		{
			ViewboxDb.TableObject obj = ViewboxSession.TempTableObjects.LastOrDefault();
			if (obj != null && obj.Filter != null)
			{
				obj.Filter = null;
			}
		}

		public ActionResult Index(int id = 0, long start = 0L)
		{
			IOptimization opt = ViewboxSession.Optimizations.FirstOrDefault();
			if (opt == null || (id >= 0 && ViewboxApplication.HideDocumentsMenu) || ViewboxSession.Archives.Count == 0 || ViewboxSession.Archives.All((IArchive a) => a.Database.ToLower() != opt.FindValue(OptimizationType.System).ToLower()))
			{
				return RedirectToAction("Index", "Home");
			}
			ViewboxSession.ClearSavedDocumentSettings("Documents");
			ITableObject currentArchive = ((id >= 0) ? ViewboxSession.TableObjects[id] : ((ViewboxSession.TempTableObjects[id] == null) ? null : ViewboxSession.TempTableObjects[id].Table));
			IArchive optArchive = ViewboxSession.Archives.FirstOrDefault((IArchive a) => a.Database.ToLower() == opt.FindValue(OptimizationType.System).ToLower());
			if (currentArchive != null && optArchive != null)
			{
				if (currentArchive.Database.ToLower() != optArchive.Database.ToLower())
				{
					id = optArchive.Id;
				}
			}
			else if (currentArchive == null && optArchive != null)
			{
				id = optArchive.Id;
			}
			DocumentsModel model = new DocumentsModel();
			model.TempId = id;
			model.OriginalTable = ((id >= 0) ? ViewboxSession.TableObjects[id] : ((ViewboxSession.TempTableObjects[id] == null) ? null : ViewboxSession.TempTableObjects[id].Table));
			model.RowsPerPage = 35;
			model.FromRow = start;
			PrepareParameters(model);
			if (id >= 0)
			{
				IArchive archive = ViewboxSession.Archives.Where((IArchive a) => a.Database.ToLower() == ViewboxSession.SelectedSystem.ToLower()).FirstOrDefault();
				IArchive archive3 = (model.Table = archive);
				if (archive3 != null)
				{
					if (!ViewboxSession.TableObjects.Contains(archive.Id))
					{
						ViewboxSession.AddTableObject(archive.Id);
					}
					ViewboxSession.SetupTableColumns(archive.Id);
					if (model.Table.Columns.Contains("archive_file"))
					{
						model.IsZipArchive = true;
						ExtractZipFile(ViewboxApplication.Database.GetSingleArchiveValue(archive, "archive_file"), ViewboxApplication.Database.DecryptArchivePassword(archive, ViewboxApplication.Database.GetSingleArchiveValue(archive, "archive_password"), ViewboxApplication.ArchiveSecurityKey), model);
					}
					else
					{
						model.Documents = ViewboxApplication.Database.GetArchiveData(model.Table, ViewboxSession.Optimizations.Last(), null, start);
					}
					model.RowsCount = ViewboxSession.GetDataCount(model.Table);
				}
			}
			else if (ViewboxSession.TempTableObjects[id] != null)
			{
				Janitor.RegisterTempObject(ViewboxSession.TempTableObjects[id].Key);
				model.Table = ViewboxSession.TempTableObjects[id].OriginalTable as IArchive;
				string sql = string.Empty;
				model.Documents = ViewboxApplication.Database.GetArchiveData(ViewboxSession.TempTableObjects[id], ViewboxSession.Optimizations.Last(), ref sql, start);
				model.RowsCount = ViewboxApplication.Database.GetArchiveDataRowCount(sql);
				bool likeFilter = false;
				bool hasParameter = false;
				AndFilter filter = ViewboxSession.TempTableObjects[id].Filter as AndFilter;
				if (filter != null && model.FilterColumns != null)
				{
					int i;
					for (i = 0; i < model.FilterColumns.Length; i++)
					{
						ColValueFilter colFilter = filter.Conditions.FirstOrDefault((IFilter w) => w is ColValueFilter && ((ColValueFilter)w).Column.Name == model.FilterColumns[i].Name) as ColValueFilter;
						model.ParameterValues[i] = ((colFilter == null || colFilter.Value == null) ? "" : colFilter.Value.ToString());
						if (i == 2 && colFilter != null && colFilter.Value != null && colFilter.Op == Operators.Equal)
						{
							string valueBefore = colFilter.Value.ToString();
							try
							{
								if (DateTime.TryParse(DateTime.ParseExact(colFilter.Value.ToString(), "yyyyMMddHHmmss", null).ToShortDateString(), out var dateValue))
								{
									model.ParameterValues[i] = dateValue.ToString("dd.MM.yyyy");
								}
							}
							catch (Exception)
							{
								model.ParameterValues[i] = valueBefore;
							}
						}
						hasParameter |= !string.IsNullOrEmpty(model.ParameterValues[i]);
						if (colFilter != null && colFilter.Op == Operators.Like)
						{
							model.ParameterValues[i] = model.ParameterValues[i].TrimStart('%');
							model.ParameterValues[i] = model.ParameterValues[i].TrimEnd('%');
							likeFilter = true;
						}
					}
				}
				model.ExactMatch = hasParameter && !likeFilter;
				foreach (IColumn col in model.Table.Columns)
				{
					col.IsVisible = ViewboxApplication.Database.SystemDb.Columns[col.Id].IsVisible;
				}
			}
			if (model.RowsCount == 0L && base.Request != null && base.Request.UrlReferrer != null && !string.IsNullOrEmpty(base.Request.UrlReferrer.AbsoluteUri))
			{
				if (ViewboxSession.HasOptChanged)
				{
					if (id < 0)
					{
						ViewboxSession.SetOptimization(ViewboxSession.TempTableObjects[id].Optimization.Id);
					}
					else
					{
						ViewboxSession.SetOptimization(ViewboxSession.LastDocOpt);
					}
				}
				if (ViewboxApplication.HideDocumentsMenu)
				{
					throw new HttpException(404, "HTTP/1.1 404 Not Found");
				}
				string lastUrl = base.Request.UrlReferrer.AbsoluteUri;
				if (!lastUrl.Contains("?isNoResult=true"))
				{
					lastUrl += "?isNoResult=true";
				}
				return Redirect(lastUrl);
			}
			if (ViewboxApplication.HideDocumentsMenu)
			{
				DataRow firstRow = model.Documents.Rows[0];
				int ord = 1;
				string filename = null;
				string reverse = null;
				foreach (IColumn column in model.Table.Columns)
				{
					string value = firstRow[ord].ToString();
					if (firstRow[ord] == DBNull.Value)
					{
						value = "";
					}
					if (column.Name.ToLower() == "path")
					{
						filename = value;
					}
					else if (column.Name.ToLower() == "reverse")
					{
						reverse = value;
					}
					ord++;
				}
				ViewboxSession.UpdateSavedDocumentSettings(filename, reverse);
				return DownloadByVirtualId(0);
			}
			ViewboxSession.LastDocOpt = ViewboxSession.LastOpt;
			return View(model);
		}

		public ActionResult SortAndFilter(string[] parameterValues, bool ExactMatch, int sortColumnId = 0, SortDirection direction = SortDirection.Ascending)
		{
			IOptimization opt = ViewboxSession.Optimizations.FirstOrDefault();
			if (opt == null || ViewboxApplication.HideDocumentsMenu || ViewboxSession.Archives.Count == 0 || ViewboxSession.Archives.All((IArchive a) => a.Database.ToLower() != opt.FindValue(OptimizationType.System).ToLower()))
			{
				return RedirectToAction("Index", "Home");
			}
			IArchive archive = ViewboxSession.Archives.Where((IArchive a) => a.Database.ToLower() == ViewboxSession.SelectedSystem.ToLower()).FirstOrDefault();
			DocumentsModel model = new DocumentsModel();
			PrepareParameters(model);
			bool hasParameter = false;
			if (model.FilterColumns != null)
			{
				for (int i = 0; i < parameterValues.Length; i++)
				{
					parameterValues[i] = ((parameterValues[i] == null || parameterValues[i] == model.FilterColumnDescriptions[i]) ? "" : parameterValues[i]);
					hasParameter = hasParameter || !string.IsNullOrEmpty(parameterValues[i]);
				}
			}
			SortCollection sort = new SortCollection();
			if (sortColumnId > 0)
			{
				sort.Add(new Sort
				{
					cid = sortColumnId,
					dir = direction
				});
			}
			string filter = ViewboxApplication.Database.PrepareArchiveDataParameters(parameterValues, ExactMatch, ViewboxSession.SelectedSystem);
			filter = filter.Replace("\\", "");
			return RedirectToAction("SortAndFilter", "DataGrid", new
			{
				id = archive.Id,
				filter = filter,
				canReturnDialogPartial = false,
				sortListString = ((sortColumnId > 0) ? (sortColumnId + "|" + (int)direction) : "")
			});
		}

		private void PrepareParameters(DocumentsModel model)
		{
			IArchive archive = ViewboxApplication.Database.SystemDb.Archives.FirstOrDefault((IArchive a) => string.CompareOrdinal(a.Database.ToLower(), ViewboxSession.SelectedSystem.ToLower()) == 0);
			if (archive == null)
			{
				return;
			}
			using DatabaseBase connection = ViewboxApplication.Database.SystemDb.ConnectionManager.GetConnection();
			List<ColumnFilters> filterColumns = connection.DbMapping.Load<ColumnFilters>("table_id = " + archive.Id);
			model.FilterColumns = new IColumn[filterColumns.Count];
			int i = 0;
			foreach (ColumnFilters relationVisibleColumn in filterColumns)
			{
				IColumn relationColumn = ViewboxApplication.Database.SystemDb.Columns.FirstOrDefault((IColumn w) => w.Id == relationVisibleColumn.ColumnId);
				model.FilterColumns[i] = relationColumn;
				i++;
			}
			model.FilterColumnDescriptions = model.FilterColumns.Select((IColumn w) => w.GetDescription()).ToArray();
			model.ParameterValues = new string[model.FilterColumns.Length];
		}

		private Dictionary<string, List<string>> GetAdditionalInformation(IArchive archive, IOptimization opt)
		{
			Dictionary<string, List<string>> additionalDictionary = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			foreach (IArchiveSetting setting in from a in archive.Settings
				orderby a.Key, a.Ordinal
				select a)
			{
				if (additionalDictionary.ContainsKey(setting.Key))
				{
					additionalDictionary[setting.Key].Add(setting.Value);
					continue;
				}
				additionalDictionary.Add(setting.Key, new List<string> { setting.Value });
			}
			string indexValue = opt.FindValue(OptimizationType.IndexTable);
			string splitValue = opt.FindValue(OptimizationType.SplitTable);
			string sortValue = opt.FindValue(OptimizationType.SortColumn);
			if (!string.IsNullOrEmpty(indexValue))
			{
				additionalDictionary.Add("indexValue", new List<string> { indexValue });
			}
			if (!string.IsNullOrEmpty(splitValue))
			{
				additionalDictionary.Add("splitValue", new List<string> { splitValue });
			}
			if (!string.IsNullOrEmpty(sortValue))
			{
				additionalDictionary.Add("sortValue", new List<string> { sortValue });
			}
			if (ViewboxSession.User != null)
			{
				additionalDictionary.Add("userId", new List<string> { ViewboxSession.User.Id.ToString() });
			}
			additionalDictionary.Add("tableId", new List<string> { archive.Id.ToString() });
			return additionalDictionary;
		}

		[HttpPost]
		public FileResult Download(string filename, string downloadName)
		{
			if (filename.ToLower().Contains(".fax"))
			{
				string pdfFileName = filename.Replace(".FAX", ".pdf").Replace(".fax", ".pdf");
				if (System.IO.File.Exists(pdfFileName))
				{
					return File(pdfFileName, "application/octet-stream", downloadName.Replace(".FAX", ".pdf").Replace(".fax", ".pdf"));
				}
			}
			try
			{
				return File(filename, "application/octet-stream", downloadName);
			}
			catch
			{
				throw new HttpException(404, "Not found");
			}
		}

		[HttpPost]
		public FileResult DownloadByFileName(string downloadName)
		{
			if (downloadName.StartsWith("http:") || downloadName.StartsWith("https:"))
			{
				using (WebClient wc = new WebClient())
				{
					if (ViewboxApplication.ArchiveAuthentication)
					{
						wc.Credentials = new NetworkCredential(ViewboxApplication.ArchiveAuthenticationUsername, ViewboxApplication.ArchiveAuthenticationPassword);
					}
					try
					{
						byte[] byteArr = wc.DownloadData(downloadName);
						return File(byteArr, "application/octet-stream", downloadName);
					}
					catch
					{
						throw new HttpException(404, "Not found");
					}
				}
			}
			string filename = string.Empty;
			if (System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory, downloadName)))
			{
				filename = Path.Combine(ViewboxApplication.DocumentsDirectory, downloadName);
			}
			else if (ViewboxApplication.DocumentsDirectory1 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory1, downloadName)))
			{
				filename = Path.Combine(ViewboxApplication.DocumentsDirectory1, downloadName);
			}
			else if (ViewboxApplication.DocumentsDirectory2 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory2, downloadName)))
			{
				filename = Path.Combine(ViewboxApplication.DocumentsDirectory2, downloadName);
			}
			else if (ViewboxApplication.DocumentsDirectory3 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory3, downloadName)))
			{
				filename = Path.Combine(ViewboxApplication.DocumentsDirectory3, downloadName);
			}
			if (ViewboxApplication.IsOnTheFlyConvertion && string.IsNullOrWhiteSpace(filename) && !string.IsNullOrWhiteSpace(ViewboxApplication.DocumentsConfigDirectory) && !string.IsNullOrWhiteSpace(downloadName))
			{
				string path3 = Path.Combine(ViewboxApplication.DocumentsConfigDirectory, downloadName);
				filename = CreateOnTheFileConverter(downloadName, path3);
			}
			if (ViewboxApplication.IsOnTheFlyConvertion && string.IsNullOrWhiteSpace(filename) && !string.IsNullOrWhiteSpace(ViewboxApplication.DocumentsConfigDirectory1) && !string.IsNullOrWhiteSpace(downloadName))
			{
				string path2 = Path.Combine(ViewboxApplication.DocumentsConfigDirectory1, downloadName);
				filename = CreateOnTheFileConverter(downloadName, path2);
			}
			if (ViewboxApplication.IsOnTheFlyConvertion && string.IsNullOrWhiteSpace(filename) && !string.IsNullOrWhiteSpace(ViewboxApplication.DocumentsConfigDirectory2) && !string.IsNullOrWhiteSpace(downloadName))
			{
				string path = Path.Combine(ViewboxApplication.DocumentsConfigDirectory2, downloadName);
				filename = CreateOnTheFileConverter(downloadName, path);
			}
			if (System.IO.File.Exists(filename))
			{
				return File(filename, "application/octet-stream", downloadName);
			}
			throw new HttpException(404, "Not found");
		}

		[HttpPost]
		public FileResult DownloadByVirtualId(int id)
		{
			List<DocumentFileModel> list = ViewboxSession.SavedDocumentSettings;
			if (list != null && id >= 0 && list.Count > id)
			{
				DocumentFileModel item = list[id];
				string downloadName = item.Url;
				if (downloadName.StartsWith("http:") || downloadName.StartsWith("https:"))
				{
					using WebClient wc = new WebClient();
					if (ViewboxApplication.ArchiveAuthentication)
					{
						wc.Credentials = new NetworkCredential(ViewboxApplication.ArchiveAuthenticationUsername, ViewboxApplication.ArchiveAuthenticationPassword);
					}
					try
					{
						byte[] byteArr = wc.DownloadData(downloadName);
						if (!string.IsNullOrWhiteSpace(item.Reverse))
						{
							string ext = item.Reverse.Trim().ToLower();
							if (!ext.StartsWith("."))
							{
								ext = "." + ext;
							}
							downloadName = "Document" + ext;
						}
						else
						{
							downloadName = "Document";
						}
						base.Response.AddHeader("Content-Disposition", new ContentDisposition
						{
							FileName = downloadName,
							Inline = true
						}.ToString());
						return File(byteArr, GetMimeType(item.Reverse));
					}
					catch
					{
						LogHelper.GetLogger().Error($"Url is not available: {downloadName}");
						throw new HttpException(404, "Not found");
					}
				}
				string filename = string.Empty;
				if (System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory, downloadName)))
				{
					filename = Path.Combine(ViewboxApplication.DocumentsDirectory, downloadName);
				}
				else if (ViewboxApplication.DocumentsDirectory1 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory1, downloadName)))
				{
					filename = Path.Combine(ViewboxApplication.DocumentsDirectory1, downloadName);
				}
				else if (ViewboxApplication.DocumentsDirectory2 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory2, downloadName)))
				{
					filename = Path.Combine(ViewboxApplication.DocumentsDirectory2, downloadName);
				}
				else if (ViewboxApplication.DocumentsDirectory3 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory3, downloadName)))
				{
					filename = Path.Combine(ViewboxApplication.DocumentsDirectory3, downloadName);
				}
				if (ViewboxApplication.IsOnTheFlyConvertion && string.IsNullOrWhiteSpace(filename) && !string.IsNullOrWhiteSpace(ViewboxApplication.DocumentsConfigDirectory) && !string.IsNullOrWhiteSpace(downloadName))
				{
					string path3 = Path.Combine(ViewboxApplication.DocumentsConfigDirectory, downloadName);
					filename = CreateOnTheFileConverter(downloadName, path3);
				}
				if (ViewboxApplication.IsOnTheFlyConvertion && string.IsNullOrWhiteSpace(filename) && !string.IsNullOrWhiteSpace(ViewboxApplication.DocumentsDirectory1) && !string.IsNullOrWhiteSpace(downloadName))
				{
					string path2 = Path.Combine(ViewboxApplication.DocumentsDirectory1, downloadName);
					filename = CreateOnTheFileConverter(downloadName, path2);
				}
				if (ViewboxApplication.IsOnTheFlyConvertion && string.IsNullOrWhiteSpace(filename) && !string.IsNullOrWhiteSpace(ViewboxApplication.DocumentsDirectory2) && !string.IsNullOrWhiteSpace(downloadName))
				{
					string path = Path.Combine(ViewboxApplication.DocumentsDirectory2, downloadName);
					filename = CreateOnTheFileConverter(downloadName, path);
				}
				try
				{
					FileInfo fi = new FileInfo(filename);
					if (fi.Exists)
					{
						downloadName = id + (string.IsNullOrWhiteSpace(fi.Extension) ? "" : fi.Extension);
						return File(filename, GetMimeType(fi.Extension), downloadName);
					}
				}
				catch
				{
					LogHelper.GetLogger().Error($"File is not exist: {filename}");
				}
			}
			throw new HttpException(404, "Not found");
		}

		public ActionResult DownloadWarning()
		{
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Warning,
				Content = Resources.NoFileSelected,
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.OK
					}
				}
			});
		}

		[HttpPost]
		public ActionResult DownloadMoreFileByPath(string ids, long start = 0L, string tobjId = null)
		{
			List<IDownloadInfo> downloadFileInfos = new List<IDownloadInfo>();
			IArchive archive = ViewboxSession.Archives.Where((IArchive a) => a.Database.ToLower() == ViewboxSession.SelectedSystem.ToLower()).FirstOrDefault();
			long tId = 0L;
			try
			{
				if (tobjId.Contains("?"))
				{
					tobjId = tobjId.Split('?').First();
				}
				tId = long.Parse(tobjId);
			}
			catch
			{
			}
			DataTable document = new DataTable();
			if (tobjId != null && tId != 0)
			{
				ViewboxDb.TableObject obj = ViewboxSession.TempTableObjects[(int)tId];
				string sql = string.Empty;
				document = ViewboxApplication.Database.GetArchiveData(obj, ViewboxSession.Optimizations.Last(), ref sql, start);
			}
			else
			{
				ViewboxDb.TableObject obj2 = ViewboxSession.TempTableObjects.LastOrDefault();
				document = ViewboxApplication.Database.GetArchiveData(archive, ViewboxSession.Optimizations.Last(), obj2, start);
			}
			FileInfo packageFileInfo = new FileInfo(Path.Combine(ViewboxApplication.TemporaryDirectory, ViewboxSession.User.UserName + "_belegs.zip"));
			if (packageFileInfo.Exists)
			{
				packageFileInfo.Delete();
			}
			if (!ViewboxApplication.BelegFileMerge)
			{
				if (string.IsNullOrEmpty(ids))
				{
					return null;
				}
				string[] idlist2 = ids.Split(',');
				if (idlist2.Length != 0)
				{
					List<DocumentFileModel> list2 = ViewboxSession.SavedDocumentSettings;
					string[] array = idlist2;
					foreach (string ppath2 in array)
					{
						int id2 = list2.FindIndex((DocumentFileModel x) => x.Url == ppath2);
						DocumentFileModel item2 = list2[id2];
						string downloadName2 = item2.Url;
						if (downloadName2.StartsWith("http:") || downloadName2.StartsWith("https:"))
						{
							using WebClient wc = new WebClient();
							if (ViewboxApplication.ArchiveAuthentication)
							{
								wc.Credentials = new NetworkCredential(ViewboxApplication.ArchiveAuthenticationUsername, ViewboxApplication.ArchiveAuthenticationPassword);
							}
							try
							{
								wc.DownloadFile(downloadName2, ViewboxApplication.TemporaryDirectory + new FileInfo(downloadName2).Name);
								if (!string.IsNullOrWhiteSpace(item2.Reverse))
								{
									string ext = item2.Reverse.Trim().ToLower();
									if (!ext.StartsWith("."))
									{
										ext = "." + ext;
									}
									downloadName2 = "Document" + ext;
								}
								else
								{
									downloadName2 = "Document";
								}
								downloadFileInfos.Add(new DownloadFileInfo(ViewboxApplication.TemporaryDirectory + new FileInfo(downloadName2).Name, string.Empty, string.Empty, string.Empty, string.Empty));
							}
							catch
							{
								LogHelper.GetLogger().Error($"Url is not available: {downloadName2}");
								throw new HttpException(404, "Not found");
							}
						}
						string filename2 = string.Empty;
						if (System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory, downloadName2)))
						{
							filename2 = Path.Combine(ViewboxApplication.DocumentsDirectory, downloadName2);
						}
						else if (ViewboxApplication.DocumentsDirectory1 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory1, downloadName2)))
						{
							filename2 = Path.Combine(ViewboxApplication.DocumentsDirectory1, downloadName2);
						}
						else if (ViewboxApplication.DocumentsDirectory2 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory2, downloadName2)))
						{
							filename2 = Path.Combine(ViewboxApplication.DocumentsDirectory2, downloadName2);
						}
						else if (ViewboxApplication.DocumentsDirectory3 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory3, downloadName2)))
						{
							filename2 = Path.Combine(ViewboxApplication.DocumentsDirectory3, downloadName2);
						}
						if (ViewboxApplication.IsOnTheFlyConvertion && string.IsNullOrWhiteSpace(filename2) && !string.IsNullOrWhiteSpace(downloadName2))
						{
							if (!string.IsNullOrWhiteSpace(ViewboxApplication.DocumentsConfigDirectory))
							{
								string path3 = Path.Combine(ViewboxApplication.DocumentsConfigDirectory, downloadName2);
								filename2 = CreateOnTheFileConverter(downloadName2, path3);
							}
							else if (!string.IsNullOrWhiteSpace(ViewboxApplication.DocumentsDirectory1))
							{
								string path2 = Path.Combine(ViewboxApplication.DocumentsDirectory1, downloadName2);
								filename2 = CreateOnTheFileConverter(downloadName2, path2);
							}
							else if (!string.IsNullOrWhiteSpace(ViewboxApplication.DocumentsDirectory2))
							{
								string path = Path.Combine(ViewboxApplication.DocumentsDirectory2, downloadName2);
								filename2 = CreateOnTheFileConverter(downloadName2, path);
							}
						}
						try
						{
							string realExtension = new FileInfo(downloadName2).Extension;
							if (!FileFormatHandler.Instance.AllowedFileFormats.Contains(realExtension))
							{
								foreach (DataRow row3 in document.Rows)
								{
									try
									{
										if (row3["path"].ToString().ToLower() == downloadName2.ToLower())
										{
											realExtension = row3["reverse"].ToString();
										}
									}
									catch
									{
									}
								}
							}
							if (filename2 != string.Empty)
							{
								FileInfo fi2 = new FileInfo(filename2);
								if (fi2.Exists)
								{
									if (ViewboxApplication.AFPConverter && downloadName2.Contains(".AFP"))
									{
										DataRow Row = (from DataRow a in document.Rows
											where filename2.Contains(a["PATH"].ToString())
											select a).FirstOrDefault();
										string width = Row["Width"].ToString();
										string height = Row["Height"].ToString();
										string positionX = Row["PositionX"].ToString();
										string positionY = Row["PositionY"].ToString();
										string overlay = Row["Overlay"].ToString();
										string text = Row["ipttxt"].ToString();
										string textPages = Row["ipttxt_page"].ToString();
										FileInfo fileInfo = new FileInfo(filename2);
										if (width == string.Empty)
										{
											downloadFileInfos.Add(new DownloadFileInfo(filename2, ViewboxApplication.ConvertedAFPFilesDirectory, ViewboxApplication.OverlaysDirectory + overlay + ".jpg", "TIFF", filename2.Replace(fileInfo.Extension, ".TIFF"), 1250, 900, 300, 85, text, textPages, isConvert: true));
										}
										else
										{
											downloadFileInfos.Add(new DownloadFileInfo(filename2, ViewboxApplication.ConvertedAFPFilesDirectory, ViewboxApplication.OverlaysDirectory + overlay + ".jpg", "TIFF", filename2.Replace(fileInfo.Extension, ".TIFF"), int.Parse(width), int.Parse(height), int.Parse(positionX), int.Parse(positionY), text, textPages, isConvert: true));
										}
									}
									else if (realExtension == string.Empty)
									{
										downloadFileInfos.Add(new DownloadFileInfo(filename2, string.Empty, string.Empty, string.Empty, string.Empty));
									}
								}
							}
							if (ViewboxApplication.MerckFileMerge)
							{
								DataRow itemRow = null;
								foreach (DataRow row2 in document.Rows)
								{
									if (row2["path"].ToString() == downloadName2)
									{
										itemRow = row2;
										break;
									}
								}
								string sourcePath = "";
								if (ViewboxApplication.DocumentsDirectory != null)
								{
									sourcePath = ViewboxApplication.DocumentsDirectory + itemRow["mietvertrag"].ToString().Replace("/", string.Empty) + "\\" + itemRow["gjahr"].ToString() + "\\";
								}
								else if (ViewboxApplication.DocumentsDirectory1 != null)
								{
									sourcePath = ViewboxApplication.DocumentsDirectory1 + itemRow["mietvertrag"].ToString().Replace("/", string.Empty) + "\\" + itemRow["gjahr"].ToString() + "\\";
								}
								else if (ViewboxApplication.DocumentsDirectory2 != null)
								{
									sourcePath = ViewboxApplication.DocumentsDirectory2 + itemRow["mietvertrag"].ToString().Replace("/", string.Empty) + "\\" + itemRow["gjahr"].ToString() + "\\";
								}
								else if (ViewboxApplication.DocumentsDirectory3 != null)
								{
									sourcePath = ViewboxApplication.DocumentsDirectory3 + itemRow["mietvertrag"].ToString().Replace("/", string.Empty) + "\\" + itemRow["gjahr"].ToString() + "\\";
								}
								downloadFileInfos.Add(new DownloadFileInfo(downloadName2, ViewboxApplication.ConvertedMerckFile, string.Empty, string.Empty, string.Empty, 1250, 900, 300, 85, "", "", isConvert: false, itemRow["korrespondenzvorfall"].ToString(), itemRow["mietvertrag"].ToString().Replace("/", string.Empty), itemRow["gjahr"].ToString(), sourcePath));
							}
							if (!string.IsNullOrEmpty(realExtension))
							{
								if (System.IO.File.Exists(filename2))
								{
									string s5 = (downloadName2.Contains("\\") ? downloadName2.Split('\\').Last() : downloadName2);
									byte[] buffer5 = System.IO.File.ReadAllBytes(filename2);
									if (FileFormatHandler.Instance.AllowedFileFormats.Contains(ViewboxApplication.BelegUseExtensionByTheExtesionColumnNotByFile ? realExtension.ToLower() : Path.GetExtension(filename2).ToLower()))
									{
										downloadFileInfos.Add(new DownloadFileInfo(s5 + (ViewboxApplication.BelegUseExtensionByTheExtesionColumnNotByFile ? realExtension.ToLower() : ""), buffer5, realExtension));
									}
									else
									{
										realExtension = "";
										foreach (DataRow row in document.Rows)
										{
											if (filename2.ToLower().Contains(row["path"].ToString().ToLower()))
											{
												try
												{
													realExtension = row["reverse"].ToString();
												}
												catch
												{
												}
											}
										}
										downloadFileInfos.Add(new DownloadFileInfo(filename2 + (ViewboxApplication.BelegUseExtensionByTheExtesionColumnNotByFile ? realExtension.ToLower() : ""), buffer5, realExtension));
									}
								}
								else if (System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory, downloadName2) + "." + realExtension))
								{
									filename2 = Path.Combine(ViewboxApplication.DocumentsDirectory, downloadName2) + "." + realExtension;
									string s4 = (downloadName2.Contains("\\") ? downloadName2.Split('\\').Last() : downloadName2);
									byte[] buffer4 = System.IO.File.ReadAllBytes(filename2);
									downloadFileInfos.Add(new DownloadFileInfo(s4, buffer4, realExtension));
								}
								else if (System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory1, downloadName2) + "." + realExtension))
								{
									filename2 = Path.Combine(ViewboxApplication.DocumentsDirectory1, downloadName2) + "." + realExtension;
									string s3 = (downloadName2.Contains("\\") ? downloadName2.Split('\\').Last() : downloadName2);
									byte[] buffer3 = System.IO.File.ReadAllBytes(filename2);
									downloadFileInfos.Add(new DownloadFileInfo(s3, buffer3, realExtension));
								}
								else if (System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory2, downloadName2) + "." + realExtension))
								{
									filename2 = Path.Combine(ViewboxApplication.DocumentsDirectory2, downloadName2) + "." + realExtension;
									string s2 = (downloadName2.Contains("\\") ? downloadName2.Split('\\').Last() : downloadName2);
									byte[] buffer2 = System.IO.File.ReadAllBytes(filename2);
									downloadFileInfos.Add(new DownloadFileInfo(s2, buffer2, realExtension));
								}
								else if (System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory3, downloadName2) + "." + realExtension))
								{
									filename2 = Path.Combine(ViewboxApplication.DocumentsDirectory3, downloadName2) + "." + realExtension;
									string s = (downloadName2.Contains("\\") ? downloadName2.Split('\\').Last() : downloadName2);
									byte[] buffer = System.IO.File.ReadAllBytes(filename2);
									downloadFileInfos.Add(new DownloadFileInfo(s, buffer, realExtension));
								}
							}
						}
						catch
						{
							LogHelper.GetLogger().Error($"File is not exist: {filename2}");
						}
						string dirPath = string.Empty;
						string dirName = Path.GetFileNameWithoutExtension(downloadName2);
						if (Directory.Exists(Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory, dirName)))
						{
							dirPath = Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory, dirName);
						}
						else if (ViewboxApplication.DocumentsAttachmentDirectory1 != null && Directory.Exists(Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory1, dirName)))
						{
							dirPath = Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory1, dirName);
						}
						else if (ViewboxApplication.DocumentsAttachmentDirectory2 != null && Directory.Exists(Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory2, dirName)))
						{
							dirPath = Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory2, dirName);
						}
						else if (ViewboxApplication.DocumentsAttachmentDirectory3 != null && Directory.Exists(Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory3, dirName)))
						{
							dirPath = Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory3, dirName);
						}
						if (dirPath != string.Empty)
						{
							downloadFileInfos.Add(new DownloadDirectoryInfo(dirPath, dirName));
						}
					}
					return DownloadFilesActionResult(downloadFileInfos);
				}
			}
			else
			{
				if (string.IsNullOrEmpty(ids))
				{
					return null;
				}
				string[] idlist = ids.Split(',');
				if (idlist.Length != 0)
				{
					List<DocumentFileModel> list = ViewboxSession.SavedDocumentSettings;
					string[] array2 = idlist;
					foreach (string ppath in array2)
					{
						int id = list.FindIndex((DocumentFileModel x) => x.Url == ppath);
						DocumentFileModel item = list[id];
						string downloadName = item.Url;
						string filename = Path.Combine(ViewboxApplication.DocumentsDirectory, downloadName);
						try
						{
							FileInfo fi = new FileInfo(filename);
							if (fi.Exists)
							{
								downloadFileInfos.Add(new DownloadFileInfo(fi.FullName, string.Empty, string.Empty, string.Empty, string.Empty));
								continue;
							}
							string dokId = fi.Name.Replace(fi.Extension, "");
							List<string> parts = ViewboxSession.GetBelegParts(ViewboxSession.SelectedSystem, dokId);
							using (FileStream destStream = System.IO.File.Create(filename))
							{
								foreach (string filePath in parts)
								{
									using FileStream sourceStream = System.IO.File.OpenRead(Path.Combine(ViewboxApplication.DocumentsDirectory, filePath));
									sourceStream.CopyTo(destStream);
								}
							}
							downloadFileInfos.Add(new DownloadFileInfo(filename, string.Empty, string.Empty, string.Empty, string.Empty));
						}
						catch (Exception ex)
						{
							LogHelper.GetLogger().Error($"File is not exist: {filename}" + " - " + ex.Message);
						}
					}
					return DownloadFilesActionResult(downloadFileInfos);
				}
			}
			throw new HttpException(404, "Not found");
		}

		private static string CreateOnTheFileConverter(string downloadName, string path)
		{
			FileInfo fi = new FileInfo(path);
			path = path.Substring(0, path.Length - fi.Extension.Length) + ".md";
			string filename = "";
			string type = (System.IO.File.Exists(path) ? OnTheFlyConverter.GetFileTypeFromMd(path) : "application/pdf");
			object obj = (ViewboxApplication.DocumentsDirectory ?? ViewboxApplication.DocumentsDirectory1) ?? ViewboxApplication.DocumentsDirectory2;
			if (obj == null)
			{
				obj = ViewboxApplication.DocumentsDirectory3;
			}
			string outDir = (string)obj;
			if (outDir != null && OnTheFlyConverter.CreateDocument(path.Substring(0, path.Length - 3), Path.Combine(outDir, downloadName), type))
			{
				filename = outDir + downloadName;
			}
			return filename;
		}

		public static string GetMimeType(string extension)
		{
			if (string.IsNullOrWhiteSpace(extension))
			{
				return "application/octet-stream";
			}
			string ext = extension.Trim().ToLower();
			if (!ext.StartsWith("."))
			{
				ext = "." + ext;
			}
			switch (ext)
			{
			case ".doc":
				return "application/msword";
			case ".pdf":
				return "application/pdf";
			case ".ps":
				return "application/postscript";
			case ".rtf":
				return "application/rtf";
			case ".xls":
				return "application/vnd.ms-excel";
			case ".mpp":
				return "application/vnd.ms-project";
			case ".ppt":
				return "application/vnd.ms-powerpoint";
			case ".otf":
				return "application/x-font-otf";
			case ".bmp":
				return "image/bmp";
			case ".gif":
				return "image/gif";
			case ".jpg":
				return "image/jpeg";
			case ".pcx":
				return "image/x-pcx";
			case ".tiff":
			case ".tif":
				return "image/tiff";
			case ".htm":
			case ".html":
				return "text/html";
			default:
				return "application/octet-stream";
			}
		}

		[HttpPost]
		public FileResult DownloadNew(string id)
		{
			IArchive archive = ViewboxSession.Archives.Where((IArchive a) => a.Database.ToLower() == ViewboxSession.SelectedSystem.ToLower()).FirstOrDefault();
			IEnumerable<IArchiveSetting> webServiceAddress = archive.Settings.Where((IArchiveSetting a) => a.Key.ToLower() == "webserviceaddress");
			IOptimization opt = ViewboxSession.Optimizations.FirstOrDefault();
			if (opt != null && webServiceAddress.Any())
			{
				Dictionary<string, List<string>> additionalDictionary = GetAdditionalInformation(archive, opt);
				additionalDictionary.Add("documentDirectory", new List<string> { ViewboxApplication.DocumentsDirectory });
				if (ViewboxApplication.DocumentsDirectory1 != null)
				{
					additionalDictionary.Add("documentDirectory1", new List<string> { ViewboxApplication.DocumentsDirectory1 });
				}
				if (ViewboxApplication.DocumentsDirectory2 != null)
				{
					additionalDictionary.Add("documentDirectory2", new List<string> { ViewboxApplication.DocumentsDirectory2 });
				}
				if (ViewboxApplication.DocumentsDirectory3 != null)
				{
					additionalDictionary.Add("documentDirectory3", new List<string> { ViewboxApplication.DocumentsDirectory3 });
				}
				additionalDictionary.Add("temporaryDirectory", new List<string> { ViewboxApplication.TemporaryDirectory });
				string additional = Xml.WriteXmlString(additionalDictionary);
				string tempPath = (string)CallWCFMethod("GetPath", webServiceAddress.First().Value, id, additional);
				byte[] bytes;
				using (MemoryStream memStream = new MemoryStream())
				{
					using FileStream fileStream = new FileStream(tempPath, FileMode.Open);
					fileStream.CopyTo(memStream);
					bytes = memStream.ToArray();
				}
				System.IO.File.Delete(tempPath);
				try
				{
					return File(bytes, "application/octet-stream", id + ".zip");
				}
				catch
				{
					throw new HttpException(404, "Not found");
				}
			}
			return null;
		}

		public static object CallWCFMethod(string method, string endpointUri, params object[] args)
		{
			BasicHttpBinding binding = new BasicHttpBinding();
			EndpointAddress endpoint = new EndpointAddress(endpointUri);
			IArchiveWebService webService = ChannelFactory<IArchiveWebService>.CreateChannel(binding, endpoint);
			return webService.GetType().GetMethod(method).Invoke(webService, args);
		}

		private void ExtractZipFile(string filename, string password, DocumentsModel model, string search = "")
		{
			using (ZipFile zip = ZipFile.Read(ViewboxApplication.DocumentsDirectory + "\\" + filename))
			{
				foreach (ZipEntry e2 in zip.Entries)
				{
					e2.ExtractWithPassword(ViewboxApplication.TemporaryDirectory, ExtractExistingFileAction.DoNotOverwrite, password);
					if (!e2.IsDirectory && e2.FileName.Contains(search))
					{
						model.Files.Add(e2);
					}
					else
					{
						model.TemporaryDirectory = ViewboxApplication.TemporaryDirectory + e2.FileName.Replace("/", "\\");
					}
				}
			}
			if (ViewboxApplication.DocumentsDirectory1 != null)
			{
				using ZipFile zipFile = ZipFile.Read(ViewboxApplication.DocumentsDirectory1 + "\\" + filename);
				foreach (ZipEntry e4 in zipFile.Entries)
				{
					e4.ExtractWithPassword(ViewboxApplication.TemporaryDirectory, ExtractExistingFileAction.DoNotOverwrite, password);
					if (!e4.IsDirectory && e4.FileName.Contains(search))
					{
						model.Files.Add(e4);
					}
					else
					{
						model.TemporaryDirectory = ViewboxApplication.TemporaryDirectory + e4.FileName.Replace("/", "\\");
					}
				}
			}
			if (ViewboxApplication.DocumentsDirectory2 != null)
			{
				using ZipFile zipFile2 = ZipFile.Read(ViewboxApplication.DocumentsDirectory2 + "\\" + filename);
				foreach (ZipEntry e3 in zipFile2.Entries)
				{
					e3.ExtractWithPassword(ViewboxApplication.TemporaryDirectory, ExtractExistingFileAction.DoNotOverwrite, password);
					if (!e3.IsDirectory && e3.FileName.Contains(search))
					{
						model.Files.Add(e3);
					}
					else
					{
						model.TemporaryDirectory = ViewboxApplication.TemporaryDirectory + e3.FileName.Replace("/", "\\");
					}
				}
			}
			if (ViewboxApplication.DocumentsDirectory3 == null)
			{
				return;
			}
			using ZipFile zipFile3 = ZipFile.Read(ViewboxApplication.DocumentsDirectory3 + "\\" + filename);
			foreach (ZipEntry e in zipFile3.Entries)
			{
				e.ExtractWithPassword(ViewboxApplication.TemporaryDirectory, ExtractExistingFileAction.DoNotOverwrite, password);
				if (!e.IsDirectory && e.FileName.Contains(search))
				{
					model.Files.Add(e);
				}
				else
				{
					model.TemporaryDirectory = ViewboxApplication.TemporaryDirectory + e.FileName.Replace("/", "\\");
				}
			}
		}

		public ActionResult Filter(int id, int column, string filter, bool exactmatch)
		{
			return RedirectToAction("Filter", "DataGrid", new { id, column, filter, exactmatch });
		}

		public ActionResult Sort(string parameterValues, bool ExactMatch, int sortColumnId = 0, SortDirection direction = SortDirection.Ascending)
		{
			return SortAndFilter(parameterValues.Split('#'), ExactMatch, sortColumnId, direction);
		}

		public JsonResult Thumbnail(string filename)
		{
			try
			{
				FileStream fileStream;
				if (System.IO.File.Exists(filename))
				{
					fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
					byte[] data2 = new byte[(int)fileStream.Length];
					fileStream.Read(data2, 0, data2.Length);
					return Json(new
					{
						image = Convert.ToBase64String(data2)
					}, JsonRequestBehavior.AllowGet);
				}
				fileStream = new FileStream("Content\\img\\archive\\no_preview.png", FileMode.Open, FileAccess.Read);
				byte[] data = new byte[(int)fileStream.Length];
				fileStream.Read(data, 0, data.Length);
				return Json(new
				{
					image = Convert.ToBase64String(data)
				}, JsonRequestBehavior.AllowGet);
			}
			catch
			{
				return null;
			}
		}

		public JsonResult CheckThumbnailAfterEventClick(int id, string filename, long start, string tobjId)
		{
			try
			{
				long tempId = 0L;
				try
				{
					if (tobjId.Contains("?"))
					{
						tobjId = Regex.Match(tobjId, "\\d+").Value;
					}
					long.TryParse(tobjId, out tempId);
				}
				catch (Exception)
				{
				}
				if (filename.Contains("Content\\img\\archive\\no_preview.png"))
				{
					List<DocumentFileModel> list = ViewboxSession.SavedDocumentSettings;
					if (list != null && id >= 0 && list.Count > id)
					{
						if (ViewboxApplication.BelegFileMerge)
						{
							DocumentFileModel item = list[id];
							string downloadName = item.Url;
							string belegFilename = Path.Combine(ViewboxApplication.DocumentsDirectory, downloadName);
							try
							{
								FileInfo fi = new FileInfo(belegFilename);
								if (!fi.Exists)
								{
									string dokId = fi.Name.Replace(fi.Extension, "");
									List<string> parts = ViewboxSession.GetBelegParts(ViewboxSession.SelectedSystem, dokId);
									using FileStream destStream = System.IO.File.Create(belegFilename);
									foreach (string filePath in parts)
									{
										using FileStream sourceStream = System.IO.File.OpenRead(Path.Combine(ViewboxApplication.DocumentsDirectory, filePath));
										sourceStream.CopyTo(destStream);
									}
								}
							}
							catch (Exception ex3)
							{
								LogHelper.GetLogger().Error($"File is not exist: {belegFilename}" + " - " + ex3.Message);
							}
						}
						string path = "";
						path = ((!ViewboxApplication.MerckFileMerge) ? GenerateThumbnailPath(id, list[id].Url, start, merckThumbnail: false, tempId) : GenerateThumbnailPath(id, list[id].Url, start, merckThumbnail: true, tempId));
						if (System.IO.File.Exists(path))
						{
							return Json(path, JsonRequestBehavior.AllowGet);
						}
					}
				}
				return Json(filename, JsonRequestBehavior.AllowGet);
			}
			catch (Exception)
			{
				return Json(filename, JsonRequestBehavior.AllowGet);
			}
		}

		private string GenerateThumbnailPath(int id, string filename, long start, bool merckThumbnail, long tobjId)
		{
			IArchive archive = ViewboxSession.Archives.Where((IArchive a) => a.Database.ToLower() == ViewboxSession.SelectedSystem.ToLower()).FirstOrDefault();
			DataTable document = new DataTable();
			ViewboxDb.TableObject obj = ViewboxSession.TempTableObjects.LastOrDefault();
			document = ViewboxApplication.Database.GetArchiveData(archive, ViewboxSession.Optimizations.Last(), obj, start);
			foreach (DataRow Row in document.Rows)
			{
				if (!(Row["path"].ToString().ToLower() == filename.ToLower()))
				{
					continue;
				}
				string originalFilePath = string.Empty;
				string thumbnailFilePath = string.Empty;
				if (ViewboxApplication.DocumentsDirectory != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory, filename)))
				{
					originalFilePath = Path.Combine(ViewboxApplication.DocumentsDirectory, filename);
					thumbnailFilePath = Path.Combine(ViewboxApplication.ThumbnailsDirectory, Row["thumbnail_path"].ToString());
				}
				else if (ViewboxApplication.DocumentsDirectory1 != null && ViewboxApplication.DocumentsDirectory1 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory1, filename)))
				{
					originalFilePath = Path.Combine(ViewboxApplication.DocumentsDirectory1, filename);
					thumbnailFilePath = Path.Combine(ViewboxApplication.ThumbnailsDirectory1, Row["thumbnail_path"].ToString());
				}
				else if (ViewboxApplication.DocumentsDirectory2 != null && ViewboxApplication.DocumentsDirectory2 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory2, filename)))
				{
					originalFilePath = Path.Combine(ViewboxApplication.DocumentsDirectory2, filename);
					thumbnailFilePath = Path.Combine(ViewboxApplication.ThumbnailsDirectory2, Row["thumbnail_path"].ToString());
				}
				else if (ViewboxApplication.DocumentsDirectory3 != null && ViewboxApplication.DocumentsDirectory3 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory3, filename)))
				{
					originalFilePath = Path.Combine(ViewboxApplication.DocumentsDirectory3, filename);
					thumbnailFilePath = Path.Combine(ViewboxApplication.ThumbnailsDirectory3, Row["thumbnail_path"].ToString());
				}
				else if (originalFilePath == string.Empty)
				{
					string realExtension = "";
					foreach (DataRow row2 in document.Rows)
					{
						try
						{
							if (row2["path"].ToString().ToLower() == filename.ToLower())
							{
								realExtension = row2["reverse"].ToString();
							}
						}
						catch
						{
						}
					}
					if (ViewboxApplication.DocumentsDirectory != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory, filename) + "." + realExtension))
					{
						originalFilePath = Path.Combine(ViewboxApplication.DocumentsDirectory, filename) + "." + realExtension;
						thumbnailFilePath = Path.Combine(ViewboxApplication.ThumbnailsDirectory, Row["thumbnail_path"].ToString());
					}
					else if (ViewboxApplication.DocumentsDirectory1 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory1, filename) + "." + realExtension))
					{
						originalFilePath = Path.Combine(ViewboxApplication.DocumentsDirectory1, filename) + "." + realExtension;
						thumbnailFilePath = Path.Combine(ViewboxApplication.ThumbnailsDirectory1, Row["thumbnail_path"].ToString());
					}
					else if (ViewboxApplication.DocumentsDirectory2 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory2, filename) + "." + realExtension))
					{
						originalFilePath = Path.Combine(ViewboxApplication.DocumentsDirectory2, filename) + "." + realExtension;
						thumbnailFilePath = Path.Combine(ViewboxApplication.ThumbnailsDirectory2, Row["thumbnail_path"].ToString());
					}
					else if (ViewboxApplication.DocumentsDirectory3 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory3, filename) + "." + realExtension))
					{
						originalFilePath = Path.Combine(ViewboxApplication.DocumentsDirectory3, filename) + "." + realExtension;
						thumbnailFilePath = Path.Combine(ViewboxApplication.ThumbnailsDirectory3, Row["thumbnail_path"].ToString());
					}
				}
				LogHelper.GetLogger().Info($"File path: {originalFilePath}, Thumbnail path: {thumbnailFilePath}");
				if (ViewboxApplication.PayRollConverter && originalFilePath.Contains(".TXT"))
				{
					PayRollConverter.Converter PayrollConverter = new PayRollConverter.Converter(originalFilePath, thumbnail: true, thumbnailFilePath);
					originalFilePath = PayrollConverter.Start();
					string fileExtension3 = new FileInfo(originalFilePath).Extension;
					OnTheFlyConverter.CreateThumbnail(originalFilePath, thumbnailFilePath, fileExtension3);
					return thumbnailFilePath;
				}
				if (!System.IO.File.Exists(thumbnailFilePath) && !ViewboxApplication.MerckFileMerge && !ViewboxApplication.PayRollConverter)
				{
					FileInfo thumbnailFile2 = new FileInfo(Row["path"].ToString());
					string fileExtension2 = thumbnailFile2.Extension;
					try
					{
						if (!FileFormatHandler.Instance.AllowedFileFormats.Contains(fileExtension2))
						{
							fileExtension2 = string.Format(".{0}", Row["reverse"].ToString());
							if (fileExtension2 == ".")
							{
								fileExtension2 = thumbnailFile2.Extension;
							}
						}
					}
					catch (Exception)
					{
						fileExtension2 = thumbnailFile2.Extension;
					}
					if (ViewboxApplication.AFPConverter && fileExtension2.Contains("AFP"))
					{
						string width = Row["width"].ToString();
						string height = Row["height"].ToString();
						string positionX = Row["positionX"].ToString();
						string positionY = Row["positionY"].ToString();
						string overlay = Row["overlay"].ToString();
						string text = Row["ipttxt"].ToString();
						string textPages = Row["ipttxt_page"].ToString();
						FileInfo fileInfo = new FileInfo(originalFilePath);
						if (width == string.Empty)
						{
							AFPConverter.Converter converter2 = new AFPConverter.Converter(originalFilePath, ViewboxApplication.ConvertedAFPFilesDirectory, ViewboxApplication.OverlaysDirectory + overlay + ".jpg", "TIFF", originalFilePath.Replace(fileInfo.Extension, ".TIFF"), 1250, 900, 0, 0, text, textPages);
							originalFilePath = converter2.Start();
						}
						else
						{
							AFPConverter.Converter converter = new AFPConverter.Converter(originalFilePath, ViewboxApplication.ConvertedAFPFilesDirectory, ViewboxApplication.OverlaysDirectory + overlay + ".jpg", "TIFF", originalFilePath.Replace(fileInfo.Extension, ".TIFF"), int.Parse(width), int.Parse(height), int.Parse(positionX), int.Parse(positionY), text, textPages);
							originalFilePath = converter.Start();
						}
						fileExtension2 = new FileInfo(originalFilePath).Extension;
					}
					OnTheFlyConverter.CreateThumbnail(originalFilePath, thumbnailFilePath, fileExtension2);
					return thumbnailFilePath;
				}
				if (ViewboxApplication.MerckFileMerge)
				{
					string extension = "";
					string sourcePath = "";
					if (ViewboxApplication.DocumentsDirectory != null)
					{
						sourcePath = ViewboxApplication.DocumentsDirectory + Row["mietvertrag"].ToString().Replace("/", string.Empty) + "\\" + Row["gjahr"].ToString() + "\\";
					}
					else if (ViewboxApplication.DocumentsDirectory1 != null)
					{
						sourcePath = ViewboxApplication.DocumentsDirectory1 + Row["mietvertrag"].ToString().Replace("/", string.Empty) + "\\" + Row["gjahr"].ToString() + "\\";
					}
					else if (ViewboxApplication.DocumentsDirectory2 != null)
					{
						sourcePath = ViewboxApplication.DocumentsDirectory2 + Row["mietvertrag"].ToString().Replace("/", string.Empty) + "\\" + Row["gjahr"].ToString() + "\\";
					}
					else if (ViewboxApplication.DocumentsDirectory3 != null)
					{
						sourcePath = ViewboxApplication.DocumentsDirectory3 + Row["mietvertrag"].ToString().Replace("/", string.Empty) + "\\" + Row["gjahr"].ToString() + "\\";
					}
					string thumbnailDirectory = ViewboxApplication.ThumbnailsDirectory + Row["mietvertrag"].ToString().Replace("/", string.Empty) + "\\" + Row["gjahr"].ToString() + "\\";
					if (ViewboxApplication.ThumbnailsDirectory != null && !Directory.Exists(thumbnailDirectory))
					{
						Directory.CreateDirectory(ViewboxApplication.ThumbnailsDirectory + Row["mietvertrag"].ToString().Replace("/", string.Empty) + "\\" + Row["gjahr"].ToString() + "\\");
					}
					thumbnailFilePath = thumbnailDirectory + Row["thumbnail_path"];
					global::MerckConverter.Converter MerckConverter = new global::MerckConverter.Converter(sourcePath, Row["korrespondenzvorfall"].ToString());
					originalFilePath = MerckConverter.ThumbnailGenerate();
					extension = new FileInfo(originalFilePath).Extension;
					OnTheFlyConverter.CreateThumbnail(originalFilePath, thumbnailFilePath, extension);
					return thumbnailFilePath;
				}
			}
			string thumbnail_pathTemp = string.Empty;
			string original_pathTemp = string.Empty;
			string helperThumbnail = ViewboxSession.GetThumbnailPathByPath(archive, filename);
			if (System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory, filename)))
			{
				thumbnail_pathTemp = Path.Combine(ViewboxApplication.ThumbnailsDirectory, helperThumbnail);
				original_pathTemp = Path.Combine(ViewboxApplication.DocumentsDirectory, filename);
			}
			else if (ViewboxApplication.DocumentsDirectory1 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory1, filename)))
			{
				thumbnail_pathTemp = Path.Combine(ViewboxApplication.ThumbnailsDirectory1, helperThumbnail);
				original_pathTemp = Path.Combine(ViewboxApplication.DocumentsDirectory1, filename);
			}
			else if (ViewboxApplication.DocumentsDirectory2 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory2, filename)))
			{
				thumbnail_pathTemp = Path.Combine(ViewboxApplication.ThumbnailsDirectory2, helperThumbnail);
				original_pathTemp = Path.Combine(ViewboxApplication.DocumentsDirectory2, filename);
			}
			else if (ViewboxApplication.DocumentsDirectory3 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory3, filename)))
			{
				thumbnail_pathTemp = Path.Combine(ViewboxApplication.ThumbnailsDirectory3, helperThumbnail);
				original_pathTemp = Path.Combine(ViewboxApplication.DocumentsDirectory3, filename);
			}
			LogHelper.GetLogger().Info($"File path: {original_pathTemp}, Thumbnail path: {thumbnail_pathTemp}");
			if (!string.IsNullOrEmpty(thumbnail_pathTemp))
			{
				if (!System.IO.File.Exists(thumbnail_pathTemp))
				{
					FileInfo thumbnailFile = new FileInfo(filename);
					string fileExtension = thumbnailFile.Extension;
					try
					{
						if (!FileFormatHandler.Instance.AllowedFileFormats.Contains(fileExtension.ToLower()))
						{
							fileExtension = $".{ViewboxSession.GetReserve(archive, filename)}";
							if (fileExtension == ".")
							{
								fileExtension = thumbnailFile.Extension;
							}
						}
						if (ViewboxApplication.BelegUseExtensionByTheExtesionColumnNotByFile)
						{
							string fileExt = string.Empty;
							foreach (DataRow row in document.Rows)
							{
								if (row["path"].ToString().ToLower() == filename.ToLower())
								{
									fileExt = row["reverse"].ToString();
									break;
								}
							}
							if (string.IsNullOrWhiteSpace(fileExt))
							{
								if (original_pathTemp.ToLower().Contains(".tif"))
								{
									fileExtension = ".tif";
								}
								else if (original_pathTemp.ToLower().Contains(".pdf"))
								{
									fileExtension = ".pdf";
								}
							}
							else
							{
								fileExtension = fileExt;
							}
						}
					}
					catch (Exception)
					{
						fileExtension = thumbnailFile.Extension;
					}
					OnTheFlyConverter.CreateThumbnail(original_pathTemp, thumbnail_pathTemp, fileExtension);
				}
				return thumbnail_pathTemp;
			}
			return string.Empty;
		}

		private ActionResult DownloadFilesActionResult(IEnumerable<IDownloadInfo> downloadFileInfos)
		{
			DownloadJob conversion = DownloadJob.Start(downloadFileInfos, ViewboxSession.User);
			string title = "Download";
			title += ((downloadFileInfos.FirstOrDefault((IDownloadInfo x) => x.IsConvert) != null) ? (" and " + Resources.Conversion) : string.Empty);
			return PartialView("_DownloadPartial", new ConversionProgressDialogModel
			{
				Title = title,
				Key = conversion.Key,
				DownLoadJob = conversion
			});
		}

		public ActionResult ConversionStatusInformations(string jobKey)
		{
			DownloadJob job = Base.List.FirstOrDefault((Base x) => x.Key == jobKey) as DownloadJob;
			List<object> stateInformation = new List<object>();
			if (job != null && job.AfpConverter != null)
			{
				stateInformation.Add(new
				{
					Status = ((job.Status.ToString() == "Running") ? Resources.Running : ((job.Status.ToString() == "Chrashed") ? Resources.Chrashed : Resources.Finished)),
					DocumentCount = job.GetDocumentCount,
					DocumentIndex = job.GetCurrentDocumentIndex,
					PageCount = job.AfpConverter.GetPageCount,
					PageIndex = job.AfpConverter.GetCurrentPageIndex
				});
			}
			else if (job != null && job.PayrollConverter != null)
			{
				stateInformation.Add(new
				{
					Status = ((job.Status.ToString() == "Running") ? Resources.Running : ((job.Status.ToString() == "Chrashed") ? Resources.Chrashed : Resources.Finished)),
					DocumentCount = job.GetDocumentCount,
					DocumentIndex = job.GetCurrentDocumentIndex,
					PageCount = job.PayrollConverter.GetPageCount,
					PageIndex = job.PayrollConverter.GetCurrentPageIndex
				});
			}
			else if (job != null && job.MerckConverter != null)
			{
				stateInformation.Add(new
				{
					Status = ((job.Status.ToString() == "Running") ? Resources.Running : ((job.Status.ToString() == "Chrashed") ? Resources.Chrashed : Resources.Finished)),
					DocumentCount = job.GetDocumentCount,
					DocumentIndex = job.GetCurrentDocumentIndex,
					PageCount = 0,
					PageIndex = 0
				});
			}
			else if (job != null)
			{
				stateInformation.Add(new
				{
					Status = ((job != null && job.Status.ToString() == "Running") ? Resources.Running : ((job.Status.ToString() == "Chrashed") ? Resources.Chrashed : Resources.Finished)),
					DocumentCount = job.GetDocumentCount,
					DocumentIndex = job.GetCurrentDocumentIndex,
					PageCount = 0,
					PageIndex = 0
				});
			}
			return Json(stateInformation, JsonRequestBehavior.AllowGet);
		}

		public ActionResult ExecuteCountTable(int id, int start = 0, int size = 0)
		{
			if (size != 0)
			{
				ViewboxSession.User.DisplayRowCount = size;
			}
			else
			{
				size = ViewboxSession.User.DisplayRowCount;
			}
			ITableObject obj = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
			DocumentsModel model = new DocumentsModel();
			model.OriginalTable = obj;
			model.RowsPerPage = size;
			model.FromRow = start;
			ViewboxDb.TableObject tobj = ViewboxSession.TempTableObjects[id];
			if (tobj == null)
			{
				model.RowsCount = ((model.OriginalTable.Type != TableType.Issue) ? ViewboxSession.GetDataCount(model.OriginalTable) : ViewboxApplication.Database.TempDatabase.ComputeDataCount(model.OriginalTable));
			}
			else if (tobj.Table != null && tobj.Table.PageSystem != null)
			{
				PageSystem ps = tobj.Table.PageSystem;
				if (ps.IsEnded)
				{
					model.RowsCount = ps.Count;
				}
				else
				{
					ps.CountStep = 0L;
					model.RowsCount = ViewboxApplication.Database.TempDatabase.ComputePagedDataCount(tobj.Table);
				}
			}
			else
			{
				model.RowsCount = ((tobj.Table != null) ? tobj.Table.RowCount : 0);
			}
			if (tobj != null)
			{
				ViewboxApplication.Database.SystemDb.SetRowCount(tobj.Table, model.RowsCount);
			}
			return PartialView("_NavigationControlsPartial", model);
		}

		public FileResult DownloadFileByPath(string filePath)
		{
			ContentDisposition cd = new ContentDisposition
			{
				FileName = "beleg.zip",
				Inline = false
			};
			base.Response.AppendHeader("Content-Disposition", cd.ToString());
			return new FilePathResult(filePath, "application/zip");
		}

		public string GetBelegPath(string filter)
		{
			string path = string.Empty;
			IArchive archive = ViewboxSession.Archives.Where((IArchive a) => a.Database.ToLower() == ViewboxSession.SelectedSystem.ToLower()).FirstOrDefault();
			List<string> filterConditions = new List<string>();
			filter = filter.Replace("Equal(", "").Replace(")", "").Replace("\"", "")
				.Replace("%", "")
				.Replace("And(", "");
			string[] filterParts = filter.Split(',');
			int i;
			for (i = 0; i < filterParts.Length; i++)
			{
				if (i % 2 == 0)
				{
					IColumn col = ViewboxApplication.Database.SystemDb.Columns.SingleOrDefault((IColumn c) => c.Id == int.Parse(filterParts[i]));
					filterConditions.Add($"`{col.Name}` = ");
				}
				else
				{
					string lastCondition = filterConditions.LastOrDefault();
					lastCondition += $"'{filterParts[i]}'";
					filterConditions[filterConditions.Count - 1] = lastCondition;
				}
			}
			return ViewboxSession.GetPath(archive, string.Join(" AND ", filterConditions.ToArray()));
		}

		public int GetBelegMaxItemNumber(IArchive tobj, string column, IFilter filter)
		{
			List<string> filterConditions = new List<string>();
			string filt = filter.ToOriginalString().Replace("Equal(", "").Replace(")", "")
				.Replace("\"", "")
				.Replace("%", "")
				.Replace("AND(", "");
			string[] filterParts = filt.Split(',');
			int i;
			for (i = 0; i < filterParts.Length; i++)
			{
				if (i % 2 == 0)
				{
					IColumn col = ViewboxApplication.Database.SystemDb.Columns.SingleOrDefault((IColumn c) => c.Id == int.Parse(filterParts[i]));
					filterConditions.Add($"`{col.Name}` = ");
				}
				else
				{
					string lastCondition = filterConditions.LastOrDefault();
					lastCondition += $"'{filterParts[i]}'";
					filterConditions[filterConditions.Count - 1] = lastCondition;
				}
			}
			return ViewboxSession.GetMaxValueOfColumn(tobj, column, string.Join(" AND ", filterConditions.ToArray()));
		}

		public FileResult GetDocumentsOverViewAttachment(string file, string attachment)
		{
			string path = string.Empty;
			string ext = Path.GetExtension(attachment);
			string attachmentDir = Path.GetFileNameWithoutExtension(file);
			if (System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory, attachmentDir, attachment)))
			{
				path = Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory, attachmentDir, attachment);
			}
			else if (ViewboxApplication.DocumentsDirectory1 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory1, attachmentDir, attachment)))
			{
				path = Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory1, attachmentDir, attachment);
			}
			else if (ViewboxApplication.DocumentsDirectory2 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory2, attachmentDir, attachment)))
			{
				path = Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory2, attachmentDir, attachment);
			}
			else if (ViewboxApplication.DocumentsDirectory3 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory3, attachmentDir, attachment)))
			{
				path = Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory3, attachmentDir, attachment);
			}
			return File(path, GetMimeType(ext), attachment);
		}

		public FileResult GetDocumentsOverViewFile(string file, string ext)
		{
			string originalPath = string.Empty;
			string original_pathTemp = string.Empty;
			string thumbnail_pathTemp = string.Empty;
			if (System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsPreviewDirectory, file)))
			{
				thumbnail_pathTemp = Path.Combine(ViewboxApplication.ThumbnailsDirectory, file.Replace(ext, ".jpg"));
				original_pathTemp = Path.Combine(ViewboxApplication.DocumentsPreviewDirectory, file);
			}
			else if (ViewboxApplication.DocumentsDirectory1 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsPreviewDirectory1, file)))
			{
				thumbnail_pathTemp = Path.Combine(ViewboxApplication.ThumbnailsDirectory1, file.Replace(ext, ".jpg"));
				original_pathTemp = Path.Combine(ViewboxApplication.DocumentsPreviewDirectory1, file);
			}
			else if (ViewboxApplication.DocumentsDirectory2 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsPreviewDirectory2, file)))
			{
				thumbnail_pathTemp = Path.Combine(ViewboxApplication.ThumbnailsDirectory2, file.Replace(ext, ".jpg"));
				original_pathTemp = Path.Combine(ViewboxApplication.DocumentsPreviewDirectory2, file);
			}
			else if (ViewboxApplication.DocumentsDirectory3 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsPreviewDirectory3, file)))
			{
				thumbnail_pathTemp = Path.Combine(ViewboxApplication.ThumbnailsDirectory3, file.Replace(ext, ".jpg"));
				original_pathTemp = Path.Combine(ViewboxApplication.DocumentsPreviewDirectory3, file);
			}
			return File(original_pathTemp, GetMimeType(ext));
		}

		public PartialViewResult DocumentOverviewAttachmentsDownload(string filter)
		{
			DocumentsOverviewModel model = new DocumentsOverviewModel();
			string beleg = GetBelegPath(filter);
			string belegNameWithoutExtension = beleg.Substring(0, beleg.Length - 5);
			string path = string.Empty;
			List<Tuple<string, string>> filesAndItsSizes = new List<Tuple<string, string>>();
			string str1 = Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory, belegNameWithoutExtension);
			if (Directory.Exists(Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory, belegNameWithoutExtension)))
			{
				path = Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory, belegNameWithoutExtension);
			}
			else if (ViewboxApplication.DocumentsDirectory1 != null && Directory.Exists(Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory1, belegNameWithoutExtension)))
			{
				path = Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory1, belegNameWithoutExtension);
			}
			else if (ViewboxApplication.DocumentsDirectory2 != null && Directory.Exists(Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory2, belegNameWithoutExtension)))
			{
				path = Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory2, belegNameWithoutExtension);
			}
			else if (ViewboxApplication.DocumentsDirectory3 != null && Directory.Exists(Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory3, belegNameWithoutExtension)))
			{
				path = Path.Combine(ViewboxApplication.DocumentsAttachmentDirectory3, belegNameWithoutExtension);
			}
			if (!string.IsNullOrEmpty(path))
			{
				string[] files = Directory.GetFiles(path);
				if (files.Length != 0)
				{
					string[] array = files;
					foreach (string info in array)
					{
						FileInfo fi = new FileInfo(info);
						filesAndItsSizes.Add(new Tuple<string, string>(fi.Name, $"{Convert.ToDouble(fi.Length) / 1024.0 / 1024.0:N2}".ToString() + "MB"));
					}
				}
			}
			model.Name = beleg;
			model.Attachments = filesAndItsSizes;
			return PartialView("_DocumentOverviewAttachmentsDownloadPartial", model);
		}

		public PartialViewResult GetDocumentsOverView(int id, string filter)
		{
			DocumentsOverviewModel model = new DocumentsOverviewModel();
			FileInfo belegFile = new FileInfo(GetBelegPath(filter));
			if (ViewboxApplication.MayrMelnhofBelegarchiv)
			{
				ITableObject tobj = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
				FullColumnCollection cols = new FullColumnCollection();
				foreach (IColumn col in tobj.Columns)
				{
					cols.Add(col);
				}
				IFilter filt = ViewboxSession.GetFilter(id, filter, ViewboxSession.TempTableObjects, ViewboxSession.TableObjects, decode: false, forcedForCol: true, cols);
				ColValueFilter ticketnumber = (filt as AndFilter).Conditions.FirstOrDefault((IFilter c) => (c as ColValueFilter).Column.Name == "tn") as ColValueFilter;
				ColValueFilter itemnumber = (filt as AndFilter).Conditions.FirstOrDefault((IFilter c) => (c as ColValueFilter).Column.Name == "nr") as ColValueFilter;
				(filt as AndFilter).Conditions.Remove(itemnumber);
				int maxItemNumber = GetBelegMaxItemNumber(tobj as Archive, itemnumber.Column.Name, filt);
				if (belegFile.Extension == ".html")
				{
					model.TicketNumber = Convert.ToInt64(ticketnumber.Value);
					model.TicketNumberColumnId = ticketnumber.Column.Id;
					model.ItemNumberCount = maxItemNumber;
					model.OnItemNumber = Convert.ToInt32(itemnumber.Value);
					model.OnItemNumberColumnId = itemnumber.Column.Id;
					model.NextToItemNumber = model.OnItemNumber + 1;
					model.BackToItemNumber = ((model.OnItemNumber > 1) ? (model.OnItemNumber - 1) : 0);
					model.TableId = id;
					model.IsHtml = true;
					model.Name = belegFile.Name;
					model.Extension = belegFile.Extension;
					return PartialView("_DocumentsOverviewPartial", model);
				}
			}
			else if (belegFile.Extension == ".html")
			{
				model.IsHtml = true;
				model.Name = belegFile.Name;
				model.Extension = belegFile.Extension;
				return PartialView("_DocumentsOverviewPartial", model);
			}
			string originalPath = string.Empty;
			string original_pathTemp = string.Empty;
			string thumbnail_pathTemp = string.Empty;
			if (System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory, belegFile.Name)))
			{
				thumbnail_pathTemp = Path.Combine(ViewboxApplication.ThumbnailsDirectory, belegFile.Name.Replace(belegFile.Extension, ".jpg"));
				original_pathTemp = Path.Combine(ViewboxApplication.DocumentsDirectory, belegFile.Name);
			}
			else if (ViewboxApplication.DocumentsDirectory1 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory1, belegFile.Name)))
			{
				thumbnail_pathTemp = Path.Combine(ViewboxApplication.ThumbnailsDirectory1, belegFile.Name.Replace(belegFile.Extension, ".jpg"));
				original_pathTemp = Path.Combine(ViewboxApplication.DocumentsDirectory1, belegFile.Name);
			}
			else if (ViewboxApplication.DocumentsDirectory2 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory2, belegFile.Name)))
			{
				thumbnail_pathTemp = Path.Combine(ViewboxApplication.ThumbnailsDirectory2, belegFile.Name.Replace(belegFile.Extension, ".jpg"));
				original_pathTemp = Path.Combine(ViewboxApplication.DocumentsDirectory2, belegFile.Name);
			}
			else if (ViewboxApplication.DocumentsDirectory3 != null && System.IO.File.Exists(Path.Combine(ViewboxApplication.DocumentsDirectory3, belegFile.Name)))
			{
				thumbnail_pathTemp = Path.Combine(ViewboxApplication.ThumbnailsDirectory3, belegFile.Name.Replace(belegFile.Extension, ".jpg"));
				original_pathTemp = Path.Combine(ViewboxApplication.DocumentsDirectory3, belegFile.Name);
			}
			if (!System.IO.File.Exists(thumbnail_pathTemp))
			{
				OnTheFlyConverter.CreateThumbnail(original_pathTemp, thumbnail_pathTemp, belegFile.Extension);
			}
			FileStream fileStream = new FileStream(thumbnail_pathTemp, FileMode.Open, FileAccess.Read);
			byte[] data = new byte[(int)fileStream.Length];
			fileStream.Read(data, 0, data.Length);
			model.Body = Convert.ToBase64String(data);
			return PartialView("_DocumentsOverviewPartial", model);
		}
	}
}
