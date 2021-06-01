using System;
using System.Threading;
using SystemDb;
using Viewbox.Properties;

namespace Viewbox.Job
{
	public class PopulateIndexesJob : Base
	{
		private readonly Action<object, CancellationToken> JobLogic = delegate
		{
			ViewboxApplication.Database.SystemDb.PopulateWithIndexes();
			ViewboxApplication.Database.SystemDb.LoadIndexesObjects();
		};

		private PopulateIndexesJob(Action<Base> jobStarted, Action<Base> jobFinished, Action<Base, Exception> jobCrashed)
		{
			Base.AddJob(base.Key, this);
			StartJob(JobLogic, null);
			base.JobStarted += delegate(Base job)
			{
				jobStarted(job);
			};
			base.JobFinished += delegate(Base job)
			{
				jobFinished(job);
			};
			base.JobCrashed += delegate(Base job, Exception e)
			{
				jobCrashed(job, e);
			};
			foreach (ILanguage lang in ViewboxApplication.Languages)
			{
				base.Descriptions.Add(lang.CountryCode, Resources.ResourceManager.GetString("Indexes"));
			}
		}

		internal static PopulateIndexesJob Create(Action<Base> jobStarted, Action<Base> jobFinished, Action<Base, Exception> jobCrashed)
		{
			return new PopulateIndexesJob(jobStarted, jobFinished, jobCrashed);
		}
	}
}
