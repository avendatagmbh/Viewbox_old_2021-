using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DbAccess;
using DbAccess.Structures;
using System.Threading;
using DbSearchBase.Interfaces;
using DbSearchDatabase.DistinctDb;
using log4net;
using AV.Log;

namespace DbSearchLogic.SearchCore.KeySearch {
    public class RowIdEnumerator : BinaryRowIdEnumerator, IRowIdEnumerator {

        #region [ Constants ]

        internal ILog _log = LogHelper.GetLogger();

        internal const string sqlSelectFromIdx = "SELECT rowIds, id, hasRowNoTable FROM {0} WHERE LENGTH(rowIds) > 4";

        #endregion [ Constants ]

        #region [ Private members ]
        
        private int _lastIdxRowId = 0;

        #endregion [ Private members ]

        #region [ Constructor ]

        public RowIdEnumerator(DbConfig dbConfigIdx, KeyCandidateColumn candidateColumn) {
            _dbConfigIdx = dbConfigIdx;
            _candidateColumn = candidateColumn;
            _bufferSize = DbDistincter.MAX_ROWS_PER_STREAM * 3;
            _databaseConn = ConnectionManager.CreateConnection(_dbConfigIdx);
            _databaseConn.Open();
            _databaseConn.SetHighTimeout();
            _sqlSelectFromIdxTable = string.Format(sqlSelectFromIdx, idxTablePrefix + _candidateColumn.ColumnId) + " AND id > {0}";
            _sqlSelectFromRownoTable = string.Format(sqlSelectFromRowno, _candidateColumn.ColumnId);
        }

        #endregion [ Constructor ]

        #region [ Destructor ]

        ~RowIdEnumerator() {
            Dispose();
        }

        #endregion [ Destructor ]

        #region [ Public methods ]

        public bool GetRowIds(out Dictionary<int, List<int>> rowIds) {
            string sql = string.Format(_sqlSelectFromIdxTable, _lastIdxRowId);
            rowIds = new Dictionary<int, List<int>>();

            try {
                long currentBufferSize = 0;
                // continue processing rows from rowno table if present
                if (_transformedRownoIds != null) ProcessRowFromRownoTable(ref rowIds, ref currentBufferSize);

                // continue read row ids from idx table if buffer is not full
                if (currentBufferSize < _bufferSize) {
                    IDataReader readerIdx = GetDataReaderIdx(sql);
                    while ((currentBufferSize < _bufferSize) && readerIdx.Read()) {
                        _idxId = Convert.ToInt32(readerIdx[1]);
                        bool hasRowIdTable = Convert.ToBoolean(readerIdx[2]);
                        long bytesRead;
                        if (!hasRowIdTable) {
                            List<int> transformedRowIds = TransformBytesToRowIds(readerIdx, out bytesRead);
                            rowIds.Add(_idxId, transformedRowIds);
                            currentBufferSize += transformedRowIds.Count;
                        } else {
                            _transformedRownoIds = TransformBytesToRowIds(readerIdx, out bytesRead);
                            _transformedRownoIdLastIndex = -1;
                            ProcessRowFromRownoTable(ref rowIds, ref currentBufferSize);
                        }
                    }
                }
            } catch (Exception ex) {
                _log.Log(LogLevelEnum.Info, "Exception occurred in RowIdEnumerator." + ex.Message, true);
            }
            if (rowIds.Any())
                return true;
            return false;
        }

        #endregion [ Public methods ]

        #region [ Private methods ]

        #region [ Reading rowIds from rowno table ]

        private void ProcessRowFromRownoTable(ref Dictionary<int, List<int>> rowIds, ref long currentBufferSize) {
            rowIds.Add(_idxId, GetRowIdsFromRownoTable(ref currentBufferSize));
            // set _transformedRownoIds to null if all rows from rowno table were processed
            if (_transformedRownoIdLastIndex == (_transformedRownoIds.Count - 1)) _transformedRownoIds = null;
        }
        
        internal List<int> GetRowIdsFromRownoTable(ref long currentBufferSize) {
            List<int> rowIds = new List<int>();
            long bytesRead;
            _transformedRownoIdLastIndex += 1;
            string rowNoSql = _sqlSelectFromRownoTable + " WHERE id = " + _transformedRownoIds[_transformedRownoIdLastIndex];
            IDataReader readerRowno = GetDataReaderRowno(rowNoSql);
            if ((currentBufferSize < _bufferSize) && readerRowno.Read()) {
                List<int> transformedRowIds = TransformBytesToRowIds(readerRowno, out bytesRead);
                currentBufferSize += transformedRowIds.Count;
                rowIds.AddRange(transformedRowIds);
            }
            return rowIds;
        }

        #endregion [ Reading rowIds from rowno table ]

        #endregion [ Private methods ]
    }
}
