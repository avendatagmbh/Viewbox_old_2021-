using System.Diagnostics;
using System.IO;

namespace Utils
{
    /// <summary>
    /// </summary>
    public class WebDevWebserverManager
    {
        public const string WEB_SERVER_PATH_V1 =
            @"C:\Program Files (x86)\Common Files\microsoft shared\DevServer\10.0\WebDev.WebServer40.EXE";

        public const string WEB_SERVER_PATH_V2 =
            @"C:\Program Files\Common Files\microsoft shared\DevServer\10.0\WebDev.WebServer40.EXE";

        /// <summary>
        ///   Starts a WebDev.WebServer40 process.
        /// </summary>
        /// <param name="processStartInfoArguments"> For example: /path:"C:\temp\viewbox_src" /port:50000 </param>
        /// <returns> True if WebDev.WebServer40 process is started, False otherwise. </returns>
        public static bool StartWebDevWebserverAsAdministrator(string processStartInfoArguments)
        {
            string webserverPath = getWebDevWebserverPath();
            if (webserverPath == null)
                return false;
            KillAllRunningProcessesNamed("WebDev.WebServer40");
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = webserverPath;
            info.UseShellExecute = true;
            //info.Verb = "runas"; // Provides Run as Administrator
            info.Arguments = processStartInfoArguments;
            if (Process.Start(info) == null)
                return false;
            return true;
        }

        /// <summary>
        ///   Returns the WebDev.WebServer40 path if exist on local computer file system, else returns null.
        /// </summary>
        private static string getWebDevWebserverPath()
        {
            if (File.Exists(WEB_SERVER_PATH_V1))
                return WEB_SERVER_PATH_V1;
            else if (File.Exists(WEB_SERVER_PATH_V2))
                return WEB_SERVER_PATH_V2;
            return null;
        }

        public static void KillAllRunningProcessesNamed(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
                process.Kill();
        }
    }
}