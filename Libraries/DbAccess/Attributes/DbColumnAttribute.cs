using System;

namespace DbAccess.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DbColumnAttribute : Attribute
	{
		public string Name { get; private set; }

		public string Description { get; set; }

		public bool AllowDbNull { get; set; }

		public bool StoreAsVarBinary { get; set; }

		public int Length { get; set; }

		public bool AutoLoad { get; set; }

		public bool AutoIncrement { get; set; }

		public bool IsInverseMapping { get; set; }

		public bool CascadeOnDelete { get; set; }

		public string DefaultValue { get; set; }

		public DbColumnAttribute(string name)
		{
			Name = name;
			AutoLoad = true;
			AllowDbNull = true;
			StoreAsVarBinary = false;
			AutoIncrement = false;
			IsInverseMapping = false;
			CascadeOnDelete = true;
			DefaultValue = null;
		}
	}
}
