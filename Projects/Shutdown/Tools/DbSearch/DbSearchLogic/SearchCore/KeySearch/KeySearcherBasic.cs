using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using AvdCommon.Logging;
using DbAccess;
using DbAccess.Structures;
using DbSearchLogic.Config;
using DbSearchLogic.SearchCore.Structures.Db;
using System.Linq;
using AV.Log;

namespace DbSearchLogic.SearchCore.KeySearch
{
    /// <summary>
    /// A primary key searcher BASIC approach
    /// </summary>
    public class KeySearcherBasic : KeySearcherBase {

        #region [ Members ]
        
        #endregion [ Members ]

        #region [ Constructors ]

        /// <summary>
        /// The object constructors
        /// </summary>
        /// <param name="currentProfile">The loaded profile</param>
        public KeySearcherBasic(Profile currentProfile, Func<string, IEnumerable<KeyCandidate>> actionGetAllColumnsAsKey, Func<IEnumerable<KeyCandidate>, int, IEnumerable<KeyCandidate>> actionCreateKeyCombinations) {
            logPrefix = "Key search (SQL approach)";
            this.currentProfile = currentProfile;
            this.actionGetAllColumnsAsKey = actionGetAllColumnsAsKey;
            this.actionCreateKeyCombinations = actionCreateKeyCombinations;
            programInfo.HostName = currentProfile.DbProfile.DbConfigView.Hostname;
            programInfo.DbName = currentProfile.DbProfile.DbConfigView.DbName;
        }

        #endregion [ Constructors ]
        
        #region [ Public methods ]
        
        /// <summary>
        /// Init the composite key search
        /// </summary>
        /// <param name="keySearchParameter">The parameters</param>
        /// <returns>True if key search initialized and can be performed</returns>
        public override bool InitCompositeKeySearch(KeySearchParameter keySearchParameter) {
            _log.Log(LogLevelEnum.Info, logPrefix + " - composite key search initialization started.", true);
            
            // do not start composite key search if key candidates in simple key init are not initialized
            if (simpleKeysInitialized)
                compositeKeysInitialized = true;
            else
                compositeKeysInitialized = false;

            _log.Log(LogLevelEnum.Info, logPrefix + " - composite key search initialization finished!", true);
            return compositeKeysInitialized;
        }

        #endregion [ Public methods ]
    }
}
