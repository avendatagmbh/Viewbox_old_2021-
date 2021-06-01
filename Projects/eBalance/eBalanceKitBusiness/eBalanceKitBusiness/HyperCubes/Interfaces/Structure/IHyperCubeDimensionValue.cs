// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-26
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using Taxonomy;

namespace eBalanceKitBusiness.HyperCubes.Interfaces.Structure {
    /// <summary>
    /// This interface represents a hypercube value of a hypercube dimension.
    /// </summary>
    public interface IHyperCubeDimensionValue : INotifyPropertyChanged {

        /// <summary>
        /// Assigned hypercube dimension.
        /// </summary>
        IHyperCubeDimension Dimension { get; }

        /// <summary>
        /// Returns the hierarchical parent of this value or null, if this value is a root value.
        /// </summary>
        IHyperCubeDimensionValue Parent { get; }
        
        /// <summary>
        /// Returs an enumeration of all hierarchical child nodes.
        /// </summary>
        IEnumerable<IHyperCubeDimensionValue> Children { get; }

        /// <summary>
        /// Returns true, iif. this value has no hierarchical parents.
        /// </summary>
        bool IsRoot { get; }

        /// <summary>
        /// Returns the assigned XBRL element.
        /// </summary>
        IElement Element { get; }

        /// <summary>
        /// Returns the internal id of the assigned XBRL element.
        /// </summary>
        long ElementId { get; }
        
        /// <summary>
        /// Returns the label of this value.
        /// </summary>
        string Label { get; }

        bool IsSelected { get; set; }
    }
}