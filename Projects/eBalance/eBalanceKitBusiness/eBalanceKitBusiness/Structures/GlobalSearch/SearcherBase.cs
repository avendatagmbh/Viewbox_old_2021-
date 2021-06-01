using System;
using System.Threading;
using System.Windows.Threading;
using Utils;

namespace eBalanceKitBusiness.Structures.GlobalSearch
{
    abstract public class SearcherBase : NotifyPropertyChangedBase
    {
        #region constructor
        protected SearcherBase(DbMapping.Document document) { Document = document; InitForUserChange(); }
        #endregion constructor

        #region fields
        protected readonly DbMapping.Document Document;
        protected const int MaxExpandLimit = 20;
        #endregion fields

        #region methods

        #region abstract methods
        abstract public void BringIntoView(SearchResultItem value);
        abstract public void Search(string searchString, bool searchInId, bool searchInLabel, CancellationToken token);
        #endregion abstract methods

        #region virtual methods
        public virtual void ExpandAllNodes(CancellationToken token, int limit = -1) {
            int actual = 0;
            if (token.IsCancellationRequested) {
                //token.ThrowIfCancellationRequested();
                return;
            }
            foreach (var item in ResultTreeRoots) {
                if (((ObservableCollectionAsync<IGlobalSearcherTreeNode>) item.Children).Count <= 0) continue;
                if (token.IsCancellationRequested) {
                    //token.ThrowIfCancellationRequested();
                    break;
                }
                Dispatcher.CurrentDispatcher.Invoke((Action<object>) (state => {
                    var passedItem = (IGlobalSearcherTreeNode)state;
                    if (token.IsCancellationRequested) {
                        //token.ThrowIfCancellationRequested();
                        return;
                    }
                    passedItem.IsExpanded = true;
                }), DispatcherPriority.SystemIdle, item);
                actual++;
                actual = ((GlobalSearcherTreeNode) item).ExpandAllSubnodes(actual, limit, token);
                if (limit > -1 && actual > limit) {
                    break;
                }
            }
        }

        public virtual void CollapseAllNodes() {
            foreach (var item in ResultTreeRoots) {
                if (((ObservableCollectionAsync<IGlobalSearcherTreeNode>) item.Children).Count <= 0) continue;
                ((GlobalSearcherTreeNode) item).CollapseAllSubnodes();
                item.IsExpanded = false;
            }
        }
        #endregion virtual methods

        /// <summary>
        /// update the history in database
        /// </summary>
        /// <param name="searchString">the searched string</param>
        /// <param name="token">parameter to check if the save is still scheduled</param>
        public void SaveHistory(string searchString, CancellationToken token) {
            // DEVNOTE: the Search method could store the search string in class, and we could use that here, but the parameter passing
            // ensures that there is no value overlapping between tasks.
            // if the task in cancelled while saving, the save will continue. We are checking the IsCancellation in the beginning.
            if (token.IsCancellationRequested)
                //token.ThrowIfCancellationRequested();
                return;
            if (string.IsNullOrEmpty(searchString))
                return;
            GlobalSearchHistoryManager.Instance.SaveString(searchString);
        }

        public void InitForUserChange() {
            ResultTreeRoots = new ObservableCollectionAsync<IGlobalSearcherTreeNode>();
            GlobalSearchHistoryManager.Instance.LoadHistory();
        }
        #endregion methods

        #region properties
        // The elements are showed in a list in the DlgGlobalSearch. The name of each element is a path showed as a string.
        public ObservableCollectionAsync<IGlobalSearcherTreeNode> ResultTreeRoots { get; protected set; }
        #endregion
    }
}
