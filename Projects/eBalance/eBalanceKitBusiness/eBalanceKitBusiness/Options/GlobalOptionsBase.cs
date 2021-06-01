// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-04-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Xml;
using DbAccess;
using Utils;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Options {
    internal class GlobalOptionsBase : NotifyPropertyChangedBase {

        #region Properties 

        #region Id
        private int _id;

        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id {
            get { return _id; }
            set {
                if (_id == value)
                    return;
                _id = value;
                OnPropertyChanged("Id");
            }
        }
        #endregion

        #region User
        private int _userId;
        private User _user;

        [DbColumn("user_id")]
        public int UserId {
            get { return _userId; }
            set {
                if (_userId == value)
                    return;
                _userId = value;
                Save();
            }
        }

        public User User {
            get {
                //if (_user == null) {
                //    using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                //        var users = conn.DbMapping.Load<User>("id = " + _userId);
                //        _user = users.Count == 1 ? users[0] : null;
                //    }
                //}
                return _user;
            }
            set {
                if (_user == value)
                    return;
                _user = value;
                UserId = value.Id;
            }
        }

        #endregion User

        public bool DoDbUpdate { get; set; }

        private string _xmlContent;

        /// <summary>
        /// The database column that contains the configuration for the current User.
        /// </summary>
        [DbColumn("option", Length = int.MaxValue)]
        public string XmlContent {
            get {
                if (string.IsNullOrEmpty(_xmlContent)) {
                    _xmlContent = GetDefaultXml();
                }
                return _xmlContent;
            }
            set {
                if (_xmlContent == value)
                    return;
                _xmlContent = value;
                if (!string.IsNullOrEmpty(value)) {
                    XmlDocument.InnerXml = value;
                }
                OnPropertyChanged("XmlContent");
                Save();
            }
        }

        /// <summary>
        /// An XmlDocument representation of the <see cref="XmlContent"/>.
        /// </summary>
        protected XmlDocument XmlDocument {
            get { return _xmlDocument; }
            set {
                _xmlDocument = value;
                if (_xmlContent != value.InnerXml) {
                    XmlContent = value.InnerXml;
                }
            }
        }

        /// <summary>
        /// An XmlDocument representation of the <see cref="XmlContent"/>.
        /// </summary>
        private XmlDocument _xmlDocument;

        #endregion Properties


        /// <summary>
        /// Returns the default configuraion that is hard coded.
        /// </summary>
        /// <returns></returns>
        protected string GetDefaultXml() {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<c>");
            stringBuilder.Append("<ShowSelectedLegalForm>");
            stringBuilder.Append(true);
            stringBuilder.Append("</ShowSelectedLegalForm>");
            stringBuilder.Append("<HideChosenWarnings>");
            stringBuilder.Append(false);
            stringBuilder.Append("</HideChosenWarnings>");
            stringBuilder.Append("<HideAllWarnings>");
            stringBuilder.Append(false);
            stringBuilder.Append("</HideAllWarnings>");
            stringBuilder.Append("<ShowTypeOperatingResult>");
            stringBuilder.Append(true);
            stringBuilder.Append("</ShowTypeOperatingResult>");
            stringBuilder.Append("</c>");

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns the value of the specified tag casted as Boolean.
        /// </summary>
        /// <param name="tagName">The tagName that should be equal to the property name.</param>
        /// <returns>The casted bool value or null if cast was not possible.</returns>
        public bool? GetBoolValue(string tagName) {
            bool b;
            if (bool.TryParse(GetValue(tagName), out b))
                return b;

            return null;
        }

        private string GetValue(string tagName) {
            var node = GetNode(tagName);
            return node == null ? null : node.InnerText;
        }

        /// <summary>
        /// Returns the first XmlNode from the <see cref="_xmlDocument"/> that has the TagName that was specified as parameter.
        /// </summary>
        /// <param name="tagName">The TagName (Property name) that identifies the XmlNode</param>
        /// <returns>The XmlNode</returns>
        private XmlNode GetNode(string tagName) {
            
            if (string.IsNullOrEmpty(XmlDocument.InnerXml)) {
                XmlDocument.LoadXml(XmlContent);
            }
            
            var nodes = XmlDocument.GetElementsByTagName(tagName);

            //Debug.Assert(nodes.Count == 1, "Property " + tagName + " not or mode than once specified");

            return nodes.Count > 0 ? nodes[0] : null;
        }

        /// <summary>
        /// Set the Value of the Node.
        /// </summary>
        /// <param name="tagName">The TagName of the XmlNode the <see cref="value"/> will be assigned.</param>
        /// <param name="value">The value that will be assigned as ToString().</param>
        /// <returns>Was it an existing Node the value was assigned to?</returns>
        public bool SetValue(string tagName, object value) {
            var node = GetNode(tagName);
            if (node != null) {
                node.InnerText = value.ToString();
                Save();
                return true;
            }
            else {
                node = XmlDocument.CreateElement(tagName);
                node.InnerText = value.ToString();
                XmlDocument.DocumentElement.AppendChild(node);
                //XmlDocument.ImportNode(node, false);
                Save();
                return false;
            }
        }


        /// <summary>
        /// Save the current configured options in the database if the flag DoDbUpdate is true.
        /// </summary>
        public void Save() {
            if (!DoDbUpdate) return;
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                XmlContent = XmlDocument.InnerXml;
                 conn.DbMapping.Save(this);
            }
        }

        /// <summary>
        /// Save the current configured options in the database.
        /// </summary>
        /// <returns>Was there a change between current options and the stored options?</returns>
        public bool SaveConfiguration() {
            if (XmlContent.Equals(XmlDocument.InnerXml)) {
                return false;
            }
            
            DoDbUpdate = true;
            Save();
            DoDbUpdate = false;
            return true;
        }

        /// <summary>
        /// Resets the options by reseting the XmlDocument.InnerXml.
        /// </summary>
        public virtual void Reset() {
            XmlDocument.InnerXml = XmlContent;
        }

    }
}