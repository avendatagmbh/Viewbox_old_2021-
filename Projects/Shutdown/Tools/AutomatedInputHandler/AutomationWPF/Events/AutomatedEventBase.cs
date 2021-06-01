using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutomationWPF.Helpers;

namespace AutomationWPF.Events
{
    /// <summary>
    /// Contains base properties of an event.
    /// </summary>
    public abstract class AutomatedEventBase
    {
        /// <summary>
        /// Type of the event.
        /// </summary>
        public Enums.EventTypeEnum EventType { get; set; }

        /// <summary>
        /// Type of the execution (when to exetute the event).
        /// </summary>
        public Enums.EventExecutionTypeEnum EventExecutionType { get; set; }

        public abstract string GetString();
    }
}
