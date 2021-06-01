using eBalanceKitBusiness.Structures.DbMapping;
using System.ComponentModel;
using eBalanceKitBusiness.Structures.HyperCubes;

namespace eBalanceKitBusiness.Interfaces {
    
    /// <summary>
    /// Interfalce for hyper cube items
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-05-01</since>
    public interface IHyperCubeItem {

        event PropertyChangedEventHandler PropertyChanged;

        string Dim1XbrlElementId { get; }
        string Dim2XbrlElementId { get; }
        string Dim3XbrlElementId { get; }

        /// <summary>
        /// Gets the value of the hypercube item.
        /// </summary>
        /// <value>The value.</value>
        string Value { get; set; }

        /// <summary>
        /// Gets the context of the item, which is used in xbrl export to assign the respective context.
        /// </summary>
        ScenarioContext Context { get; }
    }
}
