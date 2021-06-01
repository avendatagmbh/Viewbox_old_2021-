// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-14
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Xml.Serialization;
using DbSearchLogic.SearchCore.Structures.Db;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearchLogic.SearchCore.Result
{
    /// <summary>
    /// ColumnMap stores the mapping information of a search column to a table column
    /// </summary>
    public class ColumnMapping : IEquatable<ColumnMapping> {
        #region ColumnMapping

        public ColumnMapping(Column searchColumn, TableInfo resultTable, string tableColumnName)
        {
            SearchColumn = searchColumn;
            TableColumnName = tableColumnName;
            ResultTable = resultTable;
        }


        #endregion ColumnMapping

        #region Properties
        public TableInfo ResultTable { get; set; }
        public Column SearchColumn { get; set; }
        public string TableColumnName { get; set; }
        #endregion Properties

        /// <summary>
        /// Decides the equality of two ColumnMapping objects
        /// </summary>
        /// <param name="other">The column mapping to compared with</param>
        /// <returns>Result of the comparison</returns>
        public bool Equals(ColumnMapping other) {
            //return ResultTable == other.ResultTable && SearchColumn == other.SearchColumn && TableColumnName == other.TableColumnName;
            return ResultTable.Name == other.ResultTable.Name && SearchColumn.Query.TableName == other.SearchColumn.Query.TableName && SearchColumn.Name.ToLower() == other.SearchColumn.Name.ToLower() && TableColumnName == other.TableColumnName;
        }

        /// <summary>
        /// Returns a string view of the column map
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return this.SearchColumn.Name + " -> " + this.TableColumnName;
        }
    }
}