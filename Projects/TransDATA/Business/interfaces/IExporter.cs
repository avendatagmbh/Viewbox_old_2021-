// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-06
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Business.Structures;

namespace Business.Interfaces {
    public interface IExporter {
        ITransferProgress TransferProgress { get; set; }
        event EventHandler Finished;
        void Start();
        void Cancel();
    }
}