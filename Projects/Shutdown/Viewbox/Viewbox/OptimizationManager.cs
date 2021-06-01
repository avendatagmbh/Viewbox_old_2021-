using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SystemDb;
using ViewboxDb;

namespace Viewbox
{
	public static class OptimizationManager
	{
		public static IOptimization LastOptimization
		{
			get
			{
				return HttpContextFactory.Current.Session["LastOptimization"] as IOptimization;
			}
			private set
			{
				HttpContextFactory.Current.Session["LastOptimization"] = value;
			}
		}

		private static int AreWeInLoop { get; set; }

		public static void Check(ActionExecutingContext context)
		{
			string s_opt = context.HttpContext.Request.Params["optid"];
			if (!string.IsNullOrEmpty(s_opt))
			{
				ViewboxSession.HasOptChanged = true;
			}
			if (ViewboxSession.User == null || (!(context.ActionDescriptor.ActionName == "Index") && (!(context.ActionDescriptor.ControllerDescriptor.ControllerName == "IssueList") || !(context.ActionDescriptor.ActionName == "ShowOne"))))
			{
				return;
			}
			if (int.TryParse(s_opt, out var optid))
			{
				string oldOptValue = "";
				string newOptValue = "";
				IOptimization nextOpt = ViewboxSession.AllowedOptimizations[optid];
				IOptimization actOpt = ViewboxSession.Optimizations.LastOrDefault();
				IOptimization opt = nextOpt;
				if (opt != null)
				{
					optid = opt.Id;
				}
				if (context.ActionDescriptor.ControllerDescriptor.ControllerName == "DataGrid" && context.ActionDescriptor.ActionName == "Index")
				{
					LastOptimization = actOpt;
					if (LastOptimization != null)
					{
						oldOptValue = LastOptimization.GetOptimizationValue(OptimizationType.System);
					}
				}
				SetOptimization(optid);
				IOptimization newOpt = ViewboxSession.Optimizations.FirstOrDefault((IOptimization o) => o.Id == optid);
				if (newOpt != null)
				{
					newOptValue = newOpt.GetOptimizationValue(OptimizationType.System);
				}
				if (!string.IsNullOrWhiteSpace(oldOptValue) && !string.IsNullOrWhiteSpace(newOptValue) && oldOptValue != newOptValue)
				{
					ViewboxApplication.IsFilesSystemInitialized = false;
					int id = (int)context.ActionParameters["id"];
					ITableObject obj = ((id < 0) ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id]);
					ITableObject tableRef = ViewboxApplication.Database.SystemDb.Objects.FirstOrDefault((ITableObject x) => x.Database == newOptValue && x.Type == TableType.Table && x.IsVisible);
					ITableObject viewRef = ViewboxApplication.Database.SystemDb.Objects.FirstOrDefault((ITableObject x) => x.Database == newOptValue && x.Type == TableType.View && x.IsVisible);
					ITableObject issueRef = ViewboxApplication.Database.SystemDb.Objects.FirstOrDefault((ITableObject x) => x.Database == newOptValue && x.Type == TableType.Issue && x.IsVisible);
					if (tableRef != null)
					{
						if (tableRef.Name.Count() > 0)
						{
							context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
							{
								controller = "TableList",
								action = "Index"
							}));
						}
						else
						{
							context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
							{
								controller = "Settings",
								action = "Index"
							}));
						}
					}
					else if (viewRef != null)
					{
						if (viewRef.Name.Count() > 0)
						{
							context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
							{
								controller = "ViewList",
								action = "Index"
							}));
						}
						else
						{
							context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
							{
								controller = "Settings",
								action = "Index"
							}));
						}
					}
					else if (issueRef != null)
					{
						if (issueRef.Name.Count() > 0)
						{
							context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
							{
								controller = "IssueList",
								action = "Index"
							}));
						}
						else
						{
							context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
							{
								controller = "Settings",
								action = "Index"
							}));
						}
					}
					else
					{
						context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
						{
							controller = "Settings",
							action = "Index"
						}));
					}
				}
				if (newOpt.GetOptimization(OptimizationType.System) != actOpt.GetOptimization(OptimizationType.System))
				{
					int tableCount = ViewboxSession.GetIssueViewTableCount(TableType.Table);
					int viewCount = ViewboxSession.GetIssueViewTableCount(TableType.View);
					int issueCount = ViewboxSession.GetIssueViewTableCount(TableType.Issue);
					int documentCount = ViewboxSession.GetIssueViewTableCount(TableType.Archive);
					string controller = context.RouteData.Values["Controller"].ToString();
					TableType controlType = Enum.GetValues(typeof(TableType)).Cast<TableType>().FirstOrDefault((TableType x) => controller.Contains(x.ToString()));
					if (ViewboxSession.GetIssueViewTableCount(controlType) > 0)
					{
						context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
						{
							controller = controller,
							action = "Index"
						}));
					}
					else if (documentCount > 0 && controller == "Documents")
					{
						context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
						{
							controller = "Documents",
							action = "Index"
						}));
					}
					else if (issueCount > 0)
					{
						context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
						{
							controller = "IssueList",
							action = "Index"
						}));
					}
					else if (viewCount > 0)
					{
						context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
						{
							controller = "ViewList",
							action = "Index"
						}));
					}
					else if (tableCount > 0)
					{
						context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
						{
							controller = "TableList",
							action = "Index"
						}));
					}
					else if (documentCount > 0)
					{
						context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
						{
							controller = "Documents",
							action = "Index"
						}));
					}
					else
					{
						context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
						{
							controller = "Settings",
							action = "Index"
						}));
					}
				}
			}
			else
			{
				IOptimization optimization = ViewboxApplication.Database.SystemDb.GetUserOptimization(ViewboxSession.User);
				if (optimization != null)
				{
					SetOptimization(optimization.Id);
				}
				else
				{
					SetOptimization();
				}
			}
		}

		public static void SetOptimization(int optId = 0)
		{
			ViewboxSession.SetOptimization(optId);
		}

		public static void SetBack()
		{
			TableObject lastTo = ViewboxSession.TempTableObjects.LastOrDefault();
			if (LastOptimization != null && ((lastTo == null && LastOptimization.Id != ViewboxSession.Optimizations.LastOrDefault().Id) || (lastTo != null && lastTo.Optimization != null && lastTo.Optimization.Id != ViewboxSession.Optimizations.LastOrDefault().Id)))
			{
				SetOptimization(LastOptimization.Id);
			}
			else
			{
				SetOptimization();
			}
		}

		public static void CheckAfterExecution(TableObject prevTobj)
		{
			IOptimization opt = ViewboxSession.Optimizations.LastOrDefault();
			TableObject lastTto = ViewboxSession.TempTableObjects.LastOrDefault();
			if (prevTobj != null)
			{
				if (opt != null && opt.Id != prevTobj.Optimization.Id && prevTobj.OriginalTable.Id == lastTto.OriginalTable.Id)
				{
					SetOptimization(prevTobj.Optimization.Id);
					prevTobj.Optimization = prevTobj.Optimization.GetOptimization(OptimizationType.NotSet);
				}
				else if (lastTto != null && LastOptimization != null && prevTobj.Optimization.Id != LastOptimization.Id && prevTobj.Filter == lastTto.Filter)
				{
					SetOptimization(LastOptimization.Id);
					prevTobj.Optimization = prevTobj.Optimization.GetOptimization(OptimizationType.NotSet);
					lastTto.Optimization = lastTto.Optimization.GetOptimization(OptimizationType.NotSet);
				}
			}
		}

		private static void SelfRepair()
		{
			int revertId = ViewboxSession.TempTableObjects.Count * -1;
			if (ViewboxSession.TempTableObjects.Count <= 1)
			{
				return;
			}
			while (ViewboxSession.TempTableObjects[revertId].Optimization.Id == ViewboxSession.TempTableObjects[revertId + 1].Optimization.Id && revertId <= -2)
			{
				if (revertId != 0)
				{
					revertId++;
				}
			}
			SetOptimization(ViewboxSession.TempTableObjects[revertId].Optimization.Id);
		}
	}
}
