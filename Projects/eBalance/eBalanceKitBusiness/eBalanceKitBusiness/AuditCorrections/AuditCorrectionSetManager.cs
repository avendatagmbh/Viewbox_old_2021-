// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;

namespace eBalanceKitBusiness.AuditCorrections {
    public class AuditCorrectionSetManager : IAuditCorrectionSetManager {
        
        /// <summary>
        /// Returns the correction set which contains the specified correction.
        /// </summary>
        /// <param name="correction"></param>
        /// <returns></returns>
        public IAuditCorrectionSet GetCorrectionSet(IAuditCorrection correction) {
            throw new NotImplementedException();
        }
        
    }
}