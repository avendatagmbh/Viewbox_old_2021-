using System;
using System.Linq;
using SystemDb;

namespace Viewbox
{
	public static class ViewboxRightsExtensions
	{
		public static string GetRoles(this IUser user)
		{
			return string.Join(", ", user.Roles.Select((IRole r) => r.Name));
		}

		public static bool IsAllowed(this IOptimization opt)
		{
			return ViewboxSession.AllowedOptimizations.Contains(opt);
		}

		public static bool IsAllowed(this IOptimization opt, IRole role)
		{
			RightType result = ViewboxApplication.Database.SystemDb.RoleOptimizationRights[role, opt];
			if (result == RightType.Read || result == RightType.Write)
			{
				return true;
			}
			if (result == RightType.None || result == RightType.Inherit)
			{
				return false;
			}
			return false;
		}

		public static bool IsAllowed(this IOptimization opt, IUser user)
		{
			RightType result = ViewboxApplication.Database.SystemDb.UserOptimizationRights[user, opt];
			if (result == RightType.Read || result == RightType.Write)
			{
				return true;
			}
			if (result == RightType.None || result == RightType.Inherit)
			{
				return false;
			}
			return false;
		}

		public static bool CanWrite(this ITableObject tobj)
		{
			return ViewboxApplication.Database.SystemDb.UserTableObjectRights[ViewboxSession.User, tobj] == RightType.Write;
		}

		public static bool CanWrite(this IColumn tobj)
		{
			return ViewboxApplication.Database.SystemDb.UserColumnRights[ViewboxSession.User, tobj] == RightType.Write;
		}

		public static bool CanGrant(this IUser admin, IUser user)
		{
			if (ViewboxApplication.ShowAdminUsersInUserManagement)
			{
				if (admin == user)
				{
					return false;
				}
				if (user.UserName == "avendata_qs" || user.UserName == "avendata_admin")
				{
					return false;
				}
				if (admin.IsSuper)
				{
					return true;
				}
				if (user.IsSuper || !admin.CanGrant)
				{
					return false;
				}
			}
			else
			{
				if (!admin.CanGrant || admin == user || user.IsSuper)
				{
					return false;
				}
				if (admin.IsSuper)
				{
					return true;
				}
			}
			IOptimizationCollection adm_opts = ViewboxApplication.Database.SystemDb.GetOptimizationSubTrees(admin, grantOnly: true);
			IOptimizationCollection usr_opts = ViewboxApplication.Database.SystemDb.GetOptimizationSubTrees(user);
			return (adm_opts.Intersect(usr_opts).Any() && adm_opts.HighestLevel <= usr_opts.HighestLevel) || (adm_opts.Count > 0 && usr_opts.Count == 0);
		}

		public static bool CanGrant(this IUser admin, IRole role)
		{
			if (admin.IsSuper)
			{
				return true;
			}
			if (!admin.CanGrant || role.Flags.HasFlag(SpecialRights.Super))
			{
				return false;
			}
			IOptimizationCollection adm_opts = ViewboxApplication.Database.SystemDb.GetOptimizationSubTrees(admin, grantOnly: true);
			IOptimizationCollection role_opts = ViewboxApplication.Database.SystemDb.GetOptimizationSubTrees(role);
			return (adm_opts.Intersect(role_opts).Any() && adm_opts.HighestLevel <= role_opts.HighestLevel) || (adm_opts.Count > 0 && role_opts.Count == 0);
		}

		public static bool IsAllowedInRightsMode(this IOptimization opt)
		{
			return ViewboxSession.RightsModeObjects.Optimizations[opt.Id]?.IsAllowed() ?? false;
		}

		public static bool IsAllowedInRightsMode(this IOptimization opt, IRole role)
		{
			return ViewboxSession.RightsModeObjects.Optimizations[opt.Id]?.IsAllowed(role) ?? false;
		}

		public static bool IsAllowedInRightsMode(this IOptimization opt, IUser user)
		{
			return ViewboxSession.RightsModeObjects.Optimizations[opt.Id]?.IsAllowed(user) ?? false;
		}

		public static bool IsAllowedInRightsMode(this ICategory cat)
		{
			return ViewboxSession.RightsModeObjects.Categories[cat.Id] != null;
		}

		public static bool IsUserLogAllowedInRightsMode(this IUser grantUser, IUser user)
		{
			if (ViewboxSession.RightsModeCredential.Type == CredentialType.User)
			{
				return ViewboxApplication.Database.SystemDb.GetUserRightsToUserLogs(ViewboxSession.RightsModeCredential.Id, user);
			}
			if (ViewboxSession.RightsModeCredential.Type == CredentialType.Role)
			{
				return ViewboxApplication.Database.SystemDb.GetRoleRightsToUserLogs(ViewboxSession.RightsModeCredential.Id, user);
			}
			throw new ArgumentException("Unknown credential type: " + ViewboxSession.RightsModeCredential.Type);
		}

		public static bool IsTableTypeAllowedInRightsMode(this ICategory cat, TableType type)
		{
			if (ViewboxSession.RightsModeCredential.Type == CredentialType.User)
			{
				return ViewboxApplication.Database.SystemDb.GetUserRightToTableType(type, ViewboxSession.SelectedSystem, ViewboxSession.RightsModeCredential.Id);
			}
			if (ViewboxSession.RightsModeCredential.Type == CredentialType.Role)
			{
				return ViewboxApplication.Database.SystemDb.GetRoleRightToTableType(type, ViewboxSession.SelectedSystem, ViewboxSession.RightsModeCredential.Id);
			}
			throw new ArgumentException("Unknown credential type: " + ViewboxSession.RightsModeCredential.Type);
		}

		public static bool IsAllowedInRightsMode(this ITableObject obj)
		{
			if (obj == null)
			{
				return true;
			}
			if (ViewboxSession.RightsModeCredential.Type == CredentialType.User)
			{
				return ViewboxApplication.Database.SystemDb.GetUserRightsToTable(obj.Id, ViewboxSession.RightsModeCredential.Id);
			}
			if (ViewboxSession.RightsModeCredential.Type == CredentialType.Role)
			{
				return ViewboxApplication.Database.SystemDb.GetRoleRightsToTable(obj.Id, ViewboxSession.RightsModeCredential.Id);
			}
			throw new ArgumentException("Unknown credential type: " + ViewboxSession.RightsModeCredential.Type);
		}

		public static bool IsAllowedInRightsMode(this IColumn col)
		{
			if (ViewboxSession.RightsModeCredential.Type == CredentialType.User)
			{
				return ViewboxApplication.Database.SystemDb.GetUserRightsToColumn(ViewboxSession.RightsModeCredential.Id, col.Id);
			}
			if (ViewboxSession.RightsModeCredential.Type == CredentialType.Role)
			{
				return ViewboxApplication.Database.SystemDb.GetRoleRightsToColumn(ViewboxSession.RightsModeCredential.Id, col.Id);
			}
			throw new ArgumentException("Unknown credential type: " + ViewboxSession.RightsModeCredential.Type);
		}
	}
}
