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
    /// Interface for internal presentation tree nodes.
    /// </summary>
    public interface IPresentationTreeNode : IPresentationTreeEntry, INotifyPropertyChanged,
                                             IEnumerable<IPresentationTreeEntry> {
        /// <summary>
        /// Assigned presentation tree.
        /// </summary>
        IPresentationTree PresentationTree { get; set; }

        /// <summary>
        /// True, iif. this node has an assinged computation rule to compute the value.
        /// </summary>
        bool IsComputed { get; }

        /// <summary>
        /// True, iif. this node is source of computation rule.
        /// </summary>
        bool IsComputationSource { get; }

        /// <summary>
        /// Enumeration of all child nodes.
        /// </summary>
        IEnumerable<IPresentationTreeEntry> Children { get; }

        /// <summary>
        /// Adds the specified child to the children enumeration.
        /// </summary>
        /// <param name="child"></param>
        void AddChildren(IPresentationTreeEntry child);

        /// <summary>
        /// Removes the specified child from the children enumeration.
        /// </summary>
        /// <param name="child"></param>
        void RemoveChildren(IPresentationTreeEntry child);
    }
}