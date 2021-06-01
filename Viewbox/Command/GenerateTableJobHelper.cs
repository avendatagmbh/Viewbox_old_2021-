using System.Web.Mvc;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Properties;

namespace Viewbox.Command
{
	public class GenerateTableJobHelper : IJobStatusHelper
	{
		private readonly GenerateTableJob _gentable;

		private Controller _controller;

		public GenerateTableJobHelper(GenerateTableJob relation, Controller controller)
		{
			_gentable = relation;
			_controller = controller;
		}

		public void InitializeNotificationModelOnFinish(NotificationModel model)
		{
			bool active = !_gentable.JobShown.Contains(ViewboxSession.User);
			bool isEmpty = _gentable == null;
			model.AddNotification(NotificationModel.Type.Info, Resources.ActionFinished, _gentable.Descriptions[ViewboxSession.Language.CountryCode], null, _gentable.Key, _gentable.EndTime, active, isEmpty);
		}

		public void InitializeNotificationModelOnCrashed(NotificationModel model)
		{
		}
	}
}
