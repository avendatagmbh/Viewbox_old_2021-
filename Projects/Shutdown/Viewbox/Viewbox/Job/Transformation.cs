using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SystemDb;
using SystemDb.Internal;
using AV.Log;
using DbAccess;
using DbAccess.Enums;
using DbAccess.Structures;
using log4net;
using OttoArchive.OttoArchiveWeb;
using Utils;
using Viewbox.DcwBalance;
using Viewbox.Models;
using Viewbox.Properties;
using Viewbox.SapBalance;
using Viewbox.Structures;
using ViewboxDb;
using ViewboxDb.AggregationFunctionTranslator;
using ViewboxDb.Filters;
using WebServiceInterfaces;

namespace Viewbox.Job
{
	public class Transformation : Base
	{
		public class TableListParams
		{
			public string Search { get; set; }

			public bool ShowArchived { get; set; }

			public bool ShowHidden { get; set; }

			public bool ShowEmpty { get; set; }

			public bool ShowEmptyHidden { get; set; }
		}

		public class NotificationUrl
		{
			public string Controller { get; set; }

			public string Method { get; set; }

			public TableListParams Params { get; set; }

			public bool Visible { get; set; }

			public NotificationUrl()
			{
				Visible = true;
			}
		}

		public delegate void TransactionKeyChangedEventHandler(string key);

		protected static Dictionary<string, Transformation> _jobs = new Dictionary<string, Transformation>();

		private DatabaseBase _connection;

		public NotificationUrl notificationData;

		public new static IEnumerable<Transformation> List => _jobs.Values;

		public ViewboxDb.TableObject TransformationObject { get; set; }

		public ITableObject TableObject { get; set; }

		private string TransactionKey
		{
			set
			{
				if (value == null)
				{
				}
				if (this.KeyChanged != null)
				{
					this.KeyChanged(value);
				}
			}
		}

		public event TransactionKeyChangedEventHandler KeyChanged;

		public new static Transformation Find(string key)
		{
			return (key != null && _jobs.ContainsKey(key)) ? _jobs[key] : null;
		}

		public static Transformation Create(int id, ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, SortCollection sortList, IFilter filter, object[] paramValues, object[] displayValues, List<int> selectionTypes, List<int> itemId, int scrollPosition = 0, RelationType relationType = RelationType.Normal, string relationExtInfo = "", string relationColumnExtInfo = "", List<int> summedColumns = null, bool originalColumnIds = true, SubTotalParameters groupSubTotal = null, bool multipleOptimization = false, IDictionary<int, Tuple<int, int>> optimizationSelected = null, TransactionKeyChangedEventHandler changeEventHandler = null)
		{
			Transformation t = new Transformation(id, tableObjects, columns, opt, sortList, filter, paramValues, displayValues, itemId, selectionTypes, scrollPosition, relationType, relationExtInfo, relationColumnExtInfo, summedColumns, originalColumnIds, groupSubTotal, multipleOptimization, optimizationSelected);
			t.KeyChanged += changeEventHandler;
			return t;
		}

		public static Transformation Create(int id, ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, List<int> colIds, List<AggregationFunction> aggfunc, IFilter filter, bool save, string tableName)
		{
			return new Transformation(id, tableObjects, columns, opt, colIds, aggfunc, filter, save, tableName);
		}

		public static Transformation Create(int id, ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, List<int> colIds, AggregationCollection aggs, IFilter filter, bool save, string tableName)
		{
			return new Transformation(id, tableObjects, columns, opt, colIds, aggs, filter, save, tableName);
		}

		public static Transformation Create(ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, int id, int joinTableId, JoinColumnsCollection joinColumns, IFilter filter1, IFilter filter2, JoinType type, bool saveJoin = false, string tableName = "")
		{
			return new Transformation(tableObjects, columns, opt, id, joinTableId, joinColumns, filter1, filter2, type, saveJoin, tableName);
		}

		public static Transformation Create(ITableObject table, ArchiveType archive, long optimizedRowCount)
		{
			return new Transformation(table, archive, optimizedRowCount);
		}

		public static Transformation ArchiveTableList(IList<Tuple<ITableObject, ArchiveType, long>> tuples, NotificationUrl notification)
		{
			Transformation tr = new Transformation(tuples);
			tr.notificationData = notification;
			return tr;
		}

		public static Transformation Create(RightType right, TableType type)
		{
			return new Transformation(right, type);
		}

		public static Transformation Create(RightType right, RoleSettingsType type, ITableObject tobj)
		{
			Transformation tr = new Transformation(right, type);
			tr.TableObject = tobj;
			return tr;
		}

		public static Transformation Create(IUser user)
		{
			return new Transformation(user);
		}

		public static Transformation Create(ITableObjectCollection tableObjects, IIssueCollection issues)
		{
			return new Transformation(tableObjects, issues);
		}

		public static Transformation Create(string descriptor, string search, bool download = false, string contentType = "")
		{
			return new Transformation(descriptor, search, download, contentType);
		}

		public static Transformation Create(int documentId)
		{
			return new Transformation(documentId);
		}

		public string GetMailText()
		{
			string link = ViewboxApplication.ViewboxBasePath + "/DataGrid/Index/" + TransformationObject.Table.Id;
			string cultureName = Thread.CurrentThread.CurrentCulture.Name.Substring(0, 2);
			return base.Descriptions[cultureName] + Environment.NewLine + "Link: " + link;
		}

		protected Transformation()
		{
		}

		private Transformation(int id, ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, SortCollection sortList, IFilter filter, object[] paramValues, object[] displayValues, List<int> itemId, List<int> selectionTypes, int scrollPosition, RelationType relationType, string relationExtInfo, string relationColumnExtInfo, List<int> summedColumns, bool originalColumnIds, SubTotalParameters groupSubTotal, bool multipleOptimization = false, IDictionary<int, Tuple<int, int>> optimizationSelected = null)
		{
			_jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			base.JobFinished += delegate
			{
				Janitor.RegisterTempObject(TransformationObject.Key);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
			};
			ViewboxSession.SetupTableColumns(id);
			StartJob(delegate(object obj, CancellationToken token)
			{
				DoSortAndFilter((int)((object[])obj)[0], (ViewboxDb.TableObjectCollection)((object[])obj)[1], (ITableObjectCollection)((object[])obj)[2], (IFullColumnCollection)((object[])obj)[3], (IOptimization)((object[])obj)[4], (SortCollection)((object[])obj)[5], (IFilter)((object[])obj)[6], (IUser)((object[])obj)[7], (int)((object[])obj)[8], (object[])((object[])obj)[9], (object[])((object[])obj)[10], (List<int>)((object[])obj)[11], (List<int>)((object[])obj)[12], (RelationType)((object[])obj)[13], (string)((object[])obj)[14], (string)((object[])obj)[15], token, (List<int>)((object[])obj)[16], (bool)((object[])obj)[17], (SubTotalParameters)((object[])obj)[18], (bool)((object[])obj)[19], (IDictionary<int, Tuple<int, int>>)((object[])obj)[20], (ILanguage)((object[])obj)[21]);
				ViewboxApplication.UserSessions[((object[])obj)[7] as IUser].MarkAll();
				for (int k = 0; k < 9; k++)
				{
					((object[])obj)[k] = null;
				}
			}, new object[22]
			{
				id,
				ViewboxApplication.TempTableObjects,
				tableObjects,
				columns,
				opt,
				sortList,
				filter,
				ViewboxSession.User,
				scrollPosition,
				paramValues,
				displayValues,
				itemId,
				selectionTypes,
				relationType,
				relationExtInfo,
				relationColumnExtInfo,
				summedColumns,
				originalColumnIds,
				groupSubTotal,
				multipleOptimization,
				optimizationSelected,
				ViewboxSession.Language
			});
			ITableObject tobj = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
			if (tobj.Type == TableType.Issue && (tobj as IIssue).FilterTableObject != null)
			{
				tobj = (tobj as IIssue).FilterTableObject;
			}
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(i.CountryCode);
				base.Descriptions[i.CountryCode] = ((paramValues != null) ? Resources.ResourceManager.GetString("ExecuteIssueTitle", culture) : "");
				base.Descriptions[i.CountryCode] += ((sortList != null) ? (string.IsNullOrWhiteSpace(base.Descriptions[i.CountryCode]) ? Resources.ResourceManager.GetString("SortTitle", culture) : string.Format(" {0} {1}", Resources.ResourceManager.GetString("And", culture), Resources.ResourceManager.GetString("SortTitle", culture))) : "");
				base.Descriptions[i.CountryCode] += ((filter != null) ? (string.IsNullOrWhiteSpace(base.Descriptions[i.CountryCode]) ? Resources.ResourceManager.GetString("FilterTitle", culture) : string.Format(" {0} {1}", Resources.ResourceManager.GetString("And", culture), Resources.ResourceManager.GetString("FilterTitle", culture))) : "");
				base.Descriptions[i.CountryCode] += string.Format(" {0} {1}", Resources.ResourceManager.GetString("Of", culture), tobj.GetDescription(i));
			}
		}

		private Transformation(int id, ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, List<int> colIds, List<AggregationFunction> aggfunc, IFilter filter, bool save, string tableName)
		{
			List<string> stringAggFuncs = aggfunc.Select((AggregationFunction a) => Enum.GetName(typeof(AggregationFunction), a)).ToList();
			string descToLog = string.Join(", ", stringAggFuncs);
			_jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			IAggregationFunctionTranslator translator = DatabaseSpecificFactory.GetAggregationFunctionTranslator();
			Dictionary<Tuple<ILanguage, string>, string> aggDescriptions = new Dictionary<Tuple<ILanguage, string>, string>();
			foreach (ILanguage k in ViewboxApplication.Languages)
			{
				CultureInfo culture2 = new CultureInfo(k.CountryCode);
				foreach (object a2 in Enum.GetValues(typeof(AggregationFunction)))
				{
					aggDescriptions.Add(new Tuple<ILanguage, string>(k, translator.ConvertEnumToString((AggregationFunction)a2).ToLower()), Resources.ResourceManager.GetString("Agg" + a2.ToString(), culture2));
				}
			}
			base.JobFinished += delegate
			{
				Janitor.RegisterTempObject(TransformationObject.Key);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
			};
			StartJob(delegate(object obj, CancellationToken token)
			{
				DoGroup((int)((object[])obj)[0], (ViewboxDb.TableObjectCollection)((object[])obj)[1], (ITableObjectCollection)((object[])obj)[2], (IFullColumnCollection)((object[])obj)[3], (IOptimization)((object[])obj)[4], (List<int>)((object[])obj)[5], (List<AggregationFunction>)((object[])obj)[6], (IFilter)((object[])obj)[7], (bool)((object[])obj)[8], (string)((object[])obj)[9], (IUser)((object[])obj)[10], (Dictionary<Tuple<ILanguage, string>, string>)((object[])obj)[11], token);
				ViewboxApplication.UserSessions[((object[])obj)[10] as IUser].MarkAll();
				for (int l = 0; l < 11; l++)
				{
					((object[])obj)[l] = null;
				}
			}, new object[12]
			{
				id,
				ViewboxApplication.TempTableObjects,
				tableObjects,
				columns,
				opt,
				colIds,
				aggfunc,
				filter,
				save,
				tableName,
				ViewboxSession.User,
				aggDescriptions
			});
			ITableObject tobj = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(i.CountryCode);
				base.Descriptions[i.CountryCode] = string.Format("{0} {1} {2}", Resources.ResourceManager.GetString("Group", culture), Resources.ResourceManager.GetString("Of", culture), tobj.GetDescription(i));
			}
			LogHelper.GetLogger().InfoFormat("[FUNCTIONS] - SQL function({0}) has been started on table `{1}`.`{2}`(started by: {3})", descToLog, tobj.Database, tobj.TableName, ViewboxSession.User.UserName);
		}

		private Transformation(int id, ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, List<int> colIds, AggregationCollection aggs, IFilter filter, bool save, string tableName)
		{
			_jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			IAggregationFunctionTranslator translator = DatabaseSpecificFactory.GetAggregationFunctionTranslator();
			Dictionary<Tuple<ILanguage, string>, string> aggDescriptions = new Dictionary<Tuple<ILanguage, string>, string>();
			foreach (ILanguage k in ViewboxApplication.Languages)
			{
				CultureInfo culture2 = new CultureInfo(k.CountryCode);
				foreach (object a in Enum.GetValues(typeof(AggregationFunction)))
				{
					aggDescriptions.Add(new Tuple<ILanguage, string>(k, translator.ConvertEnumToString((AggregationFunction)a)), Resources.ResourceManager.GetString("Agg" + a.ToString(), culture2));
				}
			}
			base.JobFinished += delegate
			{
				Janitor.RegisterTempObject(TransformationObject.Key);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
			};
			StartJob(delegate(object obj, CancellationToken token)
			{
				DoGroup2((int)((object[])obj)[0], (ViewboxDb.TableObjectCollection)((object[])obj)[1], (ITableObjectCollection)((object[])obj)[2], (IFullColumnCollection)((object[])obj)[3], (IOptimization)((object[])obj)[4], (List<int>)((object[])obj)[5], (AggregationCollection)((object[])obj)[6], (IFilter)((object[])obj)[7], (bool)((object[])obj)[8], (string)((object[])obj)[9], (IUser)((object[])obj)[10], (Dictionary<Tuple<ILanguage, string>, string>)((object[])obj)[11], token);
				ViewboxApplication.UserSessions[((object[])obj)[10] as IUser].MarkAll();
				for (int l = 0; l < 11; l++)
				{
					((object[])obj)[l] = null;
				}
			}, new object[12]
			{
				id,
				ViewboxApplication.TempTableObjects,
				tableObjects,
				columns,
				opt,
				colIds,
				aggs,
				filter,
				save,
				tableName,
				ViewboxSession.User,
				aggDescriptions
			});
			ITableObject tobj = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(i.CountryCode);
				base.Descriptions[i.CountryCode] = string.Format("{0} {1} {2}", Resources.ResourceManager.GetString("Group", culture), Resources.ResourceManager.GetString("Of", culture), tobj.GetDescription(i));
			}
		}

		private Transformation(ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, int id, int joinTableId, JoinColumnsCollection joinColumns, IFilter filter1, IFilter filter2, JoinType type, bool saveJoin = false, string tableName = "")
		{
			_jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			base.JobFinished += delegate
			{
				Janitor.RegisterTempObject(TransformationObject.Key);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
			};
			StartJob(delegate(object obj, CancellationToken token)
			{
				DoJoin((ViewboxDb.TableObjectCollection)((object[])obj)[0], (ITableObjectCollection)((object[])obj)[1], (IFullColumnCollection)((object[])obj)[2], (IOptimization)((object[])obj)[3], (int)((object[])obj)[4], (int)((object[])obj)[5], (JoinColumnsCollection)((object[])obj)[6], (IFilter)((object[])obj)[7], (IFilter)((object[])obj)[8], (JoinType)((object[])obj)[9], (IUser)((object[])obj)[10], (bool)((object[])obj)[11], (string)((object[])obj)[12], token);
				ViewboxApplication.UserSessions[((object[])obj)[10] as IUser].MarkAll();
				for (int k = 0; k < 12; k++)
				{
					((object[])obj)[k] = null;
				}
			}, new object[13]
			{
				ViewboxApplication.TempTableObjects,
				tableObjects,
				columns,
				opt,
				id,
				joinTableId,
				joinColumns,
				filter1,
				filter2,
				type,
				ViewboxSession.User,
				saveJoin,
				tableName
			});
			ITableObject tobj1 = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
			ITableObject tobj2 = ViewboxSession.TableObjects[joinTableId];
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(i.CountryCode);
				base.Descriptions[i.CountryCode] = string.Format("{0} {1} {2} {3} {4}", Resources.ResourceManager.GetString("Join", culture), Resources.ResourceManager.GetString("Of", culture), tobj1.GetDescription(i), Resources.ResourceManager.GetString("And", culture), tobj2.GetDescription(i));
			}
			string startedByUser = $"(started by: {ViewboxSession.User.UserName})";
			LogHelper.GetLogger().InfoFormat("[FUNCTIONS] - Join transformation has been started. Joining `{0}`.`{1}` and `{2}`.`{3}`{4}", tobj1.Database, tobj1.TableName, tobj2.Database, tobj2.TableName, startedByUser);
		}

		private Transformation(IUser user)
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
				DoGenerate();
			}, new object[1] { user });
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(i.CountryCode);
				base.Descriptions[i.CountryCode] = string.Format("{0}", Resources.ResourceManager.GetString("GenerateEmptyDistinctTable", culture));
			}
		}

		private Transformation(ITableObject table, ArchiveType archive, long optimizedRowCount)
		{
			Transformation transformation = this;
			_jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			base.JobFinished += delegate(Base j)
			{
				Janitor.RegisterTempObject(j.Key);
				transformation.UpdateTableObjectArchive(table, archive);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
				transformation.CleanUpArchiveTable(table);
			};
			base.JobCanceled += delegate
			{
				transformation.CleanUpArchiveTable(table);
			};
			StartJob(delegate(object obj, CancellationToken token)
			{
				transformation.DoArchive((ITableObject)((object[])obj)[0], (ArchiveType)((object[])obj)[1], (long)((object[])obj)[2], token);
			}, new object[3] { table, archive, optimizedRowCount });
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(i.CountryCode);
				base.Descriptions[i.CountryCode] = string.Format("{0} : {1}", (archive == ArchiveType.Archive) ? Resources.ResourceManager.GetString("ArchiveTable", culture) : Resources.ResourceManager.GetString("RestoreTable", culture), table.GetDescription());
			}
		}

		private Transformation(IEnumerable<Tuple<ITableObject, ArchiveType, long>> tuples)
		{
			Transformation transformation = this;
			_jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			base.JobFinished += delegate(Base j)
			{
				Janitor.RegisterTempObject(j.Key);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
				transformation.CleanUpArchiveTable(tuples.Select((Tuple<ITableObject, ArchiveType, long> t) => t.Item1));
			};
			base.JobCanceled += delegate
			{
				transformation.CleanUpArchiveTable(tuples.Select((Tuple<ITableObject, ArchiveType, long> t) => t.Item1));
			};
			StartJob(OnActionForAll, tuples);
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(i.CountryCode);
				if (tuples.Count() > 0)
				{
					base.Descriptions[i.CountryCode] = string.Format((tuples.ElementAt(0).Item2 == ArchiveType.Archive) ? Resources.ResourceManager.GetString("ArchivingTables", culture) : Resources.ResourceManager.GetString("RestoringTables", culture), tuples.Count());
				}
			}
		}

		private void CleanUpArchiveTable(IEnumerable<ITableObject> tables)
		{
			foreach (ITableObject table in tables)
			{
				ViewboxApplication.Database.CleanUpArchiving(table);
				ViewboxApplication.Database.SystemDb.Objects[table.Id].IsUnderArchiving = false;
			}
		}

		private void OnActionForAll(object obj, CancellationToken token)
		{
			IEnumerable<Tuple<ITableObject, ArchiveType, long>> _list = obj as IEnumerable<Tuple<ITableObject, ArchiveType, long>>;
			foreach (Tuple<ITableObject, ArchiveType, long> tuple in _list)
			{
				if (!ViewboxApplication.Database.SystemDb.Objects[tuple.Item1.Id].IsUnderArchiving)
				{
					ViewboxApplication.Database.SystemDb.Objects[tuple.Item1.Id].IsUnderArchiving = true;
					DoArchive(tuple.Item1, tuple.Item2);
					UpdateTableObjectArchive(tuple.Item1, tuple.Item2);
					token.ThrowIfCancellationRequested();
				}
			}
		}

		private void CleanUpArchiveTable(ITableObject table)
		{
			ViewboxApplication.Database.CleanUpArchiving(table);
			ViewboxApplication.Database.SystemDb.Objects[table.Id].IsUnderArchiving = false;
		}

		private void UpdateTableObjectArchive(ITableObject table, ArchiveType archive)
		{
			bool archiveTable = archive == ArchiveType.Archive;
			ViewboxApplication.Database.SystemDb.UpdateTableObjectArchived(table, archiveTable);
			ViewboxApplication.Database.SystemDb.Objects[table.Id].IsUnderArchiving = false;
		}

		private Transformation(RightType right, TableType type)
		{
			_jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			StartJob(delegate(object obj, CancellationToken token)
			{
				DoChangeRights((RightType)((object[])obj)[0], (TableType)((object[])obj)[1], (string)((object[])obj)[2], (int)((object[])obj)[3], (CredentialType)((object[])obj)[4], (int)((object[])obj)[5], (IUser)((object[])obj)[6], token);
			}, new object[7]
			{
				right,
				type,
				ViewboxSession.SelectedSystem,
				ViewboxSession.User.Id,
				ViewboxSession.RightsModeCredential.Type,
				ViewboxSession.RightsModeCredential.Id,
				ViewboxSession.User
			});
			base.JobFinished += delegate(Base j)
			{
				Janitor.RegisterTempObject(j.Key);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
			};
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				base.Descriptions[i.CountryCode] = "Table rights change";
			}
		}

		private Transformation(RightType right, RoleSettingsType type, string value = null)
		{
			_jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			StartJob(delegate(object obj, CancellationToken token)
			{
				DoChangeRoleSetting((RightType)((object[])obj)[0], (RoleSettingsType)((object[])obj)[1], (int)((object[])obj)[2], token, (string)((object[])obj)[3]);
			}, new object[4]
			{
				right,
				type,
				ViewboxSession.RightsModeCredential.Id,
				value
			});
			base.JobFinished += delegate(Base j)
			{
				Janitor.RegisterTempObject(j.Key);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
			};
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				base.Descriptions[i.CountryCode] = "Right '" + type.ToString() + "' is applied as " + right.ToString() + ".";
			}
		}

		private Transformation(int documentId)
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
				DownloadBluelineDocument((int)((object[])obj)[0], token);
			}, new object[1] { documentId });
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(i.CountryCode);
				base.Descriptions[i.CountryCode] = string.Format("{0}", Resources.ResourceManager.GetString("DownloadBluelineDocuments", culture));
			}
		}

		private Transformation(ITableObjectCollection tableObjects, IIssueCollection issues)
		{
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
				HideEveryUnreachableTables((ITableObjectCollection)((object[])obj)[0]);
			}, new object[2] { tableObjects, issues });
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				base.Descriptions[i.CountryCode] = "{Hide every unreachable tables}";
			}
		}

		private Transformation(string descriptor, string search, bool download = false, string contentType = "")
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
				GetBlueLineDocuments((string)((object[])obj)[0], token, (string)((object[])obj)[1], (bool)((object[])obj)[2], (string)((object[])obj)[3]);
			}, new object[4] { descriptor, search, download, contentType });
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(i.CountryCode);
				base.Descriptions[i.CountryCode] = string.Format("{0}", Resources.ResourceManager.GetString("GetBluelineDocuments", culture));
			}
		}

		private void CreateArchiveDocumentMetaData(DatabaseBase conn, bool delete = true)
		{
			conn.DbMapping.CreateTableIfNotExists<ArchiveDocuments>();
			string tableName = conn.DbMapping.GetTableName<ArchiveDocuments>();
			ArchiveDocument table = conn.DbMapping.Load<ArchiveDocument>("type = " + 8).FirstOrDefault((ArchiveDocument w) => w.TableName == tableName);
			if (table == null)
			{
				table = new ArchiveDocument
				{
					CategoryId = 0,
					Database = conn.DbConfig.DbName,
					TableName = tableName,
					Type = TableType.ArchiveDocument,
					DefaultSchemeId = 0,
					IsVisible = true,
					RowCount = 0L,
					TransactionNumber = "0",
					UserDefined = false,
					Ordinal = 0
				};
				conn.DbMapping.Save(table);
			}
			List<Column> columns = conn.DbMapping.Load<Column>("table_id = " + table.Id);
			int ord = 0;
			if (columns.Any())
			{
				ord = columns.Max((Column w) => w.Ordinal) + 1;
			}
			foreach (DbColumnInfo c in conn.GetColumnInfos(conn.DbConfig.DbName, tableName))
			{
				if (!(c.Name.ToLower() == "_row_no_"))
				{
					Column column = columns.FirstOrDefault((Column w) => w.Name == c.Name);
					if (column == null)
					{
						column = new Column();
					}
					column.MaxLength = c.MaxLength;
					column.Name = c.Name;
					if (column.Ordinal == 0)
					{
						column.Ordinal = ord;
					}
					column.Table = table;
					column.IsVisible = new string[4] { "doc_id", "belegart", "created", "content_type" }.Any((string w) => w == c.Name.ToLower());
					column.IsEmpty = false;
					column.UserDefined = false;
					switch (c.Type)
					{
					case DbColumnTypes.DbNumeric:
						column.DataType = SqlType.Decimal;
						break;
					case DbColumnTypes.DbInt:
					case DbColumnTypes.DbBigInt:
						column.DataType = SqlType.Integer;
						break;
					case DbColumnTypes.DbBool:
						column.DataType = SqlType.Boolean;
						break;
					case DbColumnTypes.DbDate:
						column.DataType = SqlType.Date;
						break;
					case DbColumnTypes.DbDateTime:
						column.DataType = SqlType.DateTime;
						break;
					case DbColumnTypes.DbText:
					case DbColumnTypes.DbLongText:
					case DbColumnTypes.DbBinary:
					case DbColumnTypes.DbUnknown:
						column.DataType = SqlType.String;
						break;
					case DbColumnTypes.DbTime:
						column.DataType = SqlType.Time;
						break;
					}
					conn.DbMapping.Save(column);
					table.Columns.Add(column);
					ord++;
				}
			}
			if (delete)
			{
				conn.DbMapping.Delete<ArchiveDocuments>("type = 1");
			}
		}

		private void HideEveryUnreachableTables(ITableObjectCollection tableObjects)
		{
			ILog log = LogHelper.GetLogger();
			log.Info("Hide every unreachable tables started.");
			using (DatabaseBase db = ViewboxApplication.Database.ConnectionManager.GetConnection())
			{
				HashSet<string> databases = new HashSet<string>(db.GetSchemaList(), StringComparer.OrdinalIgnoreCase);
				Dictionary<string, HashSet<string>> dbTables = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
				int updateStmtNr = db.Prepare(string.Format("UPDATE {0} SET visible = 0 WHERE id = @id LIMIT 1", db.Enquote(db.DbConfig.DbName, "tables")));
				foreach (ITableObject obj in tableObjects.Where((ITableObject o) => o != null && o.IsVisible && o.Id > 0))
				{
					ITableObject to = ((obj is Issue && ((Issue)obj).OriginalId > 0) ? tableObjects[((Issue)obj).OriginalId] : obj);
					string database = ((to.UserDefined && to.GetType() != typeof(Issue)) ? ViewboxApplication.Database.TempDatabase.ConnectionManager.DbConfig.DbName : to.Database);
					if (databases.Contains(database) && !dbTables.ContainsKey(database))
					{
						dbTables.Add(database, new HashSet<string>(db.GetTableList(database), StringComparer.OrdinalIgnoreCase));
					}
					if (!databases.Contains(database) || !dbTables[database].Contains(to.TableName))
					{
						to.IsVisible = false;
						db.SetParameterForPreparedStmt(updateStmtNr, "@id", to.Id);
						db.ExecutePreparedNonQuery(updateStmtNr);
						log.Info("Table hided: " + to.Id);
						if (obj is Issue && ((Issue)obj).OriginalId > 0)
						{
							obj.IsVisible = false;
							db.SetParameterForPreparedStmt(updateStmtNr, "@id", obj.Id);
							db.ExecutePreparedNonQuery(updateStmtNr);
							log.Info("Table hided: " + obj.Id);
						}
					}
				}
				db.ClosePreparedStmt(updateStmtNr);
			}
			log.Info("Hide every unreachable tables ended.");
		}

		private void GetBlueLineDocuments(string descriptor, CancellationToken token, string search, bool download = false, string contentType = "")
		{
			ILog log = LogHelper.GetLogger();
			log.Info("Get blueline documents started.");
			using OttoDocumentService service = new OttoDocumentService();
			InitParameter initParameter = new InitParameter();
			if (!service.Init(initParameter))
			{
				throw new Exception("Get blueline documents error (Init): " + initParameter.Errors);
			}
			log.Info("Get blueline documents. Init successFull.");
			DescriptorListParameter descriptorListParameter = new DescriptorListParameter
			{
				CloseServer = false
			};
			if (!service.GetDescriptors(descriptorListParameter))
			{
				throw new Exception("Get blueline documents error (GetDescriptors): " + descriptorListParameter.Errors);
			}
			log.Info("Get blueline documents. Getting descriptors successFull. Count: " + descriptorListParameter.output.Descriptors.Count);
			IDescriptor desc = descriptorListParameter.output.Descriptors.FirstOrDefault((IDescriptor w) => w.Id == descriptor);
			if (desc == null)
			{
				throw new Exception("Get blueline documents error. Can't find desciptor: " + descriptor);
			}
			log.Info("Get blueline documents. Given descriptor avaible: " + descriptor);
			DocumentListParameter parameter = new DocumentListParameter
			{
				input = new DocumentSearch
				{
					DescriptorId = desc.Id,
					Search = "'" + search + "'"
				},
				CloseServer = false
			};
			if (!service.GetDocumentList(parameter))
			{
				throw new Exception("Get blueline documents error (GetDocumentList): " + parameter.Errors);
			}
			log.Info("Get blueline documents. Getting documents successfull. Count: " + parameter.output.DocumentList.Count);
			int rowno = 1;
			using (DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection())
			{
				CreateArchiveDocumentMetaData(conn, delete: false);
				List<ArchiveDocuments> docs = conn.DbMapping.Load<ArchiveDocuments>("type = 1");
				foreach (IDocument item in parameter.output.DocumentList)
				{
					DocumentInfoParameter infoParam = new DocumentInfoParameter
					{
						CloseServer = false,
						document = item
					};
					if (service.GetDocumentInfo(infoParam))
					{
						ArchiveDocuments document = docs.FirstOrDefault((ArchiveDocuments w) => w.Identifier == infoParam.document.Id);
						if (document == null)
						{
							document = new ArchiveDocuments
							{
								RowNo = rowno++,
								Identifier = infoParam.document.Id,
								Type = 1
							};
							if (infoParam.document.Descriptors.ContainsKey("contentLength"))
							{
								int size = 0;
								int.TryParse(infoParam.document.Descriptors["contentLength"], out size);
								document.Size = size;
							}
							if (infoParam.document.Descriptors.ContainsKey("docId"))
							{
								document.DocId = infoParam.document.Descriptors["docId"];
							}
							else
							{
								document.DocId = search;
							}
							if (infoParam.document.Descriptors.ContainsKey("contentType"))
							{
								document.ContentType = infoParam.document.Descriptors["contentType"];
							}
							if (infoParam.document.Descriptors.ContainsKey("Belegart"))
							{
								document.Belegart = infoParam.document.Descriptors["Belegart"];
							}
							if (string.IsNullOrEmpty(document.ContentType))
							{
								document.ContentType = ((!string.IsNullOrEmpty(contentType)) ? contentType : "application/pdf");
							}
							if (infoParam.document.Descriptors.ContainsKey("dateC") && infoParam.document.Descriptors.ContainsKey("timeC"))
							{
								CultureInfo culture3 = CultureInfo.CreateSpecificCulture("de");
								if (DateTime.TryParse(infoParam.document.Descriptors["dateC"] + " " + infoParam.document.Descriptors["timeC"], culture3, DateTimeStyles.None, out var dateResult3))
								{
									document.Created = dateResult3;
								}
							}
							if (infoParam.document.Descriptors.ContainsKey("dateM") && infoParam.document.Descriptors.ContainsKey("timeM"))
							{
								CultureInfo culture2 = CultureInfo.CreateSpecificCulture("de");
								if (DateTime.TryParse(infoParam.document.Descriptors["dateM"] + " " + infoParam.document.Descriptors["timeM"], culture2, DateTimeStyles.None, out var dateResult2))
								{
									document.Created = dateResult2;
								}
							}
							if (infoParam.document.Descriptors.ContainsKey("Belegdatum") && infoParam.document.Descriptors.ContainsKey("Uhrzeit_1"))
							{
								CultureInfo culture = CultureInfo.CreateSpecificCulture("de");
								if (DateTime.TryParseExact(infoParam.document.Descriptors["Belegdatum"] + " " + infoParam.document.Descriptors["Uhrzeit_1"], "yyyyMMdd HHmmss", culture, DateTimeStyles.None, out var dateResult))
								{
									document.Created = dateResult;
								}
							}
							docs.Add(document);
							conn.DbMapping.Save(document);
						}
						string tableName = conn.DbMapping.GetTableName<ArchiveDocuments>();
						SystemDb.Internal.TableObject table = conn.DbMapping.Load<ArchiveDocument>("type = " + 8).FirstOrDefault((ArchiveDocument w) => w.TableName == tableName);
						if (table != null)
						{
							table.RowCount = docs.Count;
							conn.DbMapping.Save(table);
						}
						if (download && document.Id > 0)
						{
							DownloadBluelineDocument(document.Id, token);
							log.Info("Get blueline documents ended.");
							return;
						}
					}
					else
					{
						log.Error("Warning in GetDocuments: " + infoParam.Errors);
					}
				}
			}
			log.Info("Get blueline documents ended.");
			if (string.IsNullOrEmpty(search) || search == "%")
			{
				notificationData = new NotificationUrl();
				notificationData.Controller = "Settings";
				notificationData.Method = "AdminTasks";
			}
		}

		private void DownloadBluelineDocument(int documentId, CancellationToken token)
		{
			ILog log = LogHelper.GetLogger();
			log.Info("Download blueline documents started.");
			using OttoDocumentService service = new OttoDocumentService();
			using DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
			ArchiveDocuments document = conn.DbMapping.Load<ArchiveDocuments>("Id = " + documentId).FirstOrDefault();
			if (document != null)
			{
				string fileName = ViewboxApplication.TemporaryDirectory + document.Identifier.Replace(":", "") + MimeToExtension.ConvertMimeToExt(document.ContentType);
				if (!File.Exists(fileName))
				{
					InitParameter initParameter = new InitParameter();
					if (!service.Init(initParameter))
					{
						throw new Exception("Download blueline document error (Init): " + initParameter.Errors);
					}
					log.Info("Download blueline document. Init successFull.");
					DocumentParameter parameter = new DocumentParameter
					{
						CloseServer = false,
						input = new Document
						{
							Id = document.Identifier
						}
					};
					if (service.GetDocument(parameter))
					{
						File.WriteAllBytes(fileName, parameter.output.Binary);
					}
					else
					{
						log.Error("Warning in GetDocuments: " + parameter.Errors);
					}
					log.Info("Download blueline document ended.");
				}
				notificationData = new NotificationUrl();
				notificationData.Controller = "ArchiveDocuments";
				notificationData.Method = "OpenDocument";
				notificationData.Params = new TableListParams
				{
					Search = fileName
				};
				return;
			}
			throw new Exception("Download blueline document error - can't find document - Id =  " + documentId);
		}

		public override void Cancel()
		{
			base.Cancel();
			if (_connection != null)
			{
				_connection.CancelCommand();
			}
			_connection = null;
		}

		public override void CleanUp()
		{
			base.CleanUp();
			_jobs.Remove(base.Key);
			try
			{
				string tkey = TransformationObject.Key;
				ViewboxApplication.Database.TempDatabase.RemoveTempTable(tkey);
			}
			catch
			{
			}
		}

		private void DoExecuteNonQuery(DatabaseBase connection, string insertSql, Hashtable parameters = null)
		{
			_connection = connection;
			if (parameters == null || parameters.Count == 0)
			{
				using (_connection)
				{
					_connection.ExecuteNonQuery(insertSql);
				}
				_connection = null;
				return;
			}
			using (_connection)
			{
				using IDbCommand cmd = _connection.GetDbCommand();
				cmd.Connection = connection.Connection;
				cmd.CommandText = insertSql;
				foreach (object paramName in parameters.Keys)
				{
					IDbDataParameter pValue = cmd.CreateParameter();
					pValue.ParameterName = paramName.ToString();
					pValue.Value = parameters[paramName];
					if (pValue.Value != null && pValue.Value.ToString().StartsWith("0x"))
					{
						pValue.DbType = DbType.Binary;
						pValue.Value = Filter.StringToByteArrayFastest(pValue.Value.ToString());
						pValue.DbType = DbType.Binary;
					}
					cmd.Parameters.Add(pValue);
				}
				cmd.ExecuteNonQuery();
			}
		}

		private void DoCallProcedure(DatabaseBase connection, string commandText, Dictionary<string, object> cmdParams, IUser user)
		{
			_connection = connection;
			string transactionKey = (TransactionKey = commandText + user.UserName);
			using (_connection)
			{
				if (!ViewboxSession.CurrentlyRunningProcedures.Keys.Contains(transactionKey))
				{
					ViewboxSession.CurrentlyRunningProcedures.TryAdd(transactionKey, string.Empty);
					try
					{
						_connection.CallProcedure(commandText, cmdParams);
					}
					catch (Exception ex)
					{
						using (NDC.Push(LogHelper.GetNamespaceContext()))
						{
							_log.Error(ex.Message, ex);
						}
						throw;
					}
					finally
					{
						string cmd = "";
						ViewboxSession.CurrentlyRunningProcedures.TryRemove(transactionKey, out cmd);
					}
				}
				else
				{
					while (ViewboxSession.CurrentlyRunningProcedures.Keys.Contains(transactionKey))
					{
						Thread.Sleep(2000);
					}
					DoCallProcedure(connection, commandText, cmdParams, user);
				}
			}
		}

		private void DoGenerate()
		{
			ViewboxApplication.Database.SystemDb.CreateEmptyDistinctTable();
			notificationData = new NotificationUrl();
			notificationData.Controller = "Settings";
			notificationData.Method = "AdminTasks";
		}

		private void DoArchive(ITableObject table, ArchiveType archive, long optimizedRowCount, CancellationToken token)
		{
			ViewboxApplication.Database.ArchiveTable(table, archive, token);
			notificationData = new NotificationUrl();
			notificationData.Controller = ((table.Type == TableType.Table) ? "TableList" : "ViewList");
			notificationData.Method = "Index";
			if (archive == ArchiveType.Archive)
			{
				table.EngineType = EngineTypes.Archive;
				notificationData.Params = new TableListParams
				{
					Search = null,
					ShowArchived = true,
					ShowHidden = false,
					ShowEmpty = false,
					ShowEmptyHidden = false
				};
			}
			else
			{
				table.EngineType = EngineTypes.Undefined;
				notificationData.Params = new TableListParams
				{
					Search = null,
					ShowArchived = false,
					ShowHidden = !table.IsVisible,
					ShowEmpty = (table.IsVisible && optimizedRowCount == 0),
					ShowEmptyHidden = false
				};
			}
		}

		private void DoArchive(ITableObject table, ArchiveType archive)
		{
			ViewboxApplication.Database.ArchiveTable(table, archive);
		}

		private void DoChangeRights(RightType right, TableType type, string selectedSystem, int userId, CredentialType rightsModeCredentialType, int rightsModeCredentialId, IUser grantUser, CancellationToken token)
		{
			int[] idList = ViewboxApplication.Database.SystemDb.ReadTableTypeIds(type, selectedSystem, userId);
			int[] array = idList;
			foreach (int tableId in array)
			{
				ViewboxSession.UpdateTableObjRightJob(tableId, UpdateRightType.TableObject, right, rightsModeCredentialType, rightsModeCredentialId, grantUser);
				token.ThrowIfCancellationRequested();
			}
			notificationData = new NotificationUrl();
			switch (type)
			{
			case TableType.Table:
				notificationData.Controller = "TableList";
				break;
			case TableType.View:
				notificationData.Controller = "ViewList";
				break;
			case TableType.Issue:
				notificationData.Controller = "IssueList";
				break;
			case TableType.Archive:
				notificationData.Controller = "Documents";
				break;
			}
			notificationData.Method = "Index";
			notificationData.Params = new TableListParams();
		}

		private void DoChangeRoleSetting(RightType right, RoleSettingsType type, int roleId, CancellationToken token, string value = null)
		{
			ViewboxApplication.Database.SystemDb.UpdateRoleSetting(right, type, roleId, value);
		}

		public void DoSortAndFilter(int id, ViewboxDb.TableObjectCollection tempTableObjects, ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, SortCollection sortList, IFilter filter, IUser user, int scrollPosition, object[] paramValues, object[] displayValues, List<int> itemId, List<int> selectionTypes, RelationType relationType, string relationExtInfo, string relationColumnExtInfo, CancellationToken token, List<int> summedColumns, bool originalColumnIds = true, SubTotalParameters groupSubTotal = null, bool multipleOptimization = false, IDictionary<int, Tuple<int, int>> optimizationSelected = null, ILanguage language = null)
		{
			bool optChanged = false;
			if (ViewboxSession.LastOptimization == null)
			{
				ViewboxSession.LastOptimization = opt;
			}
			if (ViewboxSession.LastOptimization != opt)
			{
				optChanged = true;
				ViewboxSession.LastOptimization = opt;
			}
			if (ViewboxSession.OpenIssueDate == null)
			{
				ViewboxSession.OpenIssueDate = ViewboxSession.OpenIssueDateDefine();
			}
			ITableObject objTable = ((id < 0) ? tempTableObjects[id].Table : tableObjects[id]);
			ViewboxSession.OpenIssueDate.Remove(objTable.Id);
			ViewboxSession.OpenIssueDate.Add(objTable.Id, DateTime.Now.ToString(Thread.CurrentThread.CurrentCulture));
			string filterIssue = string.Empty;
			SimpleTableModel stm = new SimpleTableModel();
			if (paramValues != null)
			{
				bool isStoredProcedure = ViewboxSession.IsStoredProcedure(id, tempTableObjects, tableObjects);
				IIssue issue2;
				if (isStoredProcedure)
				{
					if (summedColumns == null && groupSubTotal == null)
					{
						ViewboxSession.ExecuteIssue(DoCallProcedure, id, tempTableObjects, tableObjects, opt, user, paramValues);
					}
					else
					{
						try
						{
							if (optChanged && (summedColumns == null || groupSubTotal == null))
							{
								ViewboxSession.ExecuteIssue(DoCallProcedure, id, tempTableObjects, tableObjects, opt, user, paramValues);
							}
							else
							{
								IIssue check = ((id < 0) ? (tempTableObjects[id].OriginalTable as IIssue) : (tableObjects[id] as IIssue));
								if (check == null)
								{
									ViewboxSession.ExecuteIssue(DoCallProcedure, id, tempTableObjects, tableObjects, opt, user, paramValues);
								}
							}
						}
						catch
						{
							ViewboxSession.ExecuteIssue(DoCallProcedure, id, tempTableObjects, tableObjects, opt, user, paramValues);
						}
					}
					issue2 = ((id < 0) ? (tempTableObjects[id].OriginalTable as IIssue) : (tableObjects[id] as IIssue));
					BuildSimpleTableStructure(issue2, displayValues, stm, itemId);
				}
				else
				{
					issue2 = ((id < 0) ? (tempTableObjects[id].OriginalTable as IIssue) : (tableObjects[id] as IIssue));
					List<string> savedFilterTypes = Filter.GetFilterTypesFromIssueCommand(issue2.Command);
					BuildSimpleTableStructure(issue2, paramValues, stm, itemId, savedFilterTypes);
				}
				filterIssue = GenerateFilterIssueAndDisplayParameters(issue2, stm, isStoredProcedure, selectionTypes, language, opt, user);
			}
			long tempTableSize = user.DisplayRowCount;
			ViewboxDb.TableObject tobj = tempTableObjects[id];
			object filterValue = null;
			if (relationType == RelationType.OttoBlueLine)
			{
				AndFilter andFilter = filter as AndFilter;
				if (andFilter?.Conditions.Any((IFilter w) => w is ColValueFilter) ?? false)
				{
					filterValue = (andFilter.Conditions.FirstOrDefault((IFilter w) => w is ColValueFilter) as ColValueFilter).Value;
				}
			}
			ViewboxDb.TableObject obj;
			if (tobj != null)
			{
				bool joinSort = tobj.Sort != null && tobj.Sort.Count != 0 && sortList == null && tobj.Filter == null;
				bool joinFilter = tobj.Filter != null && filter == null;
				bool joinIssue = tobj.ParamValues != null && !string.IsNullOrWhiteSpace(tobj.FilterIssue) && ((filter != null && tobj.Filter == null) || (sortList != null && sortList.Count() > 0));
				IFilter filterToUse = ((tobj.Sort != null || sortList != null) ? filter : filter);
				object[] disp = null;
				if (displayValues != null)
				{
					disp = displayValues;
				}
				else if (tobj.DisplayValues != null)
				{
					disp = ((IEnumerable<object>)tobj.DisplayValues).ToArray();
				}
				obj = ViewboxSession.CreateTableObject(DoExecuteNonQuery, id, tempTableObjects, tableObjects, columns, opt, user, token, sortList ?? tobj.Sort, filterToUse, paramValues ?? tobj.ParamValues, disp, itemId, selectionTypes, string.IsNullOrWhiteSpace(filterIssue) ? tobj.FilterIssue : filterIssue, (joinSort || joinFilter || joinIssue) ? tobj.Key : string.Empty, joinSort, joinFilter, joinIssue, onlyCount: false, null, tempTableSize, summedColumns, originalColumnIds, groupSubTotal, multipleOptimization, optimizationSelected);
				if (relationType == RelationType.OttoBlueLine && !string.IsNullOrEmpty(relationExtInfo) && obj.Table != null && obj.Table.RowCount == 0L && filterValue != null)
				{
					GetBlueLineDocuments(relationExtInfo, token, filterValue.ToString(), download: false, relationColumnExtInfo);
					obj = ViewboxSession.CreateTableObject(DoExecuteNonQuery, id, tempTableObjects, tableObjects, columns, opt, user, token, sortList ?? tobj.Sort, filter ?? tobj.Filter, paramValues ?? tobj.ParamValues, disp, itemId, selectionTypes, string.IsNullOrWhiteSpace(filterIssue) ? tobj.FilterIssue : filterIssue, (joinSort || joinFilter || joinIssue) ? tobj.Key : string.Empty, joinSort, joinFilter, joinIssue, onlyCount: false, null, tempTableSize, summedColumns, originalColumnIds, groupSubTotal, multipleOptimization, optimizationSelected);
				}
			}
			else
			{
				obj = ViewboxSession.CreateTableObject(DoExecuteNonQuery, id, tempTableObjects, tableObjects, columns, opt, user, token, sortList, filter, paramValues, displayValues, itemId, selectionTypes, filterIssue, null, joinSort: false, joinFilter: false, joinIssue: false, onlyCount: false, null, tempTableSize, summedColumns, originalColumnIds, groupSubTotal, multipleOptimization, optimizationSelected);
				if (relationType == RelationType.OttoBlueLine && !string.IsNullOrEmpty(relationExtInfo) && obj.Table != null && obj.Table.RowCount == 0L && filterValue != null)
				{
					GetBlueLineDocuments(relationExtInfo, token, filterValue.ToString(), download: false, relationColumnExtInfo);
					obj = ViewboxSession.CreateTableObject(DoExecuteNonQuery, id, tempTableObjects, tableObjects, columns, opt, user, token, sortList, filter, paramValues, displayValues, itemId, selectionTypes, filterIssue, null, joinSort: false, joinFilter: false, joinIssue: false, onlyCount: false, null, tempTableSize, summedColumns, originalColumnIds, groupSubTotal, multipleOptimization, optimizationSelected);
				}
			}
			if (obj.OriginalTable is IIssue)
			{
				switch (((IIssue)obj.OriginalTable).Flag)
				{
				case 0:
				case 42:
				case 43:
					obj.Additional = stm;
					break;
				case 1:
				case 10:
				{
					SapBalanceModel sapBalance = new SapBalanceModel();
					foreach (Tuple<string, string> param in stm.DisplayParameters)
					{
						sapBalance.DisplayParameters.Add(param);
					}
					BuildBalanceStructure(obj, sapBalance, ViewboxApplication.UseNewIssueMethod ? user.UserTable(obj.Table.Database) : obj.Table.Database, token, LanguageKeyTransformer.Transformer(language.CountryCode));
					obj.Additional = sapBalance;
					break;
				}
				case 2:
				case 3:
				{
					DcwBalanceModel dcwBalance = new DcwBalanceModel();
					BuildDcwBalanceStructure(obj, dcwBalance, ViewboxApplication.UseNewIssueMethod ? user.UserTable(obj.Table.Database) : obj.Table.Database, (obj.OriginalTable as IIssue).Flag - 2, token);
					obj.Additional = dcwBalance;
					break;
				}
				case 4:
				case 5:
				case 6:
				case 11:
				case 12:
				case 13:
				case 14:
				{
					StructuedTableModel structuedTable = new StructuedTableModel();
					BuildStructuredTable(obj, structuedTable, ViewboxApplication.UseNewIssueMethod ? user.UserTable(obj.Table.Database) : obj.Table.Database);
					obj.Additional = structuedTable;
					break;
				}
				case 7:
				case 8:
				{
					IIssue issue = obj.OriginalTable as IIssue;
					string offset = "0";
					foreach (Tuple<SystemDb.IParameter, string, string> param2 in stm.Parameters)
					{
						if (param2.Item1.ColumnName == "KSTAR0" || param2.Item1.ColumnName == "KOSTL0")
						{
							offset = ((param2.Item2 == null) ? "0" : param2.Item2.ToString());
							break;
						}
					}
					StructuedTableModel kostenartenhierarhy = new StructuedTableModel(offset);
					BuildStructuredTable(obj, kostenartenhierarhy, ViewboxApplication.UseNewIssueMethod ? user.UserTable(obj.Table.Database) : obj.Table.Database, stm.Parameters);
					obj.Additional = kostenartenhierarhy;
					break;
				}
				case 100:
				case 101:
				case 102:
				{
					UniversalTableModel universalTable = new UniversalTableModel();
					foreach (Tuple<string, string> param3 in stm.DisplayParameters)
					{
						universalTable.DisplayParameters.Add(param3);
					}
					BuilduniversalTable(obj, universalTable, user.UserTable(obj.Table.Database), language, stm.Parameters);
					obj.Additional = universalTable;
					break;
				}
				}
			}
			while (string.IsNullOrWhiteSpace(obj.Key))
			{
				Thread.Sleep(1000);
			}
			obj.ScrollPosition = scrollPosition;
			TransformationObject = obj;
		}

		private string GenerateFilterIssueAndDisplayParameters(IIssue issue, SimpleTableModel stm, bool isStoredProcedure, List<int> selectionTypes, ILanguage language, IOptimization opt, IUser user)
		{
			stm.DisplayParameters.Clear();
			Tuple<SystemDb.IParameter, string, string> lastParameter = null;
			string textBufferDescription = string.Empty;
			string textBufferDisplayValue = string.Empty;
			int selectionIdx = 0;
			List<string> incommingParameters = new List<string>();
			string issueExtension = string.Empty;
			string[] replacAbles = new string[11]
			{
				"<",
				">",
				"=",
				"!=",
				Operators.LessOrEqual.GetOpString(),
				Operators.GreaterOrEqual.GetOpString(),
				"∈",
				"?",
				Operators.StartsWith.GetOpString(),
				Operators.Like.GetOpString(),
				"\u02dc"
			};
			string[] replacAbles2 = new string[2] { "Von", "from" };
			for (int i = 0; i < stm.Parameters.Count; i++)
			{
				if (i + 1 < stm.Parameters.Count)
				{
					if (stm.Parameters[i].Item1.GroupId != 0)
					{
						if (stm.Parameters[i].Item1.GroupId != stm.Parameters[i + 1].Item1.GroupId)
						{
							stm.Parameters[i].Item1.GroupId = 0;
						}
						else
						{
							i++;
						}
					}
				}
				else if (stm.Parameters[i].Item1.GroupId != 0)
				{
					stm.Parameters[i].Item1.GroupId = 0;
				}
			}
			foreach (Tuple<SystemDb.IParameter, string, string> p in stm.Parameters)
			{
				string displayValue = GetDisplayValueForParameter(p);
				string[] toReplaces = ((p.Item1.GroupId == 0) ? replacAbles : replacAbles2);
				string description = GetDescriptionForParameter(p, language, toReplaces);
				if (p.Item1.FreeSelection == 0)
				{
					string label3 = Resources.DisplayEqualsFilter;
					if (p.Item1.GroupId == 0)
					{
						switch (p.Item3)
						{
						case "<":
							label3 = Resources.LessFilter;
							break;
						case "<=":
							label3 = Resources.LessOrEqualFilter;
							break;
						case ">":
							label3 = Resources.GreaterFilter;
							break;
						case ">=":
							label3 = Resources.GreaterOrEqualFilter;
							break;
						case "!=":
							label3 = Resources.DisplayNotEqualsFilter;
							break;
						case "in":
							label3 = Resources.DisplayContainsFilter;
							break;
						case "startswith":
							label3 = Resources.StartsWithFilter;
							break;
						case "like":
							label3 = Resources.LikeFilter;
							break;
						}
					}
					bool force = false;
					if (p.Item1.GroupId != 0 || p.Item3 == "between" || p.Item3 == "not between")
					{
						if (lastParameter == null)
						{
							lastParameter = p;
							textBufferDescription = description;
							textBufferDisplayValue = $"{displayValue} {Resources.And} ";
							continue;
						}
						if (p.Item2.Trim() != string.Empty && lastParameter.Item2.Trim() != string.Empty)
						{
							displayValue = textBufferDisplayValue + displayValue;
							description = textBufferDescription;
							label3 = ((p.Item3 == "not between") ? Resources.DisplayNotBetweenFilter : Resources.DisplayBetweenFilter);
						}
						else if (!isStoredProcedure)
						{
							if (lastParameter.Item2.Trim() != string.Empty)
							{
								issueExtension = $"(`{lastParameter.Item1.ColumnNameInView}` = {ViewboxApplication.Database.GetFilterIssueValue(lastParameter.Item2, lastParameter.Item1.DataType)})";
								if (!incommingParameters.Contains(issueExtension))
								{
									incommingParameters.Add(issueExtension);
								}
								displayValue = GetDisplayValueForParameter(lastParameter);
								description = textBufferDescription;
								label3 = Resources.DisplayEqualsFilter;
								force = true;
							}
							else if (p.Item2.Trim() != string.Empty)
							{
								issueExtension = $"(`{p.Item1.ColumnNameInView}` <= {ViewboxApplication.Database.GetFilterIssueValue(p.Item2, p.Item1.DataType)})";
								if (!incommingParameters.Contains(issueExtension))
								{
									incommingParameters.Add(issueExtension);
								}
								label3 = Resources.LessOrEqualFilter;
							}
						}
						else if (isStoredProcedure && lastParameter.Item2.Trim() != string.Empty)
						{
							issueExtension = $"(`{lastParameter.Item1.ColumnNameInView}` = {ViewboxApplication.Database.GetFilterIssueValue(lastParameter.Item2, lastParameter.Item1.DataType)})";
							if (!incommingParameters.Contains(issueExtension))
							{
								incommingParameters.Add(issueExtension);
							}
							displayValue = GetDisplayValueForParameter(lastParameter);
							description = textBufferDescription;
							label3 = Resources.DisplayEqualsFilter;
							force = true;
						}
						lastParameter = null;
					}
					if (ViewboxApplication.Database.GetFilterIssueValue(p.Item2, p.Item1.DataType) != "''" || force)
					{
						Tuple<string, string> temp4 = new Tuple<string, string>($"{description} {label3.ToLower()} ", displayValue);
						if (!stm.DisplayParameters.Any((Tuple<string, string> x) => x.Item1 == temp4.Item1 && x.Item2 == temp4.Item2))
						{
							stm.DisplayParameters.Add(temp4);
						}
					}
					selectionIdx++;
					continue;
				}
				int selection = ((selectionTypes != null && selectionTypes.Count > selectionIdx) ? selectionTypes[selectionIdx] : 0);
				if (p.Item1.GroupId == 0 || selection > 1)
				{
					if (lastParameter == null)
					{
						lastParameter = p;
						if (p.Item2 != null && p.Item2 != "" && p.Item2 != ";")
						{
							string label2 = string.Empty;
							switch (selection)
							{
							case 0:
								if (!isStoredProcedure)
								{
									issueExtension = $"(`{p.Item1.ColumnNameInView}` = {ViewboxApplication.Database.GetFilterIssueValue(p.Item2, p.Item1.DataType)})";
								}
								label2 = Resources.DisplayEqualsFilter;
								break;
							case 1:
								if (!isStoredProcedure)
								{
									issueExtension = $"(`{p.Item1.ColumnNameInView}` != {ViewboxApplication.Database.GetFilterIssueValue(p.Item2, p.Item1.DataType)})";
								}
								label2 = Resources.DisplayNotEqualsFilter;
								break;
							case 2:
							{
								string[] inputs = GetInputAndDisplayValuesForContainsFilter(p, out displayValue);
								if (!isStoredProcedure)
								{
									issueExtension = string.Format("(`{0}` IN ({1}))", p.Item1.ColumnNameInView, string.Join(",", inputs));
								}
								label2 = Resources.DisplayContainsFilter;
								break;
							}
							case 3:
							{
								string[] inputs = GetInputAndDisplayValuesForContainsFilter(p, out displayValue);
								if (!isStoredProcedure)
								{
									issueExtension = string.Format("(`{0}` NOT IN ({1}))", p.Item1.ColumnNameInView, string.Join(",", inputs));
								}
								label2 = Resources.DisplayNotContainsFilter;
								break;
							}
							}
							Tuple<string, string> temp3 = new Tuple<string, string>($"{description} {label2.ToLower()} ", displayValue);
							if (!stm.DisplayParameters.Any((Tuple<string, string> x) => x.Item1 == temp3.Item1 && x.Item2 == temp3.Item2))
							{
								stm.DisplayParameters.Add(temp3);
							}
							if (!isStoredProcedure && !incommingParameters.Contains(issueExtension))
							{
								incommingParameters.Add(issueExtension);
							}
						}
						if (p.Item1.GroupId == 0)
						{
							selectionIdx++;
						}
					}
					else
					{
						lastParameter = null;
						selectionIdx++;
					}
					continue;
				}
				if (lastParameter == null)
				{
					lastParameter = p;
					textBufferDisplayValue = $"{displayValue} {Resources.And} ";
					switch (selection)
					{
					case 0:
						if (!isStoredProcedure)
						{
							issueExtension = $"(`{p.Item1.ColumnNameInView}` BETWEEN {ViewboxApplication.Database.GetFilterIssueValue(p.Item2, p.Item1.DataType)} AND ";
						}
						textBufferDescription = $"{description} {Resources.DisplayBetweenFilter.ToLower()} ";
						break;
					case 1:
						if (!isStoredProcedure)
						{
							issueExtension = $"(`{p.Item1.ColumnNameInView}` NOT BETWEEN {ViewboxApplication.Database.GetFilterIssueValue(p.Item2, p.Item1.DataType)} AND ";
						}
						textBufferDescription = $"{description} {Resources.DisplayNotBetweenFilter.ToLower()} ";
						break;
					}
					continue;
				}
				if (lastParameter.Item1.GroupId == p.Item1.GroupId)
				{
					if (p.Item2.Trim() != string.Empty && lastParameter.Item2.Trim() != string.Empty)
					{
						if (!isStoredProcedure)
						{
							issueExtension = issueExtension + ViewboxApplication.Database.GetFilterIssueValue(p.Item2, p.Item1.DataType) + ")";
							if (!incommingParameters.Contains(issueExtension))
							{
								incommingParameters.Add(issueExtension);
							}
						}
						textBufferDisplayValue += displayValue;
						if (!stm.DisplayParameters.Any((Tuple<string, string> dp) => dp.Item1 == textBufferDescription && dp.Item2 == textBufferDisplayValue))
						{
							stm.DisplayParameters.Add(new Tuple<string, string>(textBufferDescription, textBufferDisplayValue));
						}
					}
					else
					{
						description = GetDescriptionForParameter(lastParameter, language, replacAbles2);
						if (lastParameter.Item2 != "")
						{
							displayValue = GetDisplayValueForParameter(lastParameter);
							string label = string.Empty;
							switch (selection)
							{
							case 0:
								if (!isStoredProcedure)
								{
									issueExtension = $"(`{lastParameter.Item1.ColumnNameInView}` = {ViewboxApplication.Database.GetFilterIssueValue(lastParameter.Item2, lastParameter.Item1.DataType)})";
								}
								label = Resources.DisplayEqualsFilter;
								break;
							case 1:
								if (!isStoredProcedure)
								{
									issueExtension = $"(`{lastParameter.Item1.ColumnNameInView}` != {ViewboxApplication.Database.GetFilterIssueValue(lastParameter.Item2, lastParameter.Item1.DataType)})";
								}
								label = Resources.DisplayNotEqualsFilter;
								break;
							}
							Tuple<string, string> temp2 = new Tuple<string, string>($"{description} {label.ToLower()} ", displayValue);
							if (!stm.DisplayParameters.Any((Tuple<string, string> x) => x.Item1 == temp2.Item1 && x.Item2 == temp2.Item2))
							{
								stm.DisplayParameters.Add(temp2);
							}
							if (!isStoredProcedure && !incommingParameters.Contains(issueExtension))
							{
								incommingParameters.Add(issueExtension);
							}
						}
						else if (p.Item2 != "")
						{
							if (!isStoredProcedure)
							{
								issueExtension = $"(`{p.Item1.ColumnNameInView}` <= {ViewboxApplication.Database.GetFilterIssueValue(p.Item2, p.Item1.DataType)})";
								if (!incommingParameters.Contains(issueExtension))
								{
									incommingParameters.Add(issueExtension);
								}
							}
							Tuple<string, string> temp = new Tuple<string, string>($"{description} {Resources.LessOrEqualFilter.ToLower()} ", displayValue);
							if (!stm.DisplayParameters.Any((Tuple<string, string> x) => x.Item1 == temp.Item1 && x.Item2 == temp.Item2))
							{
								stm.DisplayParameters.Add(temp);
							}
						}
					}
				}
				selectionIdx++;
				lastParameter = null;
			}
			string filterIssue = string.Empty;
			if (!isStoredProcedure)
			{
				if (incommingParameters.Count > 0)
				{
					filterIssue = string.Format("({0})", string.Join(" AND ", incommingParameters));
				}
				Regex checkRegex = new Regex("[a-z]", RegexOptions.IgnoreCase);
				if (string.IsNullOrEmpty(filterIssue))
				{
					filterIssue = ViewboxApplication.Database.GetFilterIssue(issue, opt, stm.Parameters, appendOptFilter: false);
				}
				else if (checkRegex.IsMatch(issue.Command))
				{
					filterIssue = filterIssue + " AND " + ViewboxApplication.Database.GetFilterIssue(issue, opt, stm.Parameters, appendOptFilter: false);
				}
				if (filterIssue != string.Empty && !checkRegex.IsMatch(filterIssue))
				{
					filterIssue = string.Empty;
				}
			}
			return filterIssue;
		}

		private string GetDescriptionForParameter(Tuple<SystemDb.IParameter, string, string> p, ILanguage language, string[] toReplaces)
		{
			string description = p.Item1.GetDescription(language);
			foreach (string toReplace in toReplaces)
			{
				if (description.EndsWith(" " + toReplace, StringComparison.OrdinalIgnoreCase))
				{
					description = description.Substring(0, description.Length - toReplace.Length - 1).Trim();
					break;
				}
			}
			return description;
		}

		private string GetDisplayValueForParameter(Tuple<SystemDb.IParameter, string, string> p)
		{
			string displayValue = p.Item2;
			DateTime dateOutput;
			if (p.Item3 == "in")
			{
				GetInputAndDisplayValuesForContainsFilter(p, out var helper);
				displayValue = helper;
			}
			else if (p.Item1.DataType == SqlType.Date && DateTime.TryParse(displayValue, out dateOutput))
			{
				displayValue = dateOutput.ToShortDateString();
			}
			else if (displayValue == "0000-00-00")
			{
				displayValue = new DateTime(1, 1, 1).ToShortDateString().Replace("1", "0");
			}
			return displayValue;
		}

		private string[] GetInputAndDisplayValuesForContainsFilter(Tuple<SystemDb.IParameter, string, string> p, out string displayValues)
		{
			StringBuilder sb = new StringBuilder();
			string[] inputs = p.Item2.Replace("\\;", "SEMICOLON").Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < inputs.Count(); i++)
			{
				inputs[i] = inputs[i].Replace("SEMICOLON", ";").Trim();
				if (p.Item1.DataType == SqlType.Date && DateTime.TryParse(inputs[i], out var dateOutput))
				{
					sb.Append(dateOutput.ToString("dd.MM.yyyy"));
				}
				else
				{
					sb.Append(inputs[i]);
				}
				sb.Append(";");
				inputs[i] = ViewboxApplication.Database.GetFilterIssueValue(inputs[i], p.Item1.DataType);
			}
			if (sb.Length > 0)
			{
				sb.Length--;
			}
			displayValues = sb.ToString();
			return inputs;
		}

		private string replaceLanguageItem(string value, ILanguage language)
		{
			if (language.CountryCode.ToLower() == "en")
			{
				return value.Replace(" from", "");
			}
			if (language.CountryCode.ToLower() == "de")
			{
				return value.Replace(" von", "");
			}
			return value;
		}

		private void BuildStructuredTable(ViewboxDb.TableObject obj, StructuedTableModel structuredTable, string userDatabase, List<Tuple<SystemDb.IParameter, string, string>> paramList = null)
		{
			IIssue issue = obj.OriginalTable as IIssue;
			if (issue != null)
			{
				if (paramList == null)
				{
					int i = 0;
					foreach (SystemDb.IParameter param in issue.Parameters)
					{
						string value = ((obj.ParamValues[i] == null) ? "" : obj.ParamValues[i].ToString());
						structuredTable.Parameters.Add(new Tuple<SystemDb.IParameter, string, string>(param, value, ""));
						i++;
					}
				}
				else
				{
					foreach (Tuple<SystemDb.IParameter, string, string> param2 in paramList)
					{
						structuredTable.Parameters.Add(param2);
					}
				}
				_log.ContextLog(LogLevelEnum.Info, string.Format("Dynamic table generation with the following parameters: {0}", string.Join(";", structuredTable.GetParameters().Select((Tuple<string, string> x, int y) => $"{x}={y}"))));
				structuredTable.IssueFlag = issue.Flag;
			}
			string key = obj.Optimization.GetOptimizationValue(OptimizationType.IndexTable);
			if (issue.Flag == 4)
			{
				IEnumerable<GuvStructureRow> structure5 = ViewboxApplication.GetGuvBalanceStructure(obj.Table, userDatabase);
				IEnumerable<GuvStructureSignRow> data3 = ViewboxApplication.GetGuvBalanceData(obj.Table, key, userDatabase);
				List<GuvStructureRow> newList4 = structure5.OrderBy((GuvStructureRow x) => Convert.ToInt32(x.Parent)).ToList();
				foreach (GuvStructureRow row9 in newList4)
				{
					structuredTable.AddNode(row9.Pos, row9.Parent, row9.Description);
				}
				foreach (GuvStructureSignRow row10 in data3)
				{
					List<string> values = new List<string>();
					values.Add(row10.Monat_S);
					values.Add(row10.Year_S);
					structuredTable.AddLeaf(row10.Konto, row10.Pos, row10.Konto + " - " + row10.Konto_Be, values);
				}
			}
			else if (issue.Flag == 5)
			{
				if (key == "1")
				{
					structuredTable.ColumnNameList.Add("Abweichung %");
					structuredTable.ColumnNameList.Add("Vorjahressaldo");
					structuredTable.ColumnNameList.Add("Jahressaldo (JBD)");
				}
				else
				{
					structuredTable.IssueFlag = 4;
				}
				IEnumerable<BilanzStructureRow> structure8 = ViewboxApplication.GetBilanzBalanceStructure(obj.Table, userDatabase);
				List<BilanzStructureSignRow> data5 = ViewboxApplication.GetBilanzBalanceData(obj.Table, key, userDatabase);
				if (data5.Count <= 1)
				{
					return;
				}
				List<BilanzStructureRow> newList5 = structure8.OrderBy((BilanzStructureRow x) => Convert.ToInt32(x.Pos)).ToList();
				foreach (BilanzStructureRow row13 in newList5)
				{
					structuredTable.AddNode(row13.Pos, row13.Parent, row13.Description);
				}
				foreach (BilanzStructureSignRow row12 in data5)
				{
					List<string> values2 = new List<string>();
					values2.Add(row12.Monat_S);
					values2.Add(row12.Year_S);
					structuredTable.AddLeaf(row12.Konto, row12.Pos, row12.Konto + " - " + row12.Konto_Be, values2);
				}
			}
			else if (issue.Flag == 6 || issue.Flag == 13)
			{
				IEnumerable<IBilanzstrukturAnzeigen> structure7 = ViewboxApplication.GetBilanzstukturAnzeigen(obj.Table, key, userDatabase, issue.Flag == 13);
				foreach (IBilanzstrukturAnzeigen row11 in structure7)
				{
					structuredTable.BuildStructure(row11.id, row11.titel, row11.parent, isLeaf: false);
				}
				structuredTable.ReOrderNodes();
			}
			else if (issue.Flag == 7)
			{
				string language2 = null;
				language2 = ((structuredTable.Parameters.FirstOrDefault((Tuple<SystemDb.IParameter, string, string> x) => x.Item1.ColumnName == "LANG_KEY") == null) ? null : ((structuredTable.Parameters.FirstOrDefault((Tuple<SystemDb.IParameter, string, string> x) => x.Item1.ColumnName == "LANG_KEY").Item2 != null && !(structuredTable.Parameters.FirstOrDefault((Tuple<SystemDb.IParameter, string, string> x) => x.Item1.ColumnName == "LANG_KEY").Item2 == "")) ? structuredTable.Parameters.FirstOrDefault((Tuple<SystemDb.IParameter, string, string> x) => x.Item1.ColumnName == "LANG_KEY").Item2 : ViewboxSession.Language.CountryCode.ToCharArray().FirstOrDefault().ToString()
					.ToUpper()));
				if (language2 == null)
				{
					language2 = LanguageKeyTransformer.Transformer(ViewboxSession.Language.CountryCode);
				}
				IEnumerable<KostenartenhierarhyStructure> structure6 = ViewboxApplication.GetKostenartenhierarhyStructure(obj.Table, key, structuredTable.Parameters.FirstOrDefault((Tuple<SystemDb.IParameter, string, string> x) => x.Item1.ColumnName == "KTOPL").Item2, language2);
				List<KostenartenhierarhyData> data4 = ViewboxApplication.GetKostenartenhierarhyData(obj.Table, key, userDatabase);
				if (data4.Count <= 0)
				{
					return;
				}
				List<KostenartenhierarhyStructure> newList3 = structure6.OrderBy((KostenartenhierarhyStructure x) => x.Parent).ToList();
				foreach (KostenartenhierarhyStructure row8 in structure6)
				{
					structuredTable.AddNode(row8.Pos, row8.Parent, row8.Pos + " - " + row8.Description);
				}
				foreach (KostenartenhierarhyData row7 in data4)
				{
					structuredTable.AddLeaf(row7.Konto, row7.Pos, row7.Konto + " " + row7.Konto_Be, new List<string>());
				}
			}
			if (issue.Flag == 8)
			{
				string language = null;
				language = ((structuredTable.Parameters.FirstOrDefault((Tuple<SystemDb.IParameter, string, string> x) => x.Item1.ColumnName == "LANG_KEY") == null) ? null : ((structuredTable.Parameters.FirstOrDefault((Tuple<SystemDb.IParameter, string, string> x) => x.Item1.ColumnName == "LANG_KEY").Item2 != null && !(structuredTable.Parameters.FirstOrDefault((Tuple<SystemDb.IParameter, string, string> x) => x.Item1.ColumnName == "LANG_KEY").Item2 == "")) ? structuredTable.Parameters.FirstOrDefault((Tuple<SystemDb.IParameter, string, string> x) => x.Item1.ColumnName == "LANG_KEY").Item2 : ViewboxSession.Language.CountryCode.ToCharArray().FirstOrDefault().ToString()
					.ToUpper()));
				if (language == null)
				{
					language = LanguageKeyTransformer.Transformer(ViewboxSession.Language.CountryCode);
				}
				IEnumerable<KostenstellengruppeAnzeigenStruct> structure4 = ViewboxApplication.GetKostenstellengruppeAnzeigenStruct(obj.Table, key, structuredTable.Parameters.FirstOrDefault((Tuple<SystemDb.IParameter, string, string> x) => x.Item1.ColumnName == "KOKRS").Item2, language);
				List<KostenstellengruppeAnzeigenData> data2 = ViewboxApplication.GetKostenstellengruppeAnzeigenData(obj.Table, key, userDatabase);
				if (data2.Count <= 0)
				{
					return;
				}
				List<KostenstellengruppeAnzeigenStruct> newList2 = structure4.OrderBy((KostenstellengruppeAnzeigenStruct x) => x.Parent).ToList();
				foreach (KostenstellengruppeAnzeigenStruct row6 in structure4)
				{
					structuredTable.AddNode(row6.Pos, row6.Parent, row6.Pos + " - " + row6.Description);
				}
				foreach (KostenstellengruppeAnzeigenData row5 in data2)
				{
					structuredTable.AddLeaf(row5.Konto, row5.Pos, row5.Konto + " " + row5.Konto_Be, new List<string>());
				}
				structuredTable.RemoveEmptyNodes2(structuredTable.TreeRoot);
			}
			else if (issue.Flag == 11)
			{
				IEnumerable<Summenbericht> structure3 = ViewboxApplication.GetSummenbericht(obj.Table, key, userDatabase);
				List<Summenbericht> newList = structure3.OrderBy((Summenbericht x) => Convert.ToInt32(x.lief)).ToList();
				structuredTable.ColumnNameList.Add("Anzahlungen");
				structuredTable.ColumnNameList.Add("Obligo");
				structuredTable.ColumnNameList.Add("Istkosten");
				structuredTable.ColumnNameList.Add("Gen.Soll");
				foreach (Summenbericht row4 in newList)
				{
					List<string> templist3 = new List<string>();
					templist3.Add(row4.GenSoll);
					templist3.Add(row4.Istkosten);
					templist3.Add(row4.Obligo);
					templist3.Add(row4.Anzahlung);
					structuredTable.AddLeaf(row4.ID, row4.Parent, row4.Objekt + ((row4.Objekt != "" && row4.Descr != "") ? " - " : "") + row4.Descr, templist3);
				}
				structuredTable.ReOrderNodes();
			}
			else if (issue.Flag == 12)
			{
				IEnumerable<SummenberichtFgnStructure> structure2 = ViewboxApplication.GetSummenberichtFgnStructure(obj.Table, key);
				List<SummenberichtFgnData> data = ViewboxApplication.GetSummenberichtFgnData(obj.Table, key, userDatabase);
				structuredTable.ColumnNameList.Add("Istkosten");
				foreach (SummenberichtFgnStructure row3 in structure2)
				{
					structuredTable.AddNode(row3.id, row3.parent_id ?? "0", row3.setname);
				}
				structuredTable.AddNode("99999999", structuredTable.TreeRoot.Key, "Nicht zugeordnet");
				structuredTable.ReOrderNodes();
				foreach (SummenberichtFgnData row2 in data)
				{
					if (row2.abw == "0,00" && row2.abwp == "0,00" && row2.istkosten == "0,00" && row2.plankosten == "0,00")
					{
						return;
					}
					List<string> templist2 = new List<string>();
					templist2.Add(row2.istkosten);
					structuredTable.AddLeafWithChildsupport(row2.kstar, (row2.id == null || row2.id == "") ? "99999999" : row2.id, $"{row2.kstar} - {row2.ltext}", templist2);
				}
				structuredTable.RemoveEmptyNodes(structuredTable.TreeRoot);
			}
			else
			{
				if (issue.Flag != 14)
				{
					return;
				}
				IEnumerable<Workflowprotokoll> structure = from x in ViewboxApplication.GetWorkflowprotokoll(obj.Table, key, userDatabase)
					orderby x.WI_PARENT
					select x;
				structuredTable.ColumnNameList.Clear();
				structuredTable.ColumnNameList.Add("Workitem-Kennung");
				structuredTable.ColumnNameList.Add("Uhrzeit");
				structuredTable.ColumnNameList.Add("Datum");
				structuredTable.ColumnNameList.Add("Status");
				foreach (Workflowprotokoll row in structure)
				{
					List<string> templist = new List<string>();
					templist.Add(row.WI_STAT_TXT);
					templist.Add(row.WI_CD);
					templist.Add(row.WI_CT);
					templist.Add(row.WI_ID_ORIG);
					structuredTable.AddLeaf(row.WI_ID_ORIG, row.WI_PARENT, row.WI_TEXT, templist);
				}
				structuredTable.ReOrderNodes();
			}
		}

		private void BuilduniversalTable(ViewboxDb.TableObject obj, UniversalTableModel universalTable, string userDatabase, ILanguage language, List<Tuple<SystemDb.IParameter, string, string>> paramList)
		{
			Tuple<SystemDb.IParameter, string, string> IsDataGridNeeded = paramList.FirstOrDefault((Tuple<SystemDb.IParameter, string, string> p) => p.Item1.Name == "in_turnOff");
			universalTable.IsDataGridNeeded = IsDataGridNeeded != null && IsDataGridNeeded.Item2.ToString() == "1";
			if (universalTable.IsDataGridNeeded)
			{
				return;
			}
			IIssue issue = obj.OriginalTable as IIssue;
			foreach (Tuple<SystemDb.IParameter, string, string> param in paramList)
			{
				universalTable.Parameters.Add(param);
			}
			_log.ContextLog(LogLevelEnum.Info, string.Format("Universal table generation with the following parameters: {0}", string.Join(";", universalTable.GetParameters().Select((Tuple<string, string> x, int y) => $"{x}={y}"))));
			string key = obj.Optimization.GetOptimizationValue(OptimizationType.IndexTable);
			IEnumerable<StructureDyn> data = from x in ViewboxApplication.GetStructureDyn(obj, userDatabase)
				orderby x.PARENT
				select x;
			if (data.Count() == 0)
			{
				return;
			}
			IEnumerable<StructureGraph> structure = ViewboxApplication.GetStructureGraph(obj, userDatabase);
			universalTable.KontoColumnName = obj.OriginalColumns.First((IColumn x) => x.Name.StartsWith("DESC_0", StringComparison.OrdinalIgnoreCase)).GetDescription(language);
			universalTable.Columns = obj.OriginalColumns.Where((IColumn x) => x.Name.StartsWith("VALUE", StringComparison.OrdinalIgnoreCase) || x.Name.StartsWith("FIELD", StringComparison.OrdinalIgnoreCase)).ToList();
			universalTable.HideEmptyNodes = issue.Flag == 101;
			Tuple<SystemDb.IParameter, string, string> in_level = universalTable.Parameters.FirstOrDefault((Tuple<SystemDb.IParameter, string, string> x) => x.Item1.Name.Equals("in_level", StringComparison.OrdinalIgnoreCase));
			if (in_level != null)
			{
				int.TryParse(in_level.Item2.ToString(), out universalTable.MaxPrintedLevel);
			}
			foreach (StructureGraph row2 in structure)
			{
				universalTable.AddNode(row2.ID, row2.PARENT, row2.GetFullDesc(), row2.ORDINAL);
			}
			foreach (StructureDyn row in data)
			{
				universalTable.AddLeaf(row.ID, row.PARENT, row.GetFullDesc(), row.FIELD, row.VALUE, row.ORDINAL);
			}
			universalTable.ReOrderNodes();
		}

		private void BuildSimpleTableStructure(IIssue issue, object[] paramValues, SimpleTableModel simpleTableModel, List<int> itemId, List<string> savedFilterTypes = null)
		{
			if (issue == null)
			{
				return;
			}
			if (itemId == null)
			{
				itemId = new List<int>();
			}
			int i = 0;
			foreach (int id in itemId)
			{
				int filterTypeIdx = -1;
				SystemDb.IParameter param = null;
				foreach (SystemDb.IParameter p in issue.Parameters)
				{
					if (p.FreeSelection == 0)
					{
						filterTypeIdx++;
					}
					if (p.Id == id)
					{
						param = p;
						break;
					}
				}
				string value = ((paramValues == null || paramValues.Length <= i || paramValues[i] == null) ? "" : paramValues[i].ToString());
				DateTime? datValue = ((paramValues == null || paramValues.Length <= i) ? null : (paramValues[i] as DateTime?));
				if (datValue.HasValue)
				{
					value = datValue.Value.ToString("dd/MM/yyyy");
				}
				string filterType = string.Empty;
				if (param.FreeSelection == 0)
				{
					filterType = ((savedFilterTypes != null && savedFilterTypes.Count > filterTypeIdx) ? savedFilterTypes[filterTypeIdx] : string.Empty);
				}
				simpleTableModel.Parameters.Add(new Tuple<SystemDb.IParameter, string, string>(param, value, filterType));
				i++;
			}
			_log.ContextLog(LogLevelEnum.Info, string.Format("Dynamic table generation with the following parameters: {0}", string.Join(";", simpleTableModel.GetParameters(issue.Id).Select((Tuple<string, string> x, int y) => $"{x}={y}"))));
		}

		private void BuildBalanceStructure(ViewboxDb.TableObject obj, SapBalanceModel balance, string userDatabase, CancellationToken token, string languageKey)
		{
			IIssue issue = obj.OriginalTable as IIssue;
			string key = string.Empty;
			balance.Flag = issue.Flag;
			if (issue != null)
			{
				int i = 0;
				foreach (SystemDb.IParameter param in issue.Parameters)
				{
					string value = ((obj.ParamValues[i] == null) ? "" : obj.ParamValues[i].ToString());
					balance.Parameters.Add(new Tuple<SystemDb.IParameter, string>(param, value));
					if (param.Name == "in_type")
					{
						int structureIndex = Array.IndexOf(obj.ItemId.ToArray(), param.Id);
						key = obj.ParamValues[structureIndex].ToString();
					}
					i++;
				}
				_log.ContextLog(LogLevelEnum.Info, string.Format("Dynamic table generation with the following parameters: {0}", string.Join(";", balance.GetParameters().Select((Tuple<string, string> x, int y) => $"{x}={y}"))));
			}
			if (issue.Flag == 1)
			{
				List<Viewbox.SapBalance.StructureRow> structure = ViewboxApplication.GetBalanceStructure(obj.Table, key, languageKey);
				List<Viewbox.SapBalance.AccountRow> accounts2 = ViewboxApplication.GetBalanceData(obj.Table, userDatabase);
				try
				{
					balance.SplitByGesber = false;
				}
				catch
				{
				}
				if (accounts2.Count > 0)
				{
					balance.Currency = accounts2.First().Currency;
					balance.AccountStructure = accounts2.First().AccountStructure;
					foreach (Viewbox.SapBalance.StructureRow row4 in structure)
					{
						token.ThrowIfCancellationRequested();
						SapBalanceModel.AccountType type2 = SapBalanceModel.AccountType.Inherit;
						SapBalanceModel.StructureType stype2 = SapBalanceModel.StructureType.MainNode;
						string description2 = ((string.IsNullOrEmpty(row4.Name) || row4.Name.Equals(row4.Account) || row4.Account.Equals(row4.Description)) ? string.Empty : $"{row4.Name} - ");
						description2 += ((string.IsNullOrEmpty(row4.Account) || row4.Account.Equals(row4.Description)) ? string.Empty : $"{row4.Account}: ");
						if (row4.Credit)
						{
							type2 |= SapBalanceModel.AccountType.Creditor;
						}
						if (row4.Debit)
						{
							type2 |= SapBalanceModel.AccountType.Debitor;
						}
						if (row4.Type == "B")
						{
							stype2 = SapBalanceModel.StructureType.RangeNode;
						}
						else if (row4.Type == "K")
						{
							stype2 = SapBalanceModel.StructureType.AccountNode;
						}
						SapBalanceModel.Node node2 = balance.AddNode(row4.Id, row4.Parent, row4.ParentGroup, description2 + row4.Description, type2, stype2, row4.AdditionalInformation, row4.SumAndAddToBalance);
						if (node2 == null)
						{
							continue;
						}
						ICoreProperty icp = ViewboxApplication.FindCoreProperty("Bilanz_Notassigned");
						if (icp != null && (row4.Description.ToLower().Contains("zugeordnet") || row4.Description.ToLower().Contains("not assigned")) && icp.Value.ToString().ToLower().Contains(key.ToLower() + "-" + obj.Optimization.GetOptimizationValue(OptimizationType.SplitTable).ToLower()))
						{
							balance.UnassignedAccounts = node2;
						}
						if (!(row4.Type != "H"))
						{
							continue;
						}
						if (row4.Type == "B")
						{
							balance.AddRange(node2, row4.AccountStart, row4.AccountEnd);
						}
						else if (row4.Type == "K")
						{
							if (string.IsNullOrEmpty(row4.Account) && !string.IsNullOrEmpty(row4.AdditionalInformation))
							{
								row4.Account = row4.AdditionalInformation;
							}
							balance.AddRange(node2, row4.Account);
						}
					}
				}
				foreach (Viewbox.SapBalance.AccountRow row2 in accounts2)
				{
					token.ThrowIfCancellationRequested();
					string accountDescription2 = string.Empty;
					if (string.IsNullOrEmpty(row2.Description))
					{
						if (structure.Any((Viewbox.SapBalance.StructureRow o) => o.Account == row2.Account))
						{
							accountDescription2 = structure.First((Viewbox.SapBalance.StructureRow o) => o.Account == row2.Account).Description;
						}
						else if (structure.Any((Viewbox.SapBalance.StructureRow o) => o.Account == row2.Account.PadLeft(10, '0')))
						{
							accountDescription2 = structure.First((Viewbox.SapBalance.StructureRow o) => o.Account == row2.Account.PadLeft(10, '0')).Description;
						}
					}
					else
					{
						accountDescription2 = row2.Description;
					}
					balance.AddAccount(row2.Account, $"{row2.Account}: {accountDescription2}", row2.Value, row2.GesBer);
				}
				balance.CompleteAssignment();
				balance.InsertAdditionalInformation();
				balance.SumGesber();
			}
			else
			{
				if (issue.Flag != 10)
				{
					return;
				}
				List<Viewbox.SapBalance.StructureRow> structure2 = ViewboxApplication.GetBalanceStructure(obj.Table, key, ViewboxSession.User.CurrentLanguage);
				List<AccountRowExtended> accounts = ViewboxApplication.GetBalanceDataExtended(obj.Table, userDatabase);
				if (accounts.Count > 0)
				{
					balance.Currency = accounts.First().Currency;
					balance.AccountStructure = accounts.First().AccountStructure;
					foreach (Viewbox.SapBalance.StructureRow row3 in structure2)
					{
						token.ThrowIfCancellationRequested();
						SapBalanceModel.AccountType type = SapBalanceModel.AccountType.Inherit;
						SapBalanceModel.StructureType stype = SapBalanceModel.StructureType.MainNode;
						string description = ((string.IsNullOrEmpty(row3.Name) || row3.Name.Equals(row3.Account)) ? string.Empty : $"{row3.Name} - ");
						description += (string.IsNullOrEmpty(row3.Account) ? string.Empty : $"{row3.Account}: ");
						if (row3.Credit)
						{
							type |= SapBalanceModel.AccountType.Creditor;
						}
						if (row3.Debit)
						{
							type |= SapBalanceModel.AccountType.Debitor;
						}
						if (row3.Type == "B")
						{
							stype = SapBalanceModel.StructureType.RangeNode;
						}
						else if (row3.Type == "K")
						{
							stype = SapBalanceModel.StructureType.AccountNode;
						}
						SapBalanceModel.Node node = balance.AddNode(row3.Id, row3.Parent, row3.ParentGroup, description + row3.Description, type, stype, row3.AdditionalInformation, row3.SumAndAddToBalance);
						if (node == null)
						{
							continue;
						}
						if (!row3.Description.ToLower().Contains("zugeordnet") && !row3.Description.ToLower().Contains("not assigned") && false)
						{
							balance.UnassignedAccounts = node;
						}
						if (!(row3.Type != "H"))
						{
							continue;
						}
						if (row3.Type == "B")
						{
							balance.AddRange(node, row3.AccountStart, row3.AccountEnd);
						}
						else if (row3.Type == "K")
						{
							if (string.IsNullOrEmpty(row3.Account) && !string.IsNullOrEmpty(row3.AdditionalInformation))
							{
								row3.Account = row3.AdditionalInformation;
							}
							balance.AddRange(node, row3.Account);
						}
					}
				}
				foreach (AccountRowExtended row in accounts)
				{
					token.ThrowIfCancellationRequested();
					string accountDescription = string.Empty;
					if (string.IsNullOrEmpty(row.Description))
					{
						if (structure2.Any((Viewbox.SapBalance.StructureRow o) => o.Account == row.Account))
						{
							accountDescription = structure2.First((Viewbox.SapBalance.StructureRow o) => o.Account == row.Account).Description;
						}
						else if (structure2.Any((Viewbox.SapBalance.StructureRow o) => o.Account == row.Account.PadLeft(10, '0')))
						{
							accountDescription = structure2.First((Viewbox.SapBalance.StructureRow o) => o.Account == row.Account.PadLeft(10, '0')).Description;
						}
					}
					else
					{
						accountDescription = row.Description;
					}
					balance.AddAccount(row.Account, $"{row.Account}: {accountDescription}", row.Value, row.GesBer, row.Value_VJ, row.Value_ABS, row.Value_REL);
				}
				balance.CompleteAssignment();
				balance.InsertAdditionalInformation();
				balance.SumGesber();
			}
		}

		private void BuildDcwBalanceStructure(ViewboxDb.TableObject obj, DcwBalanceModel balance, string userDatabase, int type, CancellationToken token)
		{
			IIssue issue = obj.OriginalTable as IIssue;
			string showMandant = "n";
			Dictionary<string, StructureSignRow> signTable = null;
			if (issue != null)
			{
				int i = 0;
				foreach (SystemDb.IParameter param in issue.Parameters)
				{
					string value = ((obj.ParamValues[i] == null) ? "" : obj.ParamValues[i].ToString());
					balance.Parameters.Add(new Tuple<SystemDb.IParameter, string>(param, value));
					if (param.Name.ToLower() == "in_curr")
					{
						balance.Currency = value;
					}
					else if (param.Name.ToLower() == "in_show_vj")
					{
						balance.ShowVJahr = value;
					}
					else if (param.Name.ToLower() == "in_show_level")
					{
						balance.ShowLevel = value;
					}
					else if (param.Name.ToLower() == "in_mdk")
					{
						balance.MandtKreis = value;
					}
					else if (param.Name.ToLower() == "in_erl_man")
					{
						showMandant = value;
					}
					else if (param.Name.ToLower() == "in_det_hk")
					{
						balance.DetHK = value;
					}
					else if (param.Name.ToLower() == "in_version")
					{
						if (obj.ParamValues[i] != null && int.TryParse(obj.ParamValues[i].ToString(), out var version))
						{
							balance.Version = version;
						}
						else
						{
							balance.Version = 1;
						}
					}
					i++;
				}
				_log.ContextLog(LogLevelEnum.Info, string.Format("Dynamic table generation with the following parameters: {0}", string.Join(";", balance.GetParameters().Select((Tuple<string, string> x, int y) => $"{x}={y}"))));
			}
			showMandant = showMandant.ToLower();
			if (string.IsNullOrEmpty(balance.ShowLevel))
			{
				balance.ShowLevel = new string(Resources.No[0], 1);
			}
			balance.ShowLevel = balance.ShowLevel.ToLower();
			balance.DetHK = balance.DetHK.ToLower();
			if (balance.ShowLevel.ToLower() == "j")
			{
				balance.ShowLevel = "u";
			}
			else if (balance.DetHK == "j")
			{
				balance.ShowLevel = "h";
			}
			else if (showMandant == "j")
			{
				balance.ShowLevel = "h";
			}
			else
			{
				balance.ShowLevel = "s";
			}
			int sign = ((!(balance.ShowLevel == "s") || issue == null || issue.Flag != 2) ? 1 : (-1));
			balance.HasVJahr = obj.Table.Columns.Any((IColumn w) => w.Name.ToLower() == "saldo_vj") && (string.IsNullOrEmpty(balance.ShowVJahr) || balance.ShowVJahr.ToLower() == "j");
			balance.HasPer = obj.Table.Columns.Any((IColumn w) => w.Name.ToLower() == "saldo_per");
			balance.HasBis = obj.Table.Columns.Any((IColumn w) => w.Name.ToLower() == "saldo_bis");
			balance.HasVon = obj.Table.Columns.Any((IColumn w) => w.Name.ToLower() == "saldo_von");
			balance.HasMandtSplit = showMandant == "j" && obj.Table.Columns.Any((IColumn w) => w.Name.ToLower() == "dzmnu_description");
			List<Viewbox.DcwBalance.AccountRow> accounts = ViewboxApplication.GetDcwAccounts(obj.Table, userDatabase, balance.HasVJahr, balance.HasPer, balance.HasVon, balance.HasBis, balance.HasMandtSplit);
			balance.HasVJahr = accounts.Any((Viewbox.DcwBalance.AccountRow w) => w.Saldo_VJ != 0.0);
			Viewbox.DcwBalance.AccountRow firstAccount = accounts.FirstOrDefault();
			if (firstAccount == null)
			{
				return;
			}
			balance.RefMandt = firstAccount.Bmnur;
			balance.Mandt = firstAccount.Dzmnu;
			balance.SaldoVon = firstAccount.Saldo_Von;
			balance.SaldoBis = firstAccount.Saldo_Bis;
			balance.SaldoPer = firstAccount.Saldo_Per;
			IEnumerable<Viewbox.DcwBalance.StructureRow> structure = ViewboxApplication.GetDcwBalanceStructure(obj.Table, balance.RefMandt, type, balance.Version);
			foreach (Viewbox.DcwBalance.StructureRow row in structure)
			{
				token.ThrowIfCancellationRequested();
				DcwBalanceModel.DcwNodeOrdering orderingType = (DcwBalanceModel.DcwNodeOrdering)Enum.Parse(typeof(DcwBalanceModel.DcwNodeOrdering), row.Numbering.ToString());
				int displaySign = row.Sign;
				if (signTable != null)
				{
					displaySign = (signTable.ContainsKey(row.Key) ? signTable[row.Key].Sign : row.Sign);
				}
				balance.AddNode(row.Key, row.Parent, row.Description, orderingType, row.BzuohFrom, row.BzuohTo, displaySign);
			}
			balance.NotAssigned = new DcwBalanceModel.DcwNode("notassignedaccount", DcwBalanceModel.DcwNodeType.Structure, DcwBalanceModel.DcwNodeSubType.SachKonto, "", "", 1, balance.TreeRoot)
			{
				OrderingType = DcwBalanceModel.DcwNodeOrdering.None,
				Description = Resources.NotAssignedAccount
			};
			balance.AddNode(balance.NotAssigned.Key, balance.NotAssigned);
			foreach (Viewbox.DcwBalance.AccountRow account in accounts)
			{
				if (account.Saldo_GJ == 0.0 && account.Saldo_VJ == 0.0)
				{
					continue;
				}
				int accountSign = 1;
				if (signTable == null && GetInt32Value(account.Bzuoh) >= 780 && issue.Flag == 2)
				{
					accountSign = sign;
				}
				DcwBalanceModel.DcwNode parent = balance.FindNode(account.Bzuoh, DcwBalanceModel.DcwNodeType.Structure) ?? balance.NotAssigned;
				DcwBalanceModel.DcwNode hkoNode = parent.Children.FirstOrDefault((DcwBalanceModel.DcwNode w) => w.Key == account.Hko);
				if (hkoNode == null)
				{
					hkoNode = balance.AddAccount(parent, DcwBalanceModel.DcwNodeSubType.HauptKonten, account.Hko, account.HkoName, (string.IsNullOrEmpty(account.Uko) && string.IsNullOrEmpty(account.MandtSplit)) ? (account.Saldo_GJ * (double)accountSign) : 0.0, (string.IsNullOrEmpty(account.Uko) && string.IsNullOrEmpty(account.MandtSplit)) ? (account.Saldo_VJ * (double)accountSign) : 0.0);
				}
				else
				{
					hkoNode.Value += ((string.IsNullOrEmpty(account.Uko) && string.IsNullOrEmpty(account.MandtSplit)) ? (account.Saldo_GJ * (double)accountSign) : 0.0);
					hkoNode.Value2 += ((string.IsNullOrEmpty(account.Uko) && string.IsNullOrEmpty(account.MandtSplit)) ? (account.Saldo_VJ * (double)accountSign) : 0.0);
				}
				DcwBalanceModel.DcwNode ukoNode = null;
				if (!string.IsNullOrEmpty(account.Uko))
				{
					ukoNode = hkoNode.Children.FirstOrDefault((DcwBalanceModel.DcwNode w) => w.Key == account.Uko);
					if (ukoNode == null)
					{
						ukoNode = balance.AddAccount(hkoNode, DcwBalanceModel.DcwNodeSubType.UnterKonten, account.Uko, account.UkoName, string.IsNullOrEmpty(account.MandtSplit) ? (account.Saldo_GJ * (double)accountSign) : 0.0, string.IsNullOrEmpty(account.MandtSplit) ? (account.Saldo_VJ * (double)accountSign) : 0.0);
					}
					else
					{
						ukoNode.Value += (string.IsNullOrEmpty(account.MandtSplit) ? (account.Saldo_GJ * (double)accountSign) : 0.0);
						ukoNode.Value2 += (string.IsNullOrEmpty(account.MandtSplit) ? (account.Saldo_VJ * (double)accountSign) : 0.0);
					}
				}
				DcwBalanceModel.DcwNode parentNode = ukoNode ?? hkoNode;
				if (!string.IsNullOrEmpty(account.MandtSplit) && parentNode != null)
				{
					DcwBalanceModel.DcwNode mandtNode = parentNode.Children.FirstOrDefault((DcwBalanceModel.DcwNode w) => w.Key == account.MandtSplit && w.Description == account.MandtSplitName);
					if (mandtNode == null)
					{
						balance.AddAccount(parentNode, DcwBalanceModel.DcwNodeSubType.MandtSplit, account.MandtSplit, account.MandtSplitName, (double)accountSign * account.Saldo_GJ, (double)accountSign * account.Saldo_VJ);
						continue;
					}
					mandtNode.Value += account.Saldo_GJ * (double)accountSign;
					mandtNode.Value2 += account.Saldo_VJ * (double)accountSign;
				}
			}
			balance.CreateSums();
			balance.ClearEmpty(balance.TreeRoot);
			balance.ClearLevels(balance.TreeRoot, balance.ShowLevel, showMandant);
			CreateOrdering(balance.TreeRoot);
		}

		private static int GetInt32Value(object obj)
		{
			int.TryParse((obj == null) ? "0" : obj.ToString(), out var val);
			return val;
		}

		private void CreateOrdering(DcwBalanceModel.DcwNode node)
		{
			IEnumerable<DcwBalanceModel.DcwNodeOrdering> orderingTypes = node.Children.Select((DcwBalanceModel.DcwNode w) => w.OrderingType).Distinct();
			foreach (DcwBalanceModel.DcwNodeOrdering orderingType in orderingTypes)
			{
				int i = 1;
				foreach (DcwBalanceModel.DcwNode children in node.Children.Where((DcwBalanceModel.DcwNode w) => w.OrderingType == orderingType))
				{
					switch (orderingType)
					{
					case DcwBalanceModel.DcwNodeOrdering.None:
						children.OrderingText = "";
						break;
					case DcwBalanceModel.DcwNodeOrdering.Numeric:
						children.OrderingText = i + ". ";
						break;
					case DcwBalanceModel.DcwNodeOrdering.Alphabetical:
						children.OrderingText = Convert.ToChar(i + 64 + ((i > 26) ? 6 : 0)) + ". ";
						break;
					case DcwBalanceModel.DcwNodeOrdering.Roman:
						children.OrderingText = RomanNumeralizer.RomanNumeralizerInstance.Convert(i) + ". ";
						break;
					}
					i++;
					CreateOrdering(children);
				}
			}
		}

		private void DoGroup(int id, ViewboxDb.TableObjectCollection tempTableObjects, ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, List<int> colIds, List<AggregationFunction> aggfunc, IFilter filter, bool save, string tableName, IUser user, Dictionary<Tuple<ILanguage, string>, string> aggDescriptions, CancellationToken token)
		{
			List<string> stringAggFunc = aggfunc.Select((AggregationFunction a) => Enum.GetName(typeof(AggregationFunction), a)).ToList();
			string descToLog = string.Join(", ", stringAggFunc);
			try
			{
				ViewboxDb.TableObject tobj = tempTableObjects[id];
				ViewboxDb.TableObject obj = (TransformationObject = ((tobj == null) ? ViewboxSession.CreateTableObject(DoExecuteNonQuery, id, tempTableObjects, tableObjects, columns, opt, token, colIds, aggfunc, filter, save, tableName, user, aggDescriptions, "") : ViewboxSession.CreateTableObject(DoExecuteNonQuery, id, tempTableObjects, tableObjects, columns, opt, token, colIds, aggfunc, (filter == null) ? tobj.Filter : filter, save, tableName, user, aggDescriptions, tobj.FilterIssue)));
				LogHelper.GetLogger().Info($"[FUNCTIONS] - temptable object for SQL Function({descToLog}) was created successfully(temptable {obj.Table.TableName})(started by: {user.UserName})");
			}
			catch (Exception e)
			{
				LogHelper.GetLogger().Error($"[FUNCTIONS] - There was a problem on creating temptable object for SQL Function({descToLog})(started by: {user.UserName})", e);
			}
		}

		private void DoGroup2(int id, ViewboxDb.TableObjectCollection tempTableObjects, ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, List<int> colIds, AggregationCollection aggs, IFilter filter, bool save, string tableName, IUser user, Dictionary<Tuple<ILanguage, string>, string> aggDescriptions, CancellationToken token)
		{
			ViewboxDb.TableObject tobj = tempTableObjects[id];
			ViewboxDb.TableObject obj = (TransformationObject = ((tobj == null) ? ViewboxSession.CreateTableObject(DoExecuteNonQuery, id, tempTableObjects, tableObjects, columns, opt, token, colIds, aggs, filter, save, tableName, user, aggDescriptions, "") : ViewboxSession.CreateTableObject(DoExecuteNonQuery, id, tempTableObjects, tableObjects, columns, opt, token, colIds, aggs, filter ?? tobj.Filter, save, tableName, user, aggDescriptions, tobj.FilterIssue)));
		}

		private void DoJoin(ViewboxDb.TableObjectCollection tempTableObjects, ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, int id, int joinTableId, JoinColumnsCollection joinColumns, IFilter filter1, IFilter filter2, JoinType type, IUser user, bool saveJoin, string tableName, CancellationToken token)
		{
			ViewboxDb.TableObject tobj = tempTableObjects[id];
			ViewboxDb.TableObject obj = (TransformationObject = ((tobj == null) ? ViewboxSession.CreateTableObject(DoExecuteNonQuery, tempTableObjects, tableObjects, columns, opt, token, id, joinTableId, joinColumns, filter1, filter2, type, user, saveJoin, tableName) : ViewboxSession.CreateTableObject(DoExecuteNonQuery, tempTableObjects, tableObjects, columns, opt, token, id, joinTableId, joinColumns, (filter1 == null) ? tobj.Filter : filter1, filter2, type, user, saveJoin, tableName)));
		}
	}
}
