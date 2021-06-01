using System;
using SystemDb.Helper;
using SystemDb.Internal;

namespace SystemDb
{
	internal class CloneBusinessObjects : ICloneBusinessObjects
	{
		public TableObject TableObjectClone(ITableObject tableobject, ICategory category, int startingordinaloffset, int defaultschemeid)
		{
			TableObject result = null;
			result = tableobject.Type switch
			{
				TableType.Table => new Table(), 
				TableType.Issue => new Issue(), 
				TableType.Archive => new Archive(), 
				TableType.View => new View(), 
				TableType.FakeProcedure => new FakeProcedure(), 
				_ => throw new NotImplementedException(), 
			};
			result.CategoryId = category.Id;
			result.Database = tableobject.Database;
			result.TableName = tableobject.TableName;
			result.Type = tableobject.Type;
			result.RowCount = tableobject.RowCount;
			result.IsVisible = tableobject.IsVisible;
			result.DefaultSchemeId = defaultschemeid;
			result.TransactionNumber = tableobject.TransactionNumber;
			result.UserDefined = tableobject.UserDefined;
			result.Ordinal = startingordinaloffset + tableobject.Ordinal;
			result.IsArchived = tableobject.IsArchived;
			return result;
		}

		public Property Property(IProperty obj)
		{
			return new Property
			{
				Key = obj.Key,
				Type = obj.Type,
				Value = obj.Value
			};
		}

		public UserPropertySettings UserPropertySettings(IUserPropertySettings obj, IProperty newProperty, IUser newUser)
		{
			return new UserPropertySettings
			{
				Value = obj.Value,
				PropertyId = newProperty.Id,
				UserId = newUser.Id
			};
		}

		public OrderArea OrderAreaClone(IOrderArea orderarea, ITableObject tableobject)
		{
			return new OrderArea
			{
				TableId = tableobject.Id,
				IndexValue = orderarea.IndexValue,
				SplitValue = orderarea.SplitValue,
				SortValue = orderarea.SortValue,
				Start = orderarea.Start,
				End = orderarea.End
			};
		}

		public NewLogActionMerge NewLogActionMergeClone(INewLogActionMerge newLogAction, IUser userobject)
		{
			NewLogActionMerge result = new NewLogActionMerge();
			result.ActionController = newLogAction.ActionController;
			result.QueryString = newLogAction.QueryString;
			result.Timestamp = newLogAction.Timestamp;
			if (userobject != null)
			{
				result.UserId = userobject.Id;
			}
			return result;
		}

		public IssueExtension IssueExtensionClone(IIssue issue, int id)
		{
			IssueExtension result = new IssueExtension();
			result.RefId = id;
			result.Command = issue.Command;
			if (issue.FilterTableObject != null)
			{
				result.TableObjectId = issue.FilterTableObject.Id;
			}
			result.UseLanguageValue = issue.UseLanguageValue;
			result.UseIndexValue = issue.UseIndexValue;
			result.UseSplitValue = issue.UseSplitValue;
			result.UseSortValue = issue.UseSortValue;
			result.Flag = issue.Flag;
			result.Checked = true;
			return result;
		}

		public Column ColumnClone(IColumn column, ITableObject tableobject)
		{
			Column column2 = new Column();
			column2.IsVisible = column.IsVisible;
			column2.IsTempedHidden = column.IsTempedHidden;
			column2.IsTempedHidden = column.IsTempedHidden;
			column2.TableId = tableobject.Id;
			column2.DataType = column.DataType;
			column2.OriginalName = column.OriginalName;
			column2.OptimizationType = (column as Column).OptimizationType;
			column2.IsEmpty = column.IsEmpty;
			column2.MaxLength = column.MaxLength;
			column2.ConstValue = column.ConstValue;
			column2.Name = column.Name;
			column2.UserDefined = column.UserDefined;
			column2.Ordinal = column.Ordinal;
			column2.FromColumn = column2.FromColumn;
			column2.FromColumnFormat = column2.FromColumnFormat;
			column2.Flag = column2.Flag;
			column2.ParamOperator = column2.ParamOperator;
			return column2;
		}

		public Role RoleClone(IRole role)
		{
			return new Role
			{
				Name = role.Name,
				Flags = role.Flags
			};
		}

		public User UserClone(IUser user)
		{
			return new User
			{
				UserName = user.UserName,
				Name = user.Name,
				Email = user.Email,
				PasswordHash = (user as User).PasswordHash,
				Flags = user.Flags,
				IsADUser = user.IsADUser,
				DisplayRowCount = user.DisplayRowCount,
				Domain = user.Domain
			};
		}

		public Parameter ParameterClone(IParameter parameter, ITableObject issue)
		{
			return new Parameter
			{
				IssueId = issue.Id,
				DataType = parameter.DataType,
				TypeModifier = parameter.TypeModifier,
				Default = parameter.Default,
				Name = parameter.Name,
				UserDefined = parameter.UserDefined,
				Ordinal = parameter.Ordinal,
				ColumnName = parameter.ColumnName,
				DatabaseName = parameter.DatabaseName,
				TableName = parameter.TableName
			};
		}

		public Relation RelationClone(int relationid, int childid, int parentid, int relationIDOffset)
		{
			return new Relation
			{
				RelationId = relationid + relationIDOffset,
				ChildId = childid,
				ParentId = parentid
			};
		}

		public Category CategoryClone(ICategory category)
		{
			return new Category();
		}

		public TableUserMapping TableUserMappingClone(RightType right, ITableObject tableobject, IUser user)
		{
			return new TableUserMapping
			{
				Right = right,
				TableId = tableobject.Id,
				UserId = user.Id
			};
		}

		public TableRoleMapping TableRoleMappingClone(RightType right, ITableObject tableobject, IRole role)
		{
			return new TableRoleMapping
			{
				Right = right,
				TableId = tableobject.Id,
				RoleId = role.Id
			};
		}

		public ColumnUserMapping ColumnUserMapping(RightType right, IColumn column, IUser user)
		{
			return new ColumnUserMapping
			{
				Right = right,
				ColumnId = column.Id,
				UserId = user.Id
			};
		}

		public ColumnRoleMapping ColumnRoleMapping(RightType right, IColumn column, IRole role)
		{
			return new ColumnRoleMapping
			{
				Right = right,
				ColumnId = column.Id,
				RoleId = role.Id
			};
		}

		public CategoryUserMapping CategoryUser(ICategory category, IUser user, RightType right)
		{
			return new CategoryUserMapping
			{
				CategoryId = category.Id,
				UserId = user.Id,
				Right = right
			};
		}

		public ParameterCollectionMapping ParameterCollectionMapping(int parameter, int manuallygeneratedId)
		{
			return new ParameterCollectionMapping
			{
				ParameterId = parameter,
				CollectionId = manuallygeneratedId
			};
		}

		public ParameterValue ParameterValue(IParameterValue paramvalue, int collectionId, int offset)
		{
			return new ParameterValue
			{
				CollectionId = collectionId + offset,
				Value = paramvalue.Value
			};
		}

		public CategoryRoleMapping CategoryRoleMappingClone(ICategory category, IRole role, RightType right)
		{
			return new CategoryRoleMapping
			{
				CategoryId = category.Id,
				RoleId = role.Id,
				Right = right
			};
		}

		public OptimizationRoleMapping OptimizationRoleClone(IOptimization optimization, IRole role, bool visibility)
		{
			return new OptimizationRoleMapping
			{
				OptimizationId = optimization.Id,
				RoleId = role.Id,
				Visible = visibility
			};
		}

		public OptimizationGroup OptimizationGroupClone(IOptimizationGroup optgroup)
		{
			return new OptimizationGroup
			{
				Type = optgroup.Type
			};
		}

		public LocalizedText LocalizedTextClone(Type type, ILanguage language, int refId, ILocalizedTextCollection desc)
		{
			LocalizedText obj = (LocalizedText)Activator.CreateInstance(type);
			obj.RefId = refId;
			obj.Text = desc[language] ?? string.Empty;
			obj.CountryCode = language.CountryCode;
			return obj;
		}

		public Optimization OptimizationClone(int parentID, IOptimization optimization, IOptimizationGroup optgroup)
		{
			return new Optimization
			{
				ParentId = parentID,
				Value = optimization.Value,
				OptimizationGroupId = optgroup.Id
			};
		}

		public OptimizationText OptimizationTextClone(ILanguage language, IOptimization optimization, ILocalizedTextCollection desc)
		{
			return new OptimizationText
			{
				RefId = optimization.Id,
				Text = (desc[language] ?? string.Empty),
				CountryCode = language.CountryCode
			};
		}

		public OptimizationUserMapping OptimizationUserMappingClone(IOptimization optimization, IUser user, bool visibility)
		{
			return new OptimizationUserMapping
			{
				OptimizationId = optimization.Id,
				UserId = user.Id,
				Visible = visibility
			};
		}

		public UserRoleMapping UserRoleMappingClone(IUser user, IRole role, int ordinal)
		{
			return new UserRoleMapping
			{
				UserId = user.Id,
				RoleId = role.Id,
				Ordinal = ordinal
			};
		}

		public Scheme SchemeClone(int id, string partial)
		{
			return new Scheme
			{
				Partial = partial
			};
		}

		public Language LanguageClone(ILanguage lang)
		{
			return new Language
			{
				LanguageName = lang.LanguageName,
				CountryCode = lang.CountryCode,
				LanguageMotto = lang.LanguageMotto
			};
		}

		public TableArchiveInformation CloneTableArchiveInformation(TableArchiveInformation tableArchiveInformation, int tableid)
		{
			return new TableArchiveInformation
			{
				TableId = tableid,
				CreateStatement = tableArchiveInformation.CreateStatement
			};
		}

		public Info CloneInfo(Info newitm)
		{
			return new Info
			{
				Key = newitm.Key,
				Value = newitm.Value
			};
		}

		public Note CloneNote(Note itm, int userid)
		{
			return new Note
			{
				Text = itm.Text,
				Title = itm.Title,
				UserId = userid
			};
		}

		public About CloneAbout(About itm)
		{
			return new About
			{
				CompanyName = itm.CompanyName,
				Version = itm.Version
			};
		}

		public HistoryParameterValue CloneUserHistoryParameterValue(HistoryParameterValue itm, int userid, int paramid)
		{
			return new HistoryParameterValue
			{
				Name = itm.Name,
				Ordinal = itm.Ordinal,
				ParameterId = paramid,
				UserId = userid,
				Value = itm.Value
			};
		}

		public UserOptimizationSettings CloneUserOptimizationSettings(UserOptimizationSettings itm, int userid, int optid)
		{
			return new UserOptimizationSettings
			{
				UserId = userid,
				OptimizationId = optid
			};
		}
	}
}
