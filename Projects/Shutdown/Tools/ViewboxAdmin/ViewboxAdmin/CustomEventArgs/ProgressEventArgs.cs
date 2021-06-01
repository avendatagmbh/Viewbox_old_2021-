using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewboxAdmin.CustomEventArgs
{
    public class ProgressEventArgs: EventArgs {

        public ProgressEventArgs(int progressPercentage) {
            this.ProgressPercentage = progressPercentage;
        }

        public int ProgressPercentage { get; private set; } 
    }
}
