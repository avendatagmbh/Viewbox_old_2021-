using Taxonomy;

namespace eBalanceKitBusiness {

    /// <summary>
    /// Interface for value mapping classes.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-01-27</since>
    public interface IValueMapping {

        int ElementId { get; set; }
        string Value { get; set; }
        string ValueOther { get; set; }
        string Comment { get; set; }
        IValueMapping GetChild(string name);
        IValueMapping Parent { get; }
        bool AutoComputationEnabled { get; set; }
        bool SupressWarningMessages { get; set; }
        bool SendAccountBalances { get; set; }
        IElement Element { get; set; }
    }
}
