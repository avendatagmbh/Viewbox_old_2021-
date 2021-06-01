using System;
using System.Text.RegularExpressions;
using DataAnalyze.Model;

namespace DataAnalyze.Strategies.Analyzer {
    internal class IsNumericAnalyze : IAnalyzer {
        #region Properties
        /// <summary>
        /// The result container.
        /// </summary>
        public ResultItem Result { get; set; }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Does the analysis.
        /// It checks whether the input item is null (otherwise indeterminable).
        /// /// It checks whether the input item numeric (otherwise false).
        /// </summary>
        /// <param name="Item"></param>
        public void Analyze(Object Item) {
            Result = Item == null
                         ? new ResultItem(DataAnalyze.Result.Indeterminable)
                         : (IsNumeric(Item)
                                ? new ResultItem(DataAnalyze.Result.True)
                                : new ResultItem(DataAnalyze.Result.False));
        }

        /// <summary>
        /// Is the object numerical? If Yes, the value will be give back by reference.
        /// </summary>
        /// <param name="_expression">The proofen Object.</param>
        /// <returns>
        /// <c>true</c> if object is numeric, else <c>false</c>.
        /// </returns>
        private bool IsNumeric(object _expression) {
            if (_expression == null) {
                return false;
            }

            String tmpValue = _expression.ToString();

            Match match = Regex.Match(tmpValue,
                                      @"^[+-]?[0-9]{1,3}(?:(?:(?:,?[0-9]{3})+(?:\.[0-9]*)?)|(?:(?:,?[0-9]{3}){0}(?:\.[0-9]{1,2})?))");
            return match.Success;
        }
        #endregion Methods
    }
}