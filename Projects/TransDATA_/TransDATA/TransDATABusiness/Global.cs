/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-11-19      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TransDATABusiness.Structures;

namespace TransDATABusiness {
    
    public class Global {
        
        public static void Init() {
            // init application config
            AppConfig = new AppConfig();
        }

        /// <summary>
        /// Application configuration object.
        /// </summary>
        public static AppConfig AppConfig { get; private set; }

    }
}
