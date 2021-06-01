// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Business.Interfaces;
using Business.Events;

namespace Business.Interfaces {
    public interface IDataTransferAgent {
        ITransferProgress TransferProgress { get; }
        DateTime TransferStartTime { get; set; }
        event EventHandler Finished;
        void Start();
        void Cancel();
        bool CheckDataAccessSource();
        bool CheckDataAccessDestination();
        string GetSourceTooltip();
        string GetDestinationTooltip();
        IInputAgent GetFirstInputAgent();
        IOutputAgent GetFirstOutputAgent();
        string GetLogDirectory();
    }
}