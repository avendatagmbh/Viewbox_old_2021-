// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;

namespace Business.Interfaces {
    public interface ITransferTableProgress {
        string TableName { get; }
        string Filter { get; }
        DateTime StartTime { get; }
        string StartTimeString { get; }
        long DatasetsTotal { get; }
        long DataSetsProcessed { get; set; }
        string DatasetProgressString { get; }
    }
}