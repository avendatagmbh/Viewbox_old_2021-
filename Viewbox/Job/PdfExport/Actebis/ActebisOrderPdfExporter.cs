using Viewbox.Job.PdfExport.General;
using Viewbox.Job.PdfExport.Layouts;
using Viewbox.Properties;

namespace Viewbox.Job.PdfExport.Actebis
{
	public class ActebisOrderPdfExporter : OrderPdfExporter
	{
		protected override void CustomizeLayout(OrderPdfLayout layout)
		{
			//layout.Logo = Resources.AlsoLogo;
			layout.FooterEnabled = true;
		}
	}
}
