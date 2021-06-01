using System.Drawing;
using System.Threading;
using Viewbox.Job.PdfExport.General;
using Viewbox.Job.PdfExport.Layouts;

namespace Viewbox.Job.PdfExport.ViewboxPdf
{
	internal class ViewboxActebisFacturaPdfExportClone : FacturaPdfExporter
	{
		protected override void CustomizeLayout(FacturaPdfLayout layout)
		{
			layout.Logo = new Bitmap(Thread.GetDomain().BaseDirectory + "/Content/img/pdf/viewbox-legacy-small-logo.png");
			layout.FooterEnabled = false;
		}
	}
}
