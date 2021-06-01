using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Interfaces;

namespace Business.Interfaces {
    public interface IDbImporter {
        ITransferProgress ImportProgress { get; }
        event EventHandler Finished;
        void Start();
        void Cancel();
    }
}
