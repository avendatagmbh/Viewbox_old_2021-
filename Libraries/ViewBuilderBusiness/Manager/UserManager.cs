using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using DbAccess;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilderBusiness.Manager
{
    /// <summary>
    ///   This class provides some user management functions.
    /// </summary>
    public static class UserManager
    {
        /// <summary>
        ///   Initializes the <see cref="UserManager" /> class.
        /// </summary>
        static UserManager()
        {
            Users = new ObservableCollection<UserConfig>();
            _userByName = new Dictionary<string, UserConfig>(StringComparer.OrdinalIgnoreCase);
        }

        #region fields

        /// <summary>
        ///   Dictionary of users.
        /// </summary>
        private static readonly Dictionary<string, UserConfig> _userByName;

        #endregion member varaiables

        #region properties

        /// <summary>
        ///   Gets or sets the users.
        /// </summary>
        /// <value> The users. </value>
        public static ObservableCollection<UserConfig> Users { get; private set; }

        #endregion properties

        #region methods

        /// <summary>
        ///   Adds the specified user.
        /// </summary>
        /// <param name="user"> The user. </param>
        public static void AddUser(UserConfig user)
        {
            if (!_userByName.ContainsKey(user.Name))
            {
                _userByName.Add(user.Name, user);
                Users.Add(user);
            }
            Save();
        }

        /// <summary>
        ///   Deletes the specified user.
        /// </summary>
        /// <param name="user"> The user. </param>
        public static void DeleteUser(UserConfig user)
        {
            if (_userByName.ContainsKey(user.Name))
            {
                _userByName.Remove(user.Name);
                Users.Remove(user);
            }
            Save();
        }

        /// <summary>
        ///   Updates the specified user.
        /// </summary>
        /// <param name="user"> The user. </param>
        public static void UpdateUser(UserConfig user)
        {
            Save();
        }

        /// <summary>
        ///   Existses the specified user name.
        /// </summary>
        /// <param name="userName"> Name of the user. </param>
        /// <returns> </returns>
        public static bool Exists(string userName)
        {
            return _userByName.ContainsKey(userName);
        }

        /// <summary>
        ///   Loads the user config.
        /// </summary>
        public static void Load()
        {
            try
            {
                Users.Clear();
                _userByName.Clear();
                switch (ApplicationManager.ApplicationConfig.ConfigLocation)
                {
                    case ConfigLocation.Directory:
                        string filename = ApplicationManager.ApplicationConfig.ConfigDirectory + "\\" +
                                          UserConfig.ELEMENT_NAME + ".xml";
                        if (!File.Exists(filename)) return;
                        XmlDocument doc = new XmlDocument();
                        doc.Load(filename);
                        XmlNode root = doc.DocumentElement;
                        foreach (XmlNode node in root.ChildNodes)
                        {
                            switch (node.Name)
                            {
                                case "user":
                                    AddUser(new UserConfig(node));
                                    break;
                            }
                        }
                        break;
                    case ConfigLocation.Database:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Lesen der Benutzerkonfiguration: " + ex.Message);
            }
        }

        /// <summary>
        ///   Saves the user config.
        /// </summary>
        public static void Save()
        {
            try
            {
                switch (ApplicationManager.ApplicationConfig.ConfigLocation)
                {
                    case ConfigLocation.Directory:
                        XmlTextWriter writer = null;
                        try
                        {
                            // save xml file
                            writer = new XmlTextWriter(
                                ApplicationManager.ApplicationConfig.ConfigDirectory + "\\" + UserConfig.ELEMENT_NAME +
                                ".xml", Encoding.UTF8);
                            writer.Formatting = Formatting.Indented;
                            writer.Indentation = 4;
                            writer.WriteStartDocument();
                            writer.WriteStartElement("ViewBuilderUserConfig");
                            foreach (UserConfig user in Users)
                            {
                                user.Save(writer, "user");
                            }
                            writer.WriteEndDocument();
                            writer.Close();
                        }
                        finally
                        {
                            if (writer != null) writer.Close();
                        }
                        break;
                    case ConfigLocation.Database:
                        using (
                            IDatabase db =
                                ConnectionManager.CreateConnection(ApplicationManager.ApplicationConfig.ConfigDbConfig))
                        {
                            db.Open();
                            db.Close();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Speichern der Benutzerkonfiguration: " + ex.Message);
            }
        }

        #endregion methods

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/
    }
}