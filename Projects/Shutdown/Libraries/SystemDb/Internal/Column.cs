using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using DbAccess;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("columns", ForceInnoDb = true)]
	public class Column : DataObject, IColumn, IDataObject, INotifyPropertyChanged, ICloneable
	{
		public delegate void RelationAddedHandler(Column sender, Column foreign);

		private bool _isVisible;

		private bool _isTempedHidden;

		private string _originalName;

		private TableObject _table;

		private OptimizationType _optimizationType;

		private bool _isEmpty;

		private int _maxLength;

		private string _constValue;

		private SqlType _dataType;

		private int _paramOperator;

		private string _fromColumn;

		private string _fromColumnFormat;

		private short _flag;

		[DbColumn("is_visible")]
		public bool IsVisible
		{
			get
			{
				return _isVisible;
			}
			set
			{
				if (_isVisible != value)
				{
					_isVisible = value;
					NotifyPropertyChanged("IsVisible");
				}
			}
		}

		public bool IsTempedHidden
		{
			get
			{
				return _isTempedHidden;
			}
			set
			{
				if (_isTempedHidden != value)
				{
					_isTempedHidden = value;
					NotifyPropertyChanged("IsTempedHidden");
				}
			}
		}

		[DbColumn("table_id")]
		public int TableId { get; set; }

		public ITableObject Table
		{
			get
			{
				return _table;
			}
			set
			{
				if (_table != value)
				{
					_table = value as TableObject;
					TableId = Table.Id;
					NotifyPropertyChanged("Table");
				}
			}
		}

		[DbColumn("data_type")]
		public SqlType DataType
		{
			get
			{
				return _dataType;
			}
			set
			{
				if (_dataType != value)
				{
					_dataType = value;
					NotifyPropertyChanged("DataType");
				}
			}
		}

		[DbColumn("original_name", Length = 128, AllowDbNull = true)]
		public string OriginalName
		{
			get
			{
				return _originalName;
			}
			set
			{
				if (_originalName != value)
				{
					_originalName = value;
					NotifyPropertyChanged("OriginalName");
				}
			}
		}

		[DbColumn("optimization_type", AllowDbNull = true)]
		public OptimizationType OptimizationType
		{
			get
			{
				return _optimizationType;
			}
			set
			{
				if (_optimizationType != value)
				{
					_optimizationType = value;
					NotifyPropertyChanged("OptimizationGroupId");
				}
			}
		}

		[DbColumn("is_empty")]
		public bool IsEmpty
		{
			get
			{
				return _isEmpty;
			}
			set
			{
				if (_isEmpty != value)
				{
					_isEmpty = value;
					NotifyPropertyChanged("IsEmpty");
				}
			}
		}

		[DbColumn("max_length")]
		public int MaxLength
		{
			get
			{
				return _maxLength;
			}
			set
			{
				if (_maxLength != value)
				{
					_maxLength = value;
					NotifyPropertyChanged("MaxLength");
				}
			}
		}

		[DbColumn("param_operator")]
		public int ParamOperator
		{
			get
			{
				return _paramOperator;
			}
			set
			{
				if (_paramOperator != value)
				{
					_paramOperator = value;
					NotifyPropertyChanged("ParamOperator");
				}
			}
		}

		[DbColumn("const_value", Length = 128, AllowDbNull = true)]
		public string ConstValue
		{
			get
			{
				return _constValue;
			}
			set
			{
				if (_constValue != value)
				{
					_constValue = value;
					NotifyPropertyChanged("ConstValue");
				}
			}
		}

		[DbColumn("from_column", Length = 255, AllowDbNull = true)]
		public string FromColumn
		{
			get
			{
				return _fromColumn;
			}
			set
			{
				if (_fromColumn != value)
				{
					_fromColumn = value;
					NotifyPropertyChanged("FromColumn");
				}
			}
		}

		[DbColumn("from_column_format", Length = 255, AllowDbNull = true)]
		public string FromColumnFormat
		{
			get
			{
				return _fromColumnFormat;
			}
			set
			{
				if (_fromColumnFormat != value)
				{
					_fromColumnFormat = value;
					NotifyPropertyChanged("FromColumnFormat");
				}
			}
		}

		[DbColumn("flag", AllowDbNull = true)]
		public short Flag
		{
			get
			{
				return _flag;
			}
			set
			{
				if (_flag != value)
				{
					_flag = value;
					NotifyPropertyChanged("Flag");
				}
			}
		}

		public bool IsOptEmpty { get; set; }

		public bool IsOneDistinct { get; set; }

		public string HeaderClass { get; set; }

		public string HeaderStyle { get; set; }

		public int UserDefinedWidth { get; set; }

		public int OriginalWidth { get; set; }

		public string MaxValue { get; set; }

		public string MinValue { get; set; }

		public bool HasIndex
		{
			get
			{
				if (Table.Indexes.GetByColumnName(base.Name).Count() > 0)
				{
					return true;
				}
				return false;
			}
		}

		public bool ExactMatchUnchecked { get; set; }

		public Column()
			: base(DataObjectType.Column)
		{
		}

		public List<int> FromColumnIndexes()
		{
			List<int> ret = new List<int>();
			if (string.IsNullOrEmpty(FromColumnFormat))
			{
				return ret;
			}
			for (int i = 0; i < FromColumnFormat.Length; i++)
			{
				if (FromColumnFormat[i] == '1')
				{
					ret.Add(i);
				}
			}
			return ret;
		}

		public object Clone()
		{
			Column c = new Column();
			base.Clone(ref c);
			c.IsVisible = IsVisible;
			c.IsTempedHidden = IsTempedHidden;
			c.Table = Table;
			c.DataType = DataType;
			c.OriginalName = OriginalName;
			c.OptimizationType = OptimizationType;
			c.IsEmpty = IsEmpty;
			c.MaxLength = MaxLength;
			c.ConstValue = ConstValue;
			c.FromColumn = FromColumn;
			c.ParamOperator = ParamOperator;
			c.FromColumnFormat = FromColumnFormat;
			c.Flag = Flag;
			c.IsOptEmpty = IsOptEmpty;
			c.IsOneDistinct = IsOneDistinct;
			c.MaxValue = MaxValue;
			c.MinValue = MinValue;
			c.ExactMatchUnchecked = ExactMatchUnchecked;
			c.UserDefinedWidth = UserDefinedWidth;
			return c;
		}

		public static List<Column> FastLoad(DatabaseBase conn)
		{
			List<Column> result = new List<Column>();
			using IDataReader reader = conn.ExecuteReader("SELECT is_visible,table_id,data_type,original_name,optimization_type,is_empty,max_length,id,name,user_defined,const_value,ordinal,from_column,from_column_format, flag, param_operator FROM columns");
			while (reader.Read())
			{
				Column column = new Column
				{
					IsVisible = (!reader.IsDBNull(0) && Convert.ToBoolean(reader[0])),
					IsTempedHidden = false,
					TableId = ((!reader.IsDBNull(1)) ? reader.GetInt32(1) : 0),
					DataType = ((!reader.IsDBNull(2)) ? ((SqlType)Enum.Parse(typeof(SqlType), Convert.ToInt32(reader[2]).ToString(CultureInfo.InvariantCulture))) : SqlType.String),
					OriginalName = (reader.IsDBNull(3) ? null : reader.GetString(3)),
					OptimizationType = ((!reader.IsDBNull(4)) ? ((OptimizationType)Enum.Parse(typeof(OptimizationType), Convert.ToInt32(reader[4]).ToString(CultureInfo.InvariantCulture))) : OptimizationType.NotSet),
					IsEmpty = (!reader.IsDBNull(5) && Convert.ToBoolean(reader[5])),
					MaxLength = ((!reader.IsDBNull(6)) ? reader.GetInt32(6) : 0),
					Id = ((!reader.IsDBNull(7)) ? reader.GetInt32(7) : 0),
					Name = (reader.IsDBNull(8) ? null : reader.GetString(8)),
					UserDefined = (!reader.IsDBNull(9) && Convert.ToBoolean(reader[9])),
					ConstValue = (reader.IsDBNull(10) ? null : reader.GetString(10)),
					Ordinal = ((!reader.IsDBNull(11)) ? reader.GetInt32(11) : 0),
					FromColumn = (reader.IsDBNull(12) ? null : reader.GetString(12)),
					FromColumnFormat = (reader.IsDBNull(13) ? null : reader.GetString(13)),
					Flag = (short)((!reader.IsDBNull(14)) ? reader.GetInt16(14) : 0),
					ParamOperator = ((!reader.IsDBNull(15)) ? reader.GetInt32(15) : 0)
				};
				result.Add(column);
			}
			return result;
		}

		public override string ToString()
		{
			return base.Name;
		}
	}
}
