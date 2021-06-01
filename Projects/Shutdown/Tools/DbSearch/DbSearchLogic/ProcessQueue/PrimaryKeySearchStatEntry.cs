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
    public class PrimaryKeySearchStatEntry : KeySearchStatEntityBase, INotifyPropertyChanged, IPrimaryKeySearchStatEntry {
        
        #region [ Constructor ]

        public PrimaryKeySearchStatEntry() { }

        public PrimaryKeySearchStatEntry(int keyComplexity, int numberOfKeyCandidates, CancellationTokenSource cancellationTokenSource, string profileName) {
            KeyComplexity = keyComplexity;
            NumberOfKeyCandidates = numberOfKeyCandidates;
            CancellationTokenSource = cancellationTokenSource;
            ProfileName = profileName;
            Info = string.Format("[{0}]", ProfileName);
            NumberOfKeysFound = 0; 
            SearchStarted = DateTime.UtcNow;
        }

        #endregion [ Constructor ]

        /// <inheritdoc />
        public override KeySearchTypeEnum SearchType { get { return KeySearchTypeEnum.PrimaryKeySearch; } }
    }
}
