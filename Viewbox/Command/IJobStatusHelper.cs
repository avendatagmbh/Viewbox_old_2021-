using Viewbox.Models;

namespace Viewbox.Command
{
	public interface IJobStatusHelper
	{
		void InitializeNotificationModelOnFinish(NotificationModel model);

		void InitializeNotificationModelOnCrashed(NotificationModel model);
	}
}
