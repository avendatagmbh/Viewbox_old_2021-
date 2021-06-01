using Viewbox.Job.PdfExport.General;
using Viewbox.Job.PdfExport.Layouts;
using Viewbox.Properties;

namespace Viewbox.Job.PdfExport.Actebis
{
	internal class ActebisFacturaPdfExporter : FacturaPdfExporter
	{
		protected override void CustomizeLayout(FacturaPdfLayout layout)
		{
			//layout.Logo = Resources.AlsoLogo;
			layout.FooterEnabled = true;
		}
	}
}
