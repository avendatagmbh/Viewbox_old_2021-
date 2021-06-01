using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gudusoft.gsqlparser;

namespace SqlParser {

    public class JoinIterator {
        #region Declaration
        /// <summary>
        /// Contains every Table and Column as Text
        /// </summary>
        private string mMessage;
        /// <summary>
        /// Contains the Columns (values) for each table
        /// </summary>
        private Dictionary<string, List<string>> mColumnsForTables;
        #endregion Declaration

        public JoinIterator() {
            mMessage = string.Empty;
            mColumnsForTables = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        }

        public void IterateJoinList(TLzJoinList pJoinList) {
            foreach (TLzJoin lJoin in pJoinList) {
                VisitJoin(lJoin);
            }
        }

        public Dictionary<string, List<string>> TablesAndColumns {
            get { return mColumnsForTables; }
        }

        private void VisitJoin(TLzJoin pJoin) {
            switch (pJoin.JoinTableType) {
                case (TJoinTableType.jttTable): {
                        if (!(mColumnsForTables.Keys.Contains(pJoin.JoinTable.AsText))) {
                            mColumnsForTables.Add(pJoin.JoinTable.AsText, new List<string>());
                        }
                        break;
                    };
                case (TJoinTableType.jttJoin): {
                        //SubJoin
                        VisitJoin(pJoin.JoinJoin);
                        break;
                    };
            }
            IterateJoinItems(pJoin.JoinItems);
        }

        private void IterateJoinItems(TLzJoinItemList pJoinItems) {
            foreach (TLzJoinItem lJoinItem in pJoinItems) {
                VisitJoinItem(lJoinItem);
            }
        }

        private void VisitJoinItem(TLzJoinItem pJoinItem) {
            if (pJoinItem.JoinItemTableType == TJoinTableType.jttTable) {
                if (!(mColumnsForTables.Keys.Contains(pJoinItem.JoinItemTable.AsText))) {
                    mColumnsForTables.Add(pJoinItem.JoinItemTable.AsText, new List<string>());
                }
                if (pJoinItem.JoinQualType == TSelectJoinQual.sjqOn) {
                    TLzCustomExpression expr = pJoinItem.JoinQual;
                    SqlNodeIterator lSqlIter = new SqlNodeIterator(expr);
                    lSqlIter.Convert();
                    Dictionary<string, List<string>> lColTables = lSqlIter.ColumnsForTable;
                    foreach (KeyValuePair<string, List<string>> lColumnsforTable in lColTables) {
                        if (!(mColumnsForTables.Keys.Contains(lColumnsforTable.Key))) {
                            mColumnsForTables.Add(lColumnsforTable.Key, new List<string>());
                        }
                        foreach (string lColumn in lColumnsforTable.Value) {
                            if (!(mColumnsForTables[lColumnsforTable.Key].Contains(lColumn))) {
                                mColumnsForTables[lColumnsforTable.Key].Add(lColumn);
                            }
                        }
                    }
                }
            } else {
                VisitJoin(pJoinItem.JoinItemJoin);
            }
        }
    }
}
