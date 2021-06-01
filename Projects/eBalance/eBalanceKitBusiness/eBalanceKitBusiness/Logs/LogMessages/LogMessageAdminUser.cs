using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Logs.Enums;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Logs.LogMessages {
    class LogMessageAdminUser   : LogMessageAdmin{
        public LogMessageAdminUser(DbAdminLog dbAdminLog)
            : base(dbAdminLog) {

        }

        protected override string TranslateField(string type) {
            switch (type) {
                case "EncryptedKey":
                case "PasswordHash":
                    return ResourcesCommon.Password;
                case "FullName":
                    return ResourcesCommon.Username;
                case "UserName":
                    return ResourcesCommon.UserShortname;
                case "IsAdmin":
                    return ResourcesCommon.Administrator;
                case "IsActive":
                    return ResourcesCommon.Active;
                case "AssignedCompanies":
                    return ResourcesCommon.AssignedCompanies;
                case "IsCompanyAdmin":
                    return ResourcesCommon.CompanyAdministrator;
                default:
                    return base.TranslateField(type);
            }
        }


        protected override string ContentMessage() {
            try {
                User user = UserManager.Instance.GetUser((int)DbAdminLog.ReferenceId);
                string userString = user == null ? string.Empty : user.DisplayString;

                if (DbAdminLog.ActionType == ActionTypes.Delete ||
                    DbAdminLog.ActionType == ActionTypes.New) {
                    return string.Format(ResourcesLogging.UserHasBeen, userString);
                }
                if (DbAdminLog.ActionType == ActionTypes.Change) {
                    string type = LogEntryBase.GetAttribute(DbAdminLog.Info, "Type");
                    return string.Format(ResourcesLogging.FieldOfUserHasBeen, TranslateField(type), userString);
                }
            } catch (Exception) { }
            return ResourcesLogging.UserUnknownHasBeen;
        }

        protected override string Name() { return ResourcesCommon.Users; }

        public override string GetOldValue(){
            if (DbAdminLog.Info == null) {
                return base.GetOldValue();
            }

            // hide the passwords
            if (LogEntryBase.GetAttribute(DbAdminLog.Info, "Type") == "EncryptedKey" || LogEntryBase.GetAttribute(DbAdminLog.Info, "Type") == "PasswordHash") {
                return string.Empty;
            }

            // show not the values of the assigned companies id, but the display string
            if (LogEntryBase.GetAttribute(DbAdminLog.Info, "Type") == "AssignedCompanies") {
                return DbAdminLog.OldValue == null ? string.Empty : MakeCompanyDisplayStringsFromValue(DbAdminLog.OldValue);
            }
            return base.GetOldValue();
        }

        /// <summary>
        /// Apply an XmlReader to the given string and join the inner values of Companies.
        /// These values are company ids. Their display name are joined.
        /// </summary>
        /// <param name="oldNewValue">xml string</param>
        /// <returns>The joined display strings</returns>
        private string MakeCompanyDisplayStringsFromValue(string oldNewValue) {
            using (XmlReader reader = new XmlTextReader(new StringReader(oldNewValue))) {
                reader.MoveToContent();
                StringBuilder stringBuilder = new StringBuilder();
                reader.Read();
                while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != XmlNodeType.None) {
                    stringBuilder.Append(CompanyManager.Instance.GetCompany(reader.ReadElementContentAsInt()).DisplayString);
                    stringBuilder.Append(" ");
                }
                return stringBuilder.ToString();
            }
        }

        public override string GetNewValue() {
            if (DbAdminLog.Info == null) {
                return base.GetOldValue();
            }

            // hide the passwords
            if (LogEntryBase.GetAttribute(DbAdminLog.Info, "Type") == "EncryptedKey" || LogEntryBase.GetAttribute(DbAdminLog.Info, "Type") == "PasswordHash") {
                return string.Empty;
            }

            // show not the values of the assigned companies id, but the display string
            if (LogEntryBase.GetAttribute(DbAdminLog.Info, "Type") == "AssignedCompanies") {
                return DbAdminLog.NewValue == null ? string.Empty : MakeCompanyDisplayStringsFromValue(DbAdminLog.NewValue);
            }
            return base.GetNewValue();
        }
    }
}
