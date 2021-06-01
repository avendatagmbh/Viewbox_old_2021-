using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Localisation;

namespace TravelBox {
    
    public static class TravelBoxSession {

        public static bool IsInitialized {
            get { return Language != null /*&& User != null && HttpContext.Current.Session != null && UserObjects != null*/; }
        }

        public static ILanguage Language {
            get { return (HttpContext.Current.Session != null ? HttpContext.Current.Session["Language"] as ILanguage : null) ?? TravelBoxApplication.BrowserLanguage; }
            set {
                if (value == null) return;
                HttpContext.Current.Session["Language"] = value;
                MvcApplication.SetLanguage(Language);
            }
        }
    }
}