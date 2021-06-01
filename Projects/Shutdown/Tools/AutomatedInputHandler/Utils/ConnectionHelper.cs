// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2013-02-19
// copyright 2013 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace Utils {
    public static class ConnectionHelper {

        /// <summary>
        /// Checks if an internet connection exists by opening the <param name="webUrl" /> for reading.
        /// </summary>
        /// <param name="webUrl">The url that should be tried to open. Per default it's http://www.avendata.de</param>
        /// <returns>Success?</returns>
        public static bool CheckForInternetConnection(string webUrl = "http://www.avendata.de") {
            try {
                using (var client = new System.Net.WebClient())
                using (var stream = client.OpenRead(webUrl)) {
                    return true;
                }
            }
            catch {
                return false;
            }
        } 
    }
}