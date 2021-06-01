// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-18
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using federalGazetteBusiness.External;

namespace federalGazetteBusiness {
    public enum ActionType {
        Cancel,
        RequestPdf
    }

    public static class FederalGazetteOrderStatusConverter {

        //public class ConverterInfos {
        //    public string Image { get; set; } 
        //    public string Description { get; set; } 
        //    //public string Image { get; set; } 
        //}

        //private static ConverterInfos GetInfo(orderStatusType orderStatus) {
            
        //}

        public static bool IsAllowed(object value, object action, bool resultIfAllowed = true) {
            if (value == null || action == null) {
                System.Diagnostics.Debug.Fail("value or action is null");
                return true;
            }
            if (!(value is orderStatusType) || !(action is ActionType)) {
                System.Diagnostics.Debug.Fail("!(value is orderStatusType) || !(action is ActionType) value=" + value.GetType() + " and action=" + action.GetType());
                return false;
            }
            var val = (orderStatusType)value;
            switch (val) {
                case orderStatusType.in_produktion:
                    switch ((ActionType)action) {
                        case ActionType.Cancel:
                            return !resultIfAllowed;
                        case ActionType.RequestPdf:
                            return !resultIfAllowed;
                    }
                    break;
                case orderStatusType.endfreigabe:
                    switch ((ActionType)action) {
                        case ActionType.Cancel:
                            return !resultIfAllowed;
                        case ActionType.RequestPdf:
                            return !resultIfAllowed;
                    }
                    break;
                case orderStatusType.auftrag_eingegangen:
                    switch ((ActionType)action) {
                        case ActionType.Cancel:
                            return resultIfAllowed;
                        case ActionType.RequestPdf:
                            return !resultIfAllowed;
                    }
                    break;
                case orderStatusType.geloescht_banz:
                    switch ((ActionType)action) {
                        case ActionType.Cancel:
                            return !resultIfAllowed;
                        case ActionType.RequestPdf:
                            return !resultIfAllowed;
                    }
                    break;
                case orderStatusType.in_bearbeitung:
                    switch ((ActionType)action) {
                        case ActionType.Cancel:
                            return !resultIfAllowed;
                        case ActionType.RequestPdf:
                            return !resultIfAllowed;
                    }
                    break;
                case orderStatusType.storniert_banz:
                    switch ((ActionType)action) {
                        case ActionType.Cancel:
                            return !resultIfAllowed;
                        case ActionType.RequestPdf:
                            return !resultIfAllowed;
                    }
                    break;
                case orderStatusType.storniert_kunde:
                    switch ((ActionType)action) {
                        case ActionType.Cancel:
                            return !resultIfAllowed;
                        case ActionType.RequestPdf:
                            return !resultIfAllowed;
                    }
                    break;
                case orderStatusType.veroeffentlicht:
                    switch ((ActionType)action) {
                        case ActionType.Cancel:
                            return !resultIfAllowed;
                        case ActionType.RequestPdf:
                            return resultIfAllowed;
                    }
                    break;
            }
            System.Diagnostics.Debug.Fail("something went wrong");
            return false;
        }

        public static string GetImgPath(object value) {
            if (value == null) {
                return "/eBalanceKitResources;component/Resources/Refresh.png";
            }
            if (!(value is orderStatusType)) {
                return "/eBalanceKitResources;component/Resources/icon_exclamation.png";
            }
            var val = (orderStatusType)value;
            switch (val) {
                case orderStatusType.in_produktion:
                    break;
                case orderStatusType.endfreigabe:
                    break;
                case orderStatusType.auftrag_eingegangen:
                    break;
                case orderStatusType.geloescht_banz:
                    break;
                case orderStatusType.in_bearbeitung:
                    break;
                case orderStatusType.storniert_banz:
                    break;
                case orderStatusType.storniert_kunde:
                    break;
                case orderStatusType.veroeffentlicht:
                    break;
            }
            System.Diagnostics.Debug.Fail("something went wrong");
            return string.Empty;
        }

        public static string GetExplanation(object value) {
            if (value == null) {
                return "Error";
            }
            if (!(value is orderStatusType)) {
#if DEBUG
                return "I'm not a status, I am a " + value.GetType();
#else
                return string.Empty;
#endif
            }
            var val = (orderStatusType)value;
            switch (val) {
                case orderStatusType.in_produktion:
                    return "The order is in produktion";
                    break;
                case orderStatusType.endfreigabe:
                    return "The order is in the final release stage.";
                    break;
                case orderStatusType.auftrag_eingegangen:
                    return "The order has been received.";
                    break;
                case orderStatusType.geloescht_banz:
                    return "The order has been deleted by the federal gazette";
                    break;
                case orderStatusType.in_bearbeitung:
                    return "The order is in process";
                    break;
                case orderStatusType.storniert_banz:
                    return "The order has been canceled by the federal gazette";
                    break;
                case orderStatusType.storniert_kunde:
                    return "The order has been canceled by the costumer";
                    break;
                case orderStatusType.veroeffentlicht:
                    return "The order has been published";
                    break;
            }

//#if DEBUG
            System.Diagnostics.Debug.Fail("something went wrong");
            return "I'm a new status: " + val.ToString();
//#else
//            return string.Empty;
//#endif
        }

    }
}