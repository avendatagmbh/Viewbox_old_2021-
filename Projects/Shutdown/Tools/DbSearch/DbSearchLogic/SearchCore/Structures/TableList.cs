using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbSearchLogic.SearchCore.Structures.Db;

namespace DbSearchLogic.SearchCore.Structures {
    class TableList {

        public TableList() {
            mLock = new object();
            SmallTables = new List<TableInfo>();
            HugeTables = new List<TableInfo>();
        }

        public List<TableInfo> SmallTables { get; private set; }
        public List<TableInfo> HugeTables { get; private set; }

        private object mLock;

        /// <summary>
        /// Returns the count of remaining tables.
        /// </summary>
        public int RemainingTables {
            get {
                int nResult = 0;
                lock (mLock) {
                    nResult += SmallTables.Count;
                    nResult += HugeTables.Count;
                }
                return nResult;
            }
        }

        /// <summary>
        /// Returns a table from the list
        /// </summary>
        /// <returns>A free table</returns>
        public bool GetFreeTable(bool bPreferSmallTable, out TableInfo sTable) {
            bool bResult;
            lock (mLock) {
                if (bPreferSmallTable) {
                    // return a small table, if possible
                    if (SmallTables.Count > 0) {
                        sTable = this.SmallTables.First();
                        this.SmallTables.Remove(sTable);
                        bResult = true;
                    } else {
                        if (this.HugeTables.Count > 0) {
                            sTable = this.HugeTables.First();
                            this.HugeTables.Remove(sTable);
                            bResult = true;
                        } else {
                            sTable = null;
                            bResult = false;
                        }
                    }
                } else {
                    // return a huge table, if possible
                    if (this.HugeTables.Count > 0) {
                        sTable = this.HugeTables.First();
                        this.HugeTables.Remove(sTable);
                        bResult = true;
                    } else {
                        if (SmallTables.Count > 0) {
                            sTable = this.SmallTables.First();
                            this.SmallTables.Remove(sTable);
                            bResult = true;
                        } else {
                            sTable = null;
                            bResult = false;
                        }
                    }
                }
            }

            return bResult;
        }
    }
}
