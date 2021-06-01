using System.Web.Mvc;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Properties;

namespace Viewbox.Command
{
	public class CreateArchiveFilesJobHelper : IJobStatusHelper
	{
		private readonly CreateArchiveFilesJob _archiveFile;

		private Controller _controller;

		public CreateArchiveFilesJobHelper(CreateArchiveFilesJob relation, Controller controller)
		{
			_archiveFile = relation;
			_controller = controller;
		}

		public void InitializeNotificationModelOnFinish(NotificationModel model)
		{
			bool active = !_archiveFile.JobShown.Contains(ViewboxSession.User);
			bool isEmpty = _archiveFile == null;
			model.AddNotification(NotificationModel.Type.Info, Resources.ActionFinished, _archiveFile.Descriptions[ViewboxSession.Language.CountryCode], null, _archiveFile.Key, _archiveFile.EndTime, active, isEmpty);
		}

		public void InitializeNotificationModelOnCrashed(NotificationModel model)
		{
		}
	}
}
