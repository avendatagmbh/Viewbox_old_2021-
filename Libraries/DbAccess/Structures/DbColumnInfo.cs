using DbAccess.Enums;

namespace DbAccess.Structures
{
	public class DbColumnInfo
	{
		private string mDefaultValue;

		public DbColumnTypes Type { get; set; }

		public string Name { get; set; }

		public string OriginalType { get; set; }

		public int MaxLength { get; set; }

		public string DefaultValue
		{
			get
			{
				return mDefaultValue;
			}
			set
			{
				mDefaultValue = value;
				HasDefaultValue = true;
			}
		}

		public bool HasDefaultValue { get; private set; }

		public bool AllowDBNull { get; set; }

		public bool AutoIncrement { get; set; }

		public int NumericScale { get; set; }

		public bool IsPrimaryKey { get; set; }

		public bool IsUnsigned { get; set; }

		public bool IsIdentity { get; set; }

		public string Comment { get; set; }

		public int OrdinalPosition { get; set; }

		public DbColumnInfo Clone()
		{
			return MemberwiseClone() as DbColumnInfo;
		}
	}
}
