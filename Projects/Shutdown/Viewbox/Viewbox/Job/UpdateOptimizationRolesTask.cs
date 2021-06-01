using System.Collections.Generic;
using SystemDb;
using Viewbox.Models;

namespace Viewbox.Job
{
	public class UpdateOptimizationRolesTask : TransformationNew
	{
		private Dictionary<int, bool> _settings;

		private IRole _role;

		public UpdateOptimizationRolesTask(IRole role, Dictionary<int, bool> settings)
		{
			_settings = settings;
			_role = role;
		}

		protected override void DoWork()
		{
			if (_role != null)
			{
				RoleManagement.UpdateOptimizations(_role, _settings);
			}
		}
	}
}
