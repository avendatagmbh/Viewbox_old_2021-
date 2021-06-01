namespace DbSearchLogic.SearchCore.TableRelation {

    /// <summary>
    /// 
    /// </summary>
    public class TableRelationColumns {

        /// <summary>
        /// Initializes a new instance of the <see cref="TableRelationColumns"/> class.
        /// </summary>
        /// <param name="baseColumn">The base column.</param>
        /// <param name="foreignColumn">The foreign column.</param>
        public TableRelationColumns(string baseColumn, string foreignColumn) {
            this.Base = baseColumn;
            this.Foreign = foreignColumn;
        }

        /// <summary>
        /// Gets or sets the base.
        /// </summary>
        /// <value>The base.</value>
        public string Base { get; private set; }

        /// <summary>
        /// Gets or sets the foreign.
        /// </summary>
        /// <value>The foreign.</value>
        public string Foreign { get; private set; }
    }
}
