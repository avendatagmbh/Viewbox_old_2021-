using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using SystemDb;
using AV.Log;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Job
{
	public class RelationJob : Base
	{
		private static readonly Dictionary<string, RelationJob> _jobs = new Dictionary<string, RelationJob>();

		private readonly JoinColumnsCollection _joinColumnsCollection;

		public RelationJob(JoinColumnsCollection joinColumnsCollection)
		{
			_joinColumnsCollection = joinColumnsCollection;
			Base.AddJob(base.Key, this);
			foreach (ILanguage language in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(language.CountryCode);
				base.Descriptions.Add(language.CountryCode, Resources.ResourceManager.GetString("Relation", culture));
			}
		}

		private RelationJob(int id, int joinTableId, List<JoinColumns> columns, ITableObject tableobject)
		{
			RelationJob relationJob = this;
			base.Id = id;
			_jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			JoinColumnsCollection columnCollection = new JoinColumnsCollection();
			foreach (JoinColumns joinColumns in columns)
			{
				columnCollection.Add(joinColumns);
			}
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
				relationJob.DoRelations((int)((object[])obj)[0], (int)((object[])obj)[1], (JoinColumnsCollection)((object[])obj)[2], (ITableObjectCollection)((object[])obj)[4], tableobject);
				ViewboxApplication.UserSessions[((object[])obj)[3] as IUser].MarkAll();
			}, new object[5]
			{
				id,
				joinTableId,
				columnCollection,
				ViewboxSession.User,
				ViewboxSession.TableObjects
			});
			foreach (ILanguage language in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(language.CountryCode);
				base.Descriptions.Add(language.CountryCode, Resources.ResourceManager.GetString("Relation", culture));
			}
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < columns.Count; i++)
			{
				sb.Append($"{columns[i].Column1} - {columns[i].Column2}");
				if (i < columns.Count - 1)
				{
					sb.Append(", ");
				}
			}
			LogHelper.GetLogger().InfoFormat("[FUNCTIONS] - Creating realitons has been started. Binding table {0} to {1}: {2}", id, joinTableId, sb.ToString());
		}

		private RelationJob(int id, string[] deleteRelations, ITableObject tableobject)
		{
			RelationJob relationJob = this;
			base.Id = id;
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
				relationJob.DoDeleteRelations((int)((object[])obj)[0], (string[])((object[])obj)[1], (ITableObjectCollection)((object[])obj)[2], token, tableobject);
				ViewboxApplication.UserSessions[((object[])obj)[3] as IUser].MarkAll();
			}, new object[4]
			{
				id,
				deleteRelations,
				ViewboxSession.TableObjects,
				ViewboxSession.User
			});
			foreach (ILanguage language in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(language.CountryCode);
				base.Descriptions.Add(language.CountryCode, Resources.ResourceManager.GetString("DeleteRelations", culture));
			}
			LogHelper.GetLogger().InfoFormat("[FUNCTIONS] - Relation delete has been started on table {0}. Deleting relation(s) {1}", id, string.Join(", ", deleteRelations));
		}

		public static RelationJob Create(int id, int joinTableId, List<JoinColumns> columns, ITableObject tableobject)
		{
			return new RelationJob(id, joinTableId, columns, tableobject);
		}

		public static RelationJob Create(int id, string[] deleteRelations, ITableObject tableobject)
		{
			return new RelationJob(id, deleteRelations, tableobject);
		}

		public void StartJob()
		{
			StartJob(delegate(object obj, CancellationToken ctoken)
			{
				Relations(ctoken);
			}, null);
		}

		private void DoRelations(int id, int joinTableId, JoinColumnsCollection columnCollection, ITableObjectCollection tableObjectCollection, ITableObject tableobject)
		{
			try
			{
				ViewboxApplication.Database.AddRelations(id, joinTableId, tableObjectCollection, columnCollection, tableobject);
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < columnCollection.Count; i++)
				{
					sb.Append($"{columnCollection[i].Column1} - {columnCollection[i].Column2}");
					if (i < columnCollection.Count - 1)
					{
						sb.Append(", ");
					}
				}
				LogHelper.GetLogger().InfoFormat("[FUNCTIONS] - Relation creation has ended. Binding table {0} to {1}: {2} is completed.", id, joinTableId, sb.ToString());
			}
			catch (Exception e)
			{
				LogHelper.GetLogger().Error($"[FUNCTIONS] - There was an error on creating realtions between table {id} and {joinTableId}.", e);
			}
		}

		private void DoDeleteRelations(int id, string[] deleteRelations, ITableObjectCollection tableObjectCollection, CancellationToken ctk, ITableObject tableobject)
		{
			try
			{
				ViewboxApplication.Database.SystemDb.DeleteRelationsFromDatabase(id, deleteRelations, tableObjectCollection, tableobject);
				LogHelper.GetLogger().InfoFormat("[FUNCTIONS] - Relation delete on table {0} has ended. Deleted relation(s): {1}.", id, string.Join(", ", deleteRelations));
			}
			catch (Exception e)
			{
				LogHelper.GetLogger().Error(string.Format("[FUNCTIONS] - There was an error on deleting realtion(s) {0}.", string.Join(", ", deleteRelations)), e);
			}
		}

		public void Relations(CancellationToken ctk)
		{
			foreach (JoinColumns joinColumns in _joinColumnsCollection)
			{
				ViewboxApplication.Database.AddRelation(joinColumns.Column1, joinColumns.Column2);
			}
		}
	}
}
