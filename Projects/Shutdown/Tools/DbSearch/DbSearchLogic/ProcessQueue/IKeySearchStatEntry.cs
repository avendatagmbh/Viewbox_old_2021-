using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DbSearchLogic.SearchCore.Events;
using DbSearchLogic.SearchCore.KeySearch;

namespace DbSearchLogic.ProcessQueue {

    /// <summary>
    /// Key search type enum
    /// </summary>
    public enum KeySearchTypeEnum {
        PrimaryKeySearch = 0,
        ForeignKeySearch = 1,
    }

    /// <summary>
    /// Entry for providing statistics information about an initated key search
    /// </summary>
    public interface IKeySearchStatEntry: IDeleted {

        /// <summary>
        /// The profile name
        /// </summary>
        string ProfileName { get; }
        /// <summary>
        /// The time when the search started
        /// </summary>
        DateTime SearchStarted { get; }
        /// <summary>
        /// Type of key search
        /// </summary>
        KeySearchTypeEnum SearchType { get; }
        /// <summary>
        /// Progress of the current search
        /// </summary>
        float Progress { get; }
        /// <summary>
        /// Set the progress manually
        /// </summary>
        /// <param name="progress"></param>
        void SetProgress(float progress);
        /// <summary>
        /// Displayed information of current search
        /// </summary>
        string Info { get; }
        /// <summary>
        /// Status information of current search
        /// </summary>
        string StatusDescription { get; set; }
        /// <summary>
        /// Maximum key complexity of current search
        /// </summary>
        int KeyComplexity { get; }
        /// <summary>
        /// The number of key candidates
        /// </summary>
        int NumberOfKeyCandidates { get; set; }
        /// <summary>
        /// The number of keys already processed
        /// </summary>
        int NumberOfKeysProcessed { get; }
        /// <summary>
        /// The number of keys found so far
        /// </summary>
        int NumberOfKeysFound { get; }
        /// <summary>
        /// The logic which handles one found key
        /// </summary>
        void KeyFound();
        /// <summary>
        /// The logic which handles one processed key candidate
        /// </summary>
        void KeyProcessed();
        /// <summary>
        /// The cancellation token source for the current task
        /// </summary>
        CancellationTokenSource CancellationTokenSource { get; }
        /// <summary>
        /// The current search task which does the key search when started
        /// </summary>
        Task<KeySearchResult> KeySearchTask { get; set; }
        /// <summary>
        /// The status of the current search task
        /// </summary>
        KeySearchResult TaskStatus { get; set; }
        /// <summary>
        /// Is selected flag
        /// </summary>
        bool IsSelected { get; set; }
        /// <summary>
        /// Cancels the current search task
        /// </summary>
        void CancelTask();
        /// <summary>
        /// Deletes the current search task
        /// </summary>
        void DeleteTask();
        /// <summary>
        /// Notify about the task is finished
        /// </summary>
        event EventHandler NotifyCompleted;
        /// <summary>
        /// Indicates whether the task is completed (even if it is failed or success)
        /// </summary>
        bool IsCompleted { get; }
        /// <summary>
        /// Indicates whether the task is running
        /// </summary>
        bool IsRunning { get; }
        /// <summary>
        /// Indicates whether the task is in waiting state
        /// </summary>
        bool IsWaiting { get; }
    }
}
