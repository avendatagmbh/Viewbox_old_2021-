using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomationWPF.Events
{
    /// <summary>
    /// Iteration event.
    /// </summary>
    public class AutomatedEventIteration : AutomatedEventBase, IIteration
    {
        /// <summary>
        /// Counter of the iteration.
        /// </summary>
        public int IterationCounter { get; set; }
        /// <summary>
        /// The identifier of the iteration.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="p_IterationCounter">Counter of the iteration.</param>
        /// <param name="p_ID">The identifier of the iteration.</param>
        public AutomatedEventIteration(int p_IterationCounter, int p_ID)
        {
            IterationCounter = p_IterationCounter;
            ID = p_ID;
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
                "; ID: " + ID.ToString() +
                "; IterationCounter: " + IterationCounter.ToString();
        }
    }
}
