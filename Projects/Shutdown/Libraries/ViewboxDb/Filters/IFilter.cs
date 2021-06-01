using System;
using System.Collections;
using System.Collections.Generic;
using SystemDb;
using DbAccess;

namespace ViewboxDb.Filters
{
	public interface IFilter
	{
		string ToString(DatabaseBase db);

		string ToString(DatabaseBase db, string prefix);

		void ReplaceColumn(ITableObject tobj);

		List<Tuple<IFilter, IColumn, string, object, string>> GetParameters(List<Tuple<IFilter, IColumn, string, object, string>> parameters);

		List<Tuple<IFilter, string, object, IColumn>> GetParameters(List<Tuple<IFilter, string, object, IColumn>> parameters);

		string GetReplacedFilter(DatabaseBase db, ref int i, List<bool> freeselection, ref int j);

		string ToOriginalString();

		bool CheckFilter();

		bool HasIncorrectColumn();

		bool HasValue();

		void ReplaceRowNoFilters(IFilter parent, DatabaseBase connection, IndexDb indexDb, IColumn rowNoColumn);

		string ToCommandString(DatabaseBase db, Hashtable commandParameters);

		object GetColumnValue(string column);

		IFilter Clone();

		string GetToolTipText();
	}
}
