using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Domain;
using Domain.TextDaten;

namespace Voyager {
    // Hinweis: Anweisungen zum Aktivieren des klassischen Modus von IIS6 oder IIS7 
    // finden Sie unter "http://go.microsoft.com/?LinkId=9394801".

    public class MvcApplication : System.Web.HttpApplication {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Routenname
                "{controller}/{action}/{id}", // URL mit Parametern
                new { controller = "Tours", action = "Index", id = UrlParameter.Optional } // Parameterstandardwerte
            );

        }

        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();
            //Domain.Manager.Manager.CreateDatabase(Config.Conf);
            //Domain.Manager.Manager.CreateTable(Config.Conf);
            //Test test = new Test();
            //Domain.Manager.Manager.TestInsert(test);
            RegisterRoutes(RouteTable.Routes);
        }

        //protected void Session_Start() {
        //    if (Context.Session != null && Context.Session.IsNewSession && Request.IsAuthenticated)
        //        CurrentUser.GetCurrentUser();
        //} 
    }
}
