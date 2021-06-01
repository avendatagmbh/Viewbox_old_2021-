using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AV.Log;
using log4net;

namespace AV.Log.ConsoleAppForTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ILog _log = LogHelper.GetLogger();
            _log.Error("exception into log", new ArgumentNullException("paramNameWhichIsNull"));
            _log.DebugWithCheck("adasdsa");
            _log.Log(LogLevelEnum.Warn, "this is a warning");
            _log.Log(LogLevelEnum.Fatal, "this is a fatal error", new ArgumentOutOfRangeException());
            _log.InfoFormatWithCheck("infor format {0}, {1}", "one", "two");
            Console.ReadLine();
        }
    }
}
