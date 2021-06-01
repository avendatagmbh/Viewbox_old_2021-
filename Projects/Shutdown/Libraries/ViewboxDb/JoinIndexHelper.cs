using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DbAccess;

namespace ViewboxDb
{
	public class JoinIndexHelper
	{
		public DbIndexInfo Index1 { get; private set; }

		public DbIndexInfo Index2 { get; private set; }

		public JoinIndexHelper(ConnectionManager cmanager, Join joining, CancellationToken token)
		{
			foreach (JoinColumns column in joining.Columns.Where((JoinColumns c) => c.Column1 == 0 || c.Column2 == 0).ToList())
			{
				joining.Columns.Remove(column);
			}
			IEnumerable<int> col1s = joining.Columns.Select((JoinColumns c) => c.Column1);
			IEnumerable<int> col2s = joining.Columns.Select((JoinColumns c) => c.Column2);
			Index1 = new DbIndexInfo(cmanager, token, joining.Table1, col1s);
			Index2 = new DbIndexInfo(cmanager, token, joining.Table2, col2s);
		}
	}
}
