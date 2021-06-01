using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using DbAccess.Structures;
using ViewBuilderBusiness.Structures;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilderBusiness.Manager
{
    /// <summary>
    ///   This class provides functionality to persist and restore the profiles.
    /// </summary>
    public static class ProfileManager
    {
        /// <summary>
        ///   Initializes the <see cref="ProfileManager" /> class.
        /// </summary>
        static ProfileManager()
        {
            ProfileNames = new ObservableCollection<string>();
        }

        #region properties

        /// <summary>
        ///   Gets or sets the profile db config.
        /// </summary>
        /// <value> The profile db config. </value>
        public static DbConfig ProfileDbConfig
        {
            get { return ApplicationManager.ApplicationConfig.ConfigDbConfig; }
        }

        /// <summary>
        ///   Gets the profile directory.
        /// </summary>
        /// <value> The profile directory. </value>
        private static string ProfileDirectory
        {
            get { return ApplicationManager.ApplicationConfig.ConfigDirectory + "\\viewbuilder\\profiles"; }
        }

        /// <summary>
        ///   Gets or sets the profiles.
        /// </summary>
        /// <value> The profiles. </value>
        public static ObservableCollection<string> ProfileNames { get; private set; }

        #endregion properties

        #region methods

        /// <summary>
        ///   Saves the specified profile.
        /// </summary>
        /// <param name="profile"> The profile. </param>
        public static void Save(ProfileConfig profile)
        {
            switch (ApplicationManager.ApplicationConfig.ConfigLocation)
            {
                case ConfigLocation.Directory:
                    if (!Directory.Exists(ProfileDirectory)) Directory.CreateDirectory(ProfileDirectory);
                    // save xml file
                    XmlTextWriter writer = new XmlTextWriter(ProfileDirectory + "\\" + profile.Name + ".xml",
                                                             Encoding.UTF8);
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;
                    writer.WriteStartDocument();
                    writer.WriteStartElement("ViewBuilderConfig");
                    writer.WriteElementString("Name", profile.Name);
                    writer.WriteElementString("Description", profile.Description);
                    writer.WriteElementString("AutoGenerateIndex",
                                              profile.AutoGenerateIndex.ToString(CultureInfo.InvariantCulture));
                    writer.WriteElementString("ViewboxDb", profile.ViewboxDbName);
                    writer.WriteElementString("MaxWorkerThreads", profile.MaxWorkerThreads.Value.ToString());

                    // write database config
                    writer.WriteStartElement("DatabaseConfig");
                    writer.WriteAttributeString("type", profile.DbConfig.DbType);
                    writer.WriteAttributeString("host", profile.DbConfig.Hostname);
                    writer.WriteAttributeString("user", profile.DbConfig.Username);
                    writer.WriteAttributeString("password", profile.DbConfig.Password);
                    writer.WriteAttributeString("port", profile.DbConfig.Port.ToString());
                    writer.WriteAttributeString("database", profile.DbConfig.DbName);
                    writer.WriteEndElement();
                    // write mail config
                    writer.WriteStartElement("MailConfig");
                    writer.WriteAttributeString("sendDailyReport", (profile.Mail.SendDailyReport ? "true" : "false"));
                    writer.WriteAttributeString("sendFinalReport", (profile.Mail.SendFinalReport ? "true" : "false"));
                    writer.WriteAttributeString("sendMailOnError", (profile.Mail.SendMailOnError ? "true" : "false"));
                    writer.WriteAttributeString("SendMailOnViewFinished",
                                                (profile.Mail.SendMailOnViewFinished ? "true" : "false"));
                    writer.WriteAttributeString("dailyReportIntervall",
                                                Enum.GetName(typeof (DailyReportIntervall),
                                                             profile.Mail.DailyReportIntervall));
                    writer.WriteEndElement();
                    // write script source config
                    writer.WriteStartElement("ScriptSource");
                    writer.WriteAttributeString("directory", profile.ScriptSource.Directory);
                    writer.WriteAttributeString("bilanzDirectory", profile.ScriptSource.BilanzDirectory);
                    writer.WriteAttributeString("extendedColumnInformationDirectory",
                                                profile.ScriptSource.ExtendedColumnInformationDirectory);
                    writer.WriteAttributeString("includeSubDirectories",
                                                (profile.ScriptSource.IncludeSubdirectories ? "true" : "false"));
                    writer.WriteAttributeString("mode",
                                                Enum.GetName(typeof (ScriptSourceMode),
                                                             profile.ScriptSource.ScriptSourceMode));
                    writer.WriteEndElement();

                    writer.WriteEndElement();

                    writer.WriteEndDocument();
                    writer.Close();
                    if (!ProfileNames.Contains(profile.Name))
                    {
                        ProfileNames.Add(profile.Name);
                        // sort profile names
                        List<string> tmp = new List<string>(ProfileNames);
                        tmp.Sort();
                        ProfileNames.Clear();
                        foreach (string profileName in tmp) ProfileNames.Add(profileName);
                    }
                    break;
                case ConfigLocation.Database:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        ///   Opens the specified profile.
        /// </summary>
        /// <param name="profileName"> Name of the profile. </param>
        /// <returns> </returns>
        public static ProfileConfig Open(string profileName)
        {
            ProfileConfig profile = new ProfileConfig();
            switch (ApplicationManager.ApplicationConfig.ConfigLocation)
            {
                case ConfigLocation.Directory:
                    string file = ProfileDirectory + "\\" + profileName + ".xml";
                    XmlDocument doc = new XmlDocument();
                    doc.Load(file);
                    OpenDirectoryProfile(doc, profile);
                    break;
                case ConfigLocation.Database:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
            return profile;
        }

        /// <summary>
        ///   Opens the specified profile.
        /// </summary>
        /// <param name="profileName"> Name of the profile. </param>
        /// <returns> </returns>
        public static ProfileConfig OpenDirectoryProfile(XmlDocument doc, ProfileConfig profile)
        {
            XmlNode root = doc.DocumentElement;
            foreach (XmlNode node in root.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Name":
                        profile.Name = node.InnerText.Trim();
                        break;
                    case "Description":
                        profile.Description = node.InnerText.Trim();
                        break;
                    case "AutoGenerateIndex":
                        profile.AutoGenerateIndex = false;
                        try
                        {
                            profile.AutoGenerateIndex = Convert.ToBoolean(node.InnerText.Trim());
                        }
                        catch (Exception)
                        {
                        }
                        break;
                    case "ViewboxDb":
                        profile.ViewboxDbName = node.InnerText.Trim();
                        break;
                    case "MaxWorkerThreads":
                        int maxWorkerThreads = Convert.ToInt32(node.InnerText.Trim());
                        foreach (MaxWorkerThreads mwt in profile.AllowedMaxWorkerThreads)
                        {
                            if (mwt.Value == maxWorkerThreads)
                            {
                                profile.MaxWorkerThreads = mwt;
                                break;
                            }
                        }
                        break;
                    case "DatabaseConfig":
                        foreach (XmlAttribute attr in node.Attributes)
                        {
                            switch (attr.Name)
                            {
                                case "type":
                                    profile.DbConfig.DbType = attr.Value;
                                    break;
                                case "host":
                                    profile.DbConfig.Hostname = attr.Value;
                                    break;
                                case "user":
                                    profile.DbConfig.Username = attr.Value;
                                    break;
                                case "password":
                                    profile.DbConfig.Password = attr.Value;
                                    break;
                                case "port":
                                    int m_Port = 0;
                                    if (Int32.TryParse(attr.Value, out m_Port) && m_Port > 0)
                                    {
                                        profile.DbConfig.Port = m_Port;
                                    }
                                    break;
                                case "database":
                                    profile.DbConfig.DbName = attr.Value;
                                    break;
                            }
                        }
                        break;
                    case "ScriptSource":
                        foreach (XmlAttribute attr in node.Attributes)
                        {
                            switch (attr.Name)
                            {
                                case "directory":
                                    profile.ScriptSource.Directory = attr.Value;
                                    break;

                                case "bilanzDirectory":
                                    profile.ScriptSource.BilanzDirectory = attr.Value;
                                    break;
                                case "extendedColumnInformationDirectory":
                                    profile.ScriptSource.ExtendedColumnInformationDirectory = attr.Value;
                                    break;
                                case "includeSubDirectories":
                                    profile.ScriptSource.IncludeSubdirectories = Boolean.Parse(attr.Value);
                                    break;
                                case "mode":
                                    profile.ScriptSource.ScriptSourceMode =
                                        (ScriptSourceMode) Enum.Parse(typeof (ScriptSourceMode), attr.Value);
                                    break;
                            }
                        }
                        break;
                    case "MailConfig":
                        foreach (XmlAttribute attr in node.Attributes)
                        {
                            switch (attr.Name)
                            {
                                case "dailyReportIntervall":
                                    profile.Mail.DailyReportIntervall =
                                        (DailyReportIntervall) Enum.Parse(typeof (DailyReportIntervall), attr.Value);
                                    break;
                                case "sendDailyReport":
                                    profile.Mail.SendDailyReport = Boolean.Parse(attr.Value);
                                    break;
                                case "sendFinalReport":
                                    profile.Mail.SendFinalReport = Boolean.Parse(attr.Value);
                                    break;
                                case "sendMailOnError":
                                    profile.Mail.SendMailOnError = Boolean.Parse(attr.Value);
                                    break;
                                case "SendMailOnViewFinished":
                                    profile.Mail.SendMailOnViewFinished = Boolean.Parse(attr.Value);
                                    break;
                            }
                        }
                        break;
                }
            }
            if (String.IsNullOrEmpty(profile.ScriptSource.BilanzDirectory))
                profile.ScriptSource.BilanzDirectory = profile.ScriptSource.Directory;
            return profile;
        }

        /// <summary>
        ///   Deletes the specified profile.
        /// </summary>
        /// <param name="profile"> The profile. </param>
        public static void DeleteProfile(string profileName)
        {
            switch (ApplicationManager.ApplicationConfig.ConfigLocation)
            {
                case ConfigLocation.Directory:
                    string filename = ProfileDirectory + "\\" + profileName + ".xml";
                    if (File.Exists(filename)) File.Delete(filename);
                    if (ProfileNames.Contains(profileName))
                    {
                        ProfileNames.Remove(profileName);
                    }
                    break;
                case ConfigLocation.Database:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        ///   Updates the profile names.
        /// </summary>
        public static void UpdateProfileNames()
        {
            ProfileNames.Clear();
            switch (ApplicationManager.ApplicationConfig.ConfigLocation)
            {
                case ConfigLocation.Directory:
                    if (Directory.Exists(ProfileDirectory))
                    {
                        foreach (FileInfo fi in new DirectoryInfo(ProfileDirectory).EnumerateFiles())
                        {
                            if (fi.Name.EndsWith(".xml"))
                            {
                                ProfileNames.Add(fi.Name.Substring(0, fi.Name.Length - 4));
                            }
                        }
                    }
                    break;
                case ConfigLocation.Database:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion methods

        /*****************************************************************************************************/

        /*****************************************************************************************************/
    }
}