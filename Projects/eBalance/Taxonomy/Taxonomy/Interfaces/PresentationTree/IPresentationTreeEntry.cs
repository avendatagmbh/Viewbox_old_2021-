// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-06-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Taxonomy.Interfaces.PresentationTree {
    /// <summary>
    /// Interface for presentation tree entries (special leaf node types, e.g. used to implement the Account class).
    /// </summary>
    public interface IPresentationTreeEntry : ISearchableNode {
        /// <summary>
        /// Enumeration of all parent nodes.
        /// </summary>
        IEnumerable<IPresentationTreeNode> Parents { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is a root node.
        /// </summary>
        /// <value><c>true</c> if this instance is root; otherwise, <c>false</c>.</value>
        bool IsRoot { get; }

        /// <summary>
        /// This value influences the position in the children enumeration of the parent node.
        /// </summary>
        double Order { get; }

        /// <summary>
        /// Returns true, if the node is a hypercube container. This 
        /// is true iif. at least one child node is a hyper cube.
        /// </summary>
        bool IsHypercubeContainer { get; }

        /// <summary>
        /// Adds the specified parent node to the Parents enumeration.
        /// </summary>
        /// <param name="parent"></param>
        void AddParent(IPresentationTreeNode parent);

        /// <summary>
        /// Removes the specified parent node from the Parents enumeration.
        /// </summary>
        /// <param name="parent"></param>
        void RemoveParent(IPresentationTreeNode parent);

        /// <summary>
        /// Removes the specified item from all parent nodes.
        /// </summary>
        void RemoveFromParents();
    }
}