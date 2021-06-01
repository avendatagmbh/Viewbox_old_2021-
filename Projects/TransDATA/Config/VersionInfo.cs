// (C)opyright 2011 AvenDATA GmbH
// ----------------------------------------
// author: Mirko Dibbert
// since : 2011-08-28

using System;
using System.Collections.Generic;
using System.Linq;

namespace Config {
    /// <summary>
    /// History of database versions, used for database updates.
    /// </summary>
    internal static class VersionInfo {
        private static readonly List<string> _dbVersionHistory = new List<string> {
                                                                                      "1.0"
                                                                                  };

        public static string CurrentDbVersion {
            get { return _dbVersionHistory.Last(); }
        }

        public static List<string> DbVersionHistory {
            get { return _dbVersionHistory; }
        }

        public static int VersionToInt(string version) {
            if (string.IsNullOrEmpty(version)) return _dbVersionHistory.Count;
            int i = 0;
            foreach (string v in DbVersionHistory) {
                if (v == version) return i;
                i++;
            }

            throw new Exception("Invalid db version: " + version);
        }

        public static bool ProgramVersionToOld(string dbVersion) {
            return VersionToInt(dbVersion) > VersionToInt(CurrentDbVersion);
        }

        public static bool NewerDbVersionExists(string version) {
            return VersionToInt(CurrentDbVersion) > VersionToInt(version);
        }
    }
}