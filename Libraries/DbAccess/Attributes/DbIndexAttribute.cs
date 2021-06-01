using System;

namespace DbAccess.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class DbIndexAttribute : Attribute
	{
		public string Name { get; set; }

		public DbIndexAttribute(string name)
		{
			Name = name;
		}
	}
}
