/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2011-04-22      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness.Import {

    public static class Encodings {

        /// <summary>
        /// Gets an enumaration of all encodings, which are available for the import processes.
        /// </summary>
        public static IEnumerable<Encoding> AvailableEncodings {
            get {
                return new List<Encoding> {
                    Encoding.GetEncoding("Windows-1252"),
                    Encoding.GetEncoding("iso-8859-1"),
                    Encoding.ASCII,
                    Encoding.UTF8,
                    Encoding.Unicode,
                    Encoding.UTF32,
                    Encoding.GetEncoding(852)
                };
            }
        }
    }
}
