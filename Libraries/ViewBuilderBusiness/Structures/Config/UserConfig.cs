using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using DbAccess;

namespace ViewBuilderBusiness.Structures.Config
{
    /// <summary>
    ///   Config class for user configuration.
    /// </summary>
    public class UserConfig : INotifyPropertyChanged
    {
        internal static string ELEMENT_NAME = "users";
        internal static string FIELDNAME_NAME = "name";
        internal static string FIELDNAME_FULL_NAME = "fullName";
        internal static string FIELDNAME_PASSWORD_HASH = "passwordHash";

        /// <summary>
        ///   Initializes a new instance of the <see cref="UserConfig" /> class.
        /// </summary>
        public UserConfig()
        {
            Name = string.Empty;
            FullName = string.Empty;
            PasswordHash = string.Empty;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="UserConfig" /> class.
        /// </summary>
        /// <param name="configNode"> The config node. </param>
        public UserConfig(XmlNode configNode)
        {
            Read(configNode);
        }

        #region events

        /// <summary>
        ///   Occurs when a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion events

        #region eventTrigger

        /// <summary>
        ///   Called when a property changed.
        /// </summary>
        /// <param name="property"> The property. </param>
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion eventTrigger

        #region fields

        /// <summary>
        ///   See property EMail.
        /// </summary>
        private string _email;

        /// <summary>
        ///   See property FullName.
        /// </summary>
        private string _fullName;

        /// <summary>
        ///   See property Name.
        /// </summary>
        private string _name;

        #endregion fields

        #region properties

        /// <summary>
        ///   Gets or sets the user name.
        /// </summary>
        /// <value> The user name. </value>
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                    OnPropertyChanged("DisplayString");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the user name.
        /// </summary>
        /// <value> The user name. </value>
        public string FullName
        {
            get { return _fullName; }
            set
            {
                if (_fullName != value)
                {
                    _fullName = value;
                    OnPropertyChanged("FullName");
                    OnPropertyChanged("DisplayString");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the E mail adress.
        /// </summary>
        /// <value> The E mail. </value>
        public string EMail
        {
            get { return _email; }
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged("EMail");
                    OnPropertyChanged("DisplayString");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the password hash.
        /// </summary>
        /// <value> The password hash. </value>
        public string PasswordHash { get; set; }

        /// <summary>
        ///   Gets the display string.
        /// </summary>
        /// <value> The display string. </value>
        public string DisplayString
        {
            get
            {
                string ds = FullName;
                if (ds.Length == 0)
                {
                    ds = Name;
                }
                else
                {
                    ds += " (" + Name + ")";
                }
                //ds += " / E-Mail: " + this.EMail;
                return ds;
            }
        }

        #endregion properties

        #region persist database

        /// <summary>
        ///   Creates the table.
        /// </summary>
        /// <param name="db"> Open database connection. </param>
        internal static void CreateTable(IDatabase db)
        {
            db.ExecuteNonQuery(
                "CRATE TABLE IF NOT EXISTS " + db.Enquote(ELEMENT_NAME) + "(" +
                db.Enquote(FIELDNAME_NAME) + " VARCHAR(20)," +
                db.Enquote(FIELDNAME_FULL_NAME) + " VARCHAR(64)," +
                db.Enquote(FIELDNAME_PASSWORD_HASH) + " VARCHAR(512)," +
                ") ENGINE = MyISAM"
                );
        }

        /// <summary>
        ///   Saves this instance to the specified db.
        /// </summary>
        /// <param name="db" Open database connection. </param>
        internal void Save(IDatabase db)
        {
        }

        /// <summary>
        ///   Reads this instance form the specified db.
        /// </summary>
        /// <param name="db"> Open database connection. </param>
        internal void Read(IDatabase db)
        {
        }

        /// <summary>
        ///   Deletes this instance from specified db.
        /// </summary>
        /// <param name="db"> Open database connection. </param>
        internal void Delete(IDatabase db)
        {
            db.ExecuteNonQuery(
                "DELETE FROM " + db.Enquote(ELEMENT_NAME) +
                " WHERE " + db.Enquote(FIELDNAME_NAME) + "='" + Name + "'"
                );
        }

        #endregion persist database

        #region persist xml

        /// <summary>
        ///   Writes this instance to the specified XmlTextWriter.
        /// </summary>
        /// <param name="writer"> The writer. </param>
        internal void Save(XmlTextWriter writer, string nodeName)
        {
            writer.WriteStartElement(nodeName);
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("fullName", FullName);
            writer.WriteAttributeString("email", EMail);
            writer.WriteAttributeString("password", PasswordHash);
            writer.WriteEndElement();
        }

        /// <summary>
        ///   Reads this instance from the specified xml node.
        /// </summary>
        /// <param name="configNode"> The config node. </param>
        internal void Read(XmlNode configNode)
        {
            foreach (XmlAttribute attr in configNode.Attributes)
            {
                switch (attr.Name)
                {
                    case "name":
                        Name = attr.Value;
                        break;
                    case "fullName":
                        FullName = attr.Value;
                        break;
                    case "email":
                        EMail = attr.Value;
                        break;

                    case "password":
                        PasswordHash = attr.Value;
                        break;
                }
            }
        }

        #endregion persist xml

        #region passwordHash

        /// <summary>
        ///   Checks the password.
        /// </summary>
        /// <param name="password"> The password. </param>
        /// <returns> </returns>
        public bool CheckPassword(string password)
        {
            string passwordHash = ComputePasswordHash(password, EMail);
            return PasswordHash.Equals(passwordHash);
        }

        /// <summary>
        ///   Sets the password.
        /// </summary>
        /// <param name="password"> The password. </param>
        public void SetPassword(string password)
        {
            PasswordHash = ComputePasswordHash(password, EMail);
        }

        /// <summary>
        ///   Computes this instance.
        /// </summary>
        /// <param name="password"> The password. </param>
        /// <param name="salt"> The salt. </param>
        /// <returns> </returns>
        private string ComputePasswordHash(string password, string salt)
        {
            return ByteArrayToString(
                new SHA256CryptoServiceProvider().ComputeHash(
                    Encoding.ASCII.GetBytes(password + salt)));
        }

        /// <summary>
        ///   Bytes the array to string.
        /// </summary>
        /// <param name="arrInput"> The arr input. </param>
        /// <returns> </returns>
        private static string ByteArrayToString(byte[] arrInput)
        {
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (int i = 0; i < arrInput.Length - 1; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }

        #endregion passwordHash

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/
    }
}