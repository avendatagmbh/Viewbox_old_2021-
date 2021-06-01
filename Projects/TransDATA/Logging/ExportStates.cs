using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logging {
    public enum ExportStates {
        None = 0,
        InProgress = 1,
        Ok = 2,
        Error = 3,
        NoColumns = 4,
    }
}
