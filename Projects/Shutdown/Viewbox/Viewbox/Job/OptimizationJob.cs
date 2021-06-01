using System.Globalization;
using System.Threading;
using SystemDb;
using Viewbox.Properties;

namespace Viewbox.Job
{
	public class OptimizationJob : Base
	{
		private readonly int _optimization;

		public OptimizationJob(int optimization)
		{
			_optimization = optimization;
			Base.AddJob(base.Key, this);
			foreach (ILanguage language in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(language.CountryCode);
				base.Descriptions.Add(language.CountryCode, Resources.ResourceManager.GetString("RemoveOptimization", culture));
			}
		}

		public void StartJob()
		{
			StartJob(delegate(object obj, CancellationToken ctoken)
			{
				RemoveOptimization(obj, ctoken);
			}, new object[1] { ViewboxSession.AllowedOptimizations });
		}

		public void RemoveOptimization(object param, CancellationToken ctk)
		{
			ViewboxApplication.Database.SystemDb.RemoveOptimizationFromAllTables(_optimization);
		}
	}
}
