using System;
using System.Collections.Generic;
using DbAccess.Structures;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.Structures.Db;

namespace DbSearchLogic.SearchCore.Structures.Result {
    /// <summary>
    /// ColumnHit stores information about all column hits.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <company>AvenDATA GmbH</company>
    /// <since>05.02.2010</since>
    public class ColumnHitInfo {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnHitInfo"/> class.
        /// </summary>
        public ColumnHitInfo(
            TableInfo tableInfo,
            string columnName,
            DbColumnTypes columnType,
            List<SearchValueResult> results
            ) {
            TableInfo = tableInfo;
            ColumnName = columnName;
            ColumnType = columnType;
            Results = results;
            if (Results != null) {
                SetStatistics();
            }else Results = new List<SearchValueResult>();
        }

        #region Properties
        //public ColumnHit ColumnHit { get; set; }
        /// <summary>
        /// Gets or sets the name of the found column.
        /// </summary>
        public string ColumnName { get; private set; }
        public TableInfo TableInfo { get; set; }

        /// <summary>
        /// Gets or sets the type of the found column.
        /// </summary>
        public DbColumnTypes ColumnType { get; private set; }
        public List<SearchValueResult> Results { get; private set; }
        public int HitCount { get; private set; }
        public int MissingValuesCount { get; private set; }

        public double HitPercentage {
            get { return HitCount/(double) (Math.Max(1, HitCount + MissingValuesCount)); }
        }

        #endregion Properties

        #region Methods
        public void AddResults(IEnumerable<SearchValueResult> searchValueResults ) {
            Results.AddRange(searchValueResults);
            SetStatistics();
        }

        private void SetStatistics() {
            HitCount = 0;
            MissingValuesCount = 0;

            foreach (var result in Results) {
                if (result.HasResults) {
                    HitCount++;
                } else
                    MissingValuesCount++;
            }
        }
        #endregion Methods
    }
}