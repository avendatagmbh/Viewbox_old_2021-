using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ADODB;
using System.Data;

namespace AdoConnector {
    
    /// <summary>
    /// Definition of the ADO DataReader-object
    /// </summary>
    public class AdoDataReader : IDataReader {

        private int recordCount;

        /// <summary>
        /// Constructor of AdoDataReader
        /// </summary>
        public AdoDataReader(AdoCommand command) {
            this.RecordSet = new Recordset();
            this.RecordSet.CursorType = CursorTypeEnum.adOpenStatic;
            this.RecordSet.CursorLocation = CursorLocationEnum.adUseServer;
            this.RecordSet.LockType = LockTypeEnum.adLockOptimistic;

            this.IsClosed = true;

            this.Command = command;
        }

        #region Properties

        /// <summary>
        /// ADO-RecordSet
        /// </summary>
        protected Recordset RecordSet {
            get;
            private set;
        }

        protected AdoCommand Command {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating the depth of nesting for the current row. (not implemented)
        /// </summary>
        public int Depth {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets a value indicating whether the data reader is closed.
        /// </summary>
        public bool IsClosed {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        public int RecordsAffected {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the number of columns in the current row. (Inherited from DataRecord.)
        /// </summary>
        public int FieldCount {
            get { return this.RecordSet.Fields.Count; }
        }

        /// <summary>
        /// Gets the column with the specified name. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object this[string name] {
            get {
                foreach (Field field in this.RecordSet.Fields) {
                    if (field.Name.Equals(name)) {
                        return field;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the column located at the specified index. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object this[int i] {
            get { return this.RecordSet.Fields[i]; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose() {
            this.Dispose();
        }

        /// <summary>
        /// Closes the DataReader Object.
        /// </summary>
        public void Close() {
            this.RecordSet.Close();
            this.IsClosed = true;
        }

        /// <summary>
        /// Returns a DataTable that describes the column metadata of the DataReader.
        /// </summary>
        /// <returns></returns>
        public DataTable GetSchemaTable() {
            DataTable dataTable = new DataTable();
            Recordset rs = ((AdoConnection)this.Command.Connection).Connection.OpenSchema(SchemaEnum.adSchemaTables);

            rs.MoveFirst();
            foreach (Field field in rs.Fields) {
                dataTable.Columns.Add(field.Name);
            }

            while (!rs.EOF) {

                DataRow dr = dataTable.NewRow();
                List<string> vals = new List<string>();

                foreach (Field field in rs.Fields) {
                    vals.Add(field.Value.ToString());
                }

                dataTable.Rows.Add(vals);

                rs.MoveNext();
            }

            return dataTable;
        }

        /// <summary>
        /// Advances the data reader to the next result, when reading the results of batch SQL statements.
        /// </summary>
        /// <returns></returns>
        public bool NextResult() {
            object outVal = null;
            this.RecordSet.NextRecordset(out outVal);
            System.IFormatProvider provider = new System.Globalization.NumberFormatInfo();
            this.RecordsAffected = System.Convert.ToInt32(outVal, provider);
            if (this.RecordsAffected == 0) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Advances the DataReader to the next record.
        /// </summary>
        /// <returns></returns>
        public bool Read() {
            if (this.IsClosed) {
                this.RecordSet.Open(this.Command.Command);
                this.IsClosed = false;
                this.recordCount = 0;
            }

            if (this.recordCount == 0) {
                this.RecordSet.MoveFirst();
            } else {
                this.RecordSet.MoveNext();
            }

            if (this.RecordSet.EOF) {
                return false;
            } else {
                this.recordCount++;
                return true;
            }
        }

        /// <summary>
        /// Gets the value of the specified column as a Boolean. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool GetBoolean(int i) {
            if (this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adBoolean)) {
                return (bool)this.RecordSet.Fields[i].Value;
            }
            throw new NullReferenceException();
        }

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column. (Inherited from IDataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public byte GetByte(int i) {
            if (this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adSmallInt)) {
                return (byte)this.RecordSet.Fields[i].Value;
            }
            throw new NullReferenceException();
        }

        /// <summary>
        /// Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given buffer offset. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <param name="fieldOffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferoffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
            byte[] b = (byte[]) this.RecordSet.Fields[i].Value;
            if (b.Length >= fieldOffset + length &&
               buffer.Length >= bufferoffset + length) {
                Array.Copy(b, fieldOffset, buffer, bufferoffset, length);
                return length;
            }
            throw new NullReferenceException();
        }

        /// <summary>
        /// Gets the character value of the specified column. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public char GetChar(int i) {
            if (this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adChar)) {
                return (char)this.RecordSet.Fields[i].Value;
            }
            throw new NullReferenceException();
        }

        /// <summary>
        /// Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given buffer offset. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <param name="fieldoffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferoffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
            char[] b = (char[])this.RecordSet.Fields[i].Value;
            if (b.Length >= fieldoffset + length &&
               buffer.Length >= bufferoffset + length) {
                Array.Copy(b, fieldoffset, buffer, bufferoffset, length);
                return length;
            }
            throw new NullReferenceException();
        }

        /// <summary>
        /// Returns an DataReader for the specified column ordinal. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public IDataReader GetData(int i) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the data type information for the specified field. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetDataTypeName(int i) {
            return this.RecordSet.Fields[i].Type.ToString();
        }

        /// <summary>
        /// Gets the date and time data value of the specified field. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public DateTime GetDateTime(int i) {
            if (this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adDate) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adDBTime) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adDBDate) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adDBTimeStamp)) {
                return (DateTime)this.RecordSet.Fields[i].Value;
            }
            throw new NullReferenceException();
        }

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public decimal GetDecimal(int i) {
            if (this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adBigInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adDecimal) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adDouble) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adInteger) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adNumeric) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adSingle) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adSmallInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adTinyInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedBigInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedSmallInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedTinyInt)) {
                return (Decimal)this.RecordSet.Fields[i].Value;
            }
            throw new NullReferenceException();
        }

        /// <summary>
        /// Gets the double-precision floating point number of the specified field. (Inherited from IDataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public double GetDouble(int i) {
            if (this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adDouble) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adInteger) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adNumeric) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adSingle) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adSmallInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adTinyInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedSmallInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedTinyInt)) {
                return (Double)this.RecordSet.Fields[i].Value;
            }
            throw new NullReferenceException();
        }

        /// <summary>
        /// Gets the Type information corresponding to the type of Object that would be returned from GetValue. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Type GetFieldType(int i) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field. (Inherited from IDataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public float GetFloat(int i) {
            if (this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adInteger) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adSingle) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adSmallInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adTinyInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedSmallInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedTinyInt)) {
                return (float)this.RecordSet.Fields[i].Value;
            }
            throw new NullReferenceException();
        }

        /// <summary>
        /// Returns the GUID value of the specified field. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Guid GetGuid(int i) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public short GetInt16(int i) {
            if (this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adSmallInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adTinyInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedSmallInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedTinyInt)) {
                return (short)this.RecordSet.Fields[i].Value;
            }
            throw new NullReferenceException();
        }

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int GetInt32(int i) {
            if (this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adInteger) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adSmallInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adTinyInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedSmallInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedTinyInt)) {
                return (int)this.RecordSet.Fields[i].Value;
            }
            throw new NullReferenceException();
        }

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public long GetInt64(int i) {
            if (this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adInteger) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adSmallInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adTinyInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedSmallInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedTinyInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adUnsignedBigInt) ||
                this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adBigInt)) {
                return (long)this.RecordSet.Fields[i].Value;
            }
            throw new NullReferenceException();
        }

        /// <summary>
        /// Gets the name for the field to find. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetName(int i) {
            return this.RecordSet.Fields[i].Name;
        }

        /// <summary>
        /// Return the index of the named field. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetOrdinal(string name) {
            for (int i = 0; i <= this.RecordSet.Fields.Count; i++) {
                if (this.RecordSet.Fields[i].Name.Equals(name)) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the string value of the specified field. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetString(int i) {
            return this.RecordSet.Fields[i].Value.ToString();
        }

        /// <summary>
        /// Return the value of the specified field. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object GetValue(int i) {
            return this.RecordSet.Fields[i].Value;
        }

        /// <summary>
        /// Populates an array of objects with the column values of the current record. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public int GetValues(object[] values) {
            if (values.Length >= this.RecordSet.Fields.Count) {
                for (int i = 0; i <= this.RecordSet.Fields.Count; i++ ) {
                    values[i] = this.RecordSet.Fields[i].Value;
                }
                return this.RecordSet.Fields.Count;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return whether the specified field is set to null. (Inherited from DataRecord.)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool IsDBNull(int i) {
            return this.RecordSet.Fields[i].Type.Equals(DataTypeEnum.adEmpty);
        }

        #endregion
    }
}
