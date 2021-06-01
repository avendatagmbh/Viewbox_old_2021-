using System;
using DataAnalyze.Model;

namespace DataAnalyze.Strategies.Analyzer {
    internal class DummyAnalyze : IAnalyzer {
        #region Properties
        /// <summary>
        /// The result container.
        /// </summary>
        public ResultItem Result { get; set; }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Does a dummy analysis.
        /// In every input it returns indeterminable result.
        /// </summary>
        /// <param name="Item"></param>
        public void Analyze(Object Item) { Result = new ResultItem(DataAnalyze.Result.Indeterminable); }
        #endregion Methods
    }
}