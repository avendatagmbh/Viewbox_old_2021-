using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using SystemDb;
using ViewboxDb;
using ViewboxDb.Filters;

namespace Viewbox
{
	public static class Helper
	{
		public class PassThroughTwoViews
		{
			public string PartialOne { get; private set; }

			public object ModelOne { get; private set; }

			public string PartialTwo { get; private set; }

			public object ModelTwo { get; private set; }

			public PassThroughTwoViews(string partialOne, object modelOne, string partialTwo, object modelTwo)
			{
				PartialOne = partialOne;
				ModelOne = modelOne;
				PartialTwo = partialTwo;
				ModelTwo = modelTwo;
			}
		}

		public class PassThroughView
		{
			public string Partial { get; private set; }

			public object Model { get; private set; }

			public PassThroughView(string partial, object model)
			{
				Partial = partial;
				Model = model;
			}
		}

		public static string GetUrl(this WebViewPage pg, IEnumerable<string> remove, params UrlParam[] parameters)
		{
			RouteValueDictionary rvd = new RouteValueDictionary(pg.ViewContext.RouteData.Values);
			NameValueCollection query = pg.Request.QueryString;
			foreach (string key in query.Keys)
			{
				if (key != null)
				{
					rvd.Remove(key);
					rvd.Add(key, query[key]);
				}
			}
			foreach (UrlParam param in parameters)
			{
				rvd.Remove(param.Name);
				rvd.Add(param.Name, param.Value);
			}
			if (remove != null)
			{
				foreach (string r in remove)
				{
					rvd.Remove(r);
				}
			}
			rvd.Remove("json");
			rvd.Remove("isNoResult");
			return pg.Url.RouteUrl(rvd);
		}

		public static string GetCleanUrl(this WebViewPage pg)
		{
			return pg.Request.Url.GetLeftPart(UriPartial.Path).Replace(pg.Request.Url.GetLeftPart(UriPartial.Authority), "");
		}

		public static string GetUrl(this WebViewPage pg, params UrlParam[] parameters)
		{
			return pg.GetUrl(null, parameters);
		}

		public static string GetUrl(this WebViewPage pg, string name, object value, IEnumerable<string> remove = null)
		{
			return pg.GetUrl(remove, new UrlParam(name, value));
		}

		public static string OptimizationUrl(this WebViewPage pg, int optid, bool resetPage = true)
		{
			RouteValueDictionary rvd = new RouteValueDictionary(pg.ViewContext.RouteData.Values);
			NameValueCollection query = pg.Request.QueryString;
			foreach (string key in query.Keys)
			{
				if (key != null)
				{
					rvd.Remove(key);
					rvd.Add(key, query[key]);
				}
			}
			rvd.Remove("optid");
			rvd.Add("optid", optid);
			if (resetPage)
			{
				rvd.Remove("page");
			}
			rvd.Remove("json");
			rvd.Remove("isNoResult");
			return pg.Url.RouteUrl(rvd);
		}

		public static string RightsMode(this WebViewPage pg, int id = 0, CredentialType type = CredentialType.User)
		{
			return pg.GetUrl(new UrlParam("rights_mode", id > 0), new UrlParam("rm_id", id), new UrlParam("rm_type", type));
		}

		public static string LanguageUrl(this WebViewPage pg, ILanguage language)
		{
			return pg.GetUrl("lang", language.CountryCode);
		}

		public static Operator Operator(this IFilter filter)
		{
			if (filter == null)
			{
				filter = new AndFilter();
			}
			if (filter.GetType() == typeof(ColValueFilter))
			{
				switch ((filter as ColValueFilter).Op)
				{
				case Operators.Equal:
					return Viewbox.Operator.Equals;
				case Operators.Greater:
					return Viewbox.Operator.Greater;
				case Operators.GreaterOrEqual:
					return Viewbox.Operator.GreaterOrEqual;
				case Operators.Less:
					return Viewbox.Operator.Less;
				case Operators.LessOrEqual:
					return Viewbox.Operator.LessOrEqual;
				case Operators.NotEqual:
					return Viewbox.Operator.NotEquals;
				case Operators.Like:
					return Viewbox.Operator.Like;
				case Operators.StartsWith:
					return Viewbox.Operator.StartsWith;
				}
			}
			else
			{
				if (filter.GetType() == typeof(ColSetFilter))
				{
					return Viewbox.Operator.In;
				}
				if (filter.GetType() == typeof(BetweenFilter))
				{
					return Viewbox.Operator.Between;
				}
				if (filter.GetType() == typeof(AndFilter))
				{
					return Viewbox.Operator.And;
				}
				if (filter.GetType() == typeof(OrFilter))
				{
					return Viewbox.Operator.Or;
				}
			}
			throw new ArgumentException();
		}

		public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> values)
		{
			return !values.Except(source).Any() && !source.Except(values).Any();
		}

		public static Dictionary<List<int>, Dictionary<ITableObject, IRelation>> GetRelationPartialDictionary(this List<IRelation> relations)
		{
			Dictionary<List<int>, Dictionary<ITableObject, IRelation>> relDict = new Dictionary<List<int>, Dictionary<ITableObject, IRelation>>();
			foreach (IRelation rel2 in relations)
			{
				List<int> columnDescriptions = new List<int>();
				columnDescriptions.AddRange(from cc in rel2
					orderby cc.Source.GetDescription()
					select cc.Source.Id);
				List<int> existingColumnsDescriptions = relDict.Keys.FirstOrDefault((List<int> r) => r.ContainsAll(columnDescriptions));
				ITableObject tableObject;
				if (!(rel2.ElementAt(0).Target is IColumn))
				{
					ITableObject issue = (rel2.ElementAt(0).Target as IParameter).Issue;
					tableObject = issue;
				}
				else
				{
					tableObject = (rel2.ElementAt(0).Target as IColumn).Table;
				}
				ITableObject table = tableObject;
				if (existingColumnsDescriptions == null || existingColumnsDescriptions.Count == 0)
				{
					Dictionary<ITableObject, IRelation> dict = new Dictionary<ITableObject, IRelation>();
					dict.Add(table, rel2);
					relDict.Add(columnDescriptions, dict);
				}
				else
				{
					relDict[existingColumnsDescriptions][table] = rel2;
				}
			}
			return relDict.ToDictionary((KeyValuePair<List<int>, Dictionary<ITableObject, IRelation>> rel) => rel.Key, (KeyValuePair<List<int>, Dictionary<ITableObject, IRelation>> rel) => relDict[rel.Key].OrderBy(delegate(KeyValuePair<ITableObject, IRelation> r)
			{
				KeyValuePair<ITableObject, IRelation> keyValuePair = r;
				return keyValuePair.Key.GetDescription();
			}).ToDictionary((KeyValuePair<ITableObject, IRelation> key) => key.Key, (KeyValuePair<ITableObject, IRelation> key) => key.Value));
		}

		public static string GetDecimalFormat(int maxLength, string number)
		{
			string content = number;
			Regex numberRegex = new Regex("^\\d+\\.\\d+$", RegexOptions.Compiled);
			IProperty property = ViewboxApplication.FindProperty("thousand");
			string decimals = new string('0', maxLength);
			string format = "{0:0,0." + decimals + "}";
			if (property != null && property.Value == "false")
			{
				format = "{0:#0." + decimals + "}";
			}
			else if (content.Length < 3)
			{
				format = "{0}";
			}
			double realNumber = 0.0;
			string numberStr = content.ToString();
			if (Thread.CurrentThread.CurrentCulture.Name == "de-DE" && numberRegex.IsMatch(content))
			{
				numberStr = numberStr.Replace('.', ',');
			}
			if (double.TryParse(numberStr, out realNumber))
			{
				if (realNumber < 1000.0 && realNumber > -1000.0)
				{
					format = "{0:0." + decimals + "}";
				}
				return string.Format(format, Math.Round(Math.Pow(10.0, decimals.Length) * realNumber) / Math.Pow(10.0, decimals.Length));
			}
			if (realNumber < 1000.0 && realNumber > -1000.0)
			{
				format = "{0:0." + decimals + "}";
			}
			return string.Format(format, content);
		}
	}
}
