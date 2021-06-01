using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DbAccess.Structures;

namespace DbSearchBase.Enums
{
    public class BindableDbColumnType {

        public DbColumnTypes DbType { get; internal set; }
        public bool IsSelected { get; set; }

        public BindableDbColumnType(DbColumnTypes dbType, bool isSelected = true) { 
            DbType = dbType;
            IsSelected = isSelected;
        }
    }

    public sealed class DbColumnTypeHelper {

        DbColumnTypeHelper() { }
        public static DbColumnTypeHelper Instance { get { return Nested.instance; } }

        private class Nested {
            static Nested() { }
            internal static readonly DbColumnTypeHelper instance = new DbColumnTypeHelper();
        }

        private readonly SemaphoreSlim _listSemaphore = new SemaphoreSlim(1);
        private List<BindableDbColumnType> _bindableDbColumnTypeList;

        public List<BindableDbColumnType> BindableDbColumnTypeList() {
            if (_bindableDbColumnTypeList != null) return _bindableDbColumnTypeList;
            _listSemaphore.Wait();
            try {
                if (_bindableDbColumnTypeList == null) {
                    _bindableDbColumnTypeList = new List<BindableDbColumnType>();
                    IEnumerable<DbColumnTypes> dbColumnTypes = Enum.GetValues(typeof (DbColumnTypes)).OfType<DbColumnTypes>();
                    foreach (DbColumnTypes dbColumnType in dbColumnTypes) {
                        _bindableDbColumnTypeList.Add(new BindableDbColumnType(dbColumnType));
                    }
                }
            } finally {
                _listSemaphore.Release();
            }
            return _bindableDbColumnTypeList;
        }
    }
}
