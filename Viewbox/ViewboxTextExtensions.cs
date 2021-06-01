using System;
using System.Collections.Generic;
using System.Linq;
using SystemDb;
using Viewbox.Properties;

namespace Viewbox
{
	public static class ViewboxTextExtensions
	{
		public static string GetDescription(this IDataObject obj)
		{
			string text = GetDescriptionOrDefault(obj);
			return string.IsNullOrWhiteSpace(text) ? obj.Name : text;
		}

		public static string GetDescription(this IDataObject obj, ILanguage language)
		{
			string text = GetDescriptionOrDefault(obj, language);
			return string.IsNullOrWhiteSpace(text) ? obj.Name : text;
		}

		public static string GetDescription(this IParameterValue value)
		{
			string text = GetDescriptionOrDefault(value);
			return string.IsNullOrWhiteSpace(text) ? value.Value : text;
		}

		public static string GetDescription(this ITableObject obj, ITableObject originalTo = null)
		{
			IIssue issue = obj as IIssue;
			if (issue != null && issue.FilterTableObject != null)
			{
				string issueText = issue.Descriptions[ViewboxSession.Language ?? ViewboxApplication.DefaultLanguage];
				return string.IsNullOrWhiteSpace(issueText) ? issue.FilterTableObject.GetDescription() : issueText;
			}
			string text = string.Empty;
			if (obj.Id < 0 && originalTo != null)
			{
				text = originalTo.Descriptions[ViewboxSession.Language ?? ViewboxApplication.DefaultLanguage];
				return string.IsNullOrWhiteSpace(text) ? originalTo.TableName : text;
			}
			text = obj.Descriptions[ViewboxSession.Language ?? ViewboxApplication.DefaultLanguage];
			return string.IsNullOrWhiteSpace(text) ? obj.TableName : text;
		}

		public static string GetObjectType(this ITableObject obj)
		{
			bool isNull = obj != null;
			string objType = string.Empty;
			if (isNull)
			{
				objType = (obj.ObjectTypes[ViewboxSession.Language ?? ViewboxApplication.DefaultLanguage] ?? obj.ObjectTypes[ViewboxApplication.DefaultLanguage]) ?? string.Empty;
			}
			return objType;
		}

		public static string GetName(this ITableObject obj)
		{
			ITable tbl = obj as ITable;
			IIssue issue = obj as IIssue;
			if (issue != null && issue.FilterTableObject != null)
			{
				return issue.FilterTableObject.GetName();
			}
			return (tbl != null) ? (tbl.OriginalName ?? tbl.TableName) : obj.TableName;
		}

		public static long GetRowCount(this ITableObject obj)
		{
			return ViewboxSession.GetDataCount(obj);
		}

		public static long GetRowCount(this ITableObject obj, IOptimization opt)
		{
			return ViewboxApplication.Database.SystemDb.GetDataCount(obj, opt);
		}

		public static string GetOptimizationValue(this IOptimization opt, OptimizationType type)
		{
			if (opt == null || opt.Id == 0)
			{
				return null;
			}
			if (opt.Group.Type == type)
			{
				return opt.Value;
			}
			return opt.Parent.GetOptimizationValue(type);
		}

		public static IOptimization GetOptimization(this IOptimization opt, OptimizationType type)
		{
			if (opt == null)
			{
				return null;
			}
			if (opt.Group.Type == type)
			{
				return opt;
			}
			return opt.Parent.GetOptimization(type);
		}

		public static string GetDescription(this ITableObject obj, ILanguage language)
		{
			string text = obj.Descriptions[language];
			return string.IsNullOrWhiteSpace(text) ? obj.GetName() : text;
		}

		public static string GetDescription(this IOptimization obj)
		{
			string text = GetDescriptionOrDefault(obj);
			return string.IsNullOrWhiteSpace(text) ? obj.GetValue() : text;
		}

		public static string GetDescription(this IOptimization obj, ILanguage language)
		{
			string text = GetDescriptionOrDefault(obj, language);
			return string.IsNullOrWhiteSpace(text) ? obj.GetValue() : text;
		}

		public static string GetName(this IOptimizationGroup obj)
		{
			return GetNameOrDefault(obj);
		}

		public static string GetName(this IOptimizationGroup obj, ILanguage language)
		{
			return GetNameOrDefault(obj, language);
		}

		public static string GetName(this IProperty obj)
		{
			return GetNameOrDefault(obj);
		}

		public static string GetDescription(this IProperty obj)
		{
			return GetDescriptionOrDefault(obj);
		}

		public static string GetDescription(this IScheme obj)
		{
			return obj.Descriptions[ViewboxSession.Language ?? ViewboxApplication.DefaultLanguage] ?? Resources.Default;
		}

		public static string GetValue(this IOptimization obj)
		{
			ILanguage language = ViewboxSession.Language ?? ViewboxApplication.DefaultLanguage;
			if (obj.Group.Type == OptimizationType.System || obj.Group.Type == OptimizationType.None)
			{
				string text = obj.Descriptions[language];
				if (!string.IsNullOrWhiteSpace(text))
				{
					return text;
				}
			}
			return string.IsNullOrWhiteSpace(obj.Value) ? Resources.ResourceManager.GetString("All", language.CultureInfo) : obj.Value;
		}

		public static string GetValue(this IOptimization obj, ILanguage language)
		{
			if (obj.Group.Type == OptimizationType.System || obj.Group.Type == OptimizationType.None)
			{
				string text = obj.Descriptions[language];
				if (!string.IsNullOrWhiteSpace(text))
				{
					return text;
				}
			}
			return obj.Value ?? Resources.ResourceManager.GetString("All", language.CultureInfo);
		}

		public static string GetName(this ICategory obj)
		{
			return obj.Names[ViewboxSession.Language ?? ViewboxApplication.DefaultLanguage];
		}

		public static string GetName(this IUser user)
		{
			return string.IsNullOrEmpty(user.Name) ? user.UserName : user.Name;
		}

		public static string GetNodeText(this RightObjectNode node)
		{
			return node.Type switch
			{
				UpdateRightType.Optimization => $"{Resources.Optimization} {ViewboxSession.Optimizations.First((IOptimization o) => o.Id == node.Id).GetDescription()}", 
				UpdateRightType.Category => $"{Resources.Category} {ViewboxSession.Categories[node.Id].GetName()}", 
				UpdateRightType.TableObject => $"{Resources.Table} {ViewboxSession.TableObjects[node.Id].GetDescription()}", 
				UpdateRightType.Column => $"{Resources.Column} {ViewboxSession.Columns[node.Id].GetDescription()}", 
				_ => throw new ArgumentException("type"), 
			};
		}

		public static List<Tuple<string, string>> GetDescriptions(this IOptimization opt, ILanguage language = null, ITableObject tobj = null)
		{
			List<Tuple<string, string>> output = new List<Tuple<string, string>>();
			IOptimization new_opt = opt;
			IIssue issue = ((tobj != null && tobj.Type == TableType.Issue) ? ((IIssue)tobj) : null);
			while (new_opt != null && new_opt.Id != 0)
			{
				if (issue != null && issue.OptimizationHidden == 1 && (new_opt.Group.Type == OptimizationType.SortColumn || new_opt.Group.Type == OptimizationType.SplitTable))
				{
					new_opt = new_opt.Parent;
					continue;
				}
				if (language == null)
				{
					output.Add(new Tuple<string, string>(new_opt.Group.GetName(), new_opt.GetDescription()));
				}
				else
				{
					output.Add(new Tuple<string, string>(new_opt.Group.GetName(language), new_opt.GetDescription(language)));
				}
				new_opt = new_opt.Parent;
			}
			return output;
		}

		private static string GetNameOrDefault(IProperty obj)
		{
			return obj.Names[ViewboxSession.Language ?? ViewboxApplication.DefaultLanguage] ?? obj.Names[ViewboxApplication.DefaultLanguage];
		}

		private static string GetDescriptionOrDefault(IProperty obj)
		{
			return obj.Descriptions[ViewboxSession.Language ?? ViewboxApplication.DefaultLanguage] ?? obj.Descriptions[ViewboxApplication.DefaultLanguage];
		}

		public static string GetNameOrDefault(IOptimizationGroup obj)
		{
			return obj.Names[ViewboxSession.Language ?? ViewboxApplication.DefaultLanguage] ?? obj.Names[ViewboxApplication.DefaultLanguage];
		}

		public static string GetNameOrDefault(IOptimizationGroup obj, ILanguage language)
		{
			return obj.Names[language] ?? obj.Names[ViewboxApplication.DefaultLanguage];
		}

		public static string GetDescriptionOrDefault(IOptimization obj)
		{
			return obj.Descriptions[ViewboxSession.Language ?? ViewboxApplication.DefaultLanguage] ?? obj.Descriptions[ViewboxApplication.DefaultLanguage];
		}

		public static string GetDescriptionOrDefault(IOptimization obj, ILanguage language)
		{
			return obj.Descriptions[language] ?? obj.Descriptions[ViewboxApplication.DefaultLanguage];
		}

		public static string GetDescriptionOrDefault(IDataObject obj)
		{
			return obj.Descriptions[ViewboxSession.Language ?? ViewboxApplication.DefaultLanguage] ?? obj.Descriptions[ViewboxApplication.DefaultLanguage];
		}

		public static string GetDescriptionOrDefault(IDataObject obj, ILanguage language)
		{
			return obj.Descriptions[language] ?? obj.Descriptions[ViewboxApplication.DefaultLanguage];
		}

		public static string GetDescriptionOrDefault(IParameterValue value)
		{
			return value.Descriptions[ViewboxSession.Language ?? ViewboxApplication.DefaultLanguage] ?? value.Descriptions[ViewboxApplication.DefaultLanguage];
		}
	}
}
