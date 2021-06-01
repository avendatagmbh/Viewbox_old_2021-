using System;
using System.Data;
using DbAccess;

namespace Viewbox.Models
{
	public class DatabaseDataReader : IDataReader, IDisposable, IDataRecord
	{
		private readonly DatabaseBase _database;

		private IDataReader _dataReader;

		public string Sql { get; private set; }

		public long ColumnCount { get; private set; }

		public int RecordsAffected
		{
			get
			{
				if (_dataReader != null)
				{
					return _dataReader.RecordsAffected;
				}
				throw new NotImplementedException();
			}
		}

		public int FieldCount
		{
			get
			{
				if (_dataReader != null)
				{
					return _dataReader.FieldCount;
				}
				throw new NotImplementedException();
			}
		}

		public object this[string name]
		{
			get
			{
				if (_dataReader != null)
				{
					return _dataReader[name];
				}
				throw new NotImplementedException();
			}
		}

		public object this[int i]
		{
			get
			{
				if (_dataReader != null)
				{
					return _dataReader[i];
				}
				throw new NotImplementedException();
			}
		}

		int IDataReader.Depth
		{
			get
			{
				if (_dataReader != null)
				{
					return _dataReader.Depth;
				}
				throw new NotImplementedException();
			}
		}

		bool IDataReader.IsClosed
		{
			get
			{
				if (_dataReader != null)
				{
					return _dataReader.IsClosed;
				}
				throw new NotImplementedException();
			}
		}

		public void Close()
		{
			if (_dataReader != null)
			{
				_dataReader.Close();
				return;
			}
			throw new NotImplementedException();
		}

		public DataTable GetSchemaTable()
		{
			if (_dataReader != null)
			{
				return _dataReader.GetSchemaTable();
			}
			throw new NotImplementedException();
		}

		public bool NextResult()
		{
			if (_dataReader != null)
			{
				return _dataReader.NextResult();
			}
			throw new NotImplementedException();
		}

		public bool Read()
		{
			if (_dataReader != null)
			{
				return _dataReader.Read();
			}
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			if (_dataReader != null)
			{
				_dataReader.Dispose();
				return;
			}
			throw new NotImplementedException();
		}

		public bool GetBoolean(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetBoolean(i);
			}
			throw new NotImplementedException();
		}

		public byte GetByte(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetByte(i);
			}
			throw new NotImplementedException();
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
			}
			throw new NotImplementedException();
		}

		public char GetChar(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetChar(i);
			}
			throw new NotImplementedException();
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
			}
			throw new NotImplementedException();
		}

		public IDataReader GetData(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetData(i);
			}
			throw new NotImplementedException();
		}

		public string GetDataTypeName(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetDataTypeName(i);
			}
			throw new NotImplementedException();
		}

		public DateTime GetDateTime(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetDateTime(i);
			}
			throw new NotImplementedException();
		}

		public decimal GetDecimal(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetDecimal(i);
			}
			throw new NotImplementedException();
		}

		public double GetDouble(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetDouble(i);
			}
			throw new NotImplementedException();
		}

		public Type GetFieldType(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetFieldType(i);
			}
			throw new NotImplementedException();
		}

		public float GetFloat(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetFloat(i);
			}
			throw new NotImplementedException();
		}

		public Guid GetGuid(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetGuid(i);
			}
			throw new NotImplementedException();
		}

		public short GetInt16(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetInt16(i);
			}
			throw new NotImplementedException();
		}

		public int GetInt32(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetInt32(i);
			}
			throw new NotImplementedException();
		}

		public long GetInt64(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetInt64(i);
			}
			throw new NotImplementedException();
		}

		public string GetName(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetName(i);
			}
			throw new NotImplementedException();
		}

		public int GetOrdinal(string name)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetOrdinal(name);
			}
			throw new NotImplementedException();
		}

		public string GetString(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetString(i);
			}
			throw new NotImplementedException();
		}

		public object GetValue(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetValue(i);
			}
			throw new NotImplementedException();
		}

		public int GetValues(object[] values)
		{
			if (_dataReader != null)
			{
				return _dataReader.GetValues(values);
			}
			throw new NotImplementedException();
		}

		public bool IsDBNull(int i)
		{
			if (_dataReader != null)
			{
				return _dataReader.IsDBNull(i);
			}
			throw new NotImplementedException();
		}

		public void LoadQuery()
		{
			if (_database != null && Sql != null)
			{
				if (_dataReader != null && !_dataReader.IsClosed)
				{
					_dataReader.Close();
				}
				_dataReader = _database.ExecuteReader(Sql);
				return;
			}
			throw new NotImplementedException();
		}

		public DatabaseDataReader(DatabaseBase database, string sql, long count)
		{
			_database = database;
			Sql = sql;
			ColumnCount = count;
		}
	}
}
