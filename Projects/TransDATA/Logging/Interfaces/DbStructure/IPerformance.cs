// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-02-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
namespace Logging.Interfaces.DbStructure {
    public interface IPerformance {
        double CpuUsage { get; set; }
        double RamLeft { get; set; }
        DateTime TimeStamp { get; set; }
        long FreeDriveSpace { get; set; }
    }
}