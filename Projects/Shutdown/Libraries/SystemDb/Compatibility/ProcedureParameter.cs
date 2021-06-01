using DbAccess.Attributes;

namespace SystemDb.Compatibility
{
	[DbTable("procedure_params")]
	internal class ProcedureParameter
	{
		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("procedure_id")]
		public int ProcedureId { get; set; }

		[DbColumn("ordinal")]
		public int Ordinal { get; set; }

		[DbColumn("name", Length = 45)]
		public string Name { get; set; }

		[DbColumn("description", Length = 255)]
		public string Comment { get; set; }

		[DbColumn("type", Length = 45)]
		public string Type { get; set; }
	}
}
