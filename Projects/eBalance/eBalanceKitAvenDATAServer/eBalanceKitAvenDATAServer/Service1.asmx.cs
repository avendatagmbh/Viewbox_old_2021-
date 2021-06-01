using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace eBalanceKitAvenDATAServer {
    /// <summary>
    /// Zusammenfassungsbeschreibung für Service1
    /// </summary>
    [WebService(Namespace = "http://www.avendata.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Um das Aufrufen dieses Webdiensts aus einem Skript mit ASP.NET AJAX zuzulassen, heben Sie die Auskommentierung der folgenden Zeile auf. 
    // [System.Web.Script.Services.ScriptService]
    public class Service1 : System.Web.Services.WebService {

        [WebMethod]
        public string CurrentVersion() {
            return "1.0.1";
        }
    }
}