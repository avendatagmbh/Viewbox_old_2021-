using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Interfaces {
    
    /// <summary>
    /// Interface for document value mapping classes.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-05-16</since>
    public interface IValueMappingDocument : IValueMapping {
        Document Document { get; set; }
    }
}
