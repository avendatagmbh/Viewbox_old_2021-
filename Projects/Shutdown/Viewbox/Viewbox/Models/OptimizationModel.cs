using System.Collections.Generic;
using System.Web.Mvc;
using SystemDb;

namespace Viewbox.Models
{
	public class OptimizationModel : BaseModel
	{
		public string SearchPhrase { get; set; }

		public WebViewPage ViewPage { get; set; }

		public string ReturnUrl { get; set; }

		public List<IOptimization> Optimizations { get; set; }

		public IOptimization SelectedOptimization { get; set; }

		public bool RightsMode => ViewboxSession.RightsMode;
	}
}
