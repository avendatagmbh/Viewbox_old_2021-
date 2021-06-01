using System.Web.Mvc;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Properties;

namespace Viewbox.Command
{
	internal class RelationJobHelper : IJobStatusHelper
	{
		private readonly RelationJob _relation;

		private Controller _controller;

		public RelationJobHelper(RelationJob relation, Controller controller)
		{
			_relation = relation;
			_controller = controller;
		}

		public void InitializeNotificationModelOnFinish(NotificationModel model)
		{
			bool active = !_relation.JobShown.Contains(ViewboxSession.User);
			bool isEmpty = _relation == null;
			model.AddNotification(NotificationModel.Type.Info, Resources.ActionFinished, _relation.Descriptions[ViewboxSession.Language.CountryCode], _relation.Id.ToString(), _relation.Key, _relation.EndTime, active, isEmpty);
		}

		public void InitializeNotificationModelOnCrashed(NotificationModel model)
		{
		}
	}
}
