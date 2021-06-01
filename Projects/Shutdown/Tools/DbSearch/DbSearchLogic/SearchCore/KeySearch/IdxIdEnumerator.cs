using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using DbAccess;
using DbAccess.Structures;
using DbSearchBase.Interfaces;
using DbSearchDatabase.DistinctDb;
using log4net;
using AV.Log;

namespace DbSearchLogic.SearchCore.KeySearch {
    public class IdxIdEnumerator : BinaryRowIdEnumerator, IIdxIdEnumerator {

        #region [ Constants ]

        internal ILog _log = LogHelper.GetLogger();

        internal const string sqlSelectFromIdx = "SELECT rowIds, id, hasRowNoTable FROM {0}";

        #endregion [ Constants ]

        #region [ Private members ]

        private int _lastIdxRowId = 0;

        #endregion [ Private members ]

        #region [ Constructor ]

        public IdxIdEnumerator(DbConfig dbConfigIdx, KeyCandidateColumn candidateColumn) {
            _dbConfigIdx = dbConfigIdx;
            _candidateColumn = candidateColumn;
            _bufferSize = DbDistincter.MAX_ROWS_PER_STREAM * 3;
            _databaseConn = ConnectionManager.CreateConnection(_dbConfigIdx);
            _databaseConn.Open();
            _databaseConn.SetHighTimeout();
            _sqlSelectFromIdxTable = string.Format(sqlSelectFromIdx, idxTablePrefix + _candidateColumn.ColumnId) + " WHERE id > {0}";
            _sqlSelectFromRownoTable = string.Format(sqlSelectFromRowno, _candidateColumn.ColumnId);
        }

        #endregion [ Constructor ]
        
        #region [ Destructor ]

        ~IdxIdEnumerator() {
            Dispose();
        }

        #endregion [ Destructor ]

        #region [ Public methods ]

        public bool GetIdxIds(out Dictionary<int, int> idxIds) {
            string sql = string.Format(_sqlSelectFromIdxTable, _lastIdxRowId);
            idxIds = new Dictionary<int, int>();
            
            try {
                long currentBufferSize = 0;
                
                // continue read row ids from idx table if buffer is not full
                if (currentBufferSize < _bufferSize) {
                    IDataReader readerIdx = GetDataReaderIdx(sql);
                    while ((currentBufferSize < _bufferSize) && readerIdx.Read()) {
                        //long bytesRead;
                        //int id = Convert.ToInt32(readerIdx[1]);
                        //TransformBytesToRowIdIdxIdPairs(readerIdx, id, idxIds, out bytesRead);
                        _idxId = Convert.ToInt32(readerIdx[1]);
                        bool hasRowIdTable = Convert.ToBoolean(readerIdx[2]);
                        long bytesRead;
                        if (!hasRowIdTable) {
                            TransformBytesToRowIdIdxIdPairs(readerIdx, _idxId, ref idxIds, out bytesRead);
                            currentBufferSize += bytesRead / 4;
                        }
                        else {
                            _transformedRownoIds = TransformBytesToRowIds(readerIdx, out bytesRead);
                            _transformedRownoIdLastIndex = -1;
                            ProcessRowFromRownoTable(ref idxIds, ref currentBufferSize);
                        }
                    }
                }
            }
            catch (Exception ex) {
                _log.Log(LogLevelEnum.Info, "Exception occurred in GetIdxIdsByRowId." + ex.Message, true);
            }
            if (idxIds.Any())
                return true;
            return false;
        }

        #endregion [ Public methods ]

        #region [ Private methods ]

        private void ProcessRowFromRownoTable(ref Dictionary<int, int> idxIdsByRowId, ref long currentBufferSize) {
            GetRowIdsFromRownoTable(ref idxIdsByRowId, ref currentBufferSize);
            // set _transformedRownoIds to null if all rows from rowno table were processed
            if (_transformedRownoIdLastIndex == (_transformedRownoIds.Count - 1)) _transformedRownoIds = null;
        }
        
        internal List<int> GetRowIdsFromRownoTable(ref Dictionary<int, int> idxIdsByRowId, ref long currentBufferSize) {
            List<int> rowIds = new List<int>();
            long bytesRead;
            _transformedRownoIdLastIndex += 1;
            string rowNoSql = _sqlSelectFromRownoTable + " WHERE id = " + _transformedRownoIds[_transformedRownoIdLastIndex];
            IDataReader readerRowno = GetDataReaderRowno(rowNoSql);
            if ((currentBufferSize < _bufferSize) && readerRowno.Read()) {
                TransformBytesToRowIdIdxIdPairs(readerRowno, _idxId, ref idxIdsByRowId, out bytesRead);
                currentBufferSize += bytesRead / 4;
            }
            return rowIds;
        }

        /// <summary>
        /// Transforms the rowIds binary field to rowId list 
        /// </summary>
        /// <param name="reader">The data reader</param>
        /// <param name="idxId">The data reader</param>
        /// <param name="rowIds">Ther rowId - idx_id pair list</param>
        /// <param name="numBytesRead">The number of bytes read</param>
        private void TransformBytesToRowIdIdxIdPairs(IDataReader reader, int idxId, ref Dictionary<int, int> rowIds, out long numBytesRead) {
            const int BUFFER_SIZE = 1024;
            long offset = 0;
            int col = 0;
            byte[] buffer = new byte[BUFFER_SIZE];
            int bytesRead = 0;
            long readerSize = reader.GetBytes(0, 0, null, 0, 0);
            while ((bytesRead = (int)reader.GetBytes(col, offset, buffer, 0, BUFFER_SIZE)) > 0) {
                for (int i = 0; i < bytesRead; i += 4) {
                    int rowId = BitConverter.ToInt32(buffer, i);
                    if (!rowIds.ContainsKey(rowId)) rowIds.Add(rowId, idxId);
                }
                offset += bytesRead;
                if (offset >= readerSize) break;
            }
            numBytesRead = offset;
        }

        #endregion [ Private methods ]
    }
}
