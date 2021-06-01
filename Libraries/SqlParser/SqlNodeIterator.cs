using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gudusoft.gsqlparser;

namespace SqlParser {

    public class SqlNodeIterator : TLzVisitorAbs {

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlNodeIterator"/> class.
        /// </summary>
        /// <param name="pParseTree">The p parse tree.</param>
        public SqlNodeIterator(TLz_Node pParseTree) {
            this.ParseTree = pParseTree;
            this.Message = string.Empty;
            this.ColumnsForTable = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        }

        /*****************************************************************************************************/

        #region properties

        /// <summary>
        /// Gets or sets the topmost node of the Parsetree given to the instance.
        /// </summary>
        private TLz_Node ParseTree { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; private set; }

        /// <summary>
        /// Gets or sets the used Tables und the used columns for each table in the statement.
        /// </summary>
        /// <value>The tables and columns.</value>
        public Dictionary<String, List<String>> ColumnsForTable { get; private set; }

        #endregion properties

        /*****************************************************************************************************/

        #region methods

        public void Convert() {
            ParseTree.Iterate(this);
        }

        public void MergeUnique(Dictionary<string, List<string>> pToMerge) {
            foreach (KeyValuePair<string, List<string>> lMergeField in pToMerge) {
                if (!this.ColumnsForTable.Keys.Contains(lMergeField.Key)) {
                    this.ColumnsForTable.Add(lMergeField.Key, new List<string>());
                }
                foreach (string lMergeColumn in lMergeField.Value) {
                    if (!this.ColumnsForTable[lMergeField.Key].Contains(lMergeColumn)) {
                        this.ColumnsForTable[lMergeField.Key].Add(lMergeColumn);
                    }
                }
            }
        }

        public override void Execute(TLzVisitedAbs pVisited) {
            if (pVisited is TSourceToken) {
                TSourceToken lsrc_token = pVisited as TSourceToken;
                string lTableName = string.Empty;
                switch (lsrc_token.DBObjType) {
                    case TDBObjType.ttObjField:
                        TSourceToken lParent = lsrc_token.ParentToken;
                        if (lParent != null) {
                            this.Message += "Datenbank Feld: ";
                            lTableName = string.Empty;
                            if (lParent.DBObjType == TDBObjType.ttObjTableAlias) {
                                this.Message += lParent.RelatedToken.RelatedToken.AsText;
                                lTableName = lParent.RelatedToken.RelatedToken.AsText;
                            } else {
                                this.Message += lParent.AsText;
                                lTableName = lParent.AsText;
                            }
                        }
                        if (!(this.ColumnsForTable.Keys.Contains(lTableName))) {
                            this.ColumnsForTable.Add(lTableName, new List<string>());
                        }
                        if (!(this.ColumnsForTable[lTableName].Contains(lsrc_token.AsText))) {
                            this.ColumnsForTable[lTableName].Add(lsrc_token.AsText);
                        }
                        this.Message += "." + lsrc_token.AsText + "\n";
                        break;
                    case TDBObjType.ttObjTable:
                        this.Message += lsrc_token.AsText;
                        lTableName = string.Empty;
                        break;
                    case TDBObjType.ttObjUnknown:
                        break;
                    default:
                        break;
                }
            }
        }

        public override void PreExecute(TLzVisitedAbs pVisited) {
            if (pVisited is TLz_FuncCall) {
                TLz_FuncCall fn = pVisited as TLz_FuncCall;
                //if (mMessage.Length > 0) { mMessage += Environment.NewLine; }
                // mMessage += fn.AsText + "function name: " + fn.FunctionName;
                for (int k = 0; k < fn.args.Count(); k++) {
                    //if (mMessage.Length > 0) { mMessage += Environment.NewLine; }
                    //mMessage += (fn.args[k] as TLzCustomExpression).AsText;
                }
            } else if (pVisited is TLz_AliasClause) {
                TLz_AliasClause lAlias = pVisited as TLz_AliasClause;
                // if (mMessage.Length > 0) { mMessage += Environment.NewLine; }
                // mMessage += "ALIAS Definition nach: " + alias.aliastext;
            } else if (pVisited is TLzField) {
                TLzField lField = pVisited as TLzField;
                SqlNodeIterator lNodeIter;
                switch (lField.FieldType) {
                    case TLzFieldType.lftAttr:
                        TLz_Attr lAttr = lField.FieldAttr;
                        break;
                    case TLzFieldType.lftColumn:
                        break;
                    case TLzFieldType.lftExpression:
                        lNodeIter = new SqlNodeIterator(lField.FieldExpr);
                        lNodeIter.Convert();
                        this.MergeUnique(lNodeIter.ColumnsForTable);
                        lNodeIter.Destroy();
                        break;
                    case TLzFieldType.lftSubquery:
                        lNodeIter = new SqlNodeIterator(lField.SubQuery);
                        lNodeIter.Convert();
                        this.MergeUnique(lNodeIter.ColumnsForTable);
                        lNodeIter.Destroy();
                        break;
                    case TLzFieldType.lftUnknown:
                        break;
                }
            }
        }

        public override void PostExecute(TLzVisitedAbs pVisited) {

        }

        #endregion methods

    }
}
