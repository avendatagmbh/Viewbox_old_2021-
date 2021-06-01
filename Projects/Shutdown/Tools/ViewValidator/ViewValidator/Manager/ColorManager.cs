using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ViewValidatorLogic.Interfaces;

namespace ViewValidator.Manager {
    static class ColorManager {
        private static SolidColorBrush _errorBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xA1, 0x23));

        public static int BrushToExcelColor(SolidColorBrush brush) {
            return brush.Color.B*256*256 +
                    brush.Color.G * 256 +
                    brush.Color.R;
        }

        public static int ColorToExcelColor(Color color) {
            return color.B * 256 * 256 +
                    color.G * 256 +
                    color.R;
        }

        public static int ExcelColorFromRowEntryType(RowEntryType type) {
            switch (type) {
                case RowEntryType.Normal:
                    return ColorToExcelColor(Colors.White);
                case RowEntryType.KeyEntry:
                    return ColorToExcelColor(Colors.LightBlue);
                case RowEntryType.Mismatch:
                    return ColorToExcelColor(Color.FromRgb(0xFF, 0xA1, 0x23));
                default:
                    throw new ArgumentOutOfRangeException("RowEntryType");
            }
        }

        public static SolidColorBrush BrushFromRowEntryType(RowEntryType type) {
            switch(type) {
                case RowEntryType.Normal:
                    return Brushes.White;
                case RowEntryType.KeyEntry:
                    return Brushes.LightBlue;
                case RowEntryType.Mismatch:
                    return _errorBrush;
                default:
                    throw new ArgumentOutOfRangeException("RowEntryType");
            }
        }
    }
}
