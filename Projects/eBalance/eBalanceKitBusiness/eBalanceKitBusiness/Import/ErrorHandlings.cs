/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Sebestyén Muráncsik  2012-10-26      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness.Import {

    public static class ErrorHandlings {

        /// <summary>
        /// Gets an enumaration of all available error handlings, which are available for the import processes.
        /// </summary>
        public static IEnumerable<String> AvailableErrorHandlings {
            get {
                return new List<String> {
                    "Halt",
                    "Skip invalid rows"
                };
            }
        }
    }
}
