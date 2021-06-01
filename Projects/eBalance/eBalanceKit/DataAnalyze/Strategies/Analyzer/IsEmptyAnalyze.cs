using System;
using DataAnalyze.Model;

namespace DataAnalyze.Strategies.Analyzer {
    internal class IsEmptyAnalyze : IAnalyzer {
        #region Properties
        /// <summary>
        /// The result container.
        /// </summary>
        public ResultItem Result { get; set; }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Does the analysis.
        /// It checks whether the input item is null or empty (otherwise false).
        /// </summary>
        /// <param name="Item"></param>
        public void Analyze(Object Item) {
            Result = Item == null
                         ? new ResultItem(DataAnalyze.Result.True)
                         : (Item.ToString() == ""
                                ? new ResultItem(DataAnalyze.Result.True)
                                : new ResultItem(DataAnalyze.Result.False));
        }
        #endregion Methods
    }
}