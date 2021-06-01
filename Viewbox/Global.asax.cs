using System;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using SystemDb;
using AV.Log;
using Viewbox.Exceptions;
using Viewbox.Job.PdfExport;

namespace Viewbox
{
	public class MvcApplication : HttpApplication
	{
		private class FirstRequestInitialization
		{
			private static bool s_InitializedAlready;

			private static object s_lock = new object();

			public static void Initialize(HttpContext context)
			{
				if (s_InitializedAlready)
				{
					return;
				}
				lock (s_lock)
				{
					if (!s_InitializedAlready)
					{
						s_InitializedAlready = true;
						string path = context.Request.Url.PathAndQuery;
						ViewboxApplication.ViewboxBasePath = context.Request.Url.AbsoluteUri.Remove(context.Request.Url.AbsoluteUri.Length - path.Length);
						FormsAuthentication.SignOut();
						context.Request.Cookies.Remove(FormsAuthentication.FormsCookieName);
					}
				}
			}
		}

		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute
			{
				View = "~/Views/Shared/CustomError.cshtml"
			});
			filters.Add(new HandleErrorAttribute
			{
				ExceptionType = typeof(ViewboxRightException),
				View = "~/Views/Error/Index.cshtml"
			});
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.MapRoute("Default", "{controller}/{action}/{id}", new
			{
				controller = "Home",
				action = "Index",
				id = UrlParameter.Optional
			});
		}

		protected void Application_Start()
		{
			string logPath = ConfigurationManager.AppSettings["LogPath"];
			LogHelper.ConfigureLogger(LogHelper.GetLogger(), logPath);
			PdfExporterConfig.Load(base.Server.MapPath("~/PdfExporter.config"));
			HttpApplicationFactory.SetCurrentApplication(new HttpApplicationStateWrapper(base.Application));
			AreaRegistration.RegisterAllAreas();
			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
			ViewboxApplication.Init(HttpApplicationFactory.Current);
		}

		protected void Application_BeginRequest(object source, EventArgs e)
		{
			HttpApplication app = (HttpApplication)source;
			if (app.Context.Session != null)
			{
				app.Context.Session.Clear();
			}
			HttpContext context = app.Context;
			FirstRequestInitialization.Initialize(context);
		}

		protected void Application_AcquireRequestState(object sender, EventArgs e)
		{
			bool check = true;
			MvcApplication send = sender as MvcApplication;
			if (send != null && send.Request.RequestContext.RouteData.Values.Count > 0)
			{
				RouteValueDictionary values = send.Request.RequestContext.RouteData.Values;
				if (values.ContainsKey("controller") && values.ContainsKey("action") && values["controller"].ToString().ToLower() == "command" && values["action"].ToString().ToLower() == "getupdates")
				{
					check = false;
				}
			}
			if (check && ViewboxApplication.Initialized && base.Context.Session != null && ViewboxSession.IsAuthenticated && !ViewboxSession.IsInitialized)
			{
				if (ViewboxSession.User == null)
				{
					FormsAuthentication.SignOut();
					base.Request.Cookies.Remove(FormsAuthentication.FormsCookieName);
					base.Response.RedirectToRoute(base.Request.RequestContext.RouteData);
					return;
				}
				ViewboxSession.Init();
			}
			if (ViewboxSession.IsInitialized)
			{
				ViewboxSession.SessionMarker.RefreshSession();
			}
			SetLanguage((base.Context.Session != null) ? (ViewboxSession.Language ?? ViewboxApplication.BrowserLanguage) : ViewboxApplication.BrowserLanguage);
		}

		protected void Session_Start()
		{
			if (ViewboxApplication.Initialized && ViewboxSession.IsAuthenticated && !ViewboxSession.IsInitialized)
			{
				ViewboxSession.Init();
			}
		}

		public static void SetLanguage(ILanguage language)
		{
			try
			{
				//string cc = (language.CountryCode.Equals("en", StringComparison.OrdinalIgnoreCase) ? "en-GB" : language.CountryCode);
				//CultureInfo ci = new CultureInfo(cc);
				//Thread.CurrentThread.CurrentUICulture = ci;
				//Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
			}
			catch (Exception)
			{
			}
		}
	}
}
