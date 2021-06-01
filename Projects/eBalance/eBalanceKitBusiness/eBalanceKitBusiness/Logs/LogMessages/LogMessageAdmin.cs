// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-07-07
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Logs.Enums;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Logs.LogMessages {
    internal abstract class LogMessageAdmin {
        protected LogMessageAdmin(DbAdminLog dbAdminLog) { DbAdminLog = dbAdminLog; }
        protected DbAdminLog DbAdminLog { get; set; }

        public virtual string GetMessage() { return Name() + "-" + ResourcesLogging.Change + ": " + ContentMessage() + ActionMessage(); }

        protected virtual string ContentMessage() { return string.Empty; }


        protected virtual string TranslateField(string type) {
            switch (type) {
                case "Comment":
                    return ResourcesCommon.Comment;
                default:
                    return type;
            }
        }

        protected virtual string ActionMessage() {
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

        protected abstract string Name();

        public virtual string GetOldValue() { return DbAdminLog.OldValue ?? string.Empty; }

        public virtual string GetNewValue() { return DbAdminLog.NewValue ?? string.Empty; }
    }
}