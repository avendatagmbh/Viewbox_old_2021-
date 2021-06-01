using System;

namespace DbAccess.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DbForeignKeyAttribute : Attribute
	{
		public string Name { get; private set; }

		public string BaseTable { get; private set; }

		public string BaseColumn { get; private set; }

		public bool CascadeOnUpdate { get; private set; }

		public bool CascadeOnDelete { get; private set; }

		public DbForeignKeyAttribute(string name, string baseTable, string baseColumn)
		{
			Name = name;
			BaseTable = baseTable;
			BaseColumn = baseColumn;
			CascadeOnUpdate = true;
			CascadeOnDelete = true;
		}
	}
}
