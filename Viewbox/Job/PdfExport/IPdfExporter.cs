using System.Data;
using System.IO;
using ViewboxDb;

namespace Viewbox.Job.PdfExport
{
	internal interface IPdfExporter
	{
		Stream TargetStream { get; set; }

		bool Closing { get; set; }

		IDataReader DataReader { get; set; }

		TableObjectCollection TempTableObjects { get; set; }

		PdfExporterConfigElement ExporterConfig { get; set; }

		bool HasGroupSubtotal { get; set; }

		void Export();

		void Init();
	}
}
