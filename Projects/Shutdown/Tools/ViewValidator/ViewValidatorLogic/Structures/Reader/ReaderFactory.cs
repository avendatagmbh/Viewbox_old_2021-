using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using ViewValidatorLogic.Logic;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidatorLogic.Structures.Reader {
    internal static class ReaderFactory {
        public static IDbReader AcquireReader(SortMethod sortMethod, IDatabase conn, int which, TableMapping tableMapping) {
            IDbReader result = null;
            if (sortMethod == SortMethod.Database) {
                result = new RowWiseReader(conn, which, tableMapping);
            } else if (sortMethod == SortMethod.Memory) {
                result = new CachedReader(conn, which, tableMapping);
            }

            return result;
        }
    }
}
