using System;
using System.Collections.Generic;
using System.Text;
using DbSearchLogic.SearchCore.Structures.Db;

namespace DbSearchLogic.SearchCore.TableRelation {

    public class TableRelation {

        /// <summary>
        /// Initializes a new instance of the <see cref="TableRelation"/> class.
        /// </summary>
        /// <param name="baseColumns">The base columns.</param>
        /// <param name="foreignColumns">The foreign columns.</param>
        /// <param name="row">The row.</param>
        public TableRelation(DbColumnSet baseColumns, DbColumnSet foreignColumns) {

            this.Columns = new List<TableRelationColumns>();
            for (int i = 0; i < baseColumns.Columns.Count; i++) {
                this.Columns.Add(new TableRelationColumns(baseColumns.Columns[i], foreignColumns.Columns[i]));
            }

            this.Id = 0;
            this.BaseTable = baseColumns.TableName;
            this.ForeignTable = foreignColumns.TableName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableRelation"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="baseTable">The base table.</param>
        /// <param name="foreignTable">The foreign table.</param>
        public TableRelation(UInt32 id, string baseTable, string foreignTable) {
            this.Id = id;
            this.BaseTable = baseTable;
            this.ForeignTable = foreignTable;
            this.Columns = new List<TableRelationColumns>();            
        }

        public UInt32 Id { get; private set; }

        /// <summary>
        /// Gets or sets the base table.
        /// </summary>
        /// <value>The base table.</value>
        public string BaseTable { get; private set; }

        /// <summary>
        /// Gets or sets the foreign table.
        /// </summary>
        /// <value>The foreign table.</value>
        public string ForeignTable { get; private set; }

        /// <summary>
        /// Gets or sets the columns.
        /// </summary>
        /// <value>The columns.</value>
        public List<TableRelationColumns> Columns { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(BaseTable + "," + ForeignTable);
            foreach (TableRelationColumns cols in this.Columns) {
                sb.Append("," + cols.Base + "->" + cols.Foreign);
            }
            return sb.ToString();
        }
    }
}
