using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SystemDb;
using Viewbox.Properties;

namespace Viewbox.Job
{
	public class Janitor : IDisposable
	{
		private static Janitor _instance;

		private static Dictionary<string, DateTime> _downloads = new Dictionary<string, DateTime>();

		private static Dictionary<string, DateTime> _tables = new Dictionary<string, DateTime>();

		private static Dictionary<IUser, List<Base>> MailSended = new Dictionary<IUser, List<Base>>();

		public static DateTime LastMailSended = DateTime.Now;

		private Task _task;

		private CancellationTokenSource _cts;

		private static Dictionary<string, Exception> _errors = new Dictionary<string, Exception>();

		public static void Start()
		{
			if (_instance == null)
			{
				_instance = new Janitor();
			}
		}

		public static void Stop()
		{
			if (_instance != null)
			{
				_instance._cts.Cancel();
				_instance.Dispose();
				_instance = null;
			}
		}

		public static void RegisterDownload(string key)
		{
			lock (_downloads)
			{
				_downloads[key] = DateTime.Now;
			}
		}

		public static void RegisterTempObject(string key)
		{
			if (key != null)
			{
				lock (_tables)
				{
					_tables[key] = DateTime.Now;
				}
			}
		}

		private Janitor()
		{
			_cts = new CancellationTokenSource();
			_task = Task.Factory.StartNew(DoCleanup, _cts.Token);
		}

		public void Dispose()
		{
		}

		private void DoCleanup()
		{
			DateTime lastcheck = DateTime.Now;
			while (true)
			{
				Thread.Sleep(100);
				if (_cts.Token.IsCancellationRequested)
				{
					break;
				}
				try
				{
				}
				catch
				{
				}
				if (!(DateTime.Now - lastcheck < ViewboxApplication.TemporaryCheckInterval))
				{
					lastcheck = DateTime.Now;
					DateTime limit = lastcheck - ViewboxApplication.TemporaryObjectsLifetime;
					try
					{
						RemoveFileDownloads(limit);
						RemoveTempTables(limit);
					}
					catch
					{
					}
				}
			}
		}

		private void RemoveFileDownloads(DateTime limit)
		{
			lock (_downloads)
			{
				foreach (string key in (from kv in _downloads
					where kv.Value < limit
					select kv.Key).ToList())
				{
					_downloads.Remove(key);
					RemoveError(key);
					Export.Find(key).CleanUp();
				}
			}
		}

		private void RemoveTempTables(DateTime limit)
		{
			lock (_tables)
			{
				HashSet<string> keys = new HashSet<string>(_tables.Where(delegate(KeyValuePair<string, DateTime> kv)
				{
					KeyValuePair<string, DateTime> keyValuePair2 = kv;
					return keyValuePair2.Value < limit;
				}).Select(delegate(KeyValuePair<string, DateTime> kv)
				{
					KeyValuePair<string, DateTime> keyValuePair = kv;
					return keyValuePair.Key;
				}));
				List<Transformation> jobs = Transformation.List.Where((Transformation j) => j.TransformationObject != null && keys.Contains(j.TransformationObject.Key)).ToList();
				foreach (Transformation job in jobs)
				{
					_tables.Remove(job.TransformationObject.Key);
					RemoveError(job.Key);
					job.CleanUp();
				}
			}
		}

		public static void RemoveTempTable(string key)
		{
			lock (_tables)
			{
				List<Transformation> jobs = Transformation.List.Where((Transformation j) => j.TransformationObject != null && key == j.TransformationObject.Key).ToList();
				foreach (Transformation job in jobs)
				{
					_tables.Remove(job.TransformationObject.Key);
					RemoveError(job.Key);
					job.CleanUp();
				}
			}
		}

		private void SendMail()
		{
			IProperty property = ViewboxApplication.FindProperty("send_job_mail");
			if (property == null || !(property.Value == "true") || (DateTime.Now - LastMailSended).Minutes <= 2)
			{
				return;
			}
			try
			{
				List<Base> mailJobs = new List<Base>(Base.List.Where((Base j) => j.Status == Base.JobStatus.Finished));
				Dictionary<IUser, Tuple<string, int>> userMail = new Dictionary<IUser, Tuple<string, int>>();
				foreach (Base i in mailJobs)
				{
					foreach (IUser u in i.Listeners)
					{
						if (i.JobShown.Contains(u) || (MailSended.ContainsKey(u) && MailSended[u].Contains(i)))
						{
							continue;
						}
						Export export = i as Export;
						Transformation transformation = i as Transformation;
						if (export != null)
						{
							if (userMail.ContainsKey(u))
							{
								userMail[u] = new Tuple<string, int>(userMail[u].Item1 + export.GetMailText() + "\n\n", userMail[u].Item2 + 1);
							}
							else
							{
								userMail.Add(u, new Tuple<string, int>(export.GetMailText() + "\n\n", 1));
							}
						}
						if (transformation != null)
						{
							if (userMail.ContainsKey(u))
							{
								userMail[u] = new Tuple<string, int>(userMail[u].Item1 + transformation.GetMailText() + "\n\n", userMail[u].Item2 + 1);
							}
							else
							{
								userMail.Add(u, new Tuple<string, int>(transformation.GetMailText() + "\n\n", 1));
							}
						}
						if (MailSended.ContainsKey(u))
						{
							MailSended[u].Add(i);
							continue;
						}
						MailSended.Add(u, new List<Base> { i });
					}
				}
				foreach (KeyValuePair<IUser, Tuple<string, int>> k in userMail)
				{
					ViewboxApplication.SendMail(k.Key, (k.Value.Item2 > 1) ? string.Format(Resources.ActionsFinished, k.Value.Item2) : Resources.ActionFinished, k.Value.Item1);
				}
				LastMailSended = DateTime.Now;
			}
			catch
			{
			}
		}

		public static void RegisterError(string key, Exception ex)
		{
			lock (_errors)
			{
				_errors[key] = ex;
			}
		}

		public static Exception GetError(string key)
		{
			return _errors.ContainsKey(key) ? _errors[key] : null;
		}

		private static void RemoveError(string key)
		{
			lock (_errors)
			{
				if (_errors.ContainsKey(key))
				{
					_errors.Remove(key);
				}
			}
		}
	}
}
