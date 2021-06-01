using System.Web.Mvc;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Properties;

namespace Viewbox.Command
{
	internal class OptimizationJobHelper : IJobStatusHelper
	{
		private readonly OptimizationJob _optjob;

		private Controller _controller;

		public OptimizationJobHelper(OptimizationJob optjob, Controller controller)
		{
			_optjob = optjob;
			_controller = controller;
		}

		public void InitializeNotificationModelOnFinish(NotificationModel model)
		{
			bool active = !_optjob.JobShown.Contains(ViewboxSession.User);
			bool isEmpty = _optjob != null;
			model.AddNotification(NotificationModel.Type.Info, Resources.ActionFinished, _optjob.Descriptions[ViewboxSession.Language.CountryCode], null, _optjob.Key, _optjob.EndTime, active, isEmpty);
		}

		public void InitializeNotificationModelOnCrashed(NotificationModel model)
		{
			bool active = !_optjob.JobShown.Contains(ViewboxSession.User);
			model.AddNotification(NotificationModel.Type.Warning, Resources.Error, _optjob.Descriptions[ViewboxSession.Language.CountryCode], null, _optjob.Key, _optjob.EndTime, active);
		}
	}
}
