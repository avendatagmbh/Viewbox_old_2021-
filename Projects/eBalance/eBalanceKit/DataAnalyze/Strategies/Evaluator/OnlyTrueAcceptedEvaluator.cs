using System.Collections.Generic;
using System.Linq;
using DataAnalyze.Model;

namespace DataAnalyze.Strategies.Evaluator {
    internal class OnlyTrueAcceptedEvaluator : IEvaluator {
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
            if (ResultSet.Any(item => item.ResultType != Result.True)) {
                Valid = false;
                return;
            }

            Valid = true;
        }
        #endregion Methods
    }
}