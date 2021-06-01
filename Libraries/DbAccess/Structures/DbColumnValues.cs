using System.Collections.Generic;
using System.Linq;

namespace DbAccess.Structures
{
	public class DbColumnValues
	{
		private readonly Dictionary<string, object> mValues;

		public List<string> Columns => mValues.Keys.ToList();

		public object this[string sColumn]
		{
			get
			{
				return mValues[sColumn];
			}
			set
			{
				mValues[sColumn] = value;
			}
		}

		public DbColumnValues()
		{
			mValues = new Dictionary<string, object>();
		}

		public void Clear()
		{
			mValues.Clear();
		}
	}
}
