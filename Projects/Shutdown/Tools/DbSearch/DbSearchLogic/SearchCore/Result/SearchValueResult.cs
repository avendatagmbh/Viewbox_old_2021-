using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvdCommon.Logging;
using DbSearchLogic.SearchCore.SearchMatrix;

namespace DbSearchLogic.SearchCore.Result {
    public class SearchValueResultEnumerator : IEnumerator<SearchValueResultEntry> {
        public SearchValueResultEnumerator(SearchValueResult svr) {
            _ids = svr.Ids.ToArray();
            _types = svr.Types.ToArray();
            _length = svr.Types.Count;
        }

        private byte[] _ids;
        private byte[] _types;
        private int _currentIndex = -1;
        private int _length;

        public void Dispose() {
            _ids = null;
            _types = null;
        }

        public bool MoveNext() {
            return ++_currentIndex < _length;
        }

        public void Reset() {
            throw new NotImplementedException();
        }

        public SearchValueResultEntry Current {
            get { 
                return new SearchValueResultEntry(BitConverter.ToUInt32(_ids,_currentIndex*4),
                (SearchValueResultEntryType)Enum.Parse(typeof(SearchValueResultEntryType),_types[_currentIndex].ToString())); 
            }
        }

        object IEnumerator.Current {
            get { return Current; }
        }
    }

    public class SearchValueResult : IEnumerable<SearchValueResultEntry> {
        #region Constructor
        public SearchValueResult(SearchValue searchValue) {
            //Hits = new List<SearchValueResultEntry>();
            SearchValue = searchValue;
            Ids = new List<byte>();
            Types = new List<byte>();
            DbIds = new List<long>();
        }


        #endregion

        #region Properties
        //public List<SearchValueResultEntry> Hits { get; private set; }
        //public long DbId { get; set; }
        public List<long> DbIds { get; private set; }
        public bool HasResults { get { return Hits > 0; } }
        public SearchValue SearchValue { get; set; }
        internal List<byte> Ids { get; set; }
        internal List<byte> Types { get; set; }
        //This greatly influences the memory consumption during search
        private const int MaxDistinctTableIds = 15;
        private const int MaxDistinctTableIdsForExactHits = 15;
        //private const int MaxDistinctTableIds = 1000000;
        //private const int MaxDistinctTableIdsForExactHits = 1000000;
        
        private int _hits = 0;
        private int _exactHits = 0;
        public int Hits { get { return Types.Count; } }
        #endregion Properties

        public void AddHit(uint id, SearchValueResultEntryType type = SearchValueResultEntryType.Exact) {
            if (_hits < MaxDistinctTableIds || (type == SearchValueResultEntryType.Exact && _exactHits < MaxDistinctTableIdsForExactHits)) {
                Ids.AddRange(BitConverter.GetBytes(id));
                Types.Add((byte) type);
            }
            //else if (_hits == MaxDistinctTableIds) {
            //    //LogManager.Warning("Es wurden schon mehr als " + MaxDistinctTableIds + " Treffer für den Suchwert \"" + SearchValue.String + "\" gefunden.");
            //}
            //if (_exactHits == MaxDistinctTableIdsForExactHits) {
            //    //LogManager.Warning("Es wurden schon mehr als " + MaxDistinctTableIds + " Treffer für den Suchwert \"" + SearchValue.String + "\" gefunden.");
            //}
            _exactHits += type == SearchValueResultEntryType.Exact ? 1 : 0;
            ++_hits;
        }

        public void AddHits(List<SearchValueResultEntry> hits) {
            foreach(var hit in hits)
                AddHit(hit.Id, hit.Type);
        }

        #region Implementation of IEnumerable

        public IEnumerator<SearchValueResultEntry> GetEnumerator() {
            return new SearchValueResultEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Clear() {
            Ids.Clear();
            Ids = new List<byte>();
            Types.Clear();
            Types = new List<byte>();
            _hits = 0;
        }

        public override string ToString() {
            return "Hits: " + Hits;
        }
        #endregion
    }
}
