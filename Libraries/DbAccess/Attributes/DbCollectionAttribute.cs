using System;

namespace DbAccess.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DbCollectionAttribute : Attribute
	{
		public string RefProperty { get; private set; }

		public bool LazyLoad { get; private set; }

		public bool SortOnLoad { get; private set; }

		public bool CascadeSave { get; private set; }

		public DbCollectionAttribute(string refProperty)
		{
			RefProperty = refProperty;
			LazyLoad = false;
			SortOnLoad = false;
			CascadeSave = true;
		}
	}
}
