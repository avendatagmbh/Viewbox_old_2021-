/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-22      initial implementation
 *************************************************************************************************************/

using System;
using System.Windows.Data;
using ProjectDb.Enums;
using ViewBuilderCommon;

namespace ViewBuilder.Converters {
    class ViewStateToImageConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            switch ((ViewscriptStates)value) {
                case ViewscriptStates.Ready:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_rdy.png";

                case ViewscriptStates.CreatingIndex:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_idx.png";
                    
                case ViewscriptStates.CreateIndexError:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_idx_err.png";

                case ViewscriptStates.CreatingTable:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_create.png";

                case ViewscriptStates.CreateTableError:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_create_err.png";

                case ViewscriptStates.CopyingTable:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_copy.png";

                case ViewscriptStates.CopyError:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_copy_err.png";

                case ViewscriptStates.Completed:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_ok.png";

                case ViewscriptStates.Warning:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_warn.png";
                   
                case ViewscriptStates.CheckingReportParameters:
                case ViewscriptStates.CheckingProcedureParameters:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_chk_prop.png";
                case ViewscriptStates.CheckingWhereCondition:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_where.png";
                case ViewscriptStates.GettingIndexInfo:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_idx.png";
                case ViewscriptStates.GeneratingDistinctValues:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_distinct.png";
                case ViewscriptStates.CheckingReportParametersError:
                case ViewscriptStates.CheckingProcedureParametersError:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_chk_prop_err.png";
                case ViewscriptStates.CheckingWhereConditionError:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_where_err.png";
                case ViewscriptStates.GettingIndexInfoError:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_idx_err.png";
                case ViewscriptStates.GeneratingDistinctValuesError:
                    return "/ViewBuilder;component/Resources/ViewStates/viewstate_distinct_err.png";

                default:
                    throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            // convert direction not used
            return null;
        }
    }
}
