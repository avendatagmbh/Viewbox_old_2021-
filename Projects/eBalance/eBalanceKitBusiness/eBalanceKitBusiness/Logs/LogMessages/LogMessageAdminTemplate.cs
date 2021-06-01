using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Logs.LogMessages {
    class LogMessageAdminTemplate   : LogMessageAdmin{
        public LogMessageAdminTemplate(DbAdminLog dbAdminLog)
            : base(dbAdminLog) {

        }

        public override string GetOldValue() { return string.Empty; }

        public override string GetNewValue() { return string.Empty; }

        protected override string ContentMessage() {
            return ResourcesLogging.TemplateHasBeen;
        }

        protected override string Name() { return ResourcesCommon.BtnTemplates; }
    }
}
