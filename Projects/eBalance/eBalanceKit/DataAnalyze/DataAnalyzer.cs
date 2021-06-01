/* 
 * USAGE:
 * Check whether the input data set is valid:
 * new DataAnalyzer(
 *   Strategy.IsPositiveOrZeroAnalyze,
 *   new List<Object>() { 2, 4, 1 },
 *   Treshold.EveryDataShouldBeSame)
 *      .DoAnalyze()
 *      .Valid;
 */
using System;
using System.Collections.Generic;
using DataAnalyze.Model;
using DataAnalyze.Strategies.Evaluator;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace DataAnalyze
{
    public class DataAnalyzer
    {
        #region Constructor
        /// <summary>
        /// The default constructor.
        /// </summary>
        public DataAnalyzer()
        {
            ResultSet = new List<ResultItem>();
        }

        /// <summary>
        /// The overridden constructor for defining all the datas.
        /// </summary>
        /// <param name="DesiredStrategy">The desired strategy to do the analysis.</param>
        /// <param name="Input">The input data set.</param>
        /// <param name="DesiredTreshold">The desired evaluation treshold.</param>
        public DataAnalyzer(Strategy DesiredStrategy, List<Object> Input, Treshold DesiredTreshold)
        {
            this.DesiredStrategy = DesiredStrategy;
            this.Input = Input;
            this.DesiredTreshold = DesiredTreshold;

            ResultSet = new List<ResultItem>();
        }

        /// <summary>
        /// The overridden constructor for defining only the input data set.
        /// </summary>
        /// <param name="Input">The input data set.</param>
        public DataAnalyzer(List<Object> Input)
        {
            this.Input = Input;

            ResultSet = new List<ResultItem>();
        }
        #endregion Constructor

        #region Properties
        /// <summary>
        /// Desired strategy to execute.
        /// </summary>
        private Strategy DesiredStrategy { get; set; }

        /// <summary>
        /// The input data set.
        /// </summary>
        private List<Object> Input { get; set; }

        /// <summary>
        /// The result set from the processor.
        /// </summary>
        private List<ResultItem> ResultSet { get; set; }

        /// <summary>
        /// The desired treshold evaluated with.
        /// </summary>
        private Treshold DesiredTreshold { get; set; }

        /// <summary>
        /// Does the input datas are valid within the given strategy and desired treshold?
        /// </summary>
        public bool Valid { get; private set; }

        #endregion Properties

        #region Methods
        /// <summary>
        /// Does the analysis on the input.
        /// </summary>
        /// <returns>The reference of theis object.</returns>
        public DataAnalyzer DoAnalyze()
        {
            if (Input != null && Input.Count > 0)
            {
                AnalyzerCore c = new AnalyzerCore(DesiredStrategy, Input);
                Valid = DoEvaluate(c.ResultSet);
            }

            return this;
        }

        /// <summary>
        /// Does the evaluation of the result set.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private bool DoEvaluate(List<ResultItem> list)
        {
            IServiceLocator locator = ServiceLocator.Current;
            IEvaluator evaluator = locator.GetInstance<IEvaluator>(DesiredTreshold.ToString());

            evaluator.Evaluate(list);

            return evaluator.Valid;
        }

        /// <summary>
        /// Initialize the Unity service.
        /// </summary>
        private void InitializeUnityService()
        {
            UnityServiceLocator serviceLocator = new UnityServiceLocator(CreateConfiguredUnityContainer());
            ServiceLocator.SetLocatorProvider(() => serviceLocator);
        }

        /// <summary>
        /// This function creates the configured unity
        /// container from configuration file.
        /// </summary>
        /// <returns>The prepared IUnityContainer.</returns>
        private IUnityContainer CreateConfiguredUnityContainer()
        {
            IUnityContainer container = new UnityContainer();

            // Load static config from the *.xml file
            container.LoadConfiguration();

            return container;
        }
        #endregion Methods
    }
}
