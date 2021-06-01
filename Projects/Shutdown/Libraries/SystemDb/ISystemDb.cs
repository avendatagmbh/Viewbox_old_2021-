using System;
using System.Collections.Generic;
using System.IO;
using SystemDb.Internal;
using SystemDb.Upgrader;
using DbAccess;
using DbAccess.Structures;

namespace SystemDb
{
	public interface ISystemDb
	{
		ILanguageCollection Languages { get; }

		ILanguage DefaultLanguage { get; }

		About About { get; }

		IOptimizationGroupCollection OptimizationGroups { get; }

		IOptimizationCollection Optimizations { get; }

		IRoleOptimizationRights RoleOptimizationRights { get; }

		IUserOptimizationRights UserOptimizationRights { get; }

		IUserNameCollection UsersByUserName { get; }

		IUserIdCollection Users { get; }

		IRoleCollection Roles { get; }

		IUserSettingsCollection UserSettings { get; }

		IPasswordCollection Passwords { get; }

		IUserRoleCollection UserRole { get; }

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

		IList<SchemeText> SchemeTexts { get; }

		INotesCollection Notes { get; }

		ITableArchiveInformationCollection ArchiveInformation { get; }

		IDataRepository BusinessObjectRepository { set; }

		string ConnectionString { get; }

		DatabaseBaseOutOfDateInformation DatabaseOutOfDateInformation { get; }

		ConnectionManager ConnectionManager { get; }

		DatabaseBase DB { get; set; }

		event Action DataBaseUpGradeChecked;

		event EventHandler ConnectionEstablished;

		event EventHandler SystemDbInitialized;

		event SystemDb.PartLoadingCompletedHandler PartLoadingCompleted;

		event ErrorEventHandler Error;

		event Action LoadingFinished;

		void Connect(string connectionString, int maxConnections);

		IOptimizationCollection GetOptimizations(IRole role, bool grantOnly = false);

		IOptimizationCollection GetOptimizations(IUser user, bool grantOnly = false);

		IOptimizationCollection GetOptimizationSubTrees(IRole role, bool grantOnly = false);

		IOptimizationCollection GetOptimizationSubTrees(IUser user, bool grantOnly = false);

		void CopyColumns(IFullColumnCollection fromcolumns, out IFullColumnCollection toColumns);

		void CopyTableObjects(ITableObjectCollection tableObjects, out ITableObjectCollection toTables);

		void CopyColumnsToUserObjects(IFullColumnCollection fromColumns, IUserObjects userObjects);

		void CopyTableObjectsToUserObjects(ITableObjectCollection tableObjects, IUserObjects userObjects);

		TableCounts GetObjectCounts(TableType type, IUser user, string system, string search, ILanguage language, IOptimization optimization, int? objectTypeFilter, int? extendedObjectTypeFilter);

		long GetDataCount(ITableObject table, IOptimization opt, ITableObject original = null, IUser user = null, bool MultiOptimizations = false, IDictionary<int, Tuple<int, int>> OptimizationSelected = null, bool writeToDatabase = true);

		void AddTableObjectsForExport(TableType type, IUser user, ITableCollection tables, IViewCollection views, ITableObjectCollection tableObjectCollection, string database, string search, int page, int size, int sortColumn, bool direction, ILanguage language, IOptimization optimization, out int count);

		void AddTableObjects(TableType type, IUser user, ITableCollection tables, IViewCollection views, IIssueCollection issues, ITableObjectCollection tableObjects, string system, string search, int page, int size, bool showEmpty, bool showHidden, bool showArchived, int sortColumn, bool direction, ILanguage language, IOptimization optimization, out int fullTableListCount, IEnumerable<int> exclude, int? objectTypeFilter = null, int? Extended_ObjectType = null, bool showEmptyHidden = false);

		void AddTableToTableObjects(IUser user, ITableObjectCollection tableObjects, int id);

		void AddTableColumns(IUser user, IFullColumnCollection columns, ITableObjectCollection tableObjects, ITableCollection tables, IViewCollection views, IOptimization optimization, bool useDistinctInfo, int id);

		bool GetRoleRightsToColumn(int roleId, int columnId);

		bool GetUserRightsToColumn(int userId, RightObjectNode node);

		bool GetUserRightsToColumn(int userId, int columnId);

		int[] ReadTableTypeIds(TableType type, string system, int userId);

		bool GetUserRightToTableType(TableType type, string system, int userId);

		bool GetRoleRightToTableType(TableType type, string system, int roleId);

		bool GetRoleRightsToTable(int tableId, int roleId);

		bool GetUserRightsToTable(int tableId, int userId);

		IUserObjects GetUserObjects(IUser user);

		IUserObjects GetRoleObjects(IRole role);

		void Dispose();

		IColumn CreateTemporaryColumn(IColumn col, string name, int ordinal, bool originalColumnIds = false);

		IColumn CreateTemporaryColumn(IColumn column, bool originalColumnIds = false);

		IColumnCollection CreateTemporaryColumnCollection();

		void SetRowCount(ITableObject tobj, long rowCount);

		ITableObject CreateTemporaryJoinTableObject(ITableObject table, string database, IColumnCollection columns1, IColumnCollection columns2, string tableName, Dictionary<string, string> descriptions, IEnumerable<ILanguage> languages, string saveName = null, int? forcedTableId = null);

		ITableObject CreateTemporaryGroupTableObject(ITableObject table, string database, List<Tuple<IColumn, string, string>> columns, string tableName, Dictionary<string, string> descriptions, IEnumerable<ILanguage> languages, Dictionary<Tuple<ILanguage, string>, string> aggDescriptions, string saveName = null);

		ITableObject CreateTemporaryTableObject(ITableObject table, IColumnCollection columns, bool originalColumnIds = false);

		ITableObjectCollection CreateTableObjectCollection();

		void UpdateProperty(IUser user, IProperty property);

		IPropertiesCollection GetProperties(IUser user);

		bool AddColumnToCollection(IFullColumnCollection columns, IColumnCollection cols);

		ICategoryCollection GetCategoryCollection();

		IUser CreateUser(DatabaseBase connection, string userName, string name, SpecialRights flags, ExportRights exportAllowed, string password, string email, int id, bool isAdUser = false, string domain = null, bool firstLogin = false);

		IRole CreateRole(DatabaseBase connection, string name, SpecialRights flags, ExportRights export, RoleType type, int id);

		void UpdateRight(ICredential credential, UpdateRightType type, int id, RightType right);

		void EditIssue(DatabaseBase connection, IIssue issue, IEnumerable<ILanguage> languages, Dictionary<string, List<string>> descriptions);

		int SaveIssue(DatabaseBase connection, ITableObject tobj, string command, string rownocommand, List<IParameter> realParameters, IEnumerable<ILanguage> languages, IUser user, Dictionary<string, List<string>> descriptions, out List<IParameter> issueParameters, int decimalSeparator, List<bool> freeselection = null);

		void SaveIssueExtensionsAndParameter(DatabaseBase connection, IEnumerable<ILanguage> languages, Dictionary<string, List<string>> descriptions, IIssue issue, List<IParameter> parameters, ITableObject tableObject);

		void SaveIssueParameters(DatabaseBase connection, IEnumerable<ILanguage> languages, Dictionary<string, List<string>> descriptions, IIssue issue, List<IParameter> parameters, ITableObject tableObject = null);

		void SaveIssueExtension(DatabaseBase connection, IIssue issue, List<IParameter> parameters, ITableObject tableObject, string rownoFilter);

		void DeleteIssue(DatabaseBase connection, IIssue issue, IUser user);

		ITableObject SaveView(DatabaseBase connection, ITableObject tobj, IEnumerable<ILanguage> languages, IUser user, string database, bool joinSave = false);

		void DeleteView(DatabaseBase connection, IView view, IUser user);

		void AddUserRoleMapping(int user, int role);

		void RemoveUserRoleMapping(int user, int role);

		void RemoveUser(int user);

		void RemoveRole(int role);

		void UpdateColumn(IUser user, int columnId, bool visible, bool save);

		void UpdateTableObjectText(ITableObject tableobject, ILanguage language);

		void UpdateCategoriesText(ICategory cat, ILanguage lang);

		void UpdateParameterValueText(IParameterValue paramvalue, ILanguage language);

		void UpdateOptimizationGroupText(IOptimizationGroup optgroup, ILanguage language);

		void UpdateOptimizationText(IOptimization optim, ILanguage language);

		void UpdateParameterText(IParameter param, ILanguage language);

		void UpdatePropertyText(IProperty property, ILanguage language);

		void UpdateSchemeText(IScheme scheme, ILanguage language);

		void DeleteColumns(List<int> columnlisttoremove);

		void DeleteTableObjects(List<ITableObject> tableobjectlisttoremove);

		void DeleteTableTexts(List<int> tabletextremove);

		void DeleteColumnTexts(List<int> columntextlisttoremove);

		void DeleteOrderArea(List<int> tablecollection);

		void UpdateColumnText(IColumn column, ILanguage language);

		void UpdateTableObject(IUser user, ITableObject tobj);

		void UpdateColumnOrder(IUser user, ITableObject tobj, string columnOrder);

		void UpdateTableObjectOrder(IUser user, TableType type, ITableObject tobj, bool remove = false);

		void UpdateTableObjectArchived(ITableObject tableObject, bool archived);

		void UpdateTableObjectCreateStructure(ITableObject tableObject, string createStructure);

		string GetCreationString(ITableObject tableObject);

		string GetUserController(IUser user);

		void UpdateUserController(IUser user, string controller);

		IOptimization GetUserOptimization(IUser user);

		void DeleteTableText(int tableId);

		void DeleteTableOriginalName(int tableId);

		void DeleteTableSchemes(int tableId);

		void DeleteUserTableSettings(int tableId);

		void DeleteTableArchiveInfo(int tableId);

		void DeleteTableUsers(int tableId);

		void DeleteTableRoles(int tableId);

		void DeleteIssues(int tableId);

		void DeleteColumn(int columnId2Remove);

		void DeleteColumnOrder(int tableID);

		void DeleteTableOrder(int tableID);

		void DeleteUsersColumn(int columnId);

		void DeleteColumnTexts(int columnId);

		void DeleteParameters(int issueid);

		void DeleteParameterCollection(int parameterid);

		void DeleteParameterValue(IParameterValue paramvalue);

		void DeleteParameterText(int parameterid);

		void DeleteRoleColumn(int columnId);

		void DeleteUserColumnSettings(int columnId);

		void DeleteOptimizationUser(IOptimization opt, IUser user);

		void DeleteOrderArea(int tableId);

		void DeleteOptimization(IOptimization optimization);

		void DeleteOptimizationRole(IOptimization optimization, IRole role);

		void DeleteUserOptimizationMapping(IOptimization opt, IUser user);

		void DeleteTable(int tableId);

		void DeleteTableObject(int tableobjectId);

		void DeleteOptimizationText(IOptimization opt);

		void RemoveOptimizationFromAllTables(IOptimization opt);

		void RemoveOptimizationFromAllTables(int optid);

		void UpdateUserOptimization(IUser user, IOptimization optimization, bool saveToDb = false);

		void PerformChanges();

		INote CreateNote(DatabaseBase db, IUser user, string title, string text, DateTime date);

		INote UpdateNote(DatabaseBase db, int id, IUser user, string title, string text);

		void DeleteNote(DatabaseBase db, int id, IUser user);

		void CreateServerLogEntry(DatabaseBase db, DateTime dt);

		bool IsOriginalColumnOrder(ITableObject tobj, int originalId = 0);

		List<Tuple<int, string>> ResetColumnOrder(ITableObject tobj, IUser user, ITableObject orTable, int originalId = 0);

		Dictionary<string, string> GetTableNamesAndDescriptionsFromSystemDb(TableType type = TableType.All);

		ITableObject CreateAndSaveTableObject(TableType type, string database, string name, long rowCount, List<DbColumnInfo> columnInfos, Dictionary<string, string> optimizeCriterias, Dictionary<string, string> tableDescriptions, Dictionary<string, Dictionary<string, string>> columnDictionary, string procedureCommand = "", string rowNoCommand = "", string objectTypeName = null);

		void AddTableTexts(Dictionary<string, string> tableDescriptions, ITableObject tobj, DatabaseBase db);

		void AddColumnTexts(Dictionary<string, Dictionary<string, string>> columnDictionary, DatabaseBase db, string columnName, int columnId);

		void UpdateOptimizationText(IOptimization optimization, Dictionary<string, string> langTodescription);

		List<string> GetFakeTables(string dbName);

		List<ITableObject> GetFakeTableObjects(string dbName);

		void AddFakeTable(string dbName, string table);

		void DeleteFakeTable(string dbName, string table);

		void DeleteRelations(string systemName);

		void AddRelations(List<IRelationDatabaseObject> relations);

		void ImportLoginDb(string login_db_name);

		void ImportSystemDb(string system_db_name);

		void CheckDatatypes(int fromId = 0);

		void ReorderTables(int fromId = 0);

		void CalculateOrderArea(ITableObject tobj, string orderCols, DatabaseBase viewboxDb);

		void FixSqlServerTypes();

		void UpgradeDatabase(DatabaseBase conn);

		void CheckCounts(DatabaseBase conn);

		void RemoveNotExistingTables(DatabaseBase conn);

		void ImportDescriptionsFromSystemDb(string system_db_name);

		void LoadTables(DatabaseBase db, bool useNewIssueMethod = false);

		void LoadTablesImpl(DatabaseBase db, bool useNewIssueMethod = false);

		ITableObjectCollection ModifyIssue(DatabaseBase db, IIssue issue, ITableObjectCollection tableObjectsForModifying);

		Dictionary<string, long> GetTableNamesAndComplexity(string statement, Dictionary<string, long> tableObjectsNamesAndComplexity);

		Dictionary<string, long> GetTableNamesAndComplexityNew(string statement, Dictionary<string, long> tableObjectsNamesAndComplexity);

		DatabaseBaseOutOfDateInformation HasDatabaseUpgrade(DatabaseBase conn);

		void CreateInfoTable(DatabaseBase conn);

		void UpdateParameterValue(IParameterValue iParameterValue, string p);

		void CreateParameterValue(IParameter parentparam, string p, Dictionary<ILanguage, string> texts);

		IUserSetting AddUserSetting(IUser user, string settingName, string value);

		bool RemoveUserSetting(IUser user, string settingName);

		void SetTableEngineForTableObject(ITableObject table);
	}
}
