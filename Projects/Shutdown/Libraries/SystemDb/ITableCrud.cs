using System.Collections.Generic;

namespace SystemDb
{
	public interface ITableCrud
	{
		int Id { get; }

		ITableObject Table { get; }

		ITableObject OnTable { get; }

		string Title { get; }

		int DefaultScheme { get; }

		List<ITableCrudColumn> Columns { get; }
	}
}
