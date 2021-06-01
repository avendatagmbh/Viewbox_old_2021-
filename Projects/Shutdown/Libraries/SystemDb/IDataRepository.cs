namespace SystemDb
{
	public interface IDataRepository
	{
		ILanguageCollection Languages { get; }

		IOptimizationGroupCollection OptimizationGroups { get; }

		IOptimizationCollection Optimizations { get; }

		IRoleOptimizationRights RoleOptimizationRights { get; }

		IUserOptimizationRights UserOptimizationRights { get; }

		IUserNameCollection UsersByUserName { get; }

		IUserIdCollection Users { get; }

		IRoleCollection Roles { get; }

		IPropertiesCollection Properties { get; }

		ICategoryCollection Categories { get; }

		IRoleCategoryRights RoleCategoryRights { get; }

		IUserCategoryRights UserCategoryRights { get; }

		ITableObjectCollection Objects { get; }

		IRoleTableObjectRights RoleTableObjectRights { get; }

		IUserTableObjectRights UserTableObjectRights { get; }

		IIssueCollection Issues { get; }

		IViewCollection Views { get; }

		ITableCollection Tables { get; }

		IArchive Archive { get; }

		IArchiveCollection Archives { get; }

		IFullColumnCollection Columns { get; }

		IRoleColumnRights RoleColumnRights { get; }

		IUserColumnRights UserColumnRights { get; }

		IUserColumnSettingCollection UserColumnSettings { get; }

		IUserTableObjectSettingsCollection UserTableObjectSettings { get; }

		IUserColumnOrderSettingsCollection UserColumnOrderSettings { get; }

		IUserPropertySettingCollection UserPropertySettings { get; }

		IUserControllerSettingsCollection UserControllerSettings { get; }

		IUserTableObjectOrderSettingsCollection UserTableObjectOrderSettings { get; }

		IUserOptimizationSettingsCollection UserOptimizationSettings { get; }

		ISchemeCollection Schemes { get; }

		INotesCollection Notes { get; }

		ITableArchiveInformationCollection ArchiveInformation { get; }
	}
}
