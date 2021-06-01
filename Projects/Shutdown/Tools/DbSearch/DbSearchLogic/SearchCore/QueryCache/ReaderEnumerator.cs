using System;
using System.Data;
using System.Data.Common;
using log4net;
using AV.Log;

namespace DbSearchLogic.SearchCore.QueryCache {

    internal class ReaderEnumerator : IDbColumnEnumerator {

        internal ILog _log = LogHelper.GetLogger();

        private IDataReader mReader;

        public ReaderEnumerator(IDataReader reader, string sql) {
            this.mReader = reader;
            Sql = sql;
        }

        public string Sql { get; private set; }

        #region Methods
        /// <summary>
        /// Move cursor to next position.
        /// </summary>
        /// <returns></returns>
        public bool Next() {

            try {                

                bool bResult = mReader.Read();
                if (!bResult) {
                    mReader.Close();
                    mReader.Dispose();
                    mReader = null;
                }

                return bResult;

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }
        
        public void Close() {
            try {
                if (mReader != null) { 
                    mReader.Close();
                    mReader.Dispose();
                }
            } catch (Exception) { }
        }

        private bool _hadOverflow = false;
        public object GetValue() {
            object obj;
            try {
                 obj = mReader[0];
            }catch(OverflowException) {
                //This can happen if the decimal in the database is too large to fit in a .NET decimal
                if(!_hadOverflow)
                    _log.Log(LogLevelEnum.Warn, "Decimal Überlauf, der Wert in der Datenbank ist zu groß und wird ignoriert. SQL: " + Sql, true);
                _hadOverflow = true;
                return DBNull.Value;
            }

            if (obj.GetType().Name == "MySqlDateTime") {
                MySql.Data.Types.MySqlDateTime mySqlDT = (MySql.Data.Types.MySqlDateTime)obj;
                if (mySqlDT.IsValidDateTime) {
                    obj = mySqlDT.GetDateTime();
                } else {
                    obj = DateTime.MinValue;
                }
            }
            return obj;
        }

        public UInt32 GetId() {
            if (mReader.FieldCount > 1) {
                return (UInt32)mReader[1];
            } else {
                return 0;
            }
        }

        #endregion Methods
    }
}
