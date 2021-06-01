// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-03-12
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.IO;

namespace eBalanceKit.Converters {
    public class FileValideToBoolConverter {
         

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || parameter == null) return false;

            if (string.IsNullOrWhiteSpace(value.ToString())) {
                return false;
            }


            if(!File.Exists(value.ToString()))
                return false;

            try {
                File.OpenRead(value.ToString());
                return true;
            } catch (Exception) {
                return false;
            }

        }

    }
}