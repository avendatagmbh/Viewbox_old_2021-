using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutomatedInputHandler.Helper;

namespace AutomatedInputHandler.Events
{
    public abstract class AutomatedEventBase
    {
        public Enums.EventTypeEnum EventType { get; set; }

        public Enums.EventExecutionTypeEnum EventExecutionType { get; set; }

        public abstract string GetString();
    }
}
