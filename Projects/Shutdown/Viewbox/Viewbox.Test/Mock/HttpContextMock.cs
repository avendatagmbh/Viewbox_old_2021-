using System.Security.Principal;
using System.Web;

namespace Viewbox.Test.Mock
{
    public class HttpContextFactoryMock : HttpContextBase
    {
        private readonly IIdentity _identity;
        private readonly HttpRequestBase _request;
        private readonly HttpSessionMock _sessionMock;
        private IPrincipal _user;

        public HttpContextFactoryMock()
        {
            _sessionMock = new HttpSessionMock();
            _request = new HttpRequestMock();
            _identity = new GenericIdentity("avendata_admin");
            _user = new GenericPrincipal(_identity, new string[] {});
        }

        public override HttpSessionStateBase Session
        {
            get { return _sessionMock; }
        }

        public override HttpRequestBase Request
        {
            get { return _request; }
        }

        public override IPrincipal User
        {
            get { return _user; }
            set { _user = value; }
        }
    }
}