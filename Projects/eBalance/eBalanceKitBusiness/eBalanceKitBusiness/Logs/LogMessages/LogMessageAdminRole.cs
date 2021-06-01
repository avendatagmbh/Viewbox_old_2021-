// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-05-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Logs.Enums;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Logs.LogMessages {
    class LogMessageAdminRole : LogMessageAdmin{
        public LogMessageAdminRole(DbAdminLog dbAdminLog) : base(dbAdminLog) {
        }


        #region Overrides of LogMessageAdmin

        protected override string ContentMessage() {
            switch (DbAdminLog.ActionType) {
               case ActionTypes.Delete:
                    return string.Format(ResourcesLogging.RoleHasBeen, GetOldValue());
                case ActionTypes.New:
                    return string.Format(ResourcesLogging.RoleHasBeen, GetNewValue());
                default:
                    return string.Format(ResourcesLogging.RoleHasBeen,
                                         GetNewValue() + " (" + GetOldValue() + ")");
            }
        }

        protected override string Name() { return ResourcesLogging.RoleName; }

        protected override string ActionMessage() {
            string actionType = null;
            switch (DbAdminLog.ActionType) {
                case ActionTypes.New:
                    actionType = " " + ResourcesLogging.added.ToLower() + ".";
                    break;
                case ActionTypes.Delete:
                    actionType = " " + ResourcesLogging.removed.ToLower() + ".";
                    break;
                case ActionTypes.Change:
                    actionType = " " + ResourcesLogging.changed.ToLower() + ".";
                    break;
                case ActionTypes.Used:
                    actionType = " " + ResourcesLogging.used.ToLower() + ".";
                    break;
            }
            return actionType;
        }

        #endregion

        //private ChangedObject GetChangedObject() {
        //    var commentOld = LogEntryBase.GetAttribute(DbAdminLog.OldValue, "Comment");
        //    var commentNew = LogEntryBase.GetAttribute(DbAdminLog.NewValue, "Comment");
        //    var nameOld = LogEntryBase.GetAttribute(DbAdminLog.OldValue, "Name");
        //    var nameNew = LogEntryBase.GetAttribute(DbAdminLog.NewValue, "Name");

        //    if (nameNew.Equals(nameOld)) {
        //        return ChangedObject.Comment; //ResourcesCommon.Comment;
        //    }
        //    if (commentNew.Equals(commentOld)) {
        //        return ChangedObject.Name; //ResourcesCommon.Name;
        //    }
        //    Debug.Assert(!nameNew.Equals(nameOld) || !commentNew.Equals(commentOld));
        //    return ChangedObject.Unknown; //string.Empty;
        //}

        //enum ChangedObject {
        //    Comment,
        //    Name,
        //    Unknown
        //}

        private string GetLogValue (string oldNewValue) {
            XmlReader xmlReader = new XmlTextReader(new StringReader(oldNewValue));
            xmlReader.Read();
            StringBuilder stringBuilder = new StringBuilder();
            while (xmlReader.MoveToNextAttribute()) {
                stringBuilder.Append(xmlReader.Name);
                stringBuilder.Append(" : \"");
                stringBuilder.Append(xmlReader.Value);
                stringBuilder.Append("\" ");
            }
            return stringBuilder.ToString();
        }

        public override string GetOldValue() {
            if (DbAdminLog.ActionType != ActionTypes.Change) {
                return base.GetOldValue();
            }
            return DbAdminLog.OldValue == null ? string.Empty : GetLogValue(DbAdminLog.OldValue);
        }

        public override string GetNewValue() {
            if (DbAdminLog.ActionType != ActionTypes.Change) {
                return base.GetOldValue();
            }
            return DbAdminLog.NewValue == null ? string.Empty : GetLogValue(DbAdminLog.NewValue);
        }
    }
}