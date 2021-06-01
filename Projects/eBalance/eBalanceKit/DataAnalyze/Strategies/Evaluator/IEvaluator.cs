using System.Collections.Generic;
using DataAnalyze.Model;

namespace DataAnalyze.Strategies.Evaluator {
    public interface IEvaluator {
        /// <summary>
        /// The result of the evaluation.
        /// </summary>
        bool Valid { get; set; }

        /// <summary>
        /// The analyzer function.
        /// </summary>
        void Evaluate(List<ResultItem> ResultSet);
    }
}