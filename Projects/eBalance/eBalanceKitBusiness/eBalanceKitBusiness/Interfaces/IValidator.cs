using System.Collections.Generic;
namespace eBalanceKitBusiness.Interfaces {
    
    /// <summary>
    /// Interface for validator classes.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-04-17</since>
    public interface IValidator {

        /// <summary>
        /// Starts the validation.
        /// </summary>
        bool Validate();
        List<string> GeneralErrorMessages { get; }

        void ResetValidationValues();

    }
}
