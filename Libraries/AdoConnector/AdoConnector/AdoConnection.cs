using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ADODB;

namespace AdoConnector {

    public class AdoConnection : IDbConnection {

        private string connStr;

        public AdoConnection() {
            this.Connection = new Connection();
            this.Connection.ConnectionTimeout = 99999;
            this.Connection.CursorLocation = CursorLocationEnum.adUseServer;
        }

        public Connection Connection {
            get;
            private set;
        }

        public IDbTransaction BeginTransaction(IsolationLevel il) {
            throw new NotImplementedException();
        }

        public IDbTransaction BeginTransaction() {
            throw new NotImplementedException();
        }

        public void ChangeDatabase(string databaseName) {
            throw new NotImplementedException();
        }

        public void Close() {
            this.Connection.Close();
        }

        public string ConnectionString {
            get {
                return this.connStr;
            }
            set {
                this.connStr = value;
                this.Connection.ConnectionString = value;
            }
        }

        public int ConnectionTimeout {
            get { throw new NotImplementedException(); }
        }

        public IDbCommand CreateCommand() {
            throw new NotImplementedException();
        }

        public string Database {
            get { throw new NotImplementedException(); }
        }

        public void Open() {
            this.Connection.Open();
        }

        public ConnectionState State {
            get { throw new NotImplementedException(); }
        }

        public void Dispose() {
            throw new NotImplementedException();
        }
    }
}
