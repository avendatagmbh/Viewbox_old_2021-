using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DbSearchLogic.SearchCore.KeySearch;

namespace DbSearchLogic.ProcessQueue {

    /// <inheritdoc />
    public class ForeignKeySearchStatEntry : KeySearchStatEntityBase, IForeignKeySearchStatEntry {

        #region [ Constructor ]

        public ForeignKeySearchStatEntry() { }

        public ForeignKeySearchStatEntry(int keyComplexity, int numberOfKeyCandidates, CancellationTokenSource cancellationTokenSource, string profileName, string foreignKeyTable) {
            KeyComplexity = keyComplexity;
            NumberOfKeyCandidates = numberOfKeyCandidates;
            CancellationTokenSource = cancellationTokenSource;
            ProfileName = profileName;
            ForeignKeyTableName = foreignKeyTable;
            Info = string.Format("[{0}].[{1}]", ProfileName, ForeignKeyTableName);
            NumberOfKeysFound = 0;
            SearchStarted = DateTime.UtcNow;
        }

        #endregion [ Constructor ]

        /// <inheritdoc />
        public override KeySearchTypeEnum SearchType { get { return KeySearchTypeEnum.ForeignKeySearch; } }

        /// <inheritdoc />
        public string ForeignKeyTableName { get; internal set; }
    }
}
