using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Utils;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping.GlobalSearch;

namespace eBalanceKitBusiness.Structures.GlobalSearch
{
    public class GlobalSearchHistoryManager : NotifyPropertyChangedBase {

        #region constructor
        /// <summary>
        /// Singleton class to wrap the GlobalSearchHistoryItem search history instance.
        /// </summary>
        private GlobalSearchHistoryManager() {
            SearchHistoryStrings = new ObservableCollectionAsync<string>();
        }

        private static GlobalSearchHistoryManager _instance;
        public static GlobalSearchHistoryManager Instance { get { return _instance ?? (_instance = new GlobalSearchHistoryManager()); } }
        #endregion

        #region fields
        private GlobalSearchHistoryItem _globalSearchHistoryItem;
        #endregion fields

        #region properties
        /// <summary>
        /// this is the content of the global search combobox in CtlGlobalSearchContent.xaml
        /// </summary>
        public ObservableCollectionAsync<string> SearchHistoryStrings { get; set; } 

        /// <summary>
        /// The maximalized amount of searched strings.
        /// </summary>
        public LimitedQueue<string> SearchedStrings { get; set; }

        #region HistorySize
        /// <summary>
        /// Maximal history size. Setting this may reduce the length of the history runtime.
        /// </summary>
        private int _historySize = 5;
        public int HistorySize { 
            get { return _historySize; } 
            set { 
                _historySize = value;
                SearchedStrings.Limit = value; 
            }
        }
        #endregion HistorySize
        #endregion properties

        #region methods
        /// <summary>
        /// The actual unwrap. SearchedStrings is the limited queue. SearchHistoryStrings is the output on the UI.
        /// </summary>
        public void LoadHistory() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                _globalSearchHistoryItem =
                    conn.DbMapping.Load<GlobalSearchHistoryItem>(string.Format("{0} = {1}", conn.Enquote("user_id"),
                                                                               UserManager.Instance.CurrentUser.Id)).
                        FirstOrDefault() ?? new GlobalSearchHistoryItem {UserId = UserManager.Instance.CurrentUser.Id};
            }
            SearchedStrings = string.IsNullOrEmpty(_globalSearchHistoryItem.RawHistoryList)
                                  ? new LimitedQueue<string>(HistorySize)
                                  : DecodeSearchedStrings(_globalSearchHistoryItem.RawHistoryList);
            SearchHistoryStrings.Clear();
            foreach (string searchedString in SearchedStrings) {
                SearchHistoryStrings.Add(searchedString);
            }
        }

        /// <summary>
        /// The actual wrap. SearchedStrings is the limited queue. SearchHistoryStrings is the output on the UI.
        /// </summary>
        /// <param name="searchedString"></param>
        public void SaveString(string searchedString) {
            // we check if the searched string is already in the history. If yes we add doesn't duplicate, but move it to the first place.
            // same applies to the output strings.
            if (SearchedStrings.Contains(searchedString)) {
                LimitedQueue<string> temp = new LimitedQueue<string>(HistorySize);
                foreach (string oldString in SearchedStrings) {
                    if (oldString != searchedString) {
                        temp.Enqueue(oldString);
                    }
                }
                SearchedStrings = temp;
                // DevNote: the code below is not working properly. The search string disappears if I click on search.
                //SearchHistoryStrings.Move(SearchHistoryStrings.IndexOf(searchedString), HistorySize - 1);
            }// else {
            //    SearchHistoryStrings.RemoveAt(0);
            //    SearchHistoryStrings.Add(searchedString);
            //    SearchHistoryStrings.Move(0, HistorySize - 1);
            //}
            //SearchHistoryStrings = SearchHistoryStrings;
            //OnPropertyChanged("SearchHistoryStrings");
            SearchedStrings.Enqueue(searchedString);

            SearchHistoryStrings = new ObservableCollectionAsync<string>();
            foreach (string newString in SearchedStrings) {
                SearchHistoryStrings.Add(newString);
            }
            OnPropertyChanged("SearchHistoryStrings");

            // Encode, and save
            _globalSearchHistoryItem.RawHistoryList = EncodeSearchedStrings(SearchedStrings);
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.DbMapping.Save(_globalSearchHistoryItem);
            }
        }

        private string EncodeSearchedStrings(IEnumerable<string> searchedStrings) {
            XElement element = new XElement("root",
                                            from searchedString in searchedStrings
                                            select new XElement("v", searchedString));
            // no useless carrige return or tab
            return element.ToString(SaveOptions.DisableFormatting);
        }

        private LimitedQueue<string> DecodeSearchedStrings(string rawString) {
            LimitedQueue<string> ret = new LimitedQueue<string>(HistorySize);
            XElement element = XElement.Load(new StringReader(rawString));
            foreach (XElement xElement in element.Elements()) {
                ret.Enqueue(xElement.Value);
            }
            return ret;
        }

        #region unused methods
        // DEVNOTE: uncomment if change from xml to pipe separated list encoding of searchedstrings.
        /*private string CodeSearchedStrings(IEnumerable<string> searchedStrings) {
            List<string> escapedStrings = new List<string>();
            foreach (string searchedString in searchedStrings) {
                if (searchedString.EndsWith("\\")) {
                    escapedStrings.Add(searchedString.Replace("|", "\\|") + "\\");
                } else {
                    escapedStrings.Add(searchedString.Replace("|", "\\|"));
                }
            }
            return string.Join("|", escapedStrings);
        }

        private LimitedQueue<string> DecodeSearchedStrings(string rawString) {
            LimitedQueue<string> ret = new LimitedQueue<string>(HistorySize);
            string[] temp = rawString.Split('|');
            bool skipNext = false;
            for (int i = 0; i < temp.Length; i++) {
                if (skipNext) {
                    skipNext = false;
                    continue;
                }
                if (temp[i].EndsWith("\\")) {
                    ret.Enqueue(temp[i].Remove(temp[i].Length - 1) + "|" + temp[i + 1]);
                    skipNext = true;
                } else {
                    ret.Enqueue(temp[i]);
                }
            }
            return ret;
        }*/
        #endregion unused methods
        #endregion methods
    }
}
