using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base.Exceptions;
using DbAccess;
using System.Data.Common;
using Config.Interfaces.DbStructure;
using DbAccess.Structures;
using Business.Interfaces;

namespace Business.Structures.DateReaders {
    internal class TableDataReader : IDataReader {
        internal TableDataReader(IDatabase conn, ITable table, bool useSchema = true, bool useCatalog = true) {
            Connection = conn;
            Table = table;
            UseSchema = useSchema;
            UseCatalog = useCatalog;
        }

        private IDatabase Connection { get; set; }
        public System.Data.IDataReader Reader { get;protected set; }
        private ITable Table { get; set; }
        private bool IsCancelled { get; set; }
        private bool UseSchema { get; set; }
        private bool UseCatalog { get; set; }

        public bool Read() {
            if (Reader == null)
                throw new DataReaderNotInitializedExeption("Reader == null in TableDataReader.Read()", null);
            return !IsCancelled && Reader.Read();
        }

        public object[] GetData() {
            var result = new object[Reader.FieldCount];
            var i = Reader.GetValues(result);
            return result;
        }

        

        public void Dispose() {
            if (Reader != null) Reader.Close();
            if (Connection != null) Connection.Dispose();
        }

        public void Load(bool loadAllColumns = false) {
            try {
                if (!Connection.IsOpen) Connection.Open();

                DbTableInfo ti = new DbTableInfo(
                    UseCatalog ? Table.Catalog : string.Empty,
                    UseSchema ? Table.Schema : string.Empty,
                    Table.Name,
                    Table.Type,
                    Table.Comment,
                    Table.Filter);

                var exportColumns = new List<DbColumnInfo>();

                foreach (var col in Table.Columns) {
                    DbColumnInfo ci = new DbColumnInfo() {
                        Comment = col.Comment,
                        MaxLength = col.MaxLength,
                        Name = col.Name,
                        OriginalType = col.TypeName,
                        Type = col.DbType
                    };
                    ti.Columns.Add(ci);
                    if (col.DoExport) exportColumns.Add(ci);
                }

                if (ti.Columns.Count != exportColumns.Count && !loadAllColumns) {
                    Reader = Connection.GetTextReaderFromTable(ti, exportColumns);
                } else {
                    Reader = Connection.GetTextReaderFromTable(ti);
                }
                //var sql = "SELECT * FROM " + Connection.Enquote(Table.Name);
                //Reader = Connection.ExecuteReader(sql);
            } catch (Exception ex) {
                // if IsCancelled == true, the exception is usually an execution aborted exception which could be ignored is this case
                // catching the respective error is not possible, since the execution abortion could be invoced from an external source,
                // which should invoce an error message.
                if (!IsCancelled) throw new Exception("Fehler: " + ex.Message, ex);
            }
        }

        public void Cancel() {
            IsCancelled = true;
            Connection.CancelCommand();
        }

        public void Close() {
            Reader.Close();
            if (Connection.IsOpen) Connection.Close();
        }

        public int FieldCount { get { return Reader.FieldCount; } }
    }
}
