using System;

namespace DbSearchLogic.SearchCore.QueryCache {

    internal class DbCacheLineIdEntry<T> : IComparable<DbCacheLineIdEntry<T>> where T : System.IComparable<T> {

        public DbCacheLineIdEntry(UInt32 id, T value) {
            this.Id = id;
            this.Value = value;
        }
        
        public UInt32 Id { get; set; }
        public T Value { get; set; }

        public int CompareTo(DbCacheLineIdEntry<T> other) {
            return this.Value.CompareTo(other.Value);
        }
    }
}
