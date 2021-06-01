using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DbSearchLogic.ProcessQueue;
using DbSearchLogic.SearchCore.ForeignKeySearch;

namespace DbSearchLogic.SearchCore.KeySearch {
    /// <summary>
    /// The parameters of the foreign key sesarching start method
    /// </summary>
    public class ForeignKeySearchParameter {

        #region [ Public Properties ]

        /// <summary>
        /// The base table for foreign keys
        /// </summary>
        public string BaseTableName { get; set; }

        /// <summary>
        /// The base table for foreign keys
        /// </summary>
        public int BaseTableId { get; set; }

        /// <summary>
        /// The number of tasks
        /// </summary>
        public int TaskCount { get; set; }

        /// <summary>
        /// The columns (column permutations) of the base table which are the foreign key candidates
        /// </summary>
        public ConcurrentQueue<KeyCandidate> BaseTableColumns { get; set; }
        
        /// <summary>
        /// The primary keys (detected by key search) which can be the referenced primary keys by BaseTableColumns
        /// </summary>
        public ConcurrentQueue<Key> PrimaryKeys { get; set; }

        /// <summary>
        /// The keys of the base table
        /// </summary>
        public ConcurrentQueue<Key> BaseTableKeys { get; set; }

        /// <summary>
        /// The columns (column permutations) of the tables which are the foreign key candidates
        /// </summary>
        public ConcurrentQueue<KeyCandidate> ForeignKeyCandidates { get; set; }

        /// <summary>
        /// The cancellationToken wich can stops the mechanism
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Degree of composite key complexity
        /// </summary>
        public int DegreeOfKeyComplexity { get; private set; }

        /// <summary>
        /// The foreign search mode which indicates the direction of the foreign key search
        /// </summary>
        public ForeignKeySearchMode Mode { get; private set; }

        /// <summary>
        /// The already processed foreign keys
        /// </summary>
        public List<ForeignKey> ForeignKeysRelatedToTable { get; set; }

        /// <summary>
        /// The foreign key collector instance
        /// </summary>
        public ForeignKeyCollector ForeignKeyCollectorInstance { get; set; }

        /// <summary>
        /// Entry for providing statistics information about an initated key search
        /// </summary>
        public IKeySearchStatEntry InitiatedKeySearch { get; internal set; }

        #endregion [ Public Properties ]

        #region [ Constructors ]
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ForeignKeySearchParameter(string baseTableName, int baseTableId, int degreeOfKeyComplexity, ForeignKeySearchMode mode, IKeySearchStatEntry keySearchStatEntry) {
            BaseTableName = baseTableName;
            BaseTableId = baseTableId;
            DegreeOfKeyComplexity = degreeOfKeyComplexity;
            Mode = mode;
            InitiatedKeySearch = keySearchStatEntry;
            BaseTableColumns = new ConcurrentQueue<KeyCandidate>();
            PrimaryKeys = new ConcurrentQueue<Key>();
        }

        #endregion [ Constructors ]
    }

    /// <summary>
    /// Indicates the direction of the foreign key search
    /// </summary>
    public enum ForeignKeySearchMode {
        /// <summary>
        /// This is the default
        /// </summary>
        ForeignKeyInTable = 0,
        /// <summary>
        /// DEVNOTE: use only for individual tables
        /// </summary>
        ForeignKeyReferencesForTable = 1,
        /// <summary>
        /// DEVNOTE: use only for individual tables
        /// </summary>
        BothDirections = 2,
    }
}
