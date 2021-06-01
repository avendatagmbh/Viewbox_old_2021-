using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Viewbox
{
    /// <summary>
    /// create a user specified logger mechanism. 
    /// </summary>
    public class LogFactory
    {
        static public ILogger CreateLogger(LogType logType) {
            switch (logType)
            {
                case LogType.FileLog:
                    return FileLogger.GetInstance();                   
                default:
                    return FileLogger.GetInstance();                    
            }
        }

    }

}