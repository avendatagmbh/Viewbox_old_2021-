using System.Web.Mvc;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Properties;

namespace Viewbox.Command
{
	internal class ExportJobHelper : IJobStatusHelper
	{
		private readonly Controller _controller;

		private readonly Export _export;

		public ExportJobHelper(Export export, Controller controller)
		{
			_export = export;
			_controller = controller;
		}

		public void InitializeNotificationModelOnFinish(NotificationModel model)
		{
			bool active = !_export.JobShown.Contains(ViewboxSession.User);
			string link = _controller.Url.Action("Index", "Export", new
			{
				finished = true
			});
			string exportDescription;
			try
			{
				exportDescription = _export.Descriptions[ViewboxSession.Language.CountryCode];
			}
			catch
			{
				exportDescription = string.Empty;
			}
			model.AddNotification(NotificationModel.Type.Info, Resources.ActionFinished, exportDescription, link, _export.Key, _export.EndTime, active);
		}

		public void InitializeNotificationModelOnCrashed(NotificationModel model)
		{
			bool active = !_export.JobShown.Contains(ViewboxSession.User);
			model.AddNotification(NotificationModel.Type.Warning, Resources.Error, _export.Descriptions[ViewboxSession.Language.CountryCode], null, _export.Key, _export.EndTime, active);
		}
	}
}
