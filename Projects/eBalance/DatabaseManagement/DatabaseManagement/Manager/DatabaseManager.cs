// -----------------------------------------------------------
// Created by Benjamin Held - 26.07.2011 13:56:14
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBase;
using DbAccess;

namespace DatabaseManagement.Manager {
    static class DatabaseManager {

        internal static void Init(DatabaseConfig config) {
            DatabaseConfig = config;
            ConnectionManager = new ConnectionManager(config.DbConfig, 15);
            ConnectionManager.Init();
        }

        #region Properties
        public static DatabaseConfig DatabaseConfig{get;private set;}
        internal static ConnectionManager ConnectionManager { get; private set; }
        #endregion
    }
}
