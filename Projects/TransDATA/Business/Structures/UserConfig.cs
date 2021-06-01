using System;
using System.IO;
using Base.EventArgs;
using Base.Localisation;

namespace Business.Structures {
    /// <summary>
    /// User configuration.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-08-27</since>
    public class UserConfig {
        private UserConfig() {}

        #region events

        //--------------------------------------------------------------------------------
        public static event EventHandler<MessageEventArgs> Error;

        private static void OnError(string message) {
            if (Error != null) Error(null, new MessageEventArgs(message));
        }

        //--------------------------------------------------------------------------------

        #endregion events

        #region properties

        //--------------------------------------------------------------------------------

        /// <summary>
        /// Returns the full filename of the user config file.
        /// </summary>
        private static string UserConfigFile {
            get {
                string file = "transdata.cfg";
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                              "\\AvenDATA\\TransDATA";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path + "\\" + file;
            }
        }

        #region LastUserName

        private string _lastUserName;

        /// <summary>
        /// Returns the name of the last user which has be logged in.
        /// </summary>
        public string LastUserName {
            get { return _lastUserName; }
            set {
                _lastUserName = value;
                WriteUserConfig();
            }
        }

        #endregion LastUserName

        //--------------------------------------------------------------------------------

        #endregion properties

        /// <summary>
        /// Loads an existing user config or creates a new one.
        /// </summary>
        /// <returns></returns>
        internal static UserConfig GetUserConfig() {
            var uc = new UserConfig();
            uc.ReadUserConfig();
            return uc;
        }


        /// <summary>
        /// Reads the user config from config file, if exists.
        /// </summary>
        private void ReadUserConfig() {
            string configFile = UserConfigFile;
            if (!File.Exists(configFile)) return;

            StreamReader reader = null;
            try {
                reader = new StreamReader(configFile);
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine().Trim();
                    if (line.Length == 0) continue;
                    if (!line.Contains("=")) continue;

                    var parts = new string[2];
                    parts[0] = line.Substring(0, line.IndexOf('='));
                    parts[1] = line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=') - 1);
                    switch (parts[0].Trim().ToLower()) {
                        case "lastuser":
                            _lastUserName = parts[1].Trim();
                            break;
                    }
                }
            }
            catch (Exception ex) {
                OnError(String.Format(ExceptionMessages.ReadUserConfig, ex.Message));
            }
            finally {
                if (reader != null) reader.Close();
            }
        }


        /// <summary>
        /// Writes the user config to config file.
        /// </summary>
        private void WriteUserConfig() {
            string configFile = UserConfigFile;
            StreamWriter writer = null;
            try {
                writer = new StreamWriter(configFile);
                writer.WriteLine("lastuser=" + LastUserName);
            }
            catch (Exception ex) {
                OnError(String.Format(ExceptionMessages.WriteUserConfig, ex.Message));
            }
            finally {
                if (writer != null) writer.Close();
            }
        }
    }
}