using System;
using SystemDb.Helper;
using SystemDb.Internal;

namespace SystemDb
{
	internal interface ICloneBusinessObjects
	{
		TableObject TableObjectClone(ITableObject tableobject, ICategory category, int maxordinal, int mappeddefaultscheme);

		Property Property(IProperty obj);

		OrderArea OrderAreaClone(IOrderArea orderarea, ITableObject tableobject);

		NewLogActionMerge NewLogActionMergeClone(INewLogActionMerge newLogAction, IUser userobject);

		UserPropertySettings UserPropertySettings(IUserPropertySettings obj, IProperty newProperty, IUser newUser);

		IssueExtension IssueExtensionClone(IIssue issue, int id);

		Column ColumnClone(IColumn column, ITableObject tableobject);

		Role RoleClone(IRole role);

		User UserClone(IUser user);

		Parameter ParameterClone(IParameter parameter, ITableObject issue);

		Relation RelationClone(int relationid, int childid, int parentid, int relationIDOffset);

		Category CategoryClone(ICategory category);

		TableUserMapping TableUserMappingClone(RightType right, ITableObject tableobject, IUser user);

		TableRoleMapping TableRoleMappingClone(RightType right, ITableObject tableobject, IRole role);

		ColumnUserMapping ColumnUserMapping(RightType right, IColumn column, IUser user);

		ColumnRoleMapping ColumnRoleMapping(RightType right, IColumn column, IRole role);

		CategoryUserMapping CategoryUser(ICategory category, IUser user, RightType right);

		ParameterCollectionMapping ParameterCollectionMapping(int paramid, int collectionId);

		ParameterValue ParameterValue(IParameterValue paramvalue, int collectionid, int offset);

		CategoryRoleMapping CategoryRoleMappingClone(ICategory category, IRole role, RightType right);

		OptimizationRoleMapping OptimizationRoleClone(IOptimization optimization, IRole role, bool visibility);

		OptimizationGroup OptimizationGroupClone(IOptimizationGroup optgroup);

		LocalizedText LocalizedTextClone(Type type, ILanguage language, int refId, ILocalizedTextCollection desc);

		Optimization OptimizationClone(int parentID, IOptimization optimization, IOptimizationGroup optgroup);

		OptimizationUserMapping OptimizationUserMappingClone(IOptimization optimization, IUser user, bool visibility);

		UserRoleMapping UserRoleMappingClone(IUser user, IRole role, int ordinal);

		Scheme SchemeClone(int id, string partial);

		Language LanguageClone(ILanguage lang);

		TableArchiveInformation CloneTableArchiveInformation(TableArchiveInformation tableOriginalName, int tableid);

		Info CloneInfo(Info newItm);

		Note CloneNote(Note itm, int userid);

		About CloneAbout(About itm);

		HistoryParameterValue CloneUserHistoryParameterValue(HistoryParameterValue itm, int userid, int paramid);

		UserOptimizationSettings CloneUserOptimizationSettings(UserOptimizationSettings itm, int userid, int optid);
	}
}
