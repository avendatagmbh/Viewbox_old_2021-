using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SystemDb;
using AV.Log;
using log4net;

namespace Viewbox.Job
{
	public abstract class Base
	{
		public enum JobStatus
		{
			OnHold,
			Running,
			Canceled,
			Finished,
			Crashed
		}

		public delegate void JobStartedEventHandler(Base sender);

		public delegate void JobFinishedEventHandler(Base sender);

		public delegate void JobCanceledEventHandler(Base sender);

		public delegate void JobCrashedEventHandler(Base sender, Exception ex);

		protected ILog _log = LogHelper.GetLogger();

		private Dictionary<string, string> _descriptions = new Dictionary<string, string>();

		private List<IUser> _owners = new List<IUser>();

		private List<IUser> _listeners = new List<IUser>();

		private List<IUser> _jobShown = new List<IUser>();

		private DateTime _startTime;

		private object _startTimeLock = new object();

		private DateTime _endTime;

		private object _endTimeLock = new object();

		public Dictionary<IUser, DateTime> RequestTime = new Dictionary<IUser, DateTime>();

		private JobStatus _status;

		private object _statusLock = new object();

		protected Task _task;

		protected CancellationTokenSource _cts;

		private static Dictionary<string, Base> _jobs = new Dictionary<string, Base>();

		private int _id;

		public Dictionary<string, string> Descriptions => _descriptions;

		public string Key { get; private set; }

		public List<IUser> Owners => _owners;

		public List<IUser> Listeners => _listeners;

		public List<IUser> JobShown => _jobShown;

		public DateTime StartTime
		{
			get
			{
				lock (_startTimeLock)
				{
					return _startTime;
				}
			}
			protected set
			{
				lock (_startTimeLock)
				{
					_startTime = value;
				}
			}
		}

		public DateTime EndTime
		{
			get
			{
				lock (_endTimeLock)
				{
					return _endTime;
				}
			}
			protected set
			{
				lock (_endTimeLock)
				{
					_endTime = value;
				}
			}
		}

		public TimeSpan Runtime => ((EndTime != DateTime.MinValue) ? EndTime : DateTime.Now) - StartTime;

		public JobStatus Status
		{
			get
			{
				lock (_statusLock)
				{
					return _status;
				}
			}
			protected set
			{
				lock (_statusLock)
				{
					_status = value;
				}
			}
		}

		public static IEnumerable<Base> List => _jobs.Values;

		public int Id
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
			}
		}

		public event JobStartedEventHandler JobStarted;

		public event JobFinishedEventHandler JobFinished;

		public event JobCanceledEventHandler JobCanceled;

		public event JobCrashedEventHandler JobCrashed;

		protected Base()
		{
			Key = BitConverter.ToString(Guid.NewGuid().ToByteArray()).Replace("-", string.Empty);
			Listeners.Add(ViewboxSession.User);
			Owners.Add(ViewboxSession.User);
		}

		protected void NotifyJobStarted()
		{
			if (this.JobStarted != null)
			{
				this.JobStarted(this);
			}
		}

		protected void NotifyJobFinished()
		{
			if (this.JobFinished != null)
			{
				this.JobFinished(this);
			}
		}

		protected void NotifyJobCanceled()
		{
			if (this.JobCanceled != null)
			{
				this.JobCanceled(this);
			}
		}

		protected void NotifyJobCrashed(Exception ex)
		{
			if (this.JobCrashed != null)
			{
				this.JobCrashed(this, ex);
			}
		}

		public static void AddJob(string key, Base job)
		{
			_jobs.Add(key, job);
		}

		public static void RemoveJob(string key)
		{
			_jobs.Remove(key);
		}

		public static void Clear(IUser user)
		{
			List<string> stringList = new List<string>(_jobs.Where(delegate(KeyValuePair<string, Base> j)
			{
				KeyValuePair<string, Base> keyValuePair2 = j;
				return keyValuePair2.Value.Listeners.Contains(user);
			}).Select(delegate(KeyValuePair<string, Base> j)
			{
				KeyValuePair<string, Base> keyValuePair = j;
				return keyValuePair.Key;
			}));
			foreach (string s in stringList)
			{
				_jobs.Remove(s);
			}
		}

		public static Base Find(string key)
		{
			return (key != null && _jobs.ContainsKey(key)) ? _jobs[key] : null;
		}

		protected void StartJob(Action<object, CancellationToken> action, object obj)
		{
			_cts = new CancellationTokenSource();
			Status = JobStatus.Running;
			StartTime = DateTime.Now;
			EndTime = DateTime.MinValue;
			if (ViewboxSession.User != null && !RequestTime.ContainsKey(ViewboxSession.User))
			{
				RequestTime.Add(ViewboxSession.User, StartTime);
			}
			string cc = (ViewboxSession.Language.CountryCode.Equals("en", StringComparison.OrdinalIgnoreCase) ? "en-GB" : ViewboxSession.Language.CountryCode);
			CultureInfo ci = new CultureInfo(cc);
			CancellationToken token = _cts.Token;
			_task = Task.Factory.StartNew(delegate(object objs)
			{
				CultureInfo cultureInfo = (objs as object[])[1] as CultureInfo;
				Thread.CurrentThread.CurrentUICulture = cultureInfo;
				Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cultureInfo.Name);
				NotifyJobStarted();
				int num = -1;
				try
				{
					IEnumerable<object> enumerable = (objs as object[])[0] as IEnumerable<object>;
					if (enumerable != null)
					{
						List<object> list = new List<object>(enumerable);
						if (list[0] is IView)
						{
							num = (list[0] as IView).Id;
						}
						else if (list[0] is ITable)
						{
							num = (list[0] as ITable).Id;
						}
					}
					action((objs as object[])[0], token);
					token.ThrowIfCancellationRequested();
					Status = JobStatus.Finished;
					_log.InfoFormatWithCheck("Job [objectId: {0}] finished!", num);
					NotifyJobFinished();
				}
				catch (OperationCanceledException)
				{
					Status = JobStatus.Canceled;
					_log.InfoFormatWithCheck("Job [objectId: {0}] was canceled!", num);
					NotifyJobCanceled();
				}
				catch (Exception ex2)
				{
					Status = JobStatus.Crashed;
					_log.ErrorWithCheck($"Job [objectId: {num}] crashed! {ex2.Message}", ex2);
					NotifyJobCrashed(ex2);
				}
				EndTime = DateTime.Now;
			}, new object[2] { obj, ci }, token, TaskCreationOptions.PreferFairness, TaskScheduler.Current);
		}

		public virtual void Cancel()
		{
			_cts.Cancel();
		}

		public virtual void CleanUp()
		{
			if (_jobs.ContainsKey(Key))
			{
				_jobs.Remove(Key);
			}
		}

		public static void AddUserToJobShows(string key)
		{
			if (_jobs.ContainsKey(key))
			{
				_jobs[key].JobShown.Add(ViewboxSession.User);
			}
		}
	}
}
