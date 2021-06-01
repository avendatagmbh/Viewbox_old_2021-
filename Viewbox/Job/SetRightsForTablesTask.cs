using System.Linq;
using SystemDb;
using Viewbox.Models;

namespace Viewbox.Job
{
	public class SetRightsForTablesTask : TransformationNew
	{
		private bool _visible;

		private IRole _role;

		public SetRightsForTablesTask(bool visible)
		{
			_visible = visible;
			if (ViewboxSession.RightsMode && ViewboxSession.RightsModeCredential.Type == CredentialType.Role)
			{
				_role = ViewboxApplication.Database.SystemDb.Roles.FirstOrDefault((IRole r) => r.Id == ViewboxSession.RightsModeCredential.Id);
			}
		}

		protected override void DoWork()
		{
			if (_role != null)
			{
				RoleManagement.UpdateTablesVisibility(_role, _visible);
			}
		}
	}
}
