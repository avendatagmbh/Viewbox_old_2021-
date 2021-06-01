// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace Taxonomy.Enums {
    /// <summary>
    /// 
    /// </summary>
    public enum MandatoryType {
        /// <summary>
        /// No mandatory field.
        /// </summary>
        None,
        
        /// <summary>
        /// FiscalRequirement reference = "Mussfeld"
        /// </summary>
        Default,
        
        /// <summary>
        /// FiscalRequirement reference = "Rechnerisch notwendig, soweit vorhanden"
        /// </summary>
        Computed,
        
        /// <summary>
        /// FiscalRequirement reference = "Summenmussfeld"
        /// </summary>
        Sum,
        
        /// <summary>
        /// FiscalRequirement reference = "Mussfeld, Kontennachweis erwünscht"
        /// </summary>
        AccountBalance
    }
}