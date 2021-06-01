using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gudusoft.gsqlparser;

namespace SqlParser {
    public class GetColumnsAndTables {
        #region Declaration
        /// <summary>
        /// This Dictionary contains for each Table the needed Columns in
        /// order to execute the statement
        /// </summary>
        private Dictionary<string, List<string>> mColumnsForTables;
        private TGSqlParser mParser;
        #endregion

        public GetColumnsAndTables(TSelectSqlStatement pSelect) {
            mColumnsForTables = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            SqlNodeIterator lSql_iter = new SqlNodeIterator(pSelect);
            lSql_iter.Convert();
            UniqueMergeHelper(lSql_iter.ColumnsForTable);
            TLzCustomExpression lWhere = pSelect.WhereClause;
            lSql_iter.Destroy();
            if (lWhere != null) {
                lSql_iter = new SqlNodeIterator(lWhere);
                lSql_iter.Convert();
                UniqueMergeHelper(lSql_iter.ColumnsForTable);
            }
            JoinIterator lJoin_iter = new JoinIterator();
            lJoin_iter.IterateJoinList(pSelect.JoinTables);
            UniqueMergeHelper(lJoin_iter.TablesAndColumns);
        }

        public GetColumnsAndTables(TGSqlParser pParser) {
            mParser = pParser;
            mColumnsForTables = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (TSourceToken lToken in pParser.SourceTokenList) {
                string lTableName = string.Empty;
                switch (lToken.DBObjType) {
                    case TDBObjType.ttObjTable:
                        if (!mColumnsForTables.Keys.Contains(lToken.AsText)) {
                            mColumnsForTables.Add(lToken.AsText, new List<string>());
                        }
                        break;
                    case TDBObjType.ttObjField:
                        TSourceToken lParent = lToken.ParentToken;
                        if (lParent.DBObjType == TDBObjType.ttObjTableAlias) {
                            lTableName = lParent.RelatedToken.RelatedToken.AsText;
                        } else if (lParent.DBObjType == TDBObjType.ttObjTable) {
                            lTableName = lParent.AsText;
                        }
                        if (!mColumnsForTables.Keys.Contains(lTableName)) { mColumnsForTables.Add(lTableName, new List<string>()); }
                        if (!mColumnsForTables[lTableName].Contains(lToken.AsText)) { mColumnsForTables[lTableName].Add(lToken.AsText); }
                        break;
                    case TDBObjType.ttObjUnknown:
                        break;
                }
            }
        }

        public Dictionary<string, List<string>> GetColumnsForTables {
            get { return mColumnsForTables; }
        }

        private void UniqueMergeHelper(Dictionary<string, List<string>> pToMerge) {
            foreach (KeyValuePair<string, List<string>> lToMerge in pToMerge) {
                if (!(mColumnsForTables.Keys.Contains(lToMerge.Key))) {
                    mColumnsForTables.Add(lToMerge.Key, new List<string>());
                }
                foreach (string newColumn in lToMerge.Value) {
                    if (!(mColumnsForTables[lToMerge.Key].Contains(newColumn))) {
                        mColumnsForTables[lToMerge.Key].Add(newColumn);
                    }
                }
            }
        }
    }
}
