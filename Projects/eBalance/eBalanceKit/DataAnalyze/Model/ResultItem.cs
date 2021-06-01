namespace DataAnalyze.Model {
    public class ResultItem {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultItem" /> class.
        /// </summary>
        public ResultItem() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultItem" /> class.
        /// </summary>
        /// <param name="ResultType">Type of the result.</param>
        public ResultItem(Result ResultType) { this.ResultType = ResultType; }
        #endregion

        #region Properties
        /// <summary>
        /// The result of the analyzation.
        /// </summary>
        public Result ResultType;
        #endregion
    }
}