using Viewbox.Job.PdfExport.General;
using Viewbox.Job.PdfExport.Layouts;
using Viewbox.Properties;

namespace Viewbox.Job.PdfExport.Actebis
{
	internal class ActebisKreditnotePdfExporter : KreditnotePdfExporter
	{
		protected override void CustomizeLayout(KreditnotePdfLayout layout)
		{
			//layout.Logo = Resources.AlsoLogo;
			layout.FooterEnabled = true;
		}
	}
}
