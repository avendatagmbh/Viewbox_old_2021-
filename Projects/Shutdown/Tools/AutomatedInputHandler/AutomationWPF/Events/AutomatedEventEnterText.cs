using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomationWPF.Events
{
    /// <summary>
    /// Entering text event.
    /// </summary>
    public class AutomatedEventEnterText : AutomatedEventBase, IEnterText
    {
        /// <summary>
        /// The text that is entered.
        /// </summary>
        public string TextToEnter { get; set; }

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="p_TextToEnter">The text that is entered.</param>
        public AutomatedEventEnterText(string p_TextToEnter)
        {
            TextToEnter = p_TextToEnter;
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
                "; TextToEnter: " + TextToEnter;
        }
    }
}
