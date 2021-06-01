using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace DataAnalyze.Model
{
    class AnomalyContainer
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="AnomalyContainer" /> class.
        /// </summary>
        public AnomalyContainer() { }
        #endregion

        #region Properties
        private List<Anomaly> _anomalies = new List<Anomaly>();

        /// <summary>
        /// Gets the anomalies.
        /// </summary>
        /// <value>
        /// The anomalies.
        /// </value>
        public List<Anomaly> Anomalies
        {
            get { return _anomalies; }
            private set { _anomalies = value; }
        }

        /// <summary>
        /// Gets the skippable anomalies.
        /// </summary>
        /// <value>
        /// The skippable anomalies.
        /// </value>
        public List<Anomaly> SkippableAnomalies
        {
            get {
                return _anomalies.Where(i => i.Severity != Severity.Warning).ToList();
            }
        }

        /// <summary>
        /// Gets the skippable rows.
        /// </summary>
        /// <value>
        /// The skippable rows.
        /// </value>
        public List<int> SkippableRows
        {
            get {
                return SkippableAnomalies.Select(i => i.RowNumber).Distinct().ToList();
            }
        }

        /// <summary>
        /// Gets the error count.
        /// </summary>
        /// <value>
        /// The error count.
        /// </value>
        public int ErrorCount
        {
            get
            {
                return SkippableRows.Count();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has errors.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has errors; otherwise, <c>false</c>.
        /// </value>
        public bool HasErrors
        {
            get
            {
                return ErrorCount > 0;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the specified anomaly.
        /// </summary>
        /// <param name="anomaly">The anomaly.</param>
        public void Add(Anomaly anomaly)
        {
            if (_anomalies.All(q => q.RowNumber != anomaly.RowNumber))
            {
                _anomalies.Add(anomaly);
            }
        }

        /// <summary>
        /// Gets the displayable anomalies.
        /// </summary>
        /// <value>
        /// The displayable anomalies.
        /// </value>
        public List<Anomaly> DisplayableAnomalies
        {
            get {
                return _anomalies.Where(i => i.Displayable).ToList();
            }
        }

        /// <summary>
        /// Gets the problems.
        /// </summary>
        /// <returns></returns>
        public List<Anomaly> GetProblems()
        {
            return DisplayableAnomalies.OrderBy(q => q.RowNumber).Distinct().ToList();
        }

        /// <summary>
        /// Gets the problems table, for exporting the errors.
        /// </summary>
        /// <returns>DataTable that contains the problematic rows.</returns>
        public DataTable GetProblemTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add(eBalanceKitResources.Localisation.ResourcesValidation.TableHeaderID, typeof(int));
            table.Columns.Add(eBalanceKitResources.Localisation.ResourcesValidation.TableHeaderRowNumber, typeof(int));
            table.Columns.Add(eBalanceKitResources.Localisation.ResourcesValidation.TableHeaderDate, typeof(DateTime));
            table.Columns.Add(eBalanceKitResources.Localisation.ResourcesValidation.TableHeaderSeverity, typeof(String));
            table.Columns.Add(eBalanceKitResources.Localisation.ResourcesValidation.TableHeaderProblem, typeof(String));
            table.Columns.Add(eBalanceKitResources.Localisation.ResourcesValidation.TableHeaderRowDump, typeof(String));

            int row = 1;
            foreach (Anomaly an in DisplayableAnomalies.OrderBy(q => q.RowNumber))
            {

                table.Rows.Add(
                    row,
                   (an.RowNumber +1), // Row count zerobased
                    an.CreationDate,
                    an.Severity.ToString(),
                    an.NiceProblem,
                    an.Dump());

                row++;
            }

            return table;
        }
        #endregion
    }
}
