using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchBase.Interfaces {
    public interface IIdxIdEnumerator : IDisposable {

        bool GetIdxIds(out Dictionary<int, int> idxIds);
    }
}
