using System.Collections.Generic;
using System.Web;

namespace Viewbox.Test.Mock
{
    public class HttpApplicationMock : HttpApplicationStateBase
    {
        private readonly Dictionary<string, object> _datas;

        public HttpApplicationMock()
        {
            _datas = new Dictionary<string, object>();
        }

        public override object this[string name]
        {
            get
            {
                if (_datas.ContainsKey(name))
                    return _datas[name];
                return null;
            }
            set
            {
                if (_datas.ContainsKey(name))
                    _datas[name] = value;
                else
                    _datas.Add(name, value);
            }
        }
    }
}