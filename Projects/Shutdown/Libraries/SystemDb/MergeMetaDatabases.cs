using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SystemDb.Helper;
using SystemDb.Internal;
using AV.Log;
using log4net;

namespace SystemDb
{
	internal class MergeMetaDatabases : NotifyPropChangedBase, IMergeMetaDatabases, INotifyPropertyChanged
	{
		private interface IHasId
		{
			int Id { get; set; }
		}

		private readonly List<int> _existingSchemeList = new List<int>();

		private readonly Dictionary<int, int> _objectTypeIdCorrespondance = new Dictionary<int, int>();

		private readonly Dictionary<int, int> _optimizationGroupIdCorrespondance = new Dictionary<int, int>();

		private readonly Dictionary<int, int> _optimizationGroupTextIdCorrespondance = new Dictionary<int, int>();

		private readonly Dictionary<int, int> _optimizationIdCorrespondance = new Dictionary<int, int>();

		private readonly Dictionary<int, int> _roleIdCorrespondance = new Dictionary<int, int>();

		private readonly Dictionary<int, int> _schemeIdsCorrespondance = new Dictionary<int, int>();

		private readonly Dictionary<int, int> _tableIdCorrespondence = new Dictionary<int, int>();

		private readonly Dictionary<int, int> _userIdCorrespondance = new Dictionary<int, int>();

		private readonly ILog log = LogHelper.GetLogger();

		private string _statusText;

		private ICloneBusinessObjects CloneBusinessObjects { get; set; }

		private IMapBusinessObjectList MapBusinessObjectList { get; set; }

		private Dictionary<ICategory, ICategory> categorycategoryMapping { get; set; }

		private Dictionary<ITableObject, ITableObject> tabletableMapping { get; set; }

		private Dictionary<INewLogActionMerge, INewLogActionMerge> newlogactionnewlogactionMapping { get; set; }

		private Dictionary<IColumn, IColumn> columncolumnMapping { get; set; }

		private Dictionary<IUser, IUser> useruserMapping { get; set; }

		private Dictionary<IRole, IRole> roleroleMapping { get; set; }

		private Dictionary<IUserSetting, IUserSetting> userSettingsMapping { get; set; }

		private Dictionary<IParameter, IParameter> parameterparameterMapping { get; set; }

		private Dictionary<IOptimizationGroup, IOptimizationGroup> optgroupoptgroupMapping { get; set; }

		private Dictionary<IIssue, IIssue> issueissueMapping { get; set; }

		private Dictionary<IOptimization, IOptimization> optoptMapping { get; set; }

		private Dictionary<int, int> collectionIdcollectionIdMapping { get; set; }

		private Dictionary<ParameterValue, ParameterValue> parametervalueparametervalueMapping { get; set; }

		private Dictionary<IScheme, IScheme> schemeschemeMapping { get; set; }

		private Dictionary<IProperty, IProperty> propertipropertyMapping { get; set; }

		private List<Role> _roles { get; set; }

		private List<UserSetting> _userSettings { get; set; }

		private List<Column> _column { get; set; }

		private List<TableObject> TableObjects { get; set; }

		private List<Property> _Properties { get; set; }

		private List<OrderArea> OrderAreas { get; set; }

		private List<IssueExtension> _issueextension { get; set; }

		private List<User> _users { get; set; }

		private List<Parameter> _parameeters { get; set; }

		private List<ParameterValue> _parametervalues { get; set; }

		private List<ParameterCollectionMapping> _ParameterCollectionMappings { get; set; }

		private List<TableUserMapping> _tableuserrights { get; set; }

		private List<TableRoleMapping> _tablerolerights { get; set; }

		private List<Category> _categories { get; set; }

		private List<CategoryUserMapping> _categoryusermappings { get; set; }

		private List<CategoryRoleMapping> _categoryrolemappings { get; set; }

		private List<OptimizationGroup> _optgroupmappings { get; set; }

		private List<Optimization> _optimization { get; set; }

		private List<ColumnRoleMapping> _columnroleMapping { get; set; }

		private List<ColumnUserMapping> _ColumnUserMappings { get; set; }

		private List<OptimizationUserMapping> _optuserrights { get; set; }

		private List<OptimizationRoleMapping> _optrolesrights { get; set; }

		private List<UserRoleMapping> _userrole { get; set; }

		private List<Relation> _relations { get; set; }

		private List<Scheme> _schemes { get; set; }

		private List<Language> _Languages { get; set; }

		private List<UserPropertySettings> _UserPropertySettings { get; set; }

		private List<LocalizedText> LocalizedText { get; set; }

		public string StatusText
		{
			get
			{
				return _statusText;
			}
			set
			{
				if (!string.Equals(_statusText, value, StringComparison.InvariantCulture))
				{
					_statusText = value;
					OnPropertyChanged("StatusText");
				}
			}
		}

		public MergeMetaDatabases(ICloneBusinessObjects cloneBusinessObjects, IMapBusinessObjectList mapBusinessObject)
		{
			MapBusinessObjectList = mapBusinessObject;
			MapBusinessObjectList.PropertyChanged -= MapBusinessObjectListPropertyChanged;
			MapBusinessObjectList.PropertyChanged += MapBusinessObjectListPropertyChanged;
			CloneBusinessObjects = cloneBusinessObjects;
			TableObjects = new List<TableObject>();
			OrderAreas = new List<OrderArea>();
			_issueextension = new List<IssueExtension>();
			_column = new List<Column>();
			_users = new List<User>();
			_roles = new List<Role>();
			_parameeters = new List<Parameter>();
			_tableuserrights = new List<TableUserMapping>();
			_tablerolerights = new List<TableRoleMapping>();
			_categories = new List<Category>();
			_categoryusermappings = new List<CategoryUserMapping>();
			_optgroupmappings = new List<OptimizationGroup>();
			_optimization = new List<Optimization>();
			_columnroleMapping = new List<ColumnRoleMapping>();
			_ColumnUserMappings = new List<ColumnUserMapping>();
			_categoryrolemappings = new List<CategoryRoleMapping>();
			_optuserrights = new List<OptimizationUserMapping>();
			_optrolesrights = new List<OptimizationRoleMapping>();
			_userrole = new List<UserRoleMapping>();
			_relations = new List<Relation>();
			_parametervalues = new List<ParameterValue>();
			_ParameterCollectionMappings = new List<ParameterCollectionMapping>();
			_schemes = new List<Scheme>();
			_Languages = new List<Language>();
			_Properties = new List<Property>();
			_UserPropertySettings = new List<UserPropertySettings>();
			_userSettings = new List<UserSetting>();
			LocalizedText = new List<LocalizedText>();
			tabletableMapping = new Dictionary<ITableObject, ITableObject>();
			newlogactionnewlogactionMapping = new Dictionary<INewLogActionMerge, INewLogActionMerge>();
			propertipropertyMapping = new Dictionary<IProperty, IProperty>();
			columncolumnMapping = new Dictionary<IColumn, IColumn>();
			useruserMapping = new Dictionary<IUser, IUser>();
			roleroleMapping = new Dictionary<IRole, IRole>();
			categorycategoryMapping = new Dictionary<ICategory, ICategory>();
			parameterparameterMapping = new Dictionary<IParameter, IParameter>();
			userSettingsMapping = new Dictionary<IUserSetting, IUserSetting>();
			optgroupoptgroupMapping = new Dictionary<IOptimizationGroup, IOptimizationGroup>();
			issueissueMapping = new Dictionary<IIssue, IIssue>();
			optoptMapping = new Dictionary<IOptimization, IOptimization>();
			collectionIdcollectionIdMapping = new Dictionary<int, int>();
			parametervalueparametervalueMapping = new Dictionary<ParameterValue, ParameterValue>();
			schemeschemeMapping = new Dictionary<IScheme, IScheme>();
		}

		public void Table(ITableObjectCollection fromTables, ITableObjectCollection toTables)
		{
			int itemsCurrent = 0;
			int startId = toTables.ToList().Max((ITableObject t) => t.Id);
			int startingOrdinal = GetStartingOrdinal(toTables);
			foreach (ITableObject fromTable in fromTables)
			{
				TableObject newtable = CloneBusinessObjects.TableObjectClone(fromTable, categorycategoryMapping[fromTable.Category], startingOrdinal, GetMappedSchemeId(fromTable.DefaultScheme.Id));
				ITableObject toTable = toTables.Where((ITableObject t) => t.Database == fromTable.Database && t.Name == fromTable.Name).SingleOrDefault();
				if (toTable != null)
				{
					newtable.Id = toTable.Id;
					_tableIdCorrespondence[fromTable.Id] = toTable.Id;
				}
				else
				{
					startId = (_tableIdCorrespondence[fromTable.Id] = startId + 1);
				}
				tabletableMapping.Add(fromTable, newtable);
				TableObjects.Add(newtable);
				itemsCurrent++;
			}
			MapBusinessObjectList.MapCollection(TableObjects);
		}

		public void Property(IPropertiesCollection properties, IPropertiesCollection alreadyExistsProperties)
		{
			foreach (IProperty prop in properties)
			{
				Property newprop = CloneBusinessObjects.Property(prop);
				PropertyUniqueKeyCheck(newprop, alreadyExistsProperties);
				propertipropertyMapping.Add(prop, newprop);
				_Properties.Add(newprop);
			}
			MapBusinessObjectList.MapCollection(_Properties);
		}

		public void UserPropertySettings(IUserPropertySettingCollection userPropertySettings, IUserPropertySettingCollection alreadyExistsUserPropertySettings)
		{
			foreach (KeyValuePair<IUser, IUser> item in useruserMapping)
			{
				foreach (UserPropertySettings ups in (userPropertySettings as UserPropertySettingCollection)[item.Key])
				{
					UserPropertySettings newprop = CloneBusinessObjects.UserPropertySettings(ups, propertipropertyMapping[ups.Property], useruserMapping[ups.User]);
					_UserPropertySettings.Add(newprop);
				}
			}
			MapBusinessObjectList.MapCollection(_UserPropertySettings);
		}

		public void OrderArea(ITableObjectCollection tableobjects, ITableObjectCollection tableobjectsDestination)
		{
			int itemsCurrent = 0;
			foreach (ITableObject table in tableobjects)
			{
				lock (table)
				{
					ITableObject tableObjectDest = tableobjectsDestination.FirstOrDefault((ITableObject to) => to.TableName.ToLower() == table.TableName.ToLower() && to.Database.ToLower() == table.Database.ToLower());
					foreach (IOrderArea orderarea in table.OrderAreas)
					{
						IOrderArea oldOrderArea = null;
						if (tableObjectDest != null)
						{
							if (tableObjectDest.OrderAreas.Any((IOrderArea oa) => oa.IndexValue == orderarea.IndexValue && oa.SplitValue == orderarea.SplitValue && oa.SortValue == orderarea.SortValue && oa.Start == orderarea.Start && oa.End == orderarea.End))
							{
								continue;
							}
							oldOrderArea = tableObjectDest.OrderAreas.FirstOrDefault((IOrderArea oa) => oa.IndexValue == orderarea.IndexValue && oa.SplitValue == orderarea.SplitValue && oa.SortValue == orderarea.SortValue && (oa.Start != orderarea.Start || oa.End != orderarea.End));
						}
						OrderArea neworderarea = CloneBusinessObjects.OrderAreaClone(orderarea, tabletableMapping[table]);
						if (oldOrderArea != null)
						{
							neworderarea.Id = oldOrderArea.Id;
						}
						OrderAreas.Add(neworderarea);
						itemsCurrent++;
					}
				}
			}
			MapBusinessObjectList.MapCollection(OrderAreas);
		}

		public void NewLogActionMerge(List<NewLogActionMerge> newLogActions)
		{
			List<NewLogActionMerge> tempNewLogActions = new List<NewLogActionMerge>();
			foreach (NewLogActionMerge obj2 in newLogActions)
			{
				IUser tmpUser = useruserMapping.FirstOrDefault((KeyValuePair<IUser, IUser> itm) => itm.Key.Id == obj2.UserId).Value;
				NewLogActionMerge newNewlogAction = CloneBusinessObjects.NewLogActionMergeClone(obj2, tmpUser);
				newlogactionnewlogactionMapping.Add(obj2, newNewlogAction);
				tempNewLogActions.Add(newNewlogAction);
			}
			MapBusinessObjectList.MapCollection(tempNewLogActions);
			foreach (KeyValuePair<INewLogActionMerge, INewLogActionMerge> obj in newlogactionnewlogactionMapping)
			{
				int oldParentId = obj.Key.Parentid;
				INewLogActionMerge newObj = newlogactionnewlogactionMapping.FirstOrDefault((KeyValuePair<INewLogActionMerge, INewLogActionMerge> itm) => itm.Key.Id == oldParentId).Value;
				if (newObj != null)
				{
					int newId = newObj.Id;
					obj.Value.Parentid = newId;
				}
			}
			MapBusinessObjectList.MapCollection(tempNewLogActions);
		}

		public void Columns(IFullColumnCollection columns)
		{
			int itemsCurrent = 0;
			foreach (IColumn column in columns)
			{
				tabletableMapping.ContainsKey(column.Table);
				Column newcolumn = CloneBusinessObjects.ColumnClone(column, tabletableMapping[column.Table]);
				columncolumnMapping.Add(column, newcolumn);
				_column.Add(newcolumn);
				itemsCurrent++;
			}
			MapBusinessObjectList.MapCollection(_column);
		}

		public void Users(IUserIdCollection fromUsers, IUserIdCollection toUsers)
		{
			List<IUser> toUserList = toUsers.ToList();
			List<User> newUsers = new List<User>();
			int startId = ((toUsers.Count > 0) ? toUsers.Max((IUser u) => u.Id) : 0);
			foreach (IUser user in fromUsers)
			{
				IUser toUser = toUserList.SingleOrDefault((IUser u) => u.UserName.Equals(user.UserName, StringComparison.InvariantCultureIgnoreCase));
				if (toUser != null)
				{
					_userIdCorrespondance.Add(user.Id, toUser.Id);
					useruserMapping.Add(user, toUser);
					continue;
				}
				User newuser = CloneBusinessObjects.UserClone(user);
				newuser.Id = 0;
				useruserMapping.Add(user, newuser);
				_userIdCorrespondance.Add(user.Id, ++startId);
				_users.Add(newuser);
				newUsers.Add(newuser);
			}
			MapBusinessObjectList.MapCollection(newUsers);
		}

		public void CategoryMapping(ICategoryCollection categories)
		{
			foreach (ICategory category in categories)
			{
				Category newCategory = CloneBusinessObjects.CategoryClone(category);
				categorycategoryMapping.Add(category, newCategory);
				_categories.Add(newCategory);
			}
			MapBusinessObjectList.MapCollection(_categories);
		}

		public void OptimizationGroup(List<OptimizationGroup> fromOptimizationGroup, List<OptimizationGroup> toOptimizationGroup)
		{
			int lastId = ((toOptimizationGroup.Count > 0) ? toOptimizationGroup.Max((OptimizationGroup og) => og.Id) : 0);
			List<OptimizationGroup> newOptimizationGroups = new List<OptimizationGroup>();
			foreach (OptimizationGroup optimizationGroup in fromOptimizationGroup)
			{
				OptimizationGroup newOptimizationGroup = optimizationGroup.Clone();
				newOptimizationGroup.Id = 0;
				lastId = (_optimizationGroupIdCorrespondance[optimizationGroup.Id] = lastId + 1);
				newOptimizationGroups.Add(newOptimizationGroup);
			}
			MapBusinessObjectList.MapCollection(newOptimizationGroups);
		}

		public void OptimizationGroupText(List<OptimizationGroupText> fromOptimizationGroupText, List<OptimizationGroupText> toOptimizationGroupText)
		{
			int lastId = ((toOptimizationGroupText.Count > 0) ? toOptimizationGroupText.Max((OptimizationGroupText og) => og.Id) : 0);
			List<OptimizationGroupText> newOptimizationGroupTexts = new List<OptimizationGroupText>();
			foreach (OptimizationGroupText optimizationGroupText in fromOptimizationGroupText)
			{
				OptimizationGroupText newOptimizationGroupText = optimizationGroupText.Clone() as OptimizationGroupText;
				newOptimizationGroupText.Id = 0;
				newOptimizationGroupText.RefId = _optimizationGroupIdCorrespondance[optimizationGroupText.RefId];
				lastId = (_optimizationGroupTextIdCorrespondance[optimizationGroupText.Id] = lastId + 1);
				newOptimizationGroupTexts.Add(newOptimizationGroupText);
			}
			MapBusinessObjectList.MapCollection(newOptimizationGroupTexts);
		}

		public void OptimizationGroupText(IOptimizationGroupCollection optimizationGroupCollection, ILanguageCollection languages)
		{
			LocalizedText.Clear();
			foreach (IOptimizationGroup optgroup in optimizationGroupCollection)
			{
				foreach (ILanguage language in languages)
				{
					int refid = optgroupoptgroupMapping[optgroup].Id;
					LocalizedText optimizationgroupText = CloneBusinessObjects.LocalizedTextClone(typeof(OptimizationGroupText), language, refid, optgroup.Names);
					if (!string.IsNullOrEmpty(optimizationgroupText.Text))
					{
						LocalizedText.Add(optimizationgroupText);
					}
				}
			}
			MapBusinessObjectList.MapCollection(LocalizedText);
		}

		public void OptimizationGroup(IOptimizationGroupCollection optimizationgroups)
		{
			foreach (IOptimizationGroup optgroup in optimizationgroups)
			{
				OptimizationGroup newOptimizationGroup = CloneBusinessObjects.OptimizationGroupClone(optgroup);
				optgroupoptgroupMapping.Add(optgroup, newOptimizationGroup);
				_optgroupmappings.Add(newOptimizationGroup);
			}
			MapBusinessObjectList.MapCollection(_optgroupmappings);
		}

		public void Roles(IRoleCollection fromRoles, IRoleCollection toRoles)
		{
			int startId = ((toRoles.Count > 0) ? toRoles.Max((IRole r) => r.Id) : 0);
			foreach (IRole fromRole in fromRoles)
			{
				IRole toRole = toRoles.ToList().SingleOrDefault((IRole r) => r.Name.Equals(fromRole.Name, StringComparison.InvariantCultureIgnoreCase));
				if (toRole != null)
				{
					_roleIdCorrespondance.Add(fromRole.Id, toRole.Id);
					roleroleMapping.Add(fromRole, toRole);
					continue;
				}
				Role newrole = CloneBusinessObjects.RoleClone(fromRole);
				_roleIdCorrespondance.Add(fromRole.Id, ++startId);
				roleroleMapping.Add(fromRole, newrole);
				_roles.Add(newrole);
			}
			MapBusinessObjectList.MapCollection(_roles);
		}

		public void Roles(IRoleCollection roles)
		{
			foreach (IRole role in roles)
			{
				Role newrole = CloneBusinessObjects.RoleClone(role);
				roleroleMapping.Add(role, newrole);
				_roles.Add(newrole);
			}
			MapBusinessObjectList.MapCollection(_roles);
		}

		public void Passwords(IPasswordCollection fromPasswords, IPasswordCollection toPasswords)
		{
			List<Password> newPasswords = new List<Password>();
			foreach (IPassword fromPassword in fromPasswords)
			{
				Password clone = (Password)fromPassword.Clone();
				clone.Id = 0;
				clone.UserId = _userIdCorrespondance[clone.UserId];
				newPasswords.Add(clone);
			}
			MapBusinessObjectList.MapCollection(newPasswords);
		}

		public void UserSettings(IUserSettingsCollection userSettings)
		{
			foreach (UserSetting userSetting in userSettings)
			{
				UserSetting clone = (UserSetting)userSetting.Clone();
				clone.Id = 0;
				clone.UserId = _userIdCorrespondance[clone.UserId];
				_userSettings.Add(clone);
			}
			MapBusinessObjectList.MapCollection(_userSettings);
		}

		public void IssueExtensions(List<IssueExtension> fromIssueExtension, List<IssueExtension> toIssueExtension)
		{
			List<IssueExtension> newIssueExtensions = new List<IssueExtension>();
			foreach (IssueExtension issueExtension in fromIssueExtension)
			{
				if (issueExtension.RefId == 9700 || issueExtension.RefId == 9701)
				{
					issueExtension.RefId = issueExtension.RefId;
				}
				IssueExtension newIssueExtension = issueExtension.Clone() as IssueExtension;
				if (_tableIdCorrespondence.ContainsKey(issueExtension.TableObjectId) && _tableIdCorrespondence.ContainsKey(issueExtension.RefId))
				{
					newIssueExtension.TableObjectId = _tableIdCorrespondence[issueExtension.TableObjectId];
					newIssueExtension.RefId = _tableIdCorrespondence[issueExtension.RefId];
					newIssueExtensions.Add(newIssueExtension);
				}
				else if (issueExtension.TableObjectId == 0 && _tableIdCorrespondence.ContainsKey(issueExtension.RefId))
				{
					newIssueExtension.RefId = _tableIdCorrespondence[issueExtension.RefId];
					newIssueExtensions.Add(newIssueExtension);
				}
				IssueExtension existing = toIssueExtension.FirstOrDefault((IssueExtension ie) => ie.RefId == newIssueExtension.RefId && ie.TableObjectId == newIssueExtension.TableObjectId);
				if (existing == null)
				{
					newIssueExtension.Id = 0;
				}
				else
				{
					newIssueExtension.Id = existing.Id;
				}
			}
			MapBusinessObjectList.MapCollection(newIssueExtensions);
		}

		public void IssueExtensions(IIssueCollection issues)
		{
			foreach (IIssue issue in issues)
			{
				int id = 0;
				foreach (KeyValuePair<ITableObject, ITableObject> kvp in tabletableMapping)
				{
					if (kvp.Key.Id == issue.Id)
					{
						id = kvp.Value.Id;
					}
				}
				IssueExtension issueExtension = CloneBusinessObjects.IssueExtensionClone(issue, id);
				issueExtension.TableObjectId = _tableIdCorrespondence[issueExtension.TableObjectId];
				_issueextension.Add(issueExtension);
			}
			MapBusinessObjectList.MapCollection(_issueextension);
		}

		public void ColumnRole(IRoleColumnRights rolecolumnRights, IRoleColumnRights alreadyexisting)
		{
			foreach (Tuple<IRole, IColumn, RightType> roleColumnRight in rolecolumnRights)
			{
				IRole mappedrole = roleroleMapping[roleColumnRight.Item1];
				IColumn mappedcolumn = columncolumnMapping[roleColumnRight.Item2];
				ColumnRoleMapping newcolumnrolemapping = CloneBusinessObjects.ColumnRoleMapping(roleColumnRight.Item3, mappedcolumn, mappedrole);
				CheckUniqueKeyForColumnRole(newcolumnrolemapping, alreadyexisting);
				_columnroleMapping.Add(newcolumnrolemapping);
			}
			MapBusinessObjectList.MapCollection(_columnroleMapping);
		}

		public void ColumnUser(IUserColumnRights userColumnRight, IUserColumnRights alreadyexistingUserColumnRights)
		{
			foreach (Tuple<IUser, IColumn, RightType> roleColumnRight in userColumnRight)
			{
				IUser mappeduser = useruserMapping[roleColumnRight.Item1];
				IColumn mappedcolumn = columncolumnMapping[roleColumnRight.Item2];
				ColumnUserMapping newcolumnusermapping = CloneBusinessObjects.ColumnUserMapping(roleColumnRight.Item3, mappedcolumn, mappeduser);
				CheckUniqueKeyForColumnUser(newcolumnusermapping, alreadyexistingUserColumnRights);
				_ColumnUserMappings.Add(newcolumnusermapping);
			}
			MapBusinessObjectList.MapCollection(_ColumnUserMappings);
		}

		public void CategoryUser(IUserCategoryRights userCategoryRights, IUserCategoryRights alreadyexisting)
		{
			foreach (Tuple<IUser, ICategory, RightType> roleColumnRight in userCategoryRights)
			{
				IUser mappeduser = useruserMapping[roleColumnRight.Item1];
				ICategory mappedCategory = categorycategoryMapping[roleColumnRight.Item2];
				CategoryUserMapping newcategoryusermapping = CloneBusinessObjects.CategoryUser(mappedCategory, mappeduser, roleColumnRight.Item3);
				CheckUniqueKeyForCategoryUser(newcategoryusermapping, alreadyexisting);
				_categoryusermappings.Add(newcategoryusermapping);
			}
			MapBusinessObjectList.MapCollection(_categoryusermappings);
		}

		public void CategoryRole(IRoleCategoryRights userCategoryRights, IRoleCategoryRights alreadyexisting)
		{
			foreach (Tuple<IRole, ICategory, RightType> roleColumnRight in userCategoryRights)
			{
				IRole mappeduser = roleroleMapping[roleColumnRight.Item1];
				ICategory mappedCategory = categorycategoryMapping[roleColumnRight.Item2];
				CategoryRoleMapping newcategoryusermapping = CloneBusinessObjects.CategoryRoleMappingClone(mappedCategory, mappeduser, roleColumnRight.Item3);
				CheckUniqueKeyForCategoryRole(newcategoryusermapping, alreadyexisting);
				_categoryrolemappings.Add(newcategoryusermapping);
			}
			MapBusinessObjectList.MapCollection(_categoryrolemappings);
		}

		public void TableRole(IRoleTableObjectRights userCategoryRights, IRoleTableObjectRights alreadyexisting)
		{
			foreach (Tuple<IRole, ITableObject, RightType> roleColumnRight in userCategoryRights)
			{
				IRole mappedTableRole = roleroleMapping[roleColumnRight.Item1];
				ITableObject mappedTable = tabletableMapping[roleColumnRight.Item2];
				TableRoleMapping newTableRole = CloneBusinessObjects.TableRoleMappingClone(roleColumnRight.Item3, mappedTable, mappedTableRole);
				CheckUniqueKeyForTableRole(newTableRole, alreadyexisting);
				_tablerolerights.Add(newTableRole);
			}
			MapBusinessObjectList.MapCollection(_tablerolerights);
		}

		public void TableUser(IUserTableObjectRights userCategoryRights, IUserTableObjectRights alreadyexisting)
		{
			foreach (Tuple<IUser, ITableObject, RightType> roleColumnRight in userCategoryRights)
			{
				IUser mappeduser = useruserMapping[roleColumnRight.Item1];
				ITableObject mappedCategory = tabletableMapping[roleColumnRight.Item2];
				TableUserMapping newcategoryusermapping = CloneBusinessObjects.TableUserMappingClone(roleColumnRight.Item3, mappedCategory, mappeduser);
				CheckUniqueKeyForTableUser(newcategoryusermapping, alreadyexisting);
				_tableuserrights.Add(newcategoryusermapping);
			}
			MapBusinessObjectList.MapCollection(_tableuserrights);
		}

		public void OptimizationUser(List<OptimizationUserMapping> fromOptimizationUser, List<OptimizationUserMapping> toOptimizationUser)
		{
			if (fromOptimizationUser == null || fromOptimizationUser.Count == 0)
			{
				return;
			}
			List<OptimizationUserMapping> newOptimizationUsers = new List<OptimizationUserMapping>();
			foreach (OptimizationUserMapping optimizationUser in fromOptimizationUser)
			{
				OptimizationUserMapping newOptimizationUser = optimizationUser.Clone() as OptimizationUserMapping;
				newOptimizationUser.Id = 0;
				if (_userIdCorrespondance.ContainsKey(optimizationUser.UserId) && _optimizationIdCorrespondance.ContainsKey(optimizationUser.OptimizationId))
				{
					newOptimizationUser.UserId = _userIdCorrespondance[optimizationUser.UserId];
					newOptimizationUser.OptimizationId = _optimizationIdCorrespondance[optimizationUser.OptimizationId];
				}
				newOptimizationUsers.Add(newOptimizationUser);
			}
			MapBusinessObjectList.MapCollection(newOptimizationUsers);
		}

		public void OptimizationUser(IUserOptimizationRights userCategoryRights, IUserOptimizationRights fromOptimizationRights)
		{
			foreach (Tuple<IUser, IOptimization, RightType> roleColumnRight in userCategoryRights)
			{
				IUser mappeduser = useruserMapping[roleColumnRight.Item1];
				IOptimization mappedCategory = optoptMapping[roleColumnRight.Item2];
				bool visibility = roleColumnRight.Item3 == RightType.Read;
				OptimizationUserMapping newcategoryusermapping = CloneBusinessObjects.OptimizationUserMappingClone(mappedCategory, mappeduser, visibility);
				CheckUniqueKeyForOptimizationUser(newcategoryusermapping, fromOptimizationRights);
				_optuserrights.Add(newcategoryusermapping);
			}
			MapBusinessObjectList.MapCollection(_optuserrights);
		}

		public void OptimizationRole(IRoleOptimizationRights userCategoryRights, IRoleOptimizationRights alreadyexisting)
		{
			foreach (Tuple<IRole, IOptimization, RightType> roleColumnRight in userCategoryRights)
			{
				IRole mappeduser = roleroleMapping[roleColumnRight.Item1];
				IOptimization mappedCategory = optoptMapping.FirstOrDefault((KeyValuePair<IOptimization, IOptimization> op) => op.Key.Id == roleColumnRight.Item2.Id && op.Key.Value == roleColumnRight.Item2.Value).Value;
				bool visibility = roleColumnRight.Item3 == RightType.Read;
				OptimizationRoleMapping newcategoryusermapping = CloneBusinessObjects.OptimizationRoleClone(mappedCategory, mappeduser, visibility);
				CheckUniqueKeyForOptimizationRole(newcategoryusermapping, alreadyexisting);
				_optrolesrights.Add(newcategoryusermapping);
			}
			MapBusinessObjectList.MapCollection(_optrolesrights);
		}

		public void Languages(ILanguageCollection toLanguages, ILanguageCollection fromLanguages)
		{
			foreach (ILanguage lang in toLanguages)
			{
				if (!_Languages.Exists((Language l) => l.CountryCode == lang.CountryCode))
				{
					Language newlanguage = CloneBusinessObjects.LanguageClone(lang);
					UniqueKeyForLanguage(newlanguage, fromLanguages);
					_Languages.Add(newlanguage);
				}
			}
			MapBusinessObjectList.MapCollection(_Languages);
		}

		public void UserRole(List<UserRoleMapping> fromUserRoles, List<UserRoleMapping> toUserRoles)
		{
			List<UserRoleMapping> userRoleList = new List<UserRoleMapping>();
			List<UserRoleMapping> toUserRoleList = toUserRoles.ToList();
			foreach (UserRoleMapping fromUser in fromUserRoles)
			{
				UserRoleMapping newUserRole = fromUser.Clone() as UserRoleMapping;
				newUserRole.Id = 0;
				newUserRole.RoleId = _roleIdCorrespondance[newUserRole.RoleId];
				newUserRole.UserId = _userIdCorrespondance[newUserRole.UserId];
				if (toUserRoleList.SingleOrDefault((UserRoleMapping ur) => ur.RoleId == newUserRole.RoleId && ur.UserId == newUserRole.UserId) == null)
				{
					userRoleList.Add(newUserRole);
				}
			}
			MapBusinessObjectList.MapCollection(userRoleList);
		}

		public void UserRole(IUserRoleCollection fromUserRoles, IUserRoleCollection toUserRoles)
		{
			foreach (KeyValuePair<Tuple<IUser, IRole>, IUserRoleMapping> roleColumnRight in fromUserRoles)
			{
				IRole mappeduser = roleroleMapping[roleColumnRight.Key.Item2];
				IUser mappedCategory = useruserMapping[roleColumnRight.Key.Item1];
				int ordinal = roleColumnRight.Value.Ordinal;
				UserRoleMapping newcategoryusermapping = CloneBusinessObjects.UserRoleMappingClone(mappedCategory, mappeduser, ordinal);
				CheckUniqueKeyForUserRole(newcategoryusermapping, toUserRoles);
				_userrole.Add(newcategoryusermapping);
			}
			MapBusinessObjectList.MapCollection(_userrole);
		}

		public void Parameters(IIssueCollection issues, IIssueCollection issuesDestination)
		{
			foreach (IIssue issue in issues)
			{
				IIssue destinationIssue = issuesDestination.FirstOrDefault((IIssue i) => i.Id == tabletableMapping[issue].Id);
				foreach (IParameter parameter in issue.Parameters)
				{
					IParameter destParam = null;
					if (destinationIssue != null)
					{
						destParam = destinationIssue.Parameters.FirstOrDefault((IParameter p) => p.Name.ToLower() == parameter.Name.ToLower());
					}
					Parameter newparameter = CloneBusinessObjects.ParameterClone(parameter, tabletableMapping[issue]);
					if (destParam != null)
					{
						newparameter.Id = destParam.Id;
					}
					parameterparameterMapping.Add(parameter, newparameter);
					_parameeters.Add(newparameter);
				}
			}
			MapBusinessObjectList.MapCollection(_parameeters);
		}

		public void OptimizationText(List<OptimizationText> fromOptimizationText, List<OptimizationText> toOptimizationText)
		{
			List<OptimizationText> newOptimizationTexts = new List<OptimizationText>();
			foreach (OptimizationText item in fromOptimizationText)
			{
				OptimizationText newOptimizationText = item.Clone() as OptimizationText;
				newOptimizationText.Id = 0;
				if (_optimizationIdCorrespondance.ContainsKey(newOptimizationText.RefId))
				{
					newOptimizationText.RefId = _optimizationIdCorrespondance[newOptimizationText.RefId];
					newOptimizationTexts.Add(newOptimizationText);
				}
			}
			MapBusinessObjectList.MapCollection(newOptimizationTexts);
		}

		public void OptimizationAggregate(List<Optimization> fromOptimizations, List<Optimization> toOptimizations)
		{
			int lastId = toOptimizations.ToList().Max((Optimization o) => o.Id);
			int newParentId = lastId;
			Dictionary<int, int> parentIds = new Dictionary<int, int>();
			foreach (int parentId in (from o in fromOptimizations
				orderby o.ParentId
				select o.ParentId).Distinct())
			{
				parentIds[parentId] = ((parentId != 0) ? (++newParentId) : 0);
			}
			List<Optimization> newOptimizations = new List<Optimization>();
			foreach (Optimization optimization in fromOptimizations.OrderBy((Optimization o) => o.ParentId))
			{
				Optimization newOptimization = optimization.Clone() as Optimization;
				newOptimization.Id = 0;
				newOptimization.ParentId = parentIds[newOptimization.ParentId];
				newOptimization.OptimizationGroupId = _optimizationGroupIdCorrespondance[newOptimization.OptimizationGroupId];
				Optimization existingOptimization = toOptimizations.SingleOrDefault((Optimization o) => o.Value != null && o.ParentId == newOptimization.ParentId && o.Value.Equals(newOptimization.Value, StringComparison.InvariantCultureIgnoreCase));
				if (existingOptimization != null)
				{
					if (newOptimization.ParentId == 0)
					{
						break;
					}
					_optimizationIdCorrespondance.Add(optimization.Id, existingOptimization.Id);
					optoptMapping.Add(optimization, existingOptimization);
				}
				else
				{
					newOptimizations.Add(newOptimization);
					optoptMapping.Add(optimization, newOptimization);
					_optimizationIdCorrespondance.Add(optimization.Id, ++lastId);
				}
			}
			MapBusinessObjectList.MapCollection(newOptimizations);
		}

		public void OptimizationAggregate(IOptimizationCollection fromOptimizations, IOptimizationCollection toOptimizations)
		{
			IEnumerable<Optimization> toOptimizationList = toOptimizations.ToList().OfType<Optimization>();
			int lastId = toOptimizations.ToList().Max((IOptimization o) => o.Id);
			int newParentId = lastId;
			Dictionary<int, int> parentIds = new Dictionary<int, int>();
			IEnumerable<Optimization> fromOptimizationList = fromOptimizations.ToList().OfType<Optimization>();
			foreach (int parentId in (from o in fromOptimizationList
				orderby o.ParentId
				select o.ParentId).Distinct())
			{
				parentIds[parentId] = ((parentId != 0) ? (++newParentId) : 0);
			}
			List<Optimization> newOptimizations = new List<Optimization>();
			foreach (Optimization optimization in fromOptimizationList.OrderBy((Optimization o) => o.ParentId))
			{
				Optimization newOptimization = optimization.Clone() as Optimization;
				newOptimization.Id = 0;
				newOptimization.ParentId = parentIds[newOptimization.ParentId];
				newOptimization.OptimizationGroupId = _optimizationGroupIdCorrespondance[newOptimization.OptimizationGroupId];
				Optimization existingOptimization = toOptimizationList.SingleOrDefault((Optimization o) => o.Value != null && o.ParentId == newOptimization.ParentId && o.Value.Equals(newOptimization.Value, StringComparison.InvariantCultureIgnoreCase));
				if (existingOptimization != null)
				{
					_optimizationIdCorrespondance.Add(optimization.Id, existingOptimization.Id);
					optoptMapping.Add(optimization, existingOptimization);
				}
				else
				{
					newOptimizations.Add(newOptimization);
					optoptMapping.Add(optimization, newOptimization);
					_optimizationIdCorrespondance.Add(optimization.Id, ++lastId);
				}
			}
			MapBusinessObjectList.MapCollection(newOptimizations);
		}

		public void Relation(List<Relation> relations, List<Relation> oldrelation)
		{
			int relationOffset = GetRelationOffset(oldrelation);
			foreach (Relation relation in relations)
			{
				int childid = 0;
				int parentid = 0;
				foreach (KeyValuePair<IColumn, IColumn> kvp2 in columncolumnMapping)
				{
					if (kvp2.Key.Id == relation.ChildId)
					{
						childid = kvp2.Value.Id;
					}
				}
				foreach (KeyValuePair<IColumn, IColumn> kvp in columncolumnMapping)
				{
					if (kvp.Key.Id == relation.ParentId)
					{
						parentid = kvp.Value.Id;
					}
				}
				Relation newRelation = CloneBusinessObjects.RelationClone(relation.RelationId, childid, parentid, relationOffset);
				_relations.Add(newRelation);
			}
			MapBusinessObjectList.MapCollection(_relations);
		}

		public void ParameterValues(List<ParameterValue> parameterValues, List<ParameterValue> alreadyExistingParameterValues)
		{
			int offset = GetParamValueOffset(alreadyExistingParameterValues);
			foreach (ParameterValue parameterValue in parameterValues)
			{
				try
				{
					ParameterValue newParamValue = CloneBusinessObjects.ParameterValue(parameterValue, parameterValue.CollectionId, offset);
					_parametervalues.Add(newParamValue);
					if (!collectionIdcollectionIdMapping.ContainsKey(parameterValue.CollectionId))
					{
						collectionIdcollectionIdMapping.Add(parameterValue.CollectionId, newParamValue.CollectionId);
					}
					parametervalueparametervalueMapping.Add(parameterValue, newParamValue);
				}
				catch (Exception ex2)
				{
					log.Error(ex2.Message, ex2);
					throw;
				}
			}
			try
			{
				bool wasEx = false;
				try
				{
					MapBusinessObjectList.MapCollection(_parametervalues);
				}
				catch
				{
					wasEx = true;
				}
				if (_parametervalues.Any())
				{
					wasEx = _parametervalues.FirstOrDefault().Id == 0;
				}
				if (!wasEx)
				{
					return;
				}
				int lastId = 0;
				if (alreadyExistingParameterValues.Any())
				{
					lastId = alreadyExistingParameterValues.Max((ParameterValue itm) => itm.Id);
				}
				using (List<ParameterValue>.Enumerator enumerator = _parametervalues.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						lastId = (enumerator.Current.Id = lastId + 1);
					}
				}
				MapBusinessObjectList.MapCollection(_parametervalues);
			}
			catch (Exception ex)
			{
				log.Error(ex.Message, ex);
				throw;
			}
		}

		public void ParameterCollectionMapping(List<ParameterCollectionMapping> parameterCollectionMappings)
		{
			foreach (ParameterCollectionMapping parameterCollectionMapping in parameterCollectionMappings)
			{
				collectionIdcollectionIdMapping.ContainsKey(parameterCollectionMapping.CollectionId);
				ParameterCollectionMapping newpcm = CloneBusinessObjects.ParameterCollectionMapping(GetNewParameterId(parameterCollectionMapping.ParameterId), collectionIdcollectionIdMapping[parameterCollectionMapping.CollectionId]);
				_ParameterCollectionMappings.Add(newpcm);
			}
			MapBusinessObjectList.MapCollection(_ParameterCollectionMappings);
		}

		public void ObjectTypes(List<ObjectTypes> fromObjectTypes, List<ObjectTypes> toObjectTypes, IMergeMetaDatabases dw)
		{
			List<ObjectTypes> newObjectTypes = new List<ObjectTypes>();
			int startId = 0;
			foreach (ObjectTypes fromObjectType in fromObjectTypes)
			{
				ObjectTypes clone = fromObjectType.Clone() as ObjectTypes;
				_objectTypeIdCorrespondance.Add(clone.Id, ++startId);
				clone.Id = 0;
				newObjectTypes.Add(clone);
			}
			MapBusinessObjectList.MapCollection(newObjectTypes);
		}

		public void ObjectTypeText(List<ObjectTypeText> fromObjectTypeTexts, List<ObjectTypeText> toObjectTypeText, IMergeMetaDatabases dw)
		{
			List<ObjectTypeText> newObjectTypeTexts = new List<ObjectTypeText>();
			foreach (ObjectTypeText fromObjectTypeText in fromObjectTypeTexts)
			{
				ObjectTypeText clone = fromObjectTypeText.Clone() as ObjectTypeText;
				clone.Id = 0;
				clone.RefId = _objectTypeIdCorrespondance[clone.RefId];
				newObjectTypeTexts.Add(clone);
			}
			MapBusinessObjectList.MapCollection(newObjectTypeTexts);
		}

		public void Scheme(ISchemeCollection fromSchemes, ISchemeCollection toSchemes, IMergeMetaDatabases dw)
		{
			List<IScheme> toSchemesList = toSchemes.ToList();
			int startId = ((toSchemes.Count > 0) ? toSchemes.Max((IScheme s) => s.Id) : 0);
			foreach (IScheme fromScheme in fromSchemes)
			{
				IScheme existingScheme = toSchemesList.SingleOrDefault((IScheme s) => s.Partial == fromScheme.Partial);
				if (existingScheme == null)
				{
					Scheme clone = (Scheme)fromScheme.Clone();
					_schemeIdsCorrespondance.Add(clone.Id, ++startId);
					clone.Id = 0;
					_schemes.Add(clone);
				}
				else
				{
					_existingSchemeList.Add(existingScheme.Id);
				}
			}
			MapBusinessObjectList.MapCollection(_schemes);
		}

		public void SchemeTexts(IList<SchemeText> fromSchemeTexts, IList<SchemeText> toSchemeTexts, IMergeMetaDatabases dw)
		{
			List<SchemeText> mergedSchemeTexts = new List<SchemeText>();
			foreach (SchemeText fromSchemeText in fromSchemeTexts)
			{
				SchemeText clone = fromSchemeText.Clone() as SchemeText;
				clone.RefId = _schemeIdsCorrespondance[clone.RefId];
				clone.Id = 0;
				mergedSchemeTexts.Add(clone);
			}
			MapBusinessObjectList.MapCollection(mergedSchemeTexts);
		}

		public void Scheme(ISchemeCollection schemes)
		{
			foreach (IScheme scheme in schemes)
			{
				Scheme newscheme = CloneBusinessObjects.SchemeClone(12, scheme.Partial);
				schemeschemeMapping.Add(scheme, newscheme);
				_schemes.Add(newscheme);
			}
			MapBusinessObjectList.MapCollection(_schemes);
		}

		public void TableObjectSchemeMapping_(List<TableObjectSchemeMapping> list)
		{
			List<TableObjectSchemeMapping> newList = new List<TableObjectSchemeMapping>();
			foreach (TableObjectSchemeMapping itm in list)
			{
				TableObjectSchemeMapping newItm = new TableObjectSchemeMapping();
				newItm.SchemeId = GetMappedSchemeId(itm.SchemeId);
				if (_tableIdCorrespondence.ContainsKey(itm.TableObjectId))
				{
					newItm.TableObjectId = GetMappedTableId(itm.TableObjectId);
					newList.Add(newItm);
				}
			}
			MapBusinessObjectList.MapCollection(newList);
		}

		public void TableOriginalName(List<TableOriginalName> list)
		{
			List<TableOriginalName> newList = new List<TableOriginalName>();
			foreach (TableOriginalName itm in list)
			{
				TableOriginalName newItm = new TableOriginalName();
				newItm.OriginalName = itm.OriginalName;
				newItm.RefId = GetMappedTableId(itm.RefId);
				newList.Add(newItm);
			}
			MapBusinessObjectList.MapCollection(newList);
		}

		public void TableArchiveInformation(List<TableArchiveInformation> list)
		{
			List<TableArchiveInformation> newList = new List<TableArchiveInformation>();
			foreach (TableArchiveInformation itm in list)
			{
				TableArchiveInformation newItm = CloneBusinessObjects.CloneTableArchiveInformation(itm, GetMappedTableId(itm.TableId));
				newList.Add(newItm);
			}
			MapBusinessObjectList.MapCollection(newList);
		}

		public void Info(List<Info> list, List<Info> alreadyExistingList)
		{
			List<Info> newList = new List<Info>();
			foreach (Info itm in list)
			{
				Info newItm = CloneBusinessObjects.CloneInfo(itm);
				InfoUniqueKeyCheck(newItm, alreadyExistingList);
				newList.Add(newItm);
			}
			MapBusinessObjectList.MapCollection(newList);
		}

		public void Note(List<Note> list)
		{
			List<Note> newList = new List<Note>();
			foreach (Note itm in list)
			{
				Note newItm = CloneBusinessObjects.CloneNote(itm, GetMappedUserId(itm.UserId));
				newList.Add(newItm);
			}
			MapBusinessObjectList.MapCollection(newList);
		}

		public void About(List<About> list)
		{
			List<About> newList = new List<About>();
			foreach (About itm in list)
			{
				About newItm = CloneBusinessObjects.CloneAbout(itm);
				newList.Add(newItm);
			}
			MapBusinessObjectList.MapCollection(newList);
		}

		public void UserIssueParameterHistory(List<HistoryParameterValue> list)
		{
			List<HistoryParameterValue> newList = new List<HistoryParameterValue>();
			foreach (HistoryParameterValue itm in list)
			{
				HistoryParameterValue newItm = CloneBusinessObjects.CloneUserHistoryParameterValue(itm, GetMappedUserId(itm.UserId), GetMappedParameterId(itm.ParameterId));
				newList.Add(newItm);
			}
			MapBusinessObjectList.MapCollection(newList);
		}

		public void UserOptimizationSettings(List<UserOptimizationSettings> list)
		{
			List<UserOptimizationSettings> newList = new List<UserOptimizationSettings>();
			foreach (UserOptimizationSettings itm in list)
			{
				UserOptimizationSettings newItm = CloneBusinessObjects.CloneUserOptimizationSettings(itm, GetMappedUserId(itm.UserId), GetMappedOptimizationId(itm.OptimizationId));
				newList.Add(newItm);
			}
			MapBusinessObjectList.MapCollection(newList);
		}

		public void TableText(ITableObjectCollection tableCollection, ILanguageCollection languages)
		{
			LocalizedText.Clear();
			foreach (ITableObject table in tableCollection)
			{
				foreach (ILanguage language in languages)
				{
					int refId = tabletableMapping[table].Id;
					LocalizedText tableText = CloneBusinessObjects.LocalizedTextClone(typeof(TableObjectText), language, refId, table.Descriptions);
					if (!string.IsNullOrEmpty(tableText.Text))
					{
						LocalizedText.Add(tableText);
					}
				}
			}
			MapBusinessObjectList.MapCollection(LocalizedText);
		}

		public void PropertyText(IPropertiesCollection propertyCollection, ILanguageCollection languages)
		{
			LocalizedText.Clear();
			foreach (IProperty prop in propertyCollection)
			{
				foreach (ILanguage language in languages)
				{
					int refId = propertipropertyMapping[prop].Id;
					LocalizedText tableText = CloneBusinessObjects.LocalizedTextClone(typeof(PropertyText), language, refId, prop.Descriptions);
					if (!string.IsNullOrEmpty(tableText.Text))
					{
						LocalizedText.Add(tableText);
					}
				}
			}
			MapBusinessObjectList.MapCollection(LocalizedText);
		}

		public void ColumnText(IFullColumnCollection columnCollection, ILanguageCollection languages)
		{
			LocalizedText.Clear();
			foreach (IColumn column in columnCollection)
			{
				foreach (ILanguage language in languages)
				{
					int refid = columncolumnMapping[column].Id;
					LocalizedText columnText = CloneBusinessObjects.LocalizedTextClone(typeof(ColumnText), language, refid, column.Descriptions);
					if (!string.IsNullOrEmpty(columnText.Text))
					{
						LocalizedText.Add(columnText);
					}
				}
			}
			MapBusinessObjectList.MapCollection(LocalizedText);
		}

		public void CategoryTexts(ICategoryCollection categories, ILanguageCollection languages)
		{
			LocalizedText.Clear();
			foreach (ICategory category in categories)
			{
				foreach (ILanguage language in languages)
				{
					int refid = categorycategoryMapping[category].Id;
					LocalizedText categoryText = CloneBusinessObjects.LocalizedTextClone(typeof(CategoryText), language, refid, category.Names);
					if (!string.IsNullOrEmpty(categoryText.Text))
					{
						LocalizedText.Add(categoryText);
					}
				}
			}
			MapBusinessObjectList.MapCollection(LocalizedText);
		}

		public void OptimizationText(IOptimizationCollection optimizationCollection, ILanguageCollection languages)
		{
			LocalizedText.Clear();
			foreach (IOptimization opt in optimizationCollection)
			{
				if (opt.Parent == null)
				{
					continue;
				}
				foreach (ILanguage language in languages)
				{
					int refid = optoptMapping[opt].Id;
					LocalizedText optimizationText = CloneBusinessObjects.LocalizedTextClone(typeof(OptimizationText), language, refid, opt.Descriptions);
					if (!string.IsNullOrEmpty(optimizationText.Text))
					{
						LocalizedText.Add(optimizationText);
					}
				}
			}
			MapBusinessObjectList.MapCollection(LocalizedText);
		}

		public void ParameterTexts(IParameterCollection parameterCollection, ILanguageCollection languages)
		{
			LocalizedText.Clear();
			foreach (IParameter parameter in parameterCollection)
			{
				foreach (ILanguage language in languages)
				{
					int refid = parameterparameterMapping[parameter].Id;
					LocalizedText parametercollectionText = CloneBusinessObjects.LocalizedTextClone(typeof(ParameterText), language, refid, parameter.Descriptions);
					if (!string.IsNullOrEmpty(parametercollectionText.Text))
					{
						LocalizedText.Add(parametercollectionText);
					}
				}
			}
			MapBusinessObjectList.MapCollection(LocalizedText);
		}

		public void ParameterValueText(List<ParameterValue> parameterValuesCollection, ILanguageCollection languages)
		{
			LocalizedText.Clear();
			foreach (ParameterValue parametervalue in parameterValuesCollection)
			{
				foreach (ILanguage language in languages)
				{
					try
					{
						int refid = parametervalueparametervalueMapping[parametervalue].Id;
						LocalizedText parametervalueText = CloneBusinessObjects.LocalizedTextClone(typeof(ParameterValueSetText), language, refid, parametervalue.Descriptions);
						if (!string.IsNullOrEmpty(parametervalueText.Text))
						{
							LocalizedText.Add(parametervalueText);
						}
					}
					catch (Exception ex)
					{
						log.Error(ex.Message, ex);
						throw;
					}
				}
			}
			MapBusinessObjectList.MapCollection(LocalizedText);
		}

		private void MapBusinessObjectListPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			string propertyName = propertyChangedEventArgs.PropertyName;
			if (propertyName == "ProcessedElements")
			{
				IMapBusinessObjectList s = sender as IMapBusinessObjectList;
				if (s != null)
				{
					StatusText = "Processed elements: " + s.ProcessedElements;
				}
			}
		}

		private int GetMappedTableId(int id)
		{
			foreach (KeyValuePair<ITableObject, ITableObject> kvp in tabletableMapping)
			{
				if (kvp.Key.Id == id)
				{
					return kvp.Value.Id;
				}
			}
			return -1;
		}

		private void PropertyUniqueKeyCheck(Property prop, IEnumerable<IProperty> alreadyExistingProperties)
		{
			foreach (IProperty oldprop in alreadyExistingProperties)
			{
				if (oldprop.Key == prop.Key)
				{
					prop.Id = oldprop.Id;
				}
			}
		}

		private int GetStartingOrdinal(IEnumerable<ITableObject> tableobjects)
		{
			int maxordinal = 0;
			foreach (ITableObject tableObject in tableobjects)
			{
				if (tableObject.Ordinal > maxordinal)
				{
					maxordinal = tableObject.Ordinal;
				}
			}
			return ++maxordinal;
		}

		private int GetMappedUserId(int id)
		{
			foreach (KeyValuePair<IUser, IUser> kvp in useruserMapping)
			{
				if (kvp.Key.Id == id)
				{
					return kvp.Value.Id;
				}
			}
			return -1;
		}

		private void CheckUniqueKeyForColumnRole(ColumnRoleMapping columnrolemapping, IRoleColumnRights roleColumnRights)
		{
			foreach (Tuple<IRole, IColumn, RightType> roleColumnRight in roleColumnRights)
			{
				int columnid = roleColumnRight.Item2.Id;
				int roleid = roleColumnRight.Item1.Id;
				if (columnrolemapping.ColumnId == columnid && columnrolemapping.RoleId == roleid)
				{
					columnrolemapping.ColumnId = columnid;
					columnrolemapping.RoleId = roleid;
				}
			}
		}

		private void CheckUniqueKeyForColumnUser(ColumnUserMapping columnUserMapping, IUserColumnRights userColumnRights)
		{
			foreach (Tuple<IUser, IColumn, RightType> userColumnRight in userColumnRights)
			{
				int columnid = userColumnRight.Item2.Id;
				int roleid = userColumnRight.Item1.Id;
				if (columnUserMapping.ColumnId == columnid && columnUserMapping.UserId == roleid)
				{
					columnUserMapping.ColumnId = columnid;
					columnUserMapping.UserId = roleid;
				}
			}
		}

		private void CheckUniqueKeyForCategoryUser(CategoryUserMapping categoryUserMapping, IUserCategoryRights userCategoryRights)
		{
			foreach (Tuple<IUser, ICategory, RightType> userCategoryRight in userCategoryRights)
			{
				int categoryid = userCategoryRight.Item2.Id;
				int userid = userCategoryRight.Item1.Id;
				if (categoryUserMapping.CategoryId == categoryid && categoryUserMapping.UserId == userid)
				{
					categoryUserMapping.CategoryId = categoryid;
					categoryUserMapping.UserId = userid;
				}
			}
		}

		private void CheckUniqueKeyForCategoryRole(CategoryRoleMapping categoryRoleMapping, IRoleCategoryRights roleCategoryRights)
		{
			foreach (Tuple<IRole, ICategory, RightType> roleCategoryRight in roleCategoryRights)
			{
				int categoryid = roleCategoryRight.Item2.Id;
				int userid = roleCategoryRight.Item1.Id;
				if (categoryRoleMapping.CategoryId == categoryid && categoryRoleMapping.RoleId == userid)
				{
					categoryRoleMapping.CategoryId = categoryid;
					categoryRoleMapping.RoleId = userid;
				}
			}
		}

		private void CheckUniqueKeyForTableRole(TableRoleMapping tableRoleMapping, IRoleTableObjectRights roleTableObjectRights)
		{
			foreach (Tuple<IRole, ITableObject, RightType> roleTableObjectRight in roleTableObjectRights)
			{
				int categoryid = roleTableObjectRight.Item2.Id;
				int userid = roleTableObjectRight.Item1.Id;
				if (tableRoleMapping.TableId == categoryid && tableRoleMapping.RoleId == userid)
				{
					tableRoleMapping.TableId = categoryid;
					tableRoleMapping.RoleId = userid;
				}
			}
		}

		private void CheckUniqueKeyForTableUser(TableUserMapping tableUserMapping, IUserTableObjectRights tableObjectRights)
		{
			foreach (Tuple<IUser, ITableObject, RightType> tableObjectRight in tableObjectRights)
			{
				int categoryid = tableObjectRight.Item2.Id;
				int userid = tableObjectRight.Item1.Id;
				if (tableUserMapping.TableId == categoryid && tableUserMapping.UserId == userid)
				{
					tableUserMapping.TableId = categoryid;
					tableUserMapping.UserId = userid;
				}
			}
		}

		private void CheckUniqueKeyForOptimizationUser(OptimizationUserMapping optimizationUserMapping, IUserOptimizationRights userOptimizationRights)
		{
			foreach (Tuple<IUser, IOptimization, RightType> userOptimizationRight in userOptimizationRights)
			{
				int categoryid = userOptimizationRight.Item2.Id;
				int userid = userOptimizationRight.Item1.Id;
				if (optimizationUserMapping.OptimizationId == categoryid && optimizationUserMapping.UserId == userid)
				{
					optimizationUserMapping.OptimizationId = categoryid;
					optimizationUserMapping.UserId = userid;
				}
			}
		}

		private void CheckUniqueKeyForOptimizationRole(OptimizationRoleMapping optimizationRoleMapping, IRoleOptimizationRights optimizationRights)
		{
			foreach (Tuple<IRole, IOptimization, RightType> optimizationRight in optimizationRights)
			{
				int categoryid = optimizationRight.Item2.Id;
				int userid = optimizationRight.Item1.Id;
				if (optimizationRoleMapping.OptimizationId == categoryid && optimizationRoleMapping.RoleId == userid)
				{
					optimizationRoleMapping.OptimizationId = categoryid;
					optimizationRoleMapping.RoleId = userid;
				}
			}
		}

		private void UniqueKeyForLanguage(Language language, ILanguageCollection alreadyExistingLanguages)
		{
			foreach (ILanguage language2 in alreadyExistingLanguages)
			{
				if (language.CountryCode == language2.CountryCode)
				{
					language.Id = (language2 as Language).Id;
				}
			}
		}

		private void CheckUniqueKeyForUserRole(UserRoleMapping userRoleMapping, IUserRoleCollection userRoleCollection)
		{
			foreach (KeyValuePair<Tuple<IUser, IRole>, IUserRoleMapping> roleColumnRight in userRoleCollection)
			{
				int userid = roleColumnRight.Key.Item1.Id;
				int roleid = roleColumnRight.Key.Item2.Id;
				if (userRoleMapping.RoleId == roleid && userRoleMapping.UserId == userid)
				{
					userRoleMapping.UserId = userid;
					userRoleMapping.RoleId = roleid;
				}
			}
		}

		public IOptimization CollectHighestLevel(IOptimizationCollection optimizations)
		{
			IOptimization result = null;
			foreach (IOptimization optimization in optimizations)
			{
				if (optimization.Parent == null)
				{
					result = optimization;
				}
			}
			return result;
		}

		private int GetMappedOptimizationId(int id)
		{
			foreach (KeyValuePair<IOptimization, IOptimization> kvp in optoptMapping)
			{
				if (kvp.Key.Id == id)
				{
					return kvp.Value.Id;
				}
			}
			return -1;
		}

		public void CheckUniqueKeyForOptimization(Optimization newopt, IOptimizationCollection alreadyExistingOptimizations)
		{
			foreach (IOptimization opt in alreadyExistingOptimizations)
			{
				if (opt.Parent == null)
				{
					break;
				}
				if (newopt.ParentId == opt.Parent.Id && newopt.Value == opt.Value)
				{
					newopt.Id = opt.Id;
					newopt.OptimizationGroupId = opt.Group.Id;
				}
			}
		}

		private int GetRelationOffset(List<Relation> relations)
		{
			int maxrelationid = 0;
			foreach (Relation relation in relations)
			{
				if (relation.RelationId > maxrelationid)
				{
					maxrelationid = relation.RelationId;
				}
			}
			return ++maxrelationid;
		}

		private int GetParamValueOffset(List<ParameterValue> parameterValues)
		{
			int maxcollectionId = 0;
			foreach (ParameterValue relation in parameterValues)
			{
				if (relation.CollectionId > maxcollectionId)
				{
					maxcollectionId = relation.CollectionId;
				}
			}
			return ++maxcollectionId;
		}

		private int GetNewParameterId(int paramid)
		{
			int mappedid = 0;
			foreach (KeyValuePair<IParameter, IParameter> parameter in parameterparameterMapping)
			{
				if (parameter.Key.Id == paramid)
				{
					mappedid = parameter.Value.Id;
				}
			}
			return mappedid;
		}

		private int GetMappedParameterId(int id)
		{
			foreach (KeyValuePair<IParameter, IParameter> kvp in parameterparameterMapping)
			{
				if (kvp.Key.Id == id)
				{
					return kvp.Value.Id;
				}
			}
			return -1;
		}

		private int GetMappedSchemeId(int id)
		{
			foreach (KeyValuePair<IScheme, IScheme> kvp in schemeschemeMapping)
			{
				if (kvp.Key.Id == id)
				{
					return kvp.Value.Id;
				}
			}
			return 0;
		}

		private void InfoUniqueKeyCheck(Info newitm, List<Info> alreadyExistingList)
		{
			foreach (Info olditm in alreadyExistingList)
			{
				if (olditm.Key == newitm.Key)
				{
					newitm.Id = olditm.Id;
				}
			}
		}
	}
}
