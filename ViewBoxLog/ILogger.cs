using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Viewbox
{
    public interface ILogger
    {
        void LogException(Exception e);
    }
}
