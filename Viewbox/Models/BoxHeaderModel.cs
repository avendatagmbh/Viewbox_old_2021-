using System.Collections.Generic;
using System.Linq;
using SystemDb;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class BoxHeaderModel
	{
		private List<IUser> _users;

		private List<IRole> _roles;

		private ViewboxModel ParentModel { get; set; }

		public bool IsAuthenticated => ViewboxSession.IsAuthenticated;

		public string ClassName => IsAuthenticated ? "enabled" : "disabled";

		public IEnumerable<ILanguage> Languages => ViewboxApplication.Languages;

		public ILanguage CurrentLanguage => ViewboxSession.Language ?? ViewboxApplication.BrowserLanguage;

		public int OptimizationGroupId { get; internal set; }

		public string SearchPhrase { get; internal set; }

		public string LabelSearchPhrase => Resources.SearchPhrase;

		public string LabelLanguageSelection => Resources.LanguageSelection;

		public string CaptionAccount => IsAuthenticated ? $"{Resources.Account}: {((ViewboxSession.User != null) ? ViewboxSession.User.Name : HttpContextFactory.Current.User.Identity.Name)}" : Resources.NotLoggedIn;

		public string ReturnUrl { get; set; }

		public bool RightsMode => ViewboxSession.RightsMode;

		public CredentialType RightsModeInstanceType => ViewboxSession.RightsModeCredential.Type;

		public IUser RightsModeUser => (ViewboxSession.RightsModeCredential.Type == CredentialType.User) ? ViewboxApplication.Users[ViewboxSession.RightsModeCredential.Id] : null;

		public IRole RightsModeRole => (ViewboxSession.RightsModeCredential.Type == CredentialType.Role) ? ViewboxApplication.Roles[ViewboxSession.RightsModeCredential.Id] : null;

		public List<IUser> Users
		{
			get
			{
				if (_users == null)
				{
					_users = (from user in ViewboxApplication.Users
						where ViewboxSession.User.CanGrant(user) && IsSettingSelected
						orderby user.GetName()
						select user).ToList();
				}
				return _users;
			}
		}

		public List<IRole> Roles
		{
			get
			{
				if (_roles == null)
				{
					_roles = (from role in ViewboxApplication.Roles
						where ViewboxSession.User.CanGrant(role)
						orderby role.Name
						select role).ToList();
				}
				return _roles;
			}
		}

		public IUser User => ViewboxSession.User;

		public bool IsSettingSelected
		{
			get
			{
				if (ParentModel != null)
				{
					return ParentModel.LabelCaption == Resources.Settings;
				}
				return false;
			}
		}

		public BoxHeaderModel()
		{
		}

		public BoxHeaderModel(ViewboxModel parentModel)
		{
			ParentModel = parentModel;
		}

		public DialogModel GetWaitDialog(string action)
		{
			return new DialogModel
			{
				Title = Resources.PleaseWait,
				Content = string.Format(Resources.ExecutingAction, action),
				DialogType = DialogModel.Type.Info
			};
		}
	}
}
