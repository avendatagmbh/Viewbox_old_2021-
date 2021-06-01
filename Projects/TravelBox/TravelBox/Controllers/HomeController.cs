using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Localisation;
using System.Globalization;

namespace TravelBox.Controllers {
    public class HomeController : Controller {

        public ActionResult Index() {
            //MvcApplication.SetLanguage(LanguageManager.AvailableLanguages.ToList<ILanguage>()[1]);
            return View();
        }

    }
}
