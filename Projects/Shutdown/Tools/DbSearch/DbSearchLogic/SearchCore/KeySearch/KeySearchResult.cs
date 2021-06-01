using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchLogic.SearchCore.KeySearch
{
    public enum KeySearchResult {
        NotStarted = 0,
        Finished = 1,
        FinishedAlreadyRun = 2,
        Canceled = 3,
        Running = 4,
        IdxDbMissing = 5,
    }
}
