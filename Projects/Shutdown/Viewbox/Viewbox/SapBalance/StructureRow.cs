using DbAccess.Attributes;

namespace Viewbox.SapBalance
{
	[DbTable("_bgv_bilanzstruktur")]
	public class StructureRow
	{
		[DbColumn("ID", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("BilStr", Length = 10)]
		public string Key { get; set; }

		[DbColumn("Parent")]
		public int Parent { get; set; }

		[DbColumn("Ebene")]
		public int Level { get; set; }

		[DbColumn("Nummer", Length = 10)]
		public string Name { get; set; }

		[DbColumn("Titel", Length = 255)]
		public string Description { get; set; }

		[DbColumn("Konto", Length = 20)]
		public string Account { get; set; }

		[DbColumn("KontoVon", Length = 20)]
		public string AccountStart { get; set; }

		[DbColumn("KontoBis", Length = 20)]
		public string AccountEnd { get; set; }

		[DbColumn("Typ", Length = 1)]
		public string Type { get; set; }

		[DbColumn("Soll")]
		public bool Debit { get; set; }

		[DbColumn("Haben")]
		public bool Credit { get; set; }

		[DbColumn("HighestGroupId")]
		public int ParentGroup { get; set; }

		[DbColumn("AdditionalInformation")]
		public string AdditionalInformation { get; set; }

		[DbColumn("AccountStructure")]
		public string AccountStructure { get; set; }

		[DbColumn("SumAndAddToBalance")]
		public bool SumAndAddToBalance { get; set; }

		[DbColumn("Lang_key", StoreAsVarBinary = true, AutoLoad = false)]
		public string LanguageKey { get; set; }
	}
}
