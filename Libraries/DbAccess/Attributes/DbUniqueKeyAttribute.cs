using System;

namespace DbAccess.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DbUniqueKeyAttribute : Attribute
	{
		public string Name { get; set; }

		public DbUniqueKeyAttribute()
		{
		}

		public DbUniqueKeyAttribute(string name)
		{
			Name = name;
		}
	}
}
