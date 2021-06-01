using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using DbAccess;
using DbAccess.Structures;

namespace DbSearchLogic.SearchCore.KeySearch {
    public abstract class BinaryRowIdEnumerator : IDisposable {

        #region [ Constants ]

        internal const string sqlSelectFromRowno = "SELECT rowIds, id FROM rowno_{0}";
        internal const string idxTablePrefix = "idx_";

        #endregion [ Constants ]

        #region [ Private members ]

        private SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private bool _isDisposed = false;
        
        #endregion [ Private members ]

        #region [ Internal members ]

        internal DbConfig _dbConfigIdx;
        internal IDatabase _databaseConn;
        internal IDatabase _databaseConnRowno;
        internal IDataReader _readerIdx;
        internal IDataReader _readerRowno;
        internal string _readerIdxCommandString;
        internal string _readerRownoCommandString;
        internal KeyCandidateColumn _candidateColumn;
        internal int _bufferSize;
        internal int _idxId;
        internal List<int> _transformedRownoIds;
        internal int _transformedRownoIdLastIndex;
        internal string _sqlSelectFromIdxTable;
        internal string _sqlSelectFromRownoTable;

        #endregion [ Internal members ]

        #region [ IDisposable members ]

        public void Dispose() {
            _semaphore.Wait();
            try {
                if (!_isDisposed) {
                    CleanupReaderIdx();
                    CleanupReaderRowno();
                    CleanupConnectionIdx();
                    CleanupConnectionRowno();
                    _isDisposed = true;
                }
            } finally {
                _semaphore.Release();
            }
        }

        #endregion [ IDisposable members ]

        #region [ Connection and reader factory methods ]

        internal IDatabase GetConnectionRowno() {
            if (_databaseConnRowno == null) {
                _databaseConnRowno = ConnectionManager.CreateConnection(_dbConfigIdx);
            } 
            if (!_databaseConnRowno.IsOpen) {
                _databaseConnRowno.Open();
                _databaseConnRowno.SetHighTimeout();
            }
            return _databaseConnRowno;
        }

        internal IDataReader GetDataReaderIdx(string commandString) {
            if (_readerIdx == null) {
                _readerIdx = _databaseConn.ExecuteReader(commandString);
                _readerIdxCommandString = commandString;
            } else {
                if (_readerIdxCommandString != commandString) {
                    CleanupReaderIdx();
                    _readerIdx = _databaseConn.ExecuteReader(commandString);
                    _readerIdxCommandString = commandString;
                }
            }
            return _readerIdx;
        }

        internal IDataReader GetDataReaderRowno(string commandString) {
            if (_readerRowno == null) {
                _readerRowno = GetConnectionRowno().ExecuteReader(commandString);
                _readerRownoCommandString = commandString;
            } else {
                if (_readerRownoCommandString != commandString) {
                    CleanupReaderRowno();
                    _readerRowno = GetConnectionRowno().ExecuteReader(commandString);
                    _readerRownoCommandString = commandString;
                }
            }
            return _readerRowno;
        }

        #endregion [ Connection and reader factory methods ]

        #region [ Cleanup methods ]

        internal void CleanupReaderIdx() {
            if (_readerIdx == null) return;
            if (!_readerIdx.IsClosed) _readerIdx.Close();
            _readerIdx.Dispose();
        }

        internal void CleanupReaderRowno() {
            if (_readerRowno == null) return;
            if (!_readerRowno.IsClosed) _readerRowno.Close();
            _readerRowno.Dispose();
        }

        internal void CleanupConnectionIdx() {
            if (_databaseConn != null) {
                if (_databaseConn.IsOpen) _databaseConn.Close();
                _databaseConn.Dispose();
            }
        }

        internal void CleanupConnectionRowno() {
            if (_databaseConnRowno != null) {
                if (_databaseConnRowno.IsOpen) _databaseConnRowno.Close();
                _databaseConnRowno.Dispose();
            }
        }

        #endregion [ Cleanup methods ]

        #region [ Internal methods ]

        /// <summary>
        /// Transforms the rowIds binary field to rowId list 
        /// </summary>
        /// <param name="reader">The data reader</param>
        /// <param name="numBytesRead">The number of bytes read</param>
        /// <returns>The rowId list</returns>
        internal List<int> TransformBytesToRowIds(IDataReader reader, out long numBytesRead) {
            List<int> rowIdList = new List<int>();
            const int BUFFER_SIZE = 1024;
            long offset = 0;
            int col = 0;
            byte[] buffer = new byte[BUFFER_SIZE];
            int bytesRead = 0;
            long readerSize = reader.GetBytes(0, 0, null, 0, 0);
            while ((bytesRead = (int)reader.GetBytes(col, offset, buffer, 0, BUFFER_SIZE)) > 0) {
                for (int i = 0; i < bytesRead; i += 4)
                    rowIdList.Add(BitConverter.ToInt32(buffer, i));
                offset += bytesRead;
                if (offset >= readerSize) break;
            }
            numBytesRead = offset;
            return rowIdList;
        }

        #endregion [ Internal methods ]
    }
}
