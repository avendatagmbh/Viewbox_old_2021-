using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Localisation;
using System.Globalization;

namespace TravelBox {
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            
            TravelBoxApplication.Init(Application);
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e) {
            //if (TravelBoxApplication.Initialized && Context.Session != null && ViewboxSession.IsAuthenticated && !ViewboxSession.IsInitialized) TravelBoxApplication.Init();
            //if (TravelBoxSession.IsInitialized) TravelBoxSession.SessionMarker.RefreshSession();
            SetLanguage(Context.Session != null ? (TravelBoxSession.Language ?? TravelBoxApplication.BrowserLanguage) : TravelBoxApplication.BrowserLanguage);
        }
        
        public static void SetLanguage(ILanguage language) {
            CultureInfo ci = new CultureInfo(language.CountryCode);
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
        }
    }
}