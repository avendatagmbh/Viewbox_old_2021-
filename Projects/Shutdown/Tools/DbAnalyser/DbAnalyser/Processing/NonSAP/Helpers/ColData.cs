using System;
using System.Collections.Generic;

namespace DbAnalyser.Processing.NonSAP.Helpers
{
    public class ColData : IDisposable
    {
        public int ColumnId { get; set; }
        public string ColumnName { get; set; }
        public List<string> ColumnValues { get; set; }
        public string TableName { get; set; }
        public int TableId { get; set; }
        public long RowCount { get; set; }

        // Flag: Has Dispose already been called? 
        bool disposed;

        public ColData(int colId, string colName, List<string> colValues, string tableName, int tableId)
        {
            ColumnId = colId;
            ColumnName = colName;
            ColumnValues = colValues;
            TableName = tableName;
            TableId = tableId;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                ColumnName = null;
                ColumnValues = null;
                TableName = null;
            }

            disposed = true;
        }
    }
}
