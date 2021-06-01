using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness.Structures {
    #region Nested type: Page
    public class Page {
        public Page(string columnBinding, string columnTitle) {
            ColumnTitle = columnTitle;
            ColumnBinding = columnBinding;
        }

        public string ColumnTitle { get; private set; }
        public string ColumnBinding { get; private set; }
    }
    #endregion

    #region Nested type: Pages
    public class Pages {
        private readonly List<Page> _pages;
        private int _currentPage;

        public Pages(List<Page> pages) { _pages = pages; }
        public void Add(Page page) { _pages.Add(page); }
        public void Delete(Page page) { _pages.Remove(page); }

        public void SwitchToFirstPage() { _currentPage = 0; }
        public bool HasNextPage() { return _currentPage < _pages.Count - 1; }
        public bool HasPreviousPage() { return _currentPage > 0; }

        public Page CurrentPage() { return _pages[_currentPage]; }

        public Page PreviousPage() { return _pages[--_currentPage]; }

        public Page NextPage() { return _pages[++_currentPage]; }

        public List<Page> PageList { get { return _pages; } }
    }
    #endregion
}
