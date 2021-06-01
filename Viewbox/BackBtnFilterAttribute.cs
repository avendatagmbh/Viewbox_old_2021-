using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Viewbox.Models;

namespace Viewbox
{
	public class BackBtnFilterAttribute : ActionFilterAttribute
	{
		private static bool itWasBack = false;

		private const string badoptidC = "badoptid";

		private int prevSize = 35;

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			try
			{
				HttpRequestBase request = filterContext.HttpContext.Request;
				Uri urlRefferer = request.UrlReferrer;
				object tmpSize = "";
				if (filterContext.ActionParameters.TryGetValue("size", out tmpSize) && int.TryParse(tmpSize.ToString(), out prevSize))
				{
				}
				string backOptID = request.Params["backoptid"];
				int backOptIDInteger = 0;
				if (backOptID != null && int.TryParse(backOptID, out backOptIDInteger))
				{
					OptimizationManager.SetOptimization(backOptIDInteger);
				}
				if (new string[3] { "IssueList", "ViewList", "TableList" }.Any((string o) => urlRefferer.PathAndQuery.Contains(o)))
				{
					ViewboxSession.ClearPreviousPageList();
					itWasBack = false;
				}
				if (request.Params["back"]?.ToLower().Contains("true") ?? false)
				{
					ViewboxSession.CurrentPage = request.Url.PathAndQuery;
					string lastPopedPage = ViewboxSession.PrevPage;
					if (lastPopedPage == request.Url.PathAndQuery.Replace("back=true&", "").Replace("&back=true", "").Replace("?back=true", "") || lastPopedPage.Contains("badoptid"))
					{
						lastPopedPage = ViewboxSession.PrevPage;
					}
					filterContext.Result = new RedirectResult(lastPopedPage);
					itWasBack = true;
				}
				else if (urlRefferer != null && !itWasBack)
				{
					ViewboxSession.PrevPage = urlRefferer.PathAndQuery + urlRefferer.Fragment.Replace("#", "");
				}
			}
			catch
			{
			}
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			HttpRequestBase request = filterContext.HttpContext.Request;
			Uri urlRefferer = request.UrlReferrer;
			if (itWasBack)
			{
				if (urlRefferer != null && !new string[3] { "IssueList", "ViewList", "TableList" }.Any((string o) => (urlRefferer.PathAndQuery + urlRefferer.Fragment.Replace("#", "")).Contains(o)) && ViewboxSession.GetBackStackItem(ViewboxSession.PrevPageList.Count - 1).Link != urlRefferer.PathAndQuery + urlRefferer.Fragment.Replace("#", ""))
				{
					ViewboxSession.PrevPageList.Add(new BackStack(urlRefferer.PathAndQuery + urlRefferer.Fragment.Replace("#", ""), ViewboxSession.LastOpt));
				}
				itWasBack = false;
			}
			else
			{
				DataGridModel model = null;
				if (filterContext != null && filterContext.Result != null)
				{
					ViewResultBase viewResultBase = filterContext.Result as ViewResultBase;
					if (viewResultBase != null && viewResultBase.Model != null)
					{
						model = viewResultBase.Model as DataGridModel;
					}
				}
				BackStack lastInserted = ((ViewboxSession.PrevPageList != null && ViewboxSession.GetBackStackItem(ViewboxSession.BsI) != null) ? ViewboxSession.GetBackStackItem(ViewboxSession.BsI) : null);
				BackStack lastBeforeLast = ((ViewboxSession.BsI - 1 >= 0) ? ViewboxSession.GetBackStackItem(ViewboxSession.BsI - 1) : null);
				if ((lastBeforeLast != null && !string.IsNullOrEmpty(lastInserted.Link) && lastInserted.Equals(lastBeforeLast)) || (model != null && model.IsThereEmptyMessage) || (ViewboxSession.IsThereEmptyMessage.HasValue && ViewboxSession.IsThereEmptyMessage == true))
				{
					ViewboxSession.RemoveBackStackItem(ViewboxSession.BsI);
				}
				if (urlRefferer != null && lastBeforeLast != null && (urlRefferer.PathAndQuery + urlRefferer.Fragment.Replace("#", "")).Contains("?start") && ViewboxSession.BsI > 0 && lastBeforeLast.Link != urlRefferer.LocalPath && (!lastBeforeLast.Link.Contains("?start") || (lastBeforeLast.Link.Contains("?start") && !lastBeforeLast.Link.Contains(urlRefferer.LocalPath))))
				{
					BackStack newItem2 = ViewboxSession.GetBackStackItem(ViewboxSession.BsI).Clone();
					newItem2.Link = urlRefferer.LocalPath;
					ViewboxSession.InsertBackStackItem(ViewboxSession.BsI, newItem2);
				}
				if (urlRefferer != null && lastBeforeLast != null && urlRefferer.PathAndQuery.Contains("?size") && ViewboxSession.BsI > 0 && lastBeforeLast.Link != urlRefferer.LocalPath && (!lastBeforeLast.Link.Contains("?size") || (lastBeforeLast.Link.Contains("?size") && !lastBeforeLast.Link.Contains(urlRefferer.LocalPath))))
				{
					BackStack newItem = ViewboxSession.GetBackStackItem(ViewboxSession.BsI).Clone();
					newItem.Link = urlRefferer.LocalPath;
					ViewboxSession.InsertBackStackItem(ViewboxSession.BsI, newItem);
				}
			}
			if (ViewboxSession.PrevPageList == null)
			{
				return;
			}
			foreach (BackStack item in ViewboxSession.PrevPageList)
			{
			}
		}
	}
}
