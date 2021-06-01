using System;
using System.Text;
using System.Collections.Generic;
using DataAnalyze.ComplexConfigurations;

namespace DataAnalyze.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class Anomaly
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Anomaly" /> class.
        /// </summary>
        public Anomaly()
        {
            CreationDate = DateTime.Now;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The row number where the anomaly is.
        /// </summary>
        /// <value>
        /// The row number.
        /// </value>
        public int RowNumber { get; set; }

        /// <summary>
        /// Problem type. Selected from a finite list.
        /// </summary>
        /// <value>
        /// The problem.
        /// </value>
        public ProblemType Problem { get; set; }

        /// <summary>
        /// Nice problem, shortly. It's for displaying the data.
        /// </summary>
        /// <value>
        /// The nice problem.
        /// </value>
        public String NiceProblem { get; set; }

        /// <summary>
        /// The anomaly description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public String Description { get; set; }

        /// <summary>
        /// Possible cause of the anomaly, for solving problems.
        /// </summary>
        /// <value>
        /// The possible cause.
        /// </value>
        public String PossibleCause { get; set; }

        /// <summary>
        /// Possible solution or workaround for resolving the problems.
        /// </summary>
        /// <value>
        /// The possible solution.
        /// </value>
        public String PossibleSolution { get; set; }

        /// <summary>
        /// Problem creation date.
        /// </summary>
        /// <value>
        /// The creation date.
        /// </value>
        public DateTime CreationDate { get; private set; }

        /// <summary>
        /// Problem severity level.
        /// </summary>
        /// <value>
        /// The severity.
        /// </value>
        public Severity Severity { get; set; }

        /// <summary>
        /// Gets or sets the raw data.
        /// </summary>
        /// <value>
        /// The raw data.
        /// </value>
        public List<Object> RawData { get; set; }

        /// <summary>
        /// Do we have to display this anomaly?
        /// </summary>
        public bool Displayable
        {
            get { return (Severity != Severity.Information); }
        }

        public String SeverityName
        {
            get { return Severity.ToString(); }
        }

        public String FormattedDescription
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                builder.Append(Severity.ToString());
                builder.Append(" on row #");

                builder.Append((RowNumber + 1)); // Increase by 1 because of zero-based indexing.

                if (NiceProblem != null)
                {
                    builder.Append(", reason: ")
                    .Append(NiceProblem);
                }

                if (Description != null)
                {
                    builder.Append(" (")
                           .Append(Description)
                           .Append(")");
                }

                return builder.ToString();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Overrides the default ToString() method.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return FormattedDescription;
        }

        /// <summary>
        /// Get the raw datas from the anomaly.
        /// </summary>
        /// <returns></returns>
        public String Dump()
        {
            StringBuilder builder = new StringBuilder();

            int _count = 0;
            foreach (Object cell in RawData)
            {
                builder.Append(BalanceListImportValidator.Columns[_count]);

                if (cell.ToString().Length == 0)
                {
                    builder.Append("(empty)");
                }
                else
                {
                    builder
                        .Append("= \"")
                        .Append(cell.ToString())
                        .Append("\"");
                }

                if (++_count < RawData.Count)
                {
                    builder.Append(", ");
                }
            }

            return builder.ToString();
        }
        #endregion
    }
}
