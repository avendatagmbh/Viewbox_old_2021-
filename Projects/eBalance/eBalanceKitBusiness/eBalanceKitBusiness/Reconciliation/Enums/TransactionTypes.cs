// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.Reconciliation.Enums {
    /// <summary>
    /// This enumeration lists all available transaction types, which are needed to assign the transaction to
    /// the respective reconciliation position when loading a reconciliation from database.
    /// </summary>
    public enum TransactionTypes {

        /// <summary>
        /// Unspecified position (used if no position could be assigned, e.g. in reconciliation type "Delta").
        /// </summary>
        Unspecified,
        
        /// <summary>
        /// Reclassification - Source position.
        /// </summary>
        Source,
        
        /// <summary>
        /// Reclassification - Destination position.
        /// </summary>
        Destination,

        /// <summary>
        /// Special transaction, which value equals the value of is.netIncome and which is assigned to is.netIncome by default.
        /// </summary>
        NetIncome
    }
}