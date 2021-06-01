using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomatedInputHandler.Events
{
    public class AutomatedEventWait : AutomatedEventBase, IWait
    {
        public int WaitTimeInMilliSeconds { get; set; }

        public AutomatedEventWait(int p_WaitTime)
        {
            WaitTimeInMilliSeconds = p_WaitTime;
        }

        public override string GetString()
        {
            return
                "Exec. Type: " + EventExecutionType.ToString() +
                "; Event type: " + EventType.ToString() +
                "; WaitTimeInMilliSeconds: " + WaitTimeInMilliSeconds.ToString();
        }
    }
}
