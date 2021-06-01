using System.Data;
using System.IO;
using Viewbox.Job.PdfExport.Layouts;
using ViewboxDb;

namespace Viewbox.Job.PdfExport.General
{
	public class FacturaPdfExporter : IPdfExporter
	{
		public Stream TargetStream { get; set; }

		public bool Closing { get; set; }

		public IDataReader DataReader { get; set; }

		public PdfExporterConfigElement ExporterConfig { get; set; }

		public bool HasGroupSubtotal { get; set; }

		public TableObjectCollection TempTableObjects { get; set; }

		public void Export()
		{
			DataTable dataTable = new DataTable();
			dataTable.Load(DataReader);
			FacturaPdfLayout layout = new FacturaPdfLayout();
			FillUpLayout(layout, dataTable);
			CustomizeLayout(layout);
			layout.GeneratePdf(TargetStream);
		}

		public void Init()
		{
		}

		private void FillUpLayout(FacturaPdfLayout layout, DataTable dataTable)
		{
			string totalBelop = ((!string.IsNullOrEmpty(dataTable.Rows[0]["WAERK"].ToString())) ? string.Concat(dataTable.Rows[0]["WAERK"], "  ", dataTable.Rows[0]["TOTAL"]) : dataTable.Rows[0]["TOTAL"].ToString());
			layout.FakturaId = dataTable.Rows[0]["VBELN"].ToString();
			layout.Ordrenummer = dataTable.Rows[0]["AUBEL"].ToString();
			layout.Fakturadato = dataTable.Rows[0]["FKDAT"].ToString();
			layout.Ordredato = dataTable.Rows[0]["AUDAT"].ToString();
			layout.Kundenummer = dataTable.Rows[0]["KUNNR"].ToString();
			layout.KundesRef = dataTable.Rows[0]["BSTNK"].ToString();
			layout.Salgsperson = dataTable.Rows[0]["VK_pers"].ToString();
			layout.Sendingsnummer = dataTable.Rows[0]["VGBEL"].ToString();
			layout.Leveringsdato = dataTable.Rows[0]["LFDAT"].ToString();
			layout.Leveringsbetingelser = dataTable.Rows[0]["LFBED"].ToString();
			layout.Leveringsmate = dataTable.Rows[0]["lief_Mate"].ToString();
			layout.Betalingsbetingelser = dataTable.Rows[0]["ZTERM_text"].ToString();
			layout.Mva = dataTable.Rows[0]["MVA_ges"].ToString();
			layout.MvaPercentage = string.Concat(dataTable.Rows[0]["MVA_proz"], "%");
			layout.Fraktkostnad = dataTable.Rows[0]["FRACHT"].ToString();
			layout.TotalNetto = dataTable.Rows[0]["NETWR"].ToString();
			layout.TotalBelop = totalBelop;
			layout.Fakturaadresse1 = dataTable.Rows[0]["re_Name1"].ToString();
			layout.Fakturaadresse2 = dataTable.Rows[0]["re_Stras"].ToString();
			layout.Fakturaadresse3 = string.Concat(dataTable.Rows[0]["re_Pstlz"], " ", dataTable.Rows[0]["re_Ort01"]);
			layout.Fakturaadresse4 = dataTable.Rows[0]["STCEG"].ToString();
			layout.Leveringsadresse1 = dataTable.Rows[0]["lf_Name1"].ToString();
			layout.Leveringsadresse2 = dataTable.Rows[0]["lf_Name2"].ToString();
			layout.Leveringsadresse3 = dataTable.Rows[0]["lf_Stras"].ToString();
			layout.Leveringsadresse4 = string.Concat(dataTable.Rows[0]["lf_Pstlz"], " ", dataTable.Rows[0]["lf_Ort01"]);
			layout.Kidnr = dataTable.Rows[0][37].ToString();
			layout.Forfallsdato = dataTable.Rows[0][36].ToString();
			for (int i = 0; i < dataTable.Rows.Count; i++)
			{
				layout.ProductItems.Add(new FacturaKreditnotePdfLayout.ProductItem
				{
					Produkt = dataTable.Rows[i]["MFRPN"].ToString(),
					Produktbeskrivelse = dataTable.Rows[i]["MAKTX"].ToString(),
					Antall = dataTable.Rows[i]["FKLMG"].ToString(),
					MvaPercentage2 = string.Concat(dataTable.Rows[i]["MVA_proz_ges"], "%"),
					Pris = dataTable.Rows[i]["EPREIS"].ToString(),
					Belop = dataTable.Rows[i]["PREIS"].ToString()
				});
			}
		}

		protected virtual void CustomizeLayout(FacturaPdfLayout layout)
		{
			layout.FooterEnabled = false;
		}
	}
}
