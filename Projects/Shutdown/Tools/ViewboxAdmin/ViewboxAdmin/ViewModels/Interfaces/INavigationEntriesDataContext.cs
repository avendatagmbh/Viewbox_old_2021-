using System.Collections.Generic;
using ViewboxAdmin.Structures;

namespace ViewboxAdmin.ViewModels {
    public interface INavigationEntriesDataContext {
        INavigationTree NavigationTree { get; }
        Dictionary<string, INavigationTreeEntry> NavigationTreeEntries { get; set; }
        object this[string NavTreeItem] { set; }
    }
}