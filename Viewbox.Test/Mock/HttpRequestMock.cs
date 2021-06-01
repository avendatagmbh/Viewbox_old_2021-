using System.Collections.Specialized;
using System.Web;

namespace Viewbox.Test.Mock
{
    public class HttpRequestMock : HttpRequestBase
    {
        private readonly NameValueCollection _params = new NameValueCollection();

        public override bool IsAuthenticated
        {
            get { return true; }
        }

        public override string[] UserLanguages
        {
            get { return new[] {""}; }
        }

        public override NameValueCollection Params
        {
            get { return _params; }
        }
    }
}