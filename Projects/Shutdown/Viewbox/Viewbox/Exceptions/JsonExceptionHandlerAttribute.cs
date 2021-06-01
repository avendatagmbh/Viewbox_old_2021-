using System.Web.Mvc;

namespace Viewbox.Exceptions
{
	public class JsonExceptionHandlerAttribute : FilterAttribute, IExceptionFilter
	{
		public void OnException(ExceptionContext filterContext)
		{
			filterContext.ExceptionHandled = true;
			filterContext.Result = new JsonResult
			{
				Data = new
				{
					success = false,
					error = filterContext.Exception.Message
				},
				JsonRequestBehavior = JsonRequestBehavior.AllowGet
			};
		}
	}
}
