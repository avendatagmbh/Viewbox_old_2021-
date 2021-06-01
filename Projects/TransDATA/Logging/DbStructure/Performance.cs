// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-02-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using Logging.Interfaces.DbStructure;
using Utils;
using System;

namespace Logging.DbStructure {

    [DbTable("log_performance")]
    internal class Performance : NotifyPropertyChangedBase, IPerformance {
        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        internal long Id { get; set; }
        #endregion

        #region TimeStamp
        [DbColumn("timestamp", AllowDbNull = false)]
        public DateTime TimeStamp { get; set; }
        #endregion

        #region CpuUsage
        [DbColumn("cpu_usage", AllowDbNull = false)]
        public double CpuUsage { get; set; }
        #endregion

        #region RamLeft
        [DbColumn("ram_left", AllowDbNull = false)]
        public double RamLeft { get; set; }
        #endregion

        #region FreeDriveSpace
        [DbColumn("free_drive_space", AllowDbNull = false)]
        public long FreeDriveSpace { get; set; }
        #endregion
    }
}