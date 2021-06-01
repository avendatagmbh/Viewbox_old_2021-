using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SystemDb;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Properties;

namespace Viewbox.Command
{
	public class TransformationHelper : IJobStatusHelper
	{
		private Transformation _transformation;

		private Controller _controller;

		public TransformationHelper(Transformation transformation, Controller controller)
		{
			_transformation = transformation;
			_controller = controller;
		}

		public void InitializeNotificationModelOnFinish(NotificationModel model)
		{
			bool active = !_transformation.JobShown.Contains(ViewboxSession.User);
			bool visible = true;
			string link;
			if (_transformation.notificationData == null)
			{
				link = ((_transformation.TransformationObject != null) ? _controller.Url.Action("Index", (_transformation.TransformationObject.OriginalTable.Type == TableType.Archive) ? "Documents" : ((_transformation.TransformationObject.OriginalTable.Type == TableType.ArchiveDocument) ? "ArchiveDocuments" : "DataGrid"), new
				{
					id = _transformation.TransformationObject.Table.Id
				}) : ((_transformation.TableObject == null) ? string.Empty : _controller.Url.Action("Index", "DataGrid", new
				{
					id = _transformation.TableObject.Id
				})));
			}
			else
			{
				link = _controller.Url.Action(_transformation.notificationData.Method, _transformation.notificationData.Controller, (_transformation.notificationData.Params != null) ? new RouteValueDictionary(new
				{
					search = (_transformation.notificationData.Params.Search ?? ""),
					showArchived = _transformation.notificationData.Params.ShowArchived,
					showHidden = _transformation.notificationData.Params.ShowHidden,
					showEmpty = _transformation.notificationData.Params.ShowEmpty
				}) : null);
				visible = _transformation.notificationData.Visible;
			}
			bool isEmpty = _transformation.TransformationObject != null && _transformation.TransformationObject.Table.RowCount == 0;
			string _desc = string.Empty;
			if (_transformation.Descriptions.ContainsKey(ViewboxSession.Language.CountryCode))
			{
				_desc = _transformation.Descriptions[ViewboxSession.Language.CountryCode];
			}
			bool isDownload = _transformation.notificationData != null && _transformation.notificationData.Controller == "ArchiveDocuments" && _transformation.notificationData.Method == "OpenDocument";
			model.AddNotification(isDownload ? NotificationModel.Type.DownloadBlueline : (isEmpty ? NotificationModel.Type.Warning : NotificationModel.Type.Info), Resources.ActionFinished, _desc, link, _transformation.Key, _transformation.EndTime, active, isEmpty, visible);
		}

		public void InitializeNotificationModelOnCrashed(NotificationModel model)
		{
			bool active = !_transformation.JobShown.Contains(ViewboxSession.User);
			if (_transformation.Descriptions.Count() != 0)
			{
				model.AddNotification(NotificationModel.Type.Warning, Resources.Error, _transformation.Descriptions[ViewboxSession.Language.CountryCode], null, _transformation.Key, _transformation.EndTime, active, isEmpty: false, _transformation.notificationData == null || _transformation.notificationData.Visible);
			}
		}
	}
}
