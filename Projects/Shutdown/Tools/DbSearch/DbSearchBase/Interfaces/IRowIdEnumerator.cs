using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchBase.Interfaces {
    public interface IRowIdEnumerator : IDisposable {

        bool GetRowIds(out Dictionary<int, List<int>> rowIds);
    }
}
