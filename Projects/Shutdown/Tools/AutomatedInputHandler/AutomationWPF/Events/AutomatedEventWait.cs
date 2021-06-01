using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomationWPF.Events
{
    /// <summary>
    /// Wait event.
    /// </summary>
    public class AutomatedEventWait : AutomatedEventBase, IWait
    {
        /// <summary>
        /// Wait time in milliseconds.
        /// </summary>
        public int WaitTimeInMilliSeconds { get; set; }

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="p_WaitTime">Wait time in millisecond.</param>
        public AutomatedEventWait(int p_WaitTime)
        {
            WaitTimeInMilliSeconds = p_WaitTime;
        }

        /// <summary>
        /// Gets the formatted version of the event, used in saving/loading events.
        /// </summary>
        /// <returns>Formatted version of the event.</returns>
        public override string GetString()
        {
            return
                "Exec. Type: " + EventExecutionType.ToString() +
                "; Event type: " + EventType.ToString() +
                "; WaitTimeInMilliSeconds: " + WaitTimeInMilliSeconds.ToString();
        }
    }
}
