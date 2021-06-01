using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomatedInputHandler.Events
{
    public class AutomatedEventIteration : AutomatedEventBase, IIteration
    {
        public int IterationCounter { get; set; }
        public int ID { get; set; }

        public AutomatedEventIteration(int p_IterationCounter, int p_ID)
        {
            IterationCounter = p_IterationCounter;
            ID = p_ID;
        }

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
