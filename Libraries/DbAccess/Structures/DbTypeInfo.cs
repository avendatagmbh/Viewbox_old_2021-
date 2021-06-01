using System;

namespace DbAccess.Structures
{
	public class DbTypeInfo : IComparable
	{
		public string DbName { get; set; }

		public string DisplayName { private get; set; }

		public int CompareTo(object obj)
		{
			return string.Compare(DisplayName, ((DbTypeInfo)obj).DisplayName, StringComparison.Ordinal);
		}
	}
}
