using System.Drawing;
using System.Threading;
using Viewbox.Job.PdfExport.General;
using Viewbox.Job.PdfExport.Layouts;

namespace Viewbox.Job.PdfExport.ViewboxPdf
{
	internal class ViewboxActebisKreditnotePdfExportClone : KreditnotePdfExporter
	{
		protected override void CustomizeLayout(KreditnotePdfLayout layout)
		{
			layout.Logo = new Bitmap(Thread.GetDomain().BaseDirectory + "/Content/img/pdf/viewbox-legacy-small-logo.png");
			layout.FooterEnabled = false;
		}
	}
}
