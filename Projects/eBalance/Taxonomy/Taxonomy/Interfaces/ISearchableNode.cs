using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Taxonomy.Interfaces
{
    public interface ISearchableNode
    {
        event ScrollIntoViewRequestedEventHandler ScrollIntoViewRequested;
        event ScrollIntoViewRequestedEventHandler SearchLeaveFocusRequested;
        void ScrollIntoView(IList<ISearchableNode> path);
        void SearchLeaveFocus(IList<ISearchableNode> path);

        /// <summary>
        /// Gets the assigned xbrl element.
        /// </summary>
        IElement Element { get; }
    }

    public delegate void ScrollIntoViewRequestedEventHandler(IList<ISearchableNode> path);

    public class ScrollIntoViewRequestedEventArgs : EventArgs {
        public ScrollIntoViewRequestedEventArgs(IList<ISearchableNode> path) { Path = path; }

        public IList<ISearchableNode> Path { get; set; }
    }

}
