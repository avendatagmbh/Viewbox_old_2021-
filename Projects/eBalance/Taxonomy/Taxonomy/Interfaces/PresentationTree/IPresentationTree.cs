// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-06-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Taxonomy.Interfaces.PresentationTree {
    /// <summary>
    /// Interface for presentation trees.
    /// </summary>
    public interface IPresentationTree : INotifyPropertyChanged, IEnumerable<IPresentationTreeEntry>, IComparable {
        /// <summary>
        /// Gets the assigned taxonomy role.
        /// </summary>
        IRoleType Role { get; }

        /// <summary>
        /// Gets an enumeration of all root entries in the presentation tree.
        /// </summary>
        IEnumerable<IPresentationTreeNode> RootEntries { get; }

        /// <summary>
        /// Gets an enumeration of all nodes in the presetation tree.
        /// </summary>
        IEnumerable<IPresentationTreeNode> Nodes { get; }

        /// <summary>
        /// Gets an enumeration of all nodes in the presetation tree, which are defined as hypercube container.
        /// </summary>
        IEnumerable<IPresentationTreeNode> HypercubeContainerNodes { get; }

        /// <summary>
        /// Determines whether a node with the specified key could be found in the presentation tree.
        /// </summary>
        /// <param name="key">Name or id of the xbrl element, which is assigned to the requested node.</param>
        /// <returns>
        /// 	<c>true</c> if a node with the specified key could be found; otherwise, <c>false</c>.
        /// </returns>
        bool HasNode(string key);

        /// <summary>
        /// Gets the node with the specified taxonomy id or name.
        /// </summary>
        /// <param name="key">Name or id of the xbrl element, which is assigned to the requested node.</param>
        /// <returns>The node with the specified key.</returns>
        IPresentationTreeNode GetNode(string key);

    }
}