using System.Web.Mvc;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Properties;

namespace Viewbox.Command
{
	internal class IndexesJobHelper : IJobStatusHelper
	{
		private readonly PopulateIndexesJob _indexesJob;

		private Controller _controller;

		public IndexesJobHelper(PopulateIndexesJob indexesJob, Controller controller)
		{
			_indexesJob = indexesJob;
			_controller = controller;
		}

		public void InitializeNotificationModelOnFinish(NotificationModel model)
		{
			bool active = !_indexesJob.JobShown.Contains(ViewboxSession.User);
			model.AddNotification(NotificationModel.Type.Info, Resources.ActionFinished, _indexesJob.Descriptions[ViewboxSession.Language.CountryCode], null, _indexesJob.Key, _indexesJob.EndTime, active);
		}

		public void InitializeNotificationModelOnCrashed(NotificationModel model)
		{
			bool active = !_indexesJob.JobShown.Contains(ViewboxSession.User);
			model.AddNotification(NotificationModel.Type.Warning, Resources.Error, _indexesJob.Descriptions[ViewboxSession.Language.CountryCode], null, _indexesJob.Key, _indexesJob.EndTime, active);
		}
	}
}
