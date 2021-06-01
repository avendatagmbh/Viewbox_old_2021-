using System;
using DataAnalyze.Model;

namespace DataAnalyze.Strategies.Analyzer {
    public interface IAnalyzer {
        /// <summary>
        /// The result of the analysis.
        /// </summary>
        ResultItem Result { get; set; }

        /// <summary>
        /// The analyzer function.
        /// </summary>
        void Analyze(Object Item);
    }
}