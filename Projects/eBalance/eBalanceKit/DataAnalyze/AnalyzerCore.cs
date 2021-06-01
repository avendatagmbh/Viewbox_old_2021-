using System;
using System.Collections.Generic;
using DataAnalyze.Model;
using DataAnalyze.Strategies.Analyzer;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace DataAnalyze
{
    internal class AnalyzerCore
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyzerCore" /> class.
        /// </summary>
        /// <param name="DesiredStrategy">The desired strategy.</param>
        /// <param name="Inputs">The inputs.</param>
        public AnalyzerCore(Strategy DesiredStrategy, List<Object> Inputs)
        {
            // Initializing properties.
            Source = Inputs;
            this.DesiredStrategy = DesiredStrategy;

            // Initialize Unity service
            InitializeUnityService();

            // Perform the test
            ResultSet = DoAnalyze();
        }
        #endregion Constructor

        #region Properties

        /// <summary>
        /// List of elements that needed to be analyzed.
        /// </summary>
        public List<object> Source { get; private set; }

        /// <summary>
        /// The desired analysis strategy.
        /// </summary>
        public Strategy DesiredStrategy { get; private set; }

        /// <summary>
        /// The result set container.
        /// </summary>
        public List<ResultItem> ResultSet { get; private set; }

        #endregion Properties

        #region Methods
        /// <summary>
        /// Perform the analysis.
        /// </summary>
        /// <returns>The result set of the items.</returns>
        private List<ResultItem> DoAnalyze()
        {
            IServiceLocator locator = ServiceLocator.Current;
            IAnalyzer analyzer = locator.GetInstance<IAnalyzer>(DesiredStrategy.ToString());

            List<ResultItem> resultSet = new List<ResultItem>();

            foreach (Object o in Source)
            {
                analyzer.Analyze(o);
                resultSet.Add(analyzer.Result);
            }

            return resultSet;
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
