using System.Data;
using System.IO;
using Viewbox.Job.PdfExport.Layouts;
using ViewboxDb;

namespace Viewbox.Job.PdfExport.Actebis
{
	public class ActebisDeliveryNotePdfExporter : IPdfExporter
	{
		public Stream TargetStream { get; set; }

		public bool Closing { get; set; }

		public IDataReader DataReader { get; set; }

		public TableObjectCollection TempTableObjects { get; set; }

		public PdfExporterConfigElement ExporterConfig { get; set; }

		public bool HasGroupSubtotal { get; set; }

		public void Export()
		{
			DataTable dataTable = new DataTable();
			dataTable.Load(DataReader);
			DataRow firstRow = dataTable.Rows[0];
			DeliveryNotePdfLayout layout = new DeliveryNotePdfLayout
			{
				Vbeln = firstRow["VBELN"].ToString(),
				Fakturaadresse1 = firstRow["Fakturaadresse1"].ToString(),
				Fakturaadresse2 = firstRow["Fakturaadresse2"].ToString(),
				Fakturaadresse3 = firstRow["Fakturaadresse3"].ToString(),
				Fakturaadresse4 = firstRow["Fakturaadresse4"].ToString(),
				Fakturaadresse5 = firstRow["Fakturaadresse5"].ToString(),
				Leveringsadresse1 = firstRow["Leveringsadresse1"].ToString(),
				Leveringsadresse2 = firstRow["Leveringsadresse2"].ToString(),
				Leveringsadresse3 = firstRow["Leveringsadresse3"].ToString(),
				Leveringsadresse4 = firstRow["Leveringsadresse4"].ToString(),
				Leveringsadresse5 = firstRow["Leveringsadresse5"].ToString(),
				DelNr = firstRow["VBELN"].ToString(),
				Dato = firstRow["BLDAT"].ToString(),
				Kundenummer = firstRow["Kundenummer"].ToString(),
				Kundename = firstRow["Kunde_name"].ToString(),
				Leveringsmate = firstRow["Leverningsmate"].ToString(),
				Speditor = firstRow["Speditor"].ToString(),
				Leveringsbetingelser = firstRow["Leveringsbetingelser"].ToString(),
				Ordrenummer = firstRow["Ordrenummer"].ToString(),
				Salgsperson = firstRow["Salgsperson"].ToString(),
				DeresRef = firstRow["Deres_referense"].ToString(),
				BestiltAv = firstRow["Bestillt_av"].ToString(),
				Antall = firstRow["Antall_kolli"].ToString(),
				PakkeIds = firstRow["Pakke_ID"].ToString(),
				TotalVekt = string.Format("{0} {1}", firstRow["Total_vekt1"], firstRow["Total_vekt2"])
			};
			foreach (DataRow row in dataTable.Rows)
			{
				layout.ProductItems.Add(new DeliveryNotePdfLayout.ProductItem
				{
					Nr = row["Pos_nr"].ToString(),
					Varenummer = string.Format("{0} {1}", row["Varenummer"], row["Varebeskrivelse"]),
					Bestilt = row["Bestilt"].ToString(),
					Levert = row["Delivered"].ToString(),
					Tidligere = row["Tidligere"].ToString(),
					Rest = row["Rest"].ToString(),
					PakkeNummer = row["Pakke_ID"].ToString()
				});
			}
			layout.GeneratePdf(TargetStream);
		}

		public void Init()
		{
		}
	}
}
