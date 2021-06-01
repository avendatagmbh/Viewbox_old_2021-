using System;
using System.Globalization;
using DataAnalyze.Model;

namespace DataAnalyze.Strategies.Analyzer {
    internal class IsNegativeOrZeroAnalyze : IAnalyzer {
        #region Constructor
        #endregion Constructor

        #region Properties
        /// <summary>
        /// The result container.
        /// </summary>
        public ResultItem Result { get; set; }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Does the analysis.
        /// It checks whether the input item is double (otherwise undetermined)
        /// and checks whether the input number is negative or zero (otherwise false)
        /// </summary>
        /// <param name="Item"></param>
        public void Analyze(Object Item) {
            if (IsNumeric(Item)) {
                double i = Convert.ToDouble(Item);

                Result = i <= 0 ? new ResultItem(DataAnalyze.Result.True) : new ResultItem(DataAnalyze.Result.False);
            } else {
                Result = new ResultItem(DataAnalyze.Result.Indeterminable);
            }
        }

        /// <summary>
        /// Is the object numerical? If Yes, the value will be give back by reference.
        /// </summary>
        /// <param name="expression">The proofen Object.</param>
        /// <returns>
        /// <c>true</c> if object is numeric, else <c>false</c>.
        /// </returns>
        private bool IsNumeric(object expression) {
            if (expression == null) {
                return false;
            }

            double number;
            return Double.TryParse(Convert.ToString(expression, CultureInfo.InvariantCulture), NumberStyles.Any,
                                   NumberFormatInfo.InvariantInfo, out number);
        }
        #endregion Methods
    }
}