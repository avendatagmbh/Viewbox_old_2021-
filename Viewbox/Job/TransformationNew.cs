using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AV.Log;

namespace Viewbox.Job
{
	public abstract class TransformationNew : Transformation
	{
		private CultureInfo _ci;

		protected abstract void DoWork();

		protected TransformationNew()
		{
			base.EndTime = DateTime.MinValue;
			_ci = Thread.CurrentThread.CurrentUICulture;
			_cts = new CancellationTokenSource();
			Transformation._jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			base.JobFinished += delegate(Base j)
			{
				Janitor.RegisterTempObject(j.Key);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
			};
		}

		public void StartJob()
		{
			base.StartTime = DateTime.Now;
			if (ViewboxSession.User != null && !RequestTime.ContainsKey(ViewboxSession.User))
			{
				RequestTime.Add(ViewboxSession.User, base.StartTime);
			}
			base.Status = JobStatus.Running;
			_task = Task.Factory.StartNew(DoJob, _cts.Token);
		}

		protected void DoJob()
		{
			try
			{
				NotifyJobStarted();
				Thread.CurrentThread.CurrentUICulture = _ci;
				Thread.CurrentThread.CurrentCulture = _ci;
				DoWork();
				_cts.Token.ThrowIfCancellationRequested();
				_log.InfoFormatWithCheck("Job [objectId: {0}] finished!", this);
				base.Status = JobStatus.Finished;
				NotifyJobFinished();
			}
			catch (OperationCanceledException)
			{
				_log.InfoFormatWithCheck("Job [objectId: {0}] was canceled!", this);
				base.Status = JobStatus.Canceled;
				NotifyJobCanceled();
			}
			catch (Exception ex)
			{
				_log.ErrorWithCheck($"Job [objectId: {this}] crashed! {ex.Message}", ex);
				base.Status = JobStatus.Crashed;
				NotifyJobCrashed(ex);
			}
			base.EndTime = DateTime.Now;
		}
	}
}
