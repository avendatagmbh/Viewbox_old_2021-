using System.Data;
using System.IO;
using Viewbox.Job.PdfExport.Layouts;
using ViewboxDb;

namespace Viewbox.Job.PdfExport.General
{
	public class OrderPdfExporter : IPdfExporter
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
			OrderPdfLayout layout = new OrderPdfLayout();
			FillUpLayout(layout, dataTable);
			CustomizeLayout(layout);
			layout.GeneratePdf(TargetStream);
		}

		public void Init()
		{
		}

		private void FillUpLayout(OrderPdfLayout layout, DataTable dataTable)
		{
			DataRow data = dataTable.Rows[0];
			layout.Vbeln = data["VBELN"].ToString();
			layout.Fakturaadresse1 = data["Fakturaadresse1"].ToString();
			layout.Fakturaadresse2 = data["Fakturaadresse2"].ToString();
			layout.Fakturaadresse3 = data["Fakturaadresse3"].ToString();
			layout.Fakturaadresse4 = data["Fakturaadresse4"].ToString();
			layout.Fakturaadresse5 = data["Fakturaadresse5"].ToString();
			layout.Fakturaadresse6 = data["Fakturaadresse6"].ToString();
			layout.Leveringsadresse1 = data["Leveringsadresse1"].ToString();
			layout.Leveringsadresse2 = data["Leveringsadresse2"].ToString();
			layout.Leveringsadresse3 = data["Leveringsadresse3"].ToString();
			layout.Leveringsadresse4 = data["Leveringsadresse4"].ToString();
			layout.Leveringsadresse5 = data["Leveringsadresse5"].ToString();
			layout.Leveringsadresse6 = data["Leveringsadresse6"].ToString();
			layout.Ordredato = data["Ordre_dato"].ToString();
			layout.Kundenummer = data["Kundenummer"].ToString();
			layout.Kundenavn = data["Kundenavn"].ToString();
			layout.KundensRef = data["Kundens_ref_nr"].ToString();
			layout.BestiltAv = data["Bestilt_av"].ToString();
			layout.Salgsperson = data["Salgsperson"].ToString();
			layout.SalgspersonTlfNr = data["Salgsperson_tlf_nr"].ToString();
			layout.SalgspersonEmail = data["Salgsperson_email"].ToString();
			layout.Leveringsbetingelser = data["Leveringsbetingelser"].ToString();
			layout.Leveringsmetode = data["Leveringsmetode"].ToString();
			layout.Betalingsbetingelser = data["Betalingsbetingelser"].ToString();
			layout.OnsketLeveringsdato = data["Onsket_leveringsdato"].ToString();
			layout.EstLeveringsdato = data["Est_leveringsdato"].ToString();
			layout.SumEksMva = data["Sum_eks_mwa"].ToString();
			layout.EstimertFraktkostnad = data["Estimert_frachtkostnad"].ToString();
			layout.MinimumOrdregebyr = data["Minimum_ordregebyr"].ToString();
			layout.PapirFakturagebyr = data["Papir_fakturagebyr"].ToString();
			layout.FolgeseddelPaPapier = data["Folgeseddel_pa_papier"].ToString();
			layout.Mva = data["Mwa"].ToString();
			layout.SumInklMva = data["Sum_inkl_mwa"].ToString();
			foreach (DataRow row in dataTable.Rows)
			{
				layout.ProductItems.Add(new OrderPdfLayout.ProductItem
				{
					Antall = row["Antall"].ToString(),
					Enhet = row["Enhet"].ToString(),
					Nr = row["Pos_nr"].ToString(),
					Pris = row["Pris"].ToString(),
					Produkt = row["Produkt"].ToString(),
					Produktbeskrivelse = row["Produktbeskrivelse"].ToString(),
					Total = row["Total"].ToString()
				});
			}
		}

		protected virtual void CustomizeLayout(OrderPdfLayout layout)
		{
		}
	}
}
