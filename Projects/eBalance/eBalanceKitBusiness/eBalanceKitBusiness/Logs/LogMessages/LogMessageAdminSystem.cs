using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Logs.Enums;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Logs.LogMessages {
    class LogMessageAdminSystem : LogMessageAdmin{
        public LogMessageAdminSystem(DbAdminLog dbAdminLog)
            : base(dbAdminLog) {

        }

        protected override string ContentMessage() {
            if (DbAdminLog.ActionType == ActionTypes.Delete) {
                string name = LogEntryBase.GetAttribute(DbAdminLog.Info, "Name");
                return string.Format(ResourcesLogging.SystemHasBeen, name);
            }
            if (DbAdminLog.ActionType == ActionTypes.Change) {
                Structures.DbMapping.System system = SystemManager.Instance.GetSystem((int)DbAdminLog.ReferenceId);
                string systemString = system == null ? string.Empty : system.Name;
                string type = LogEntryBase.GetAttribute(DbAdminLog.Info, "Type");
                return string.Format(ResourcesLogging.FieldOfSystemHasBeen, TranslateField(type), systemString);
            }
            return ResourcesLogging.SystemUnknownHasBeen;
        }

        protected override string Name() { return ResourcesMain.System; }
    }
}
