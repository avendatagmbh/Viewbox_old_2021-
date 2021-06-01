using System.Collections.Generic;
using SystemDb;
using Viewbox.Models;

namespace Viewbox.Job
{
	public class UpdateTableObjectRolesTask : TransformationNew
	{
		private Dictionary<int, bool> _settings;

		private IRole _role;

		public UpdateTableObjectRolesTask(IRole role, Dictionary<int, bool> settings)
		{
			_settings = settings;
			_role = role;
		}

		protected override void DoWork()
		{
			if (_role != null)
			{
				RoleManagement.UpdateTableObjectVisibilities(_role, _settings);
			}
		}
	}
}
