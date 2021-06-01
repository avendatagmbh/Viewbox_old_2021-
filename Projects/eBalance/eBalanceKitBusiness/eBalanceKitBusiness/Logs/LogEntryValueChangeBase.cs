using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness.Logs {
    public abstract class LogEntryValueChangeBase : LogEntryBase {

        public abstract string GetOldValue { get; }

        public abstract string GetNewValue { get; }

    }
}
