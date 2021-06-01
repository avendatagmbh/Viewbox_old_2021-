using System.Collections.Generic;
using System.Web;

namespace Viewbox.Test.Mock
{
    public class HttpSessionMock : HttpSessionStateBase
    {
        private readonly Dictionary<string, object> _datas;

        public HttpSessionMock()
        {
            _datas = new Dictionary<string, object>();
            this["UnitTesting"] = true;
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

        public override void Clear()
        {
            _datas.Clear();
            this["UnitTesting"] = true;
            this["ViewboxBasePath"] = "localhost";
        }
    }
}