using System;

namespace Viewbox.Job
{
	internal class ScriptDbColumn : ICloneable
	{
		public string Name { get; set; }

		public string SourceTableAlias { get; set; }

		public string SourceTableColumn { get; set; }

		public string RealTableName { get; set; }

		public string RealColumnName { get; set; }

		public object Clone()
		{
			return new ScriptDbColumn
			{
				Name = Name,
				SourceTableColumn = SourceTableColumn
			};
		}

		public override string ToString()
		{
			return Name + ", SourceTableAlias : " + SourceTableAlias + ", SourceTableColumn : " + SourceTableColumn + ", RealTableName : " + RealTableName + ", RealColumnName : " + RealColumnName;
		}
	}
}
