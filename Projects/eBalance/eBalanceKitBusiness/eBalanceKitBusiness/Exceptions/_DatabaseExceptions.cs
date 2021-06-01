using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness.Exceptions {

    public class DatabaseOutOfDateException : Exception { public DatabaseOutOfDateException(string message) : base(message) { } }
    public class ProgramOutOfDateException : Exception { public ProgramOutOfDateException(string message) : base(message) { } }
}
