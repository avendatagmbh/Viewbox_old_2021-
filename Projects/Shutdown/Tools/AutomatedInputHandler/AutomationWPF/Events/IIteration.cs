using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomationWPF.Events
{
    interface IIteration
    {
        int IterationCounter { get; set; }
        int ID { get; set; }
    }
}
