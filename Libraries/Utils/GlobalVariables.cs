using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    public class GlobalVariables
    {
        static GlobalVariables()
        {
            UnitTesting = false;
        }

        public static bool UnitTesting { get; set; }
    }
}
