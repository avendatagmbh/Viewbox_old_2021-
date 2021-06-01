using System.Collections.Generic;
using SystemDb;

namespace Viewbox.Models
{
	public class RelationModelNew : ViewboxModel
	{
		public IColumn Column { get; internal set; }

		public int RowNo { get; internal set; }

		public List<IRelation> Relations { get; internal set; }

		public override string LabelCaption => Column.GetDescription();

		public RelationModelNew(IColumn column, int rowNo, List<IRelation> relations)
		{
			Column = column;
			RowNo = rowNo;
			Relations = relations;
		}
	}
}
