// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-08
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.AuditCorrections {
    public interface IAuditCorrectionSetManager {

        /// <summary>
        /// Returns the correction set which contains the specified correction.
        /// </summary>
        /// <param name="correction"></param>
        /// <returns></returns>
        IAuditCorrectionSet GetCorrectionSet(IAuditCorrection correction);
    }
}