using Taxonomy;

namespace eBalanceKitBusiness {

    /// <summary>
    /// This interface must be implemented to assign an xbrl element for a known 
    /// internal element it, using the GCDTaxonomyIdManager.AssignElements() method.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-06-13</since>
    public interface ITaxonomyAssignment {
    
        /// <summary>
        /// Assigned internal element id.
        /// </summary>
        int AssignedElementId { get; set; }

        /// <summary>
        /// Assigned taxonomy element. Use Manager.GCDTaxonomyIdManager to assign a taxonomy element for a known internal taxonomy element id.
        /// </summary>
        IElement AssignedElement { get; set; }

        bool HasAssignment { get; }
    }
}
