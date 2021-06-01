using System;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.Structures;

namespace DbSearchLogic.SearchCore.Events {
    public class SearchEventArgs : EventArgs {

        private TableResultSet _result;
        private QueryInfo _queryInfo;

        public SearchEventArgs(QueryInfo queryInfo) {
            _queryInfo = queryInfo;
        }

        public SearchEventArgs(QueryInfo queryInfo, TableResultSet oResult) {
            _result = oResult;
            _queryInfo = queryInfo;
        }

        public QueryInfo QueryInfo {
            get { return _queryInfo; }
        }

        public TableResultSet Result {
            get { return _result; }
        }

    }
}
