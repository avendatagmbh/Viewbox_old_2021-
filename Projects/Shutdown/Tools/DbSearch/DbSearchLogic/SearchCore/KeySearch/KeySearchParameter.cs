using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbSearchLogic.ProcessQueue;

namespace DbSearchLogic.SearchCore.KeySearch{
    /// <summary>
    /// The parameters of the key search start method
    /// </summary>
    public class KeySearchParameter{
        #region [ Public Properties ]

        #region [ Start Parameters ]

        /// <summary>
        /// The number of tasks
        /// </summary>
        public int DegreeOfParallelism { get; private set; }

        /// <summary>
        /// Degree of composite key complexity
        /// </summary>
        public int DegreeOfKeyComplexity { get; private set; }

        /// <summary>
        /// The number of the current task
        /// </summary>
        public int TaskNumber{ get; set; }
        
        /// <summary>
        /// The cancellationToken wich can stops the mechanism
        /// </summary>
        public System.Threading.CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// The key collector instance
        /// </summary>
        public KeyCollector KeyCollectorInstance { get; set; }

        /// <summary>
        /// Entry for providing statistics information about an initated primary key search
        /// </summary>
        public IPrimaryKeySearchStatEntry InitiatedKeySearch { get; internal set; }

        #endregion [ Start Parameters ]

        #region [ Init Parameters ]

        /// <summary>
        /// The threshold value to decide how many table.rows process to SQL (else IDX)
        /// </summary>
        public long TresholdRowsCount { get; set; }

        /// <summary>
        /// The used threshold method
        /// </summary>
        public TresholdRowsSelector TresholdRowsMethod { get; set; }

        /// <summary>
        /// The treshold uses alg.
        /// </summary>
        public enum TresholdRowsSelector {
            NONE,
            AVG,
            MEDIAN
        }

        /// <summary>
        /// The threshold value to decide how many items need to save to the database (batch saving)
        /// </summary>
        public int TresholdDbSave { get; set; }

        #endregion [ Init Parameters ]

        #endregion [ Public Properties ]
        
        #region [ Constructors ]
        
        /// <summary>
        /// Constructor
        /// </summary>
        public KeySearchParameter(int degreeOfKeyComplexity, int degreeOfParallelism, IPrimaryKeySearchStatEntry keySearchStatEntry) {
            DegreeOfKeyComplexity = degreeOfKeyComplexity;
            DegreeOfParallelism = degreeOfParallelism;
            InitiatedKeySearch = keySearchStatEntry;
        }

        #endregion [ Constructors ]

    }
}
