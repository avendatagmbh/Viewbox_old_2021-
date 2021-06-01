using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Taxonomy;
using Taxonomy.Interfaces.PresentationTree;

namespace eBalanceKitBase.Interfaces {
    public interface INavigationTreeEntryBase {
        IElement XbrlElem { get; }
        INavigationTreeEntryBase Parent { get; set; }
        IList<INavigationTreeEntryBase> Children { get; }
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        bool IsVisible { get; set; }
        bool ContainsHiddenValue { get; }
        object Model { get; set; }
        IEnumerable<IPresentationTreeEntry> PresentationTreeRoots { get; set; }
        string RoleId { get; set; }
        /// <summary>
        /// Id of the INavigationTreeEntryBase for internal use (load and save selected/expanded state)
        /// </summary>
        string NodeId { get; set; }
    }
}
