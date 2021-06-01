using System.Collections.Generic;

namespace Viewbox.LotusNotes
{
	public class LotusNode
	{
		public List<LotusNode> Children { get; set; }

		public string UNID { get; set; }

		public string SorterFeld { get; set; }

		public string BU { get; set; }

		public string Dokart { get; set; }

		public string Kategorie { get; set; }

		public string DokDatum { get; set; }

		public string KwTermin { get; set; }

		public string AuftragsNrSap { get; set; }

		public string Produkt { get; set; }

		public string Betreff { get; set; }

		public LotusNode()
		{
			SorterFeld = string.Empty;
			BU = string.Empty;
			Dokart = string.Empty;
			Kategorie = string.Empty;
			DokDatum = string.Empty;
			KwTermin = string.Empty;
			AuftragsNrSap = string.Empty;
			Produkt = string.Empty;
			Betreff = string.Empty;
		}
	}
}
