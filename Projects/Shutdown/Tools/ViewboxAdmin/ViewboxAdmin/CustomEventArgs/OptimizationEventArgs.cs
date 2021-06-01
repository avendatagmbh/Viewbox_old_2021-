using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;

namespace ViewboxAdmin.CustomEventArgs
{
    public class OptimizationEventArgs:EventArgs
    {
        public OptimizationEventArgs(IOptimization opt) {
            this.Optimization = opt;
        }
        public IOptimization Optimization { get; private set; }
    }
}
