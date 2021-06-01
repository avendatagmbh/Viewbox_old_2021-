using System;

namespace DbAccess.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	public class DbTableAttribute : Attribute
	{
		public string Name { get; private set; }

		public string Description { get; set; }

		public bool ForceInnoDb { get; set; }

		public DbTableAttribute(string name)
		{
			Name = name;
		}
	}
}
