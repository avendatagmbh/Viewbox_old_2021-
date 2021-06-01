using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using DbAccess.Structures;
using Utils;

namespace ViewBuilderBusiness.Structures
{
    public class ViewboxStarter
    {
        #region Constructor

        public ViewboxStarter(DbConfig viewboxDbConfig)
        {
            _viewboxDbConfig = viewboxDbConfig;
        }

        #endregion Constructor

        #region Properties

        private readonly DbConfig _viewboxDbConfig;
        private ProgressCalculator _copyDataProgress;
        
        private string _viewboxDestination = @"C:\temp\viewbox_src";
        private string _viewboxSrc = @"\\lenny\Netztausch\ata\Viewbox_src";

        public static string Path1
        {
            get { return @"C:\Program Files (x86)\Common Files\microsoft shared\DevServer\10.0\WebDev.WebServer40.EXE"; }
        }

        public static string Path2
        {
            get { return @"C:\Program Files\Common Files\microsoft shared\DevServer\10.0\WebDev.WebServer40.EXE"; }
        }

        #endregion Properties

        #region Methods

        public ProgressCalculator CreateProgressCalculator()
        {
            _copyDataProgress = new ProgressCalculator();
            _copyDataProgress.Title = "Kopiere Viewbox Daten";
            _copyDataProgress.DoWork += CopyDataProgressOnDoWork;
            return _copyDataProgress;
            //copyDataProgress.RunWorkerCompleted += copyDataProgress_RunWorkerCompleted;
        }

        private void CopyDataProgressOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            ProgressCalculator progress = sender as ProgressCalculator;
            progress.SetWorkSteps(1, false);
            if (!Directory.Exists(_viewboxDestination) ||
                new FileInfo(_viewboxSrc + "\\Global.asax").LastWriteTime !=
                new FileInfo(_viewboxDestination + "\\Global.asax").LastWriteTime)
            {
                Directory.CreateDirectory(_viewboxDestination);
                progress.Description = "Lese zu kopierende Dateien";
                //string source = @"N:\beh\viewbox_src";
                List<string> files =
                    new List<string>(Directory.EnumerateFiles(_viewboxSrc, "*.*", SearchOption.AllDirectories));
                progress.SetWorkSteps(files.Count + 1, false);
                foreach (var folder in Directory.GetDirectories(_viewboxSrc, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(_viewboxDestination + folder.Substring(_viewboxSrc.Length));
                foreach (var file in files)
                {
                    if (progress.CancellationPending)
                        throw new OperationCanceledException("Operation abgebrochen");
                    progress.Description = "Kopiere Datei " + new FileInfo(file).Name;
                    File.Copy(file, _viewboxDestination + file.Substring(_viewboxSrc.Length), true);
                    progress.StepDone();
                }
            }
            progress.Description = "Ändere web.config und starte Viewbox";
            if (!File.Exists(_viewboxDestination + @"\Web_Placeholder.config"))
                throw new InvalidOperationException(string.Format("Die nötige Datei {0} existiert nicht.",
                                                                  _viewboxDestination + @"\Web_Placeholder.config"));
            using (var reader = new StreamReader(_viewboxDestination + @"\Web_Placeholder.config"))
            {
                string webConfigContent = AdjustWebConfig(reader.ReadToEnd());
                using (var writer = new StreamWriter(_viewboxDestination + @"\Web.config"))
                {
                    writer.Write(webConfigContent);
                }
            }
            string webserverPath = File.Exists(Path1) ? Path1 : Path2;
            string startProcessArguments = string.Format("/path:\"{0}\" /port:50000", _viewboxDestination);
            //Kill other running webdev server instances
            var processes = Process.GetProcessesByName("WebDev.WebServer40");
            foreach (var process in processes)
                process.Kill();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = webserverPath;
            info.UseShellExecute = true;
            //info.Verb = "runas"; // Provides Run as Administrator
            info.Arguments = startProcessArguments;
            Process lastProcess = null;
            if ((lastProcess = Process.Start(info)) != null)
            {
                Process.Start("http://localhost:50000");
                progress.UserState = true;
                progress.StepDone();
            }
            else if (lastProcess != null && lastProcess.HasExited && lastProcess.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Der Webserver wurde mit Code {0} beendet, wahrscheinlich ist er bereits gestartet und konnte nicht automatisch beendet werden.",
                        lastProcess.ExitCode));
            }
            //if (lastProcess != null && lastProcess.HasExited && lastProcess.ExitCode != 0)
            //{
            //    throw new InvalidOperationException(string.Format("Der Webserver wurde mit Code {0} beendet, wahrscheinlich ist er bereits gestartet und konnte nicht automatisch beendet werden.", lastProcess.ExitCode));
            //}
            //Process.Start("http://localhost:50000");
            //progress.UserState = true;
            //progress.StepDone();
        }

        private string AdjustWebConfig(string content)
        {
            string connectionString, dataProvider = _viewboxDbConfig.DbType;
            if (_viewboxDbConfig.DbType == "MySQL")
                connectionString =
                    string.Format(
                        "<add name=\"Viewbox.Properties.Settings.ViewboxDatabase\" connectionString=\"server={0};port={1};User Id=root;password=avendata;database={2};Connect Timeout=1000;Default Command Timeout=0;Allow Zero Datetime=True\" providerName=\"MySql.Data.MySqlClient\" />",
                        _viewboxDbConfig.Hostname, _viewboxDbConfig.Port, _viewboxDbConfig.DbName);
            else
                connectionString =
                    string.Format(
                        "<add name=\"Viewbox.Properties.Settings.ViewboxDatabase\" providerName=\"System.Data.SqlClient\" connectionString=\"Data Source={0};Initial Catalog={1};User ID=sa;Password=avendata\" />",
                        _viewboxDbConfig.Hostname, _viewboxDbConfig.DbName);
            string machineName = Environment.MachineName ?? "nomachineName";
            machineName = Regex.Replace(machineName, @"[^a-zA-Z0-9_$]", "_");
            return
                content.Replace("<<PLACEHOLDER_CONNECTIONSTRING>>", connectionString).Replace("<<DATAPROVIDER>>",
                                                                                              dataProvider).Replace(
                                                                                                  "<<TEMP_DB>>",
                                                                                                  "temp_" + machineName);
        }

        public void StartViewbox()
        {
            _copyDataProgress.RunWorkerAsync();
        }

        #endregion Methods
    }
}