// --------------------------------------------------------------------------------
// author:  Erno Taba
// since:   2012-11-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using federalGazetteBusiness;
using System.Windows.Data;
using System.Globalization;
using federalGazetteBusiness.Structures.Enum;

namespace eBalanceKit.Converters
{
    public class FederalGazetteCompanySizeToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null
                && parameter != null)
            {
                if (value is CompanySize)
                {
                    var x = (CompanySize)value;
                    return ConvertEnumValueToButtonEnable(x, parameter.ToString());
                }
            }
            return false;
        }

        /// <summary>
        /// Convert CompanySize enum value to a bool, for IsEnabled property value.
        /// </summary>
        /// <param name="value">CompanySize enum value</param>
        /// <param name="parameter">Current button name.</param>
        /// <returns>Return a bool value from this logic:
        /// Small size:     btnSmall enabled
        ///                 btnMedium enabled
        ///                 btnLarge enabled
        ///                 
        /// Midsize size:   btnSmall disabled
        ///                 btnMedium enabled
        ///                 btnLarge enabled
        ///                 
        /// Big size:       btnSmall disabled
        ///                 btnMedium disabled
        ///                 btnLarge enabled       
        /// </returns>
        public bool ConvertEnumValueToButtonEnable(CompanySize value, string parameter)
        {
            switch (value)
            { 
                case CompanySize.Small:
                    return true;
                case CompanySize.Midsize:
                    if (parameter == "btnSmall")
                        return false;
                    else
                        return true;
                case CompanySize.Big:
                    if (parameter == "btnLarge")
                        return true;
                    else
                        return false;
            }

            return false;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
