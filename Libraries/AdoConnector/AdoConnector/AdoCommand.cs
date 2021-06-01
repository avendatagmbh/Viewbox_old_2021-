using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ADODB;

namespace AdoConnector {
    
    public class AdoCommand : IDbCommand {

        private IDbConnection conn;
        private string commText;

        /// <summary>
        /// Constructor of AdoCommand
        /// </summary>
        public AdoCommand() {
            this.Command = new Command();
            this.Command.CommandType = CommandTypeEnum.adCmdText;
        }

        #region Properties

        public Command Command {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the text command to run against the data source.
        /// </summary>
        public string CommandText {
            get { return this.commText; }
            set {
                this.commText = value;
                this.Command.CommandText = value;
            }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public int CommandTimeout {
            get;
            set;
        }

        /// <summary>
        /// Indicates or specifies how the CommandText property is interpreted.
        /// </summary>
        public CommandType CommandType {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the DbConnection used by this instance of the DbCommand.
        /// </summary>
        public IDbConnection Connection {
            get { return this.conn; }
            set {
                this.conn = value;
                this.Command.ActiveConnection = ((AdoConnection)value).Connection;
            }
        }

        /// <summary>
        /// Gets the DataParameterCollection.
        /// </summary>
        public IDataParameterCollection Parameters {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the transaction within which the Command object of a .NET Framework data provider executes.
        /// </summary>
        public IDbTransaction Transaction {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets how command results are applied to the DataRow when used by the Update method of a DbDataAdapter.
        /// </summary>
        public UpdateRowSource UpdatedRowSource {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Method

        /// <summary>
        /// Attempts to cancels the execution of an DbCommand.
        /// </summary>
        public void Cancel() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new instance of an DbDataParameter object.
        /// </summary>
        /// <returns></returns>
        public IDbDataParameter CreateParameter() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET Framework data provider, and returns the number of rows affected.
        /// </summary>
        /// <returns></returns>
        public int ExecuteNonQuery() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes the CommandText against the Connection, and builds an IDataReader using one of the CommandBehavior values.
        /// </summary>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public IDataReader ExecuteReader(CommandBehavior behavior) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader.
        /// </summary>
        /// <returns></returns>
        public IDataReader ExecuteReader() {
            return new AdoDataReader(this);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query.Extra columns or rows are ignored.
        /// </summary>
        /// <returns></returns>
        public object ExecuteScalar() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a prepared (or compiled) version of the command on the data source.
        /// </summary>
        public void Prepare() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. (Inherited from IDisposable.)
        /// </summary>
        public void Dispose() {
            this.Dispose();
        }

        #endregion

    }
}
