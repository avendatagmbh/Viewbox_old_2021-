// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-02-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows.Data;
using Config.Interfaces.DbStructure;
using Config.Structures;

namespace TransDATA.Converter {
    public class EnumToTransferStateImage : IValueConverter {

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            switch ((TransferStates)value) {
                case TransferStates.NotTransfered:
                    return null;
                case TransferStates.TransferedError:
                    return "/TransDATA;component/Resources/ValidationError.png";
                case TransferStates.TransferedOk:
                    return "/TransDATA;component/Resources/Checkmark24.png";
                case TransferStates.TransferedCountDifference:
                    return "/TransDATA;component/Resources/ValidationWarn.png";
                default:
                    throw new System.NotImplementedException();
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new System.NotImplementedException();
        }
    }
}