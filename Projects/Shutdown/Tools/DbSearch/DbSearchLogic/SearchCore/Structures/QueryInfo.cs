using System.Collections.Generic;
using DbAccess;
using DbSearchLogic.SearchCore.Config;

namespace DbSearchLogic.SearchCore.Structures {

    /// <summary>
    /// Klasse zur Aufnahme aller für eine Suche benötigten Parameter.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <company>AvenDATA GmbH</company>
    /// <since>26.01.2010</since>
    public class QueryInfo {

        #region Constructor

        public QueryInfo(
            QueryConfig query,
            bool bDistinctDbExists) {

            this.QueryConfig = query;

            string[,] values = new string[query.SearchValueMatrix.NumberOfRows, query.SearchValueMatrix.NumberOfColumns];
            for (int row = 0; row < query.SearchValueMatrix.NumberOfRows; row++) {
                for (int col = 0; col < query.SearchValueMatrix.NumberOfColumns; col++) {
                    values[row, col] = query.SearchValueMatrix.Values[row, col].String;
                }
            }

            DistinctDbExists = bDistinctDbExists;

            State = query.State;
            query.State.Reset();

            //Connections = new List<IDatabase>();
        }

        #endregion Constructor

        /// <summary>
        /// Konfiguration der Abfrage.
        /// </summary>
        public QueryConfig QueryConfig { get; set; }

        /// <summary>
        /// Gibt an, ob die Suche abgebrochen werden soll.
        /// </summary>
        public bool Cancel {
            get { return _cancel; }
            set { _cancel = value; }
        }
        private bool _cancel;

        /// <summary>
        /// Gibt an, ob die Suche Pausiert werden soll.
        /// </summary>
        public bool Pause { get; set; }

        /// <summary>
        /// Statusobjekt.
        /// </summary>
        public QueryState State { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [distinct db exists].
        /// </summary>
        /// <value><c>true</c> if [distinct db exists]; otherwise, <c>false</c>.</value>
        public bool DistinctDbExists { get; set; }
    }
}
