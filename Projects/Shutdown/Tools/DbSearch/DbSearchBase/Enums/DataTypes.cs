using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchBase.Enums {
    public enum DataTypes {
        String,
        Integer,
        NumericGermanStyle, //Use comma as separator of decimals (1,00 = 1)
        NumericEnglishStyle, // Use dot as separator of decimals (1.00 = 1)
        DateTime,
        Date,
        Time

    }
}
