using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Interfaces {
    
    /// <summary>
    /// Interface for company value mapping classes.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-05-16</since>
    public interface IValueMappingCompany : IValueMapping {
        Company Company { get; set; }
    }
}
