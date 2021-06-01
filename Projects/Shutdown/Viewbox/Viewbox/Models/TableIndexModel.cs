using System.Linq;
using System.Web.Helpers;
using SystemDb;

namespace Viewbox.Models
{
	public class TableIndexModel : BaseModel
	{
		public ITableObject TableObject { get; set; }

		public string ColumnName { get; set; }

		internal static TableIndexModel Create(ITableObject iTableObject, string columnName)
		{
			return new TableIndexModel
			{
				ColumnName = columnName,
				TableObject = iTableObject
			};
		}

		public static string GetJsonIndexListColumName(ITableObject tableObject, string columnName)
		{
			return Json.Encode(from i in tableObject.Indexes.GetByColumnName(columnName)
				select i.IndexName);
		}
	}
}
