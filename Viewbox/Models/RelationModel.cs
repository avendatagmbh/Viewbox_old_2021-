using System.Collections.Generic;
using SystemDb;

namespace Viewbox.Models
{
	public class RelationModel : ViewboxModel
	{
		public string Description { get; internal set; }

		public List<string> RowValues { get; internal set; }

		public ITableObject TableObject { get; internal set; }

		public List<IRelation> Relations { get; internal set; }

		public override string LabelCaption => Description;

		public RelationModel(string description, List<string> rowValues, ITableObject tableObject, List<IRelation> relations)
		{
			Description = description;
			RowValues = rowValues;
			TableObject = tableObject;
			Relations = relations;
		}
	}
}
