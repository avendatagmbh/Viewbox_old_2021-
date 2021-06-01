using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using Viewbox.Models;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Controllers
{
	public class ObjectTypeController : BaseController
	{
		private IEnumerable<IObjectTypeText> GetObjectType()
		{
			return ViewboxApplication.Database.SystemDb.ObjectTypeTextCollection.Where((IObjectTypeText ot) => ot.CountryCode == ViewboxSession.Language.CountryCode);
		}

		public ActionResult Index(SortDirection sortDirection = SortDirection.Ascending)
		{
			IEnumerable<ValueListElement> model = from otc in GetObjectType()
				select new ValueListElement
				{
					Id = otc.RefId,
					Value = otc.Text
				};
			model = ((sortDirection != SortDirection.Descending) ? model.OrderBy((ValueListElement v) => v.Value) : model.OrderByDescending((ValueListElement v) => v.Value));
			return PartialView("_ColumnValueListPartial", new ColumnValueListModels(model)
			{
				Direction = sortDirection,
				PopupTitle = Resources.EditField
			});
		}

		public ActionResult Save(string tableTypeKey, string oldObjectType, int newObjectTypeId, int tableId, string searchContext = null)
		{
			TableType tableType = (TableType)Enum.Parse(typeof(TableType), tableTypeKey);
			SystemDb.Internal.TableObject tableObject = ViewboxApplication.Database.SystemDb.Objects.Where((ITableObject o) => o.Type == tableType && o.Id == tableId).Single() as SystemDb.Internal.TableObject;
			IObjectTypeText newObjectType = GetObjectType().SingleOrDefault((IObjectTypeText ot) => ot.RefId == newObjectTypeId);
			if (newObjectType != null)
			{
				tableObject.ObjectTypeCode = newObjectType.RefId;
				ViewboxApplication.Database.SystemDb.RefreshTableObjectType(tableObject);
				using DatabaseBase connection = ViewboxApplication.Database.ConnectionManager.GetConnection();
				connection.DbMapping.Save(tableObject);
			}
			RouteValueDictionary routeValues = null;
			if (!string.IsNullOrEmpty(searchContext))
			{
				routeValues = new RouteValueDictionary(new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(searchContext));
			}
			return tableType switch
			{
				TableType.Issue => RedirectToAction("IndexList", "IssueList", routeValues), 
				TableType.View => RedirectToAction("Index", "ViewList", routeValues), 
				_ => null, 
			};
		}
	}
}
