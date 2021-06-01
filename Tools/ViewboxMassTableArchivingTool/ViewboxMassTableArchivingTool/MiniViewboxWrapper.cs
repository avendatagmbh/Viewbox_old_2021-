using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SystemDb;
using SystemDb.Helper;
using SystemDb.Upgrader;
using AV.Log;
using ViewboxDb;
using log4net;

namespace ViewboxMassTableArchivingTool
{
    internal class MiniViewboxWrapper:IDisposable
    {
        internal ILog _log = LogHelper.GetLogger();

        public event EventHandler ViewboxDbInitialized;

        public string ConnectionString { get; set; }
        public string TempDBName { get; set; }
        public string IndexDBName { get; set; }

        internal MiniViewboxWrapper() {
            TempDBName = "temp";
            IndexDBName = "index";
            ConnectionString =
                "server=localhost;User Id=root;password=avendata;database=viewbox;Connect Timeout=1000;Default Command Timeout=0;Allow Zero Datetime=True";
        }

        public void Init() {
            Database = new ViewboxDb.ViewboxDb(TempDBName,IndexDBName);
            _disposed = false;

            Database.ViewboxDbInitialized += new EventHandler(ViewboxDatabaseInitialized);
            Database.Connect("MySQL", ConnectionString);
        }

        void ViewboxDatabaseInitialized(object sender, EventArgs args) {
            ViewboxDbInitialized(sender, args);
        }

        private ViewboxDb.ViewboxDb _database;
        public ViewboxDb.ViewboxDb Database
        {
            get { return _database as ViewboxDb.ViewboxDb; }
            set { _database = value; }
        }

        public void DoArchive(ITableObject table, ArchiveType archive)
        {
            this.Database.ArchiveTable(table, archive);
            this.UpdateTableObjectArchive(table,archive);
        }


        private void UpdateTableObjectArchive(ITableObject table, ArchiveType archive)
        {
            Boolean archiveTable = ((archive == ArchiveType.Archive));
            this.Database.SystemDb.UpdateTableObjectArchived(table, archiveTable);
            this.Database.SystemDb.Objects[table.Id].IsUnderArchiving = false;
        }

        internal IEnumerable<ITableObject> GetTablesFromQuery() {
            return Database.SystemDb.Objects;
        }


        private bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (Database != null)
                        Database.Dispose();
                }

                Database = null;
                _disposed = true;
            }
        }
    }
}
