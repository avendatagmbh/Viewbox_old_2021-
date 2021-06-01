using SystemDb;

namespace Viewbox.Models
{
	public class ExportColumn
	{
		public string Name { get; private set; }

		public SqlType Type { get; private set; }

		public ExportColumn(string name, SqlType type = SqlType.String)
		{
			Name = name;
			Type = type;
		}

		public ExportColumn(IColumn column, ILanguage language)
		{
			Name = column.GetDescription(language);
			Type = column.DataType;
		}
	}
}
