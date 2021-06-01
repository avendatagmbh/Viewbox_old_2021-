using System.Collections.Generic;
using DataAnalyze.Model;

namespace DataAnalyze.Strategies.Evaluator {
    internal class EveryDataShouldBeSameEvaluator : IEvaluator {
        #region Properties
        /// <summary>
        /// [True] if the result set with given evaluation seems valid, otherwise [False].
        /// </summary>
        public bool Valid { get; set; }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Does the evaluation.
        /// </summary>
        /// <param name="ResultSet">The input set to be checked.</param>
        public void Evaluate(List<ResultItem> ResultSet) {
            Valid = true;
            bool? LastValue = null;

            foreach (ResultItem item in ResultSet) {
                if (item.ResultType == Result.Indeterminable
                    || (LastValue != null
                        && LastValue != (item.ResultType == Result.True))) {
                    Valid = false;
                    return;
                }

                LastValue = (item.ResultType == Result.True);
            }

            // Convert bool? back to bool.
            Valid = (LastValue == true);
        }
        #endregion Methods
    }
}