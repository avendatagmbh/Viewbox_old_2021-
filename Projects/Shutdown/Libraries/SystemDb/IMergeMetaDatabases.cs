using System.Collections.Generic;
using System.ComponentModel;
using SystemDb.Internal;

namespace SystemDb
{
	internal interface IMergeMetaDatabases : INotifyPropertyChanged
	{
		string StatusText { get; set; }

		void Table(ITableObjectCollection fromTables, ITableObjectCollection toTables);

		void OrderArea(ITableObjectCollection tableobjects, ITableObjectCollection tableobjectsDestination);

		void NewLogActionMerge(List<NewLogActionMerge> newLogActions);

		void TableText(ITableObjectCollection tableCollection, ILanguageCollection languages);

		void ColumnText(IFullColumnCollection columnCollection, ILanguageCollection languages);

		void CategoryTexts(ICategoryCollection categories, ILanguageCollection languages);

		void OptimizationText(IOptimizationCollection optimizationCollection, ILanguageCollection languages);

		void ParameterTexts(IParameterCollection parameterCollection, ILanguageCollection languages);

		void ParameterValueText(List<ParameterValue> parameterValuesCollection, ILanguageCollection languages);

		void Property(IPropertiesCollection properties, IPropertiesCollection alreadyExistsProperties);

		void PropertyText(IPropertiesCollection propertyCollection, ILanguageCollection languages);

		void OptimizationGroupText(IOptimizationGroupCollection optimizationGroupCollection, ILanguageCollection languages);

		void Columns(IFullColumnCollection columns);

		void Users(IUserIdCollection users, IUserIdCollection alreadyExistingUsers);

		void CategoryMapping(ICategoryCollection categories);

		void OptimizationGroup(IOptimizationGroupCollection optimizationgroups);

		void Roles(IRoleCollection roles);

		void IssueExtensions(IIssueCollection issues);

		void ColumnRole(IRoleColumnRights rolecolumnRights, IRoleColumnRights alreadyexisting);

		void ColumnUser(IUserColumnRights userColumnRight, IUserColumnRights alreadyexistingUserColumnRights);

		void CategoryUser(IUserCategoryRights userCategoryRights, IUserCategoryRights alreadyexisting);

		void CategoryRole(IRoleCategoryRights userCategoryRights, IRoleCategoryRights alreadyexisting);

		void TableRole(IRoleTableObjectRights userCategoryRights, IRoleTableObjectRights alreadyexisting);

		void TableUser(IUserTableObjectRights userCategoryRights, IUserTableObjectRights alreadyexisting);

		void OptimizationUser(IUserOptimizationRights userCategoryRights, IUserOptimizationRights fromOptimizationRights);

		void OptimizationRole(IRoleOptimizationRights userCategoryRights, IRoleOptimizationRights alreadyexisting);

		void Languages(ILanguageCollection toLanguages, ILanguageCollection fromLanguages);

		void UserRole(IUserRoleCollection fromUserRoles, IUserRoleCollection toUserRoles);

		void Parameters(IIssueCollection issues, IIssueCollection issuesDestination);

		void UserPropertySettings(IUserPropertySettingCollection userPropertySettings, IUserPropertySettingCollection alreadyExistsUserPropertySettings);

		void OptimizationAggregate(IOptimizationCollection optimizations, IOptimizationCollection alreadyexistingoptimizations);

		void Relation(List<Relation> relations, List<Relation> oldrelation);

		void ParameterValues(List<ParameterValue> parameterValues, List<ParameterValue> alreadyExistingParameterValues);

		void ParameterCollectionMapping(List<ParameterCollectionMapping> parameterCollectionMappings);

		void Scheme(ISchemeCollection schemes);

		void TableObjectSchemeMapping_(List<TableObjectSchemeMapping> list);

		void TableOriginalName(List<TableOriginalName> list);

		void TableArchiveInformation(List<TableArchiveInformation> list);

		void Info(List<Info> list, List<Info> alreadyExistingList);

		void Note(List<Note> list);

		void About(List<About> list);

		void UserIssueParameterHistory(List<HistoryParameterValue> list);

		void UserOptimizationSettings(List<UserOptimizationSettings> list);

		void UserSettings(IUserSettingsCollection userSettings);

		void Scheme(ISchemeCollection iSchemeCollection, ISchemeCollection iSchemeCollection_2, IMergeMetaDatabases dw);

		void SchemeTexts(IList<SchemeText> iList, IList<SchemeText> iList_2, IMergeMetaDatabases dw);

		void Roles(IRoleCollection iRoleCollection, IRoleCollection iRoleCollection_2);

		void ObjectTypes(List<ObjectTypes> list, List<ObjectTypes> list_2, IMergeMetaDatabases dw);

		void ObjectTypeText(List<ObjectTypeText> list, List<ObjectTypeText> list_2, IMergeMetaDatabases dw);

		void Passwords(IPasswordCollection iPasswordCollection, IPasswordCollection iPasswordCollection_2);

		void OptimizationUser(List<OptimizationUserMapping> list, List<OptimizationUserMapping> list_2);

		void UserRole(List<UserRoleMapping> list, List<UserRoleMapping> list_2);

		void OptimizationGroup(List<OptimizationGroup> list, List<OptimizationGroup> list_2);

		void OptimizationGroupText(List<OptimizationGroupText> list, List<OptimizationGroupText> list_2);

		void OptimizationAggregate(List<Optimization> list, List<Optimization> list_2);

		void OptimizationText(List<OptimizationText> list, List<OptimizationText> list_2);

		void IssueExtensions(List<IssueExtension> list, List<IssueExtension> list_2);
	}
}
