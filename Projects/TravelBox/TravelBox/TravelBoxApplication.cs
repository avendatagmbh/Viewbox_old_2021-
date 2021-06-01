using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Localisation;
using System.Globalization;

namespace TravelBox {

    public static class TravelBoxApplication {

        static bool IsInitializing {
            get {
                return Application != null
                    && Application["travelBoxApplication.Initializing"] != null
                    && (bool)Application["TravelBoxApplication.Initializing"];
            }
            set { Application["TravelBoxApplication.Initializing"] = value; }
        }

        static HttpApplicationState _application;
        public static HttpApplicationState Application {
            get { return _application; }
        }

        public static void Init(HttpApplicationState application) {
            if (IsInitializing) throw new InvalidOperationException("already initialized");
            _application = application;

            //ServerInitializedTime = ServerStart = DateTime.Now;
            //Initializing = true;
            //Initialized = false;

            //Database = new ViewboxDb.ViewboxDb(TempDatabaseName);
            //Database.ViewboxDbInitialized += new EventHandler(ViewboxDatabaseInitialized);
            //Database.Connect(DataProvider, Settings.Default.ViewboxDatabase);

            //UserSessions = new SessionMarker();

            //try {
            //    foreach (var file in Directory.EnumerateFiles(TemporaryDirectory, "*.zip"))
            //        File.Delete(file);
            //} catch { }

            //Job.Janitor.Start();

            //TempTableObjects = new TableObjectCollection();

            //languages = new List<ILanguage>() {
            //    new Language { CountryCode = "de", LanguageName = "Deutsch", LanguageMotto = "Viewbox erleben" },
            //    //new Language { CountryCode = "en", LanguageName = "English", LanguageMotto = "Enjoy Viewbox" },
            //    /*new Language { CountryCode = "es", LanguageName = "Español", LanguageMotto = "Disfrute Viewbox" },
            //    new Language { CountryCode = "fr", LanguageName = "Française", LanguageMotto = "Jouir Viewbox" },
            //    new Language { CountryCode = "it", LanguageName = "Italiano", LanguageMotto = "Godetevi Viewbox" },
            //    new Language { CountryCode = "sv", LanguageName = "Svenska", LanguageMotto = "Njuta Viewbox" },
            //    new Language { CountryCode = "da", LanguageName = "Danske", LanguageMotto = "Nyd Viewbox" },*/
            //};
        }

        public static ILanguage BrowserLanguage {
            get {
                if (HttpContext.Current.Request.UserLanguages != null)
                    foreach (var l in HttpContext.Current.Request.UserLanguages)
                        try {
                            ILanguage lang = GetLanguageByCountryCode(new CultureInfo(l.Split(';').First()).TwoLetterISOLanguageName);
                            if (lang != null) return lang;
                        } catch { }
                return LanguageManager.FallbackLanguage;
            }
        }

        public static bool Initialized {
            get { return (bool)Application["TravelBoxboxApplication.Initialized"]; }
            private set { Application["TravelBoxboxApplication.Initialized"] = value; }
        }


        public static ILanguage GetLanguageByCountryCode(string countryCode) {
            var cc = countryCode.ToLowerInvariant();
            foreach (var l in LanguageManager.Languages) if (l.CountryCode == cc) return l;
            return null;
        }

    }
}