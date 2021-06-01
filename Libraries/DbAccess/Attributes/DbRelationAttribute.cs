using System;

namespace DbAccess.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DbRelationAttribute : Attribute
	{
		public string RelationTableName { get; set; }

		public string ColumnName { get; set; }

		public string ForeignName { get; set; }

		public bool LazyLoad { get; set; }

		public bool SortOnLoad { get; set; }

		public DbRelationAttribute(string relation_table_name, string column_name, string foreign_name)
		{
			RelationTableName = relation_table_name;
			ColumnName = column_name;
			ForeignName = foreign_name;
		}
	}
}
