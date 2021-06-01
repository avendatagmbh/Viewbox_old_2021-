using System.Web.Mvc;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Properties;

namespace Viewbox.Command
{
	public class DownloadJobHelper : IJobStatusHelper
	{
		private readonly DownloadJob _downloadJob;

		private readonly Controller _controller;

		public DownloadJobHelper(DownloadJob downloadnJob, Controller controller)
		{
			_downloadJob = downloadnJob;
			_controller = controller;
		}

		public void InitializeNotificationModelOnFinish(NotificationModel model)
		{
			string link = _controller.Url.Action("DownloadFileByPath", "Documents", new
			{
				filePath = _downloadJob.ZipFilePath
			});
			bool active = !_downloadJob.JobShown.Contains(ViewboxSession.User);
			bool isEmpty = _downloadJob == null;
			model.AddNotification(NotificationModel.Type.DownloadBlueline, Resources.ActionFinished, _downloadJob.Descriptions[ViewboxSession.Language.CountryCode], link, _downloadJob.Key, _downloadJob.EndTime, active, isEmpty);
		}

		public void InitializeNotificationModelOnCrashed(NotificationModel model)
		{
		}
	}
}
