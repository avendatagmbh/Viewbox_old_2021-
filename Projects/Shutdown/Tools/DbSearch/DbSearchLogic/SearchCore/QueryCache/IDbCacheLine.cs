using System;

namespace DbSearchLogic.SearchCore.QueryCache {
    interface IDbCacheLine {

        uint Count { get; }

        void FinializeInsertion();

        void Clear();

        object GetValue(int index);

        int Find(object value);

        UInt32 GetId(int index);
    }
}
