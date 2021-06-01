using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomatedInputHandler.Events
{
    public class AutomatedEventEnterText : AutomatedEventBase, IEnterText
    {
        public string TextToEnter { get; set; }

        public AutomatedEventEnterText(string p_TextToEnter)
        {
            TextToEnter = p_TextToEnter;
        }

        public override string GetString()
        {
            return
                "Exec. Type: " + EventExecutionType.ToString() + 
                "; Event type: " + EventType.ToString() +
                "; TextToEnter: " + TextToEnter;
        }
    }
}
