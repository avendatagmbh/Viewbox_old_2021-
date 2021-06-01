using System.Web.Mvc;
using System.Web.Routing;

namespace Viewbox.Controllers
{
	public class BaseController : Controller
	{
		public virtual void BeforeInitialize(RequestContext requestContext)
		{
			ViewboxSession.SaveStartLoadingTime();
		}

		public virtual void AfterInitialize(RequestContext requestContext)
		{
		}

		protected override void Initialize(RequestContext requestContext)
		{
			BeforeInitialize(requestContext);
			base.Initialize(requestContext);
			AfterInitialize(requestContext);
		}
	}
}
