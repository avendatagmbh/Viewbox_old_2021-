using System.Collections.Generic;
using System.Xml.Serialization;
using DbSearchLogic.SearchCore.Structures.Result;

namespace DbSearchLogic.SearchCore.Result
{
    /// <summary>
    /// ColumnHit stores information about all column hits.
    /// </summary>
    /// <author>Dennis Hamerla, Mirko Dibbert</author>
    /// <company>AvenDATA GmbH</company>
    /// <since>01.01.2010</since>
    public class ColumnHit
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnHit"/> class.
        /// </summary>
        public ColumnHit(TableResultSet tableResultSet, string searchColumnName)
        {
            // initialize variables
            TableResultSet = tableResultSet;
            SearchColumnName = searchColumnName;
            TableColumns = new List<ColumnHitInfo>();
        }


        public TableResultSet TableResultSet { get; set; }
        /// <summary>
        /// Gets or sets the name of the search column of this hit.
        /// </summary>
        public string SearchColumnName { get; set; }

        /// <summary>
        /// Gets or sets a list of all column hits for the assigned table and search column.
        /// </summary>
        public List<ColumnHitInfo> TableColumns { get; set; }
    }
}