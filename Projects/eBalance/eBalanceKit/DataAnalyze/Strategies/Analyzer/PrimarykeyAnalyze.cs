using System;
using DataAnalyze.Model;
using System.Collections.Generic;

namespace DataAnalyze.Strategies.Analyzer
{
    class PrimaryKeyAnalyze : IAnalyzer
    {
        #region Properties

        /// <summary>
        /// The result container.
        /// </summary>
        public ResultItem Result { get; set; }

        /// <summary>
        /// It's for holding the used keys.
        /// </summary>
        public static List<String> UsedKeys { get; private set; }

        #endregion Properties
        #region Methods

        /// <summary>
        /// Does the analysis.
        /// This checks that every given item is unique in the list.
        /// If it's not, then the result will be false.
        /// </summary>
        /// <param name="Item"></param>
        public void Analyze(Object Item)
        {
            if (UsedKeys.Contains(Item.ToString()))
            {
                Result = new ResultItem(DataAnalyze.Result.False);
            }
            else
            {
                UsedKeys.Add(Item.ToString());
                Result = new ResultItem(DataAnalyze.Result.True);
            }
        }

        /// <summary>
        /// Resets the items container for further usage.
        /// </summary>
        public static void Reset()
        {
            UsedKeys = new List<string>();
        }
        #endregion Methods
    }
}