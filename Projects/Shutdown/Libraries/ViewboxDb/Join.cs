using System;
using SystemDb;
using ViewboxDb.Filters;

namespace ViewboxDb
{
	public class Join : ICloneable
	{
		public ITableObject Table1 { get; set; }

		public ITableObject Table2 { get; set; }

		public JoinColumnsCollection Columns { get; set; }

		public IFilter Filter1 { get; set; }

		public IFilter Filter2 { get; set; }

		public JoinType Type { get; set; }

		public Join()
		{
		}

		public Join(ITableObject table1, ITableObject table2, JoinColumnsCollection columns, IFilter filter1, IFilter filter2, JoinType type)
		{
			Table1 = table1;
			Table2 = table2;
			Columns = columns;
			Filter1 = filter1;
			Filter2 = filter2;
			Type = type;
		}

		public override int GetHashCode()
		{
			return 0 + Columns.GetHashCode() + ((Filter1 != null) ? Filter1.GetHashCode() : 0) + ((Filter2 != null) ? Filter2.GetHashCode() : 0) + Type.GetHashCode();
		}

		public object Clone()
		{
			return new Join
			{
				Table1 = Table1,
				Table2 = Table2,
				Columns = Columns,
				Filter1 = Filter1,
				Filter2 = Filter2,
				Type = Type
			};
		}
	}
}
