// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.Reconciliation.Enums {
    /// <summary>
    /// This enumeration lists all available reconciliation types. A reconciliation type specifies the kind
    /// of a reconciliation and refers to exact one transfer type.
    /// </summary>
    public enum ReconciliationTypes {

        /// <summary>
        /// Similar to delta reconciliation, but used to store previous year values for the balance sheet.
        /// </summary>
        PreviousYearValues,
        
        /// <summary>
        /// Similar to delta reconciliation, but used to store imported values for the balance sheet.
        /// </summary>
        ImportedValues,

        /// <summary>
        /// Moves a specific position to another (the assigned transfer type is Reclassification).
        /// </summary>
        Reclassification,
        
        /// <summary>
        /// Changes the value of a position and the assigned counter position(s) (the assigned transfer type is ValueChange).
        /// </summary>
        ValueChange,

        /// <summary>
        /// Delta reconciliation: user can specify delta values for any number of positions (could be assigned to any transfer type).
        /// </summary>
        Delta,

        /// <summary>
        /// Similar to delta reconciliation, used to enter correction values due to tax audit.
        /// </summary>
        AuditCorrection,

        /// <summary>
        /// Similar to delta reconciliation, used to enter previous year correction values due to tax audit.
        /// </summary>
        AuditCorrectionPreviousYear,

        /// <summary>
        /// Similar to delta reconciliation, used to calculate reconciliation value from balance value and previous year values due to tax audit.
        /// Ü = (SB - HB - ÜV)
        /// </summary>
        TaxBalanceValue,
    }
}