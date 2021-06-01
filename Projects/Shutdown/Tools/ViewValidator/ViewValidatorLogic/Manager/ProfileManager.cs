/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-11      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using AvdCommon.Rules;
using AvdCommon.Rules.Factories;
using DbAccess.Structures;
using ViewValidatorLogic.Config;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidatorLogic.Manager {

    /// <summary>
    /// This class provides functionality to persist and restore the profiles.
    /// </summary>
    public static class ProfileManager {

        /// <summary>
        /// Initializes the <see cref="ProfileManager"/> class.
        /// </summary>
        static ProfileManager() {
            //ProfileNames = new ObservableCollection<string>();
            Profiles = new ObservableCollection<ProfileConfig>();
        }

        /*****************************************************************************************************/

        #region properties

        /// <summary>
        /// Gets or sets the profile db config.
        /// </summary>
        /// <value>The profile db config.</value>
        //public static ConfigDatabase ProfileDbConfig {
        //    get { return ApplicationManager.ApplicationConfig.ConfigDbConfig; }
        //}

        /// <summary>
        /// Gets the profile directory.
        /// </summary>
        /// <value>The profile directory.</value>
        private static string ProfileDirectory {
            get { return ApplicationManager.ApplicationConfig.ConfigDirectory + "\\ViewValidator\\profiles"; }
        }

        /// <summary>
        /// Gets or sets the profiles.
        /// </summary>
        /// <value>The profiles.</value>
        //public static ObservableCollection<string> ProfileNames { get; private set; }
        public static ObservableCollection<ProfileConfig> Profiles { get; private set; }

        #endregion properties

        /*****************************************************************************************************/

        #region methods

        #region Save
        /// <summary>
        /// Saves the specified profile.
        /// </summary>
        /// <param name="profile">The profile.</param>
        public static void Save(ProfileConfig profile) {
            switch (ApplicationManager.ApplicationConfig.ConfigLocation) {
                case ConfigLocation.Directory:
                    if (!Directory.Exists(ProfileDirectory)) Directory.CreateDirectory(ProfileDirectory);

                    // save xml file
                    XmlTextWriter writer = new XmlTextWriter(ProfileDirectory + "\\" + profile.Name + ".xml", Encoding.UTF8);
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;

                    writer.WriteStartDocument();

                    writer.WriteStartElement("ViewValidatorConfig");

                    writer.WriteElementString("Name", profile.Name);
                    writer.WriteElementString("Description", profile.Description);
                    writer.WriteElementString("ErrorLimit", profile.ValidationSetup.ErrorLimit.ToString());
                    
                    // write database config
                    ValidationSetup setup = profile.ValidationSetup;
                    WriteDbConfig("DatabaseConfigValidation", setup.DbConfigValidation, writer);
                    WriteDbConfig("DatabaseConfigView", setup.DbConfigView, writer);

                    //Custom rules
                    WriteRules(profile.CustomRules, writer, "CustomRules");


                    foreach (var tableMapping in setup.TableMappings) {
                        writer.WriteStartElement("TableMapping");
                        writer.WriteElementString("Used", tableMapping.Used.ToString());
                        WriteTable("TableValidation", tableMapping.TableValidation, writer, true);
                        WriteTable("TableView", tableMapping.TableView, writer, false);

                        writer.WriteStartElement("ColumnMappings");
                        foreach (var columnMapping in tableMapping.ColumnMappings) {
                            writer.WriteStartElement("ColumnMapping");
                            writer.WriteAttributeString("IsVisible", columnMapping.IsVisible.ToString());
                            WriteColumn(columnMapping.SourceCol, writer, false);
                            WriteColumn(columnMapping.DestinationCol, writer, true);
                            
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();

                        WriteRules(tableMapping.Rules, writer);

                        writer.WriteStartElement("KeyEntryMappings");
                        foreach (var keyMapping in tableMapping.KeyEntryMappings) {
                            writer.WriteStartElement("KeyEntryMapping");
                            WriteColumn(keyMapping.SourceCol, writer, false);
                            WriteColumn(keyMapping.DestinationCol, writer, true);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Close();

                    //if (!ProfileNames.Contains(profile.Name)) {
                    //    ProfileNames.Add(profile.Name);
                    //}
                    //if (!Profiles.Contains(profile))
                    //    Profiles.Add(profile);

                    break;

                case ConfigLocation.Database:
                    throw new NotImplementedException();

                default:
                    throw new NotImplementedException();
            }
        }

        private static void WriteRules(RuleSet rules, XmlTextWriter writer, string xmlElement = "Rules") {
            writer.WriteStartElement(xmlElement);
            //foreach (var rule in rules.ExecuteRules) {
            foreach (var rule in rules.AllRules) {
                writer.WriteStartElement("Rule");
                rule.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private static void WriteColumn(Column column, XmlTextWriter writer, bool destination) {
            writer.WriteStartElement(destination ? "DestinationCol" : "SourceCol");
            writer.WriteAttributeString("Name", column.Name);
            WriteRules(column.Rules, writer);
            writer.WriteEndElement();
        }

        private static void WriteTable(string elemName, Table table, XmlWriter writer, bool writeDbConfig) {
            writer.WriteStartElement(elemName);
            writer.WriteAttributeString("Name", table.Name);
            writer.WriteElementString("Filter", table.Filter == null ? "" : table.Filter.FilterString);
            if(writeDbConfig)
                WriteDbConfig("DbConfig", table.DbConfig, writer);
            writer.WriteEndElement();
        }

        static private void WriteDbConfig(string elemName, DbConfig config, XmlWriter writer) {
            if (config != null) {
                writer.WriteStartElement(elemName);
                writer.WriteAttributeString("type", config.DbType);
                writer.WriteAttributeString("host", config.Hostname);
                writer.WriteAttributeString("user", config.Username);
                writer.WriteAttributeString("password", config.Password);
                writer.WriteAttributeString("port", config.Port.ToString());
                writer.WriteAttributeString("database", config.DbName);
                writer.WriteEndElement();
            }
        }
        #endregion

        #region Open
        /// <summary>
        /// Opens the specified profile.
        /// </summary>
        /// <param name="profileName">Name of the profile.</param>
        /// <returns></returns>
        private static ProfileConfig Open(string profileName) {
            ProfileConfig profile = new ProfileConfig();
            ValidationSetup setup = profile.ValidationSetup;

            switch (ApplicationManager.ApplicationConfig.ConfigLocation) {
                case ConfigLocation.Directory:

                    string file = ProfileDirectory + "\\" + profileName + ".xml";

                    XmlDocument doc = new XmlDocument();
                    try {
                        doc.Load(file);
                    } catch (Exception ex) {
                        ProfileManager.DeleteProfile(profileName);
                        throw new Exception("Konnte das Profil " + profileName + " nicht laden. Profil wird gelöscht", ex);
                    }
                    XmlNode root = doc.DocumentElement;
                    foreach(XmlNode node in root.ChildNodes) {
                        switch (node.Name) {
                            case "Name":
                                profile.Name = node.InnerText.Trim();
                                break;
                            case "Description":
                                profile.Description = node.InnerText.Trim();
                                break;
                            case "ErrorLimit":
                                int errorLimit;
                                if (Int32.TryParse(node.InnerText, out errorLimit)) profile.ValidationSetup.ErrorLimit = errorLimit;
                                break;
                            case "DatabaseConfigValidation":
                                setup.DbConfigValidation = ReadDbConfig(node.Attributes);
                                break;
                            case "DatabaseConfigView":
                                setup.DbConfigView = ReadDbConfig(node.Attributes);
                                break;
                            case "TableMapping":
                                LoadTableMapping(node, setup);
                                break;
                            case "CustomRules":
                                ReadRules(profile.CustomRules, node);
                                break;

                        }
                    }

                    break;

                case ConfigLocation.Database:
                    throw new NotImplementedException();

                default:
                    throw new NotImplementedException();
            }

            var arr = profile.ValidationSetup.TableMappings.OrderBy(x => x.UniqueName).ToArray();
            profile.ValidationSetup.TableMappings.Clear();
            foreach (var item in arr) {
                profile.ValidationSetup.TableMappings.Add(item);
            }

            return profile;
        }

        private static void LoadTableMapping(XmlNode parentNode, ValidationSetup setup) {
            TableMapping tableMapping = new TableMapping();
            foreach (XmlNode node in parentNode.ChildNodes) {
                switch (node.Name) {
                    case "TableValidation":
                        tableMapping.TableValidation = ReadTable(node, setup.DbConfigValidation);
                        //tableMapping.TableValidation = new Table(node.Attributes["Name"].Value);
                        break;
                    case "TableView":
                        tableMapping.TableView = ReadTable(node, setup.DbConfigView);
                        break;
                    case "Used":
                        tableMapping.Used = node.InnerText == "True" ? true : false;
                        break;

                    //Old (version 1.0)
                    case "ColumnMapping":
                        tableMapping.ColumnMappings.Add(new ColumnMapping(node.Attributes["source"].Value, node.Attributes["destination"].Value));
                        break;

                    case "ColumnMappings":
                        ReadColumnMappings(tableMapping.ColumnMappings, node);
                        break;
                    case "Rules":
                        ReadRules(tableMapping.Rules, node);
                        break;
                    case "KeyEntryMappings":
                        ReadKeyEntryMappings(tableMapping.KeyEntryMappings, node);
                        break;
                }
            }
            setup.TableMappings.Add(tableMapping);
        }

        private static void ReadColumnMappings(ObservableCollection<ColumnMapping> columnMappings, XmlNode parentNode){
            foreach (XmlNode node in parentNode.ChildNodes){
                switch (node.Name){
                    case "ColumnMapping":
                        ColumnMapping columnMapping = GetColumnMapping(node);
                        if(columnMapping != null) columnMappings.Add(columnMapping);
                        break;
                }
            }
        }

        private static ColumnMapping GetColumnMapping(XmlNode parentNode){
            Column sourceCol = null, destCol = null;

            try {
                foreach (XmlNode node in parentNode.ChildNodes) {
                    
                    switch (node.Name) {
                        case "SourceCol":
                            //sourceCol = new Column(node.Attributes["Name"].Value);
                            sourceCol = ReadColumn(node);
                            break;
                        case "DestinationCol":
                            //destCol = new Column(node.Attributes["Name"].Value);
                            destCol = ReadColumn(node);
                            break;
                    }
                }
            } catch (Exception){
                Console.WriteLine("Could not read column mapping");
            }
            if (sourceCol != null && destCol != null) {
                ColumnMapping mapping = new ColumnMapping(sourceCol, destCol);
                if (parentNode.Attributes["IsVisible"] != null) {
                    mapping.IsVisible = Convert.ToBoolean(parentNode.Attributes["IsVisible"].Value);
                }
                return mapping;
            }

            return null;
        }

        private static Column ReadColumn(XmlNode parentNode) {
            //RuleSet rules = new RuleSet();
            Column result = new Column(parentNode.Attributes["Name"].Value);
            foreach (XmlNode node in parentNode.ChildNodes) {
                switch (node.Name) {
                    case "Rules":
                        ReadRules(result.Rules, node);
                        break;
                }
            }
            return result;
        }

        private static void ReadKeyEntryMappings(ObservableCollection<ColumnMapping> keyEntryMappings, XmlNode parentNode) {
            foreach (XmlNode node in parentNode.ChildNodes) {
                switch (node.Name) {
                    case "KeyEntryMapping":
                        ColumnMapping columnMapping = GetColumnMapping(node);
                        if (columnMapping != null)
                            keyEntryMappings.Add(columnMapping);
                        else{
                            //Old version (1.0)
                            try{
                                keyEntryMappings.Add(new ColumnMapping(node.Attributes["source"].Value,
                                                                       node.Attributes["destination"].Value));
                            }
                            catch (Exception){
                                Console.WriteLine("Could not load keyEntryMapping.");
                            }
                        }
                        break;
                }
            }
            
        }

        private static void ReadRules(RuleSet ruleSet, XmlNode parentNode) {
            foreach (XmlNode node in parentNode.ChildNodes) {
                switch (node.Name) {
                    case "Rule":
                        try {
                            //ruleSet.AddRule(RuleManager.Instance.RuleFromName(node.Attributes["Name"].Value));
                            ruleSet.AddRule(RuleFactory.RuleFromNode(node)); 
                        } catch (Exception) {

                            if (node.Attributes["Name"] != null)
                                Console.WriteLine("Could not load rule: " + node.Attributes["Name"].Value);
                            else
                                Console.WriteLine("Could not load rule, no name given.");
                        }
                        break;
                }
            }
        }

        private static Table ReadTable(XmlNode parentNode, DbConfig dbConfig) {
            Table table = new Table(parentNode.Attributes["Name"].Value, dbConfig);
            foreach (XmlNode node in parentNode.ChildNodes) {
                switch (node.Name) {
                    case "DbConfig":
                        table.DbConfig = ReadDbConfig(node.Attributes);
                        break;
                    case "Filter":
                        table.Filter = new Filter(node.InnerText);
                        break;
                }
            }
            return table;
        }

        private static DbConfig ReadDbConfig(XmlAttributeCollection xmlAttributeCollection) {
            DbConfig dbConfig = new DbConfig(xmlAttributeCollection["type"].Value);
            foreach (XmlAttribute attr in xmlAttributeCollection) {
                switch (attr.Name) {
                    case "host":
                        dbConfig.Hostname = attr.Value;
                        break;

                    case "user":
                        dbConfig.Username = attr.Value;
                        break;

                    case "port":
                        dbConfig.Port = Convert.ToInt32(attr.Value);
                        break;

                    case "password":
                        dbConfig.Password = attr.Value;
                        break;

                    case "database":
                        dbConfig.DbName = attr.Value;
                        break;
                }
            }
            return dbConfig;
        }
        #endregion

        #region DeleteProfile
        public static void DeleteProfile(string profileName) {
            bool found = false;
            foreach(var profile in Profiles)
                if (profile.Name == profileName) {
                    DeleteProfile(profile);
                    found = true;
                    break;
                }
            if (!found) {
                DeleteProfile(new ProfileConfig() { Name = profileName });
            }
        }
        public static void DeleteProfile(ProfileConfig profile) {
            string profileName = profile.Name;
            switch (ApplicationManager.ApplicationConfig.ConfigLocation) {
                case ConfigLocation.Directory:
                    string filename = ProfileDirectory + "\\" + profileName + ".xml";
                    if (File.Exists(filename)) File.Delete(filename);

                    //if (ProfileNames.Contains(profileName)) {
                    //    ProfileNames.Remove(profileName);
                    //}
                    Profiles.Remove(profile);

                    break;

                case ConfigLocation.Database:
                    throw new NotImplementedException();

                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        #region LoadProfiles
        public static void LoadProfiles() {

            //ProfileNames.Clear();
            Profiles.Clear();

            switch (ApplicationManager.ApplicationConfig.ConfigLocation) {
                case ConfigLocation.Directory:
                    if (Directory.Exists(ProfileDirectory)) {
                        foreach (FileInfo fi in new DirectoryInfo(ProfileDirectory).EnumerateFiles()) {
                            if (fi.Name.EndsWith(".xml")) {
                                string name = fi.Name.Substring(0, fi.Name.Length - 4);
                                //ProfileNames.Add(name);
                                Profiles.Add(Open(name));
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
        #endregion

        #region ProfileExists
        public static bool ProfileExists(string profileName) {
            foreach(var profile in Profiles)
                if (profile.Name.ToLower() == profileName.ToLower())
                    return true;
            return false;

        }
        #endregion

        #region AddProfile
        public static void AddProfile(ProfileConfig profile) {
            Profiles.Add(profile);
            //ProfileNames.Add(profile.Name);
            Save(profile);
        }
        #endregion
        #endregion methods


        public static void Init() {
            LoadProfiles();
        }

        public static ProfileConfig GetProfile(string name) {
            foreach (var profile in Profiles)
                if (profile.Name.ToLower() == name.ToLower())
                    return profile;
            return null;
        }
    }
}
