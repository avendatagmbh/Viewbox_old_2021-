// -----------------------------------------------------------
// Created by Benjamin Held - 31.08.2011 17:17:57
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ViewValidator.Models.Profile;

namespace ViewValidator.Converters {
    internal class RuleAppliedToBrushConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            //if (targetType != typeof(Brush)) return null;
            //return ((bool)value) ? True : False;
            List<PreviewData.Entry> entry = value as List<PreviewData.Entry>;
            int column = (int) parameter;
            if (entry != null) {
                if (entry[column].DisplayString != entry[column].RuleDisplayString) return Brushes.AntiqueWhite;
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
