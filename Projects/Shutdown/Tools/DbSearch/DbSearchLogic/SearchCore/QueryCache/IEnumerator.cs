using System;

namespace DbSearchLogic.SearchCore.QueryCache {

    internal interface IDbColumnEnumerator {

        bool Next();

        void Close(); 
        
        object GetValue();

        UInt32 GetId();
        
    }
}
