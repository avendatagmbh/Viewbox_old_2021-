using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomatedInputHandler.Events
{
    interface IWait
    {
        int WaitTimeInMilliSeconds { get; set; }
    }
}
