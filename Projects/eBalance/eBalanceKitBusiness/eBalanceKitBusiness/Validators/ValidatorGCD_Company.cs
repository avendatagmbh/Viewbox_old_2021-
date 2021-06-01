using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures.DbMapping;
using EricWrapper;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Validators {
 
    /// <summary>
    /// Validates the company specific parts of the GCD taxonomy.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-05-16</since>
    public class ValidatorGCD_Company : ValidatorBase, IValidator {

        public ValidatorGCD_Company(Document document)
            : base(document) {
                this.Root = document.ValueTreeGcd.Root;
        }

        public override bool Validate() {
            //bool result = base.Validate();
            bool result = true;

            List<string> mandatoryFields = new List<string> {
                "de-gcd_genInfo.company.id.name", 
                "de-gcd_genInfo.company.id.location", 
                "de-gcd_genInfo.company.id.location.street", 
                //"de-gcd_genInfo.company.id.location.houseNo",
                "de-gcd_genInfo.company.id.location.zipCode", 
                "de-gcd_genInfo.company.id.location.city", 
                "de-gcd_genInfo.company.id.legalStatus"};
            foreach(string mandatoryField in mandatoryFields)
                result &= CheckValueExists(mandatoryField);

            if (StaticCompanyMethods.IsPersonenGesellschaft(Document)) {
                result &= CheckValueExists("de-gcd_genInfo.report.id.statementType.tax");
            }
            else if (!StaticCompanyMethods.IsPersonenGesellschaft(Document) && !CheckValueExists("de-gcd_genInfo.report.id.accordingTo.name")) {
                //result &= !CheckValueExists("de-gcd_genInfo.report.id.statementType.tax");xx
                if (HasValue("de-gcd_genInfo.report.id.statementType.tax")) {
                    ValidationError("de-gcd_genInfo.report.id.statementType.tax", "Für Unternehmen, die keine Personengesellschaft sind und" + Environment.NewLine + " bei denen keine Gesamthand angegeben wurde, darf kein Wert angegeben werden.");
                    result = false;
                }
            }

            // check legal status
            result &= CheckLegalStatus();

            //result &= CheckSteuernummer();
            result &= CheckSteuernummer(
                GetTupleEntry("de-gcd_genInfo.company.id.idNo", "de-gcd_genInfo.company.id.idNo.type.companyId.ST13"),
                GetTupleEntry("de-gcd_genInfo.company.id.idNo", "de-gcd_genInfo.company.id.idNo.type.companyId.BF4"),
                GetTupleEntry("de-gcd_genInfo.company.id.idNo", "de-gcd_genInfo.company.id.idNo.type.companyId.STID")
            );

            // bb (until introduction of WID)
            if (!string.IsNullOrEmpty(GetTupleValue("de-gcd_genInfo.company.id.idNo", "de-gcd_genInfo.company.id.idNo.type.companyId.STWID"))) {
                result = false;
                ValidationErrorTuple("de-gcd_genInfo.company.id.idNo.type.companyId.STWID", "Bis zur Einführung der W-IdNr ist noch keine Angabe zulässig.", "genInfo.company.id.idNo");
            }

            if (StaticCompanyMethods.IsPersonenGesellschaft(Document)) {
                result &= CheckShareholder4PersonenGesellschaft();
            }

            //Sonderbilanz oder Eröffnungsbilanz
            bool isSpecialBalance = GetSingleChoiceValue("de-gcd_genInfo.report.id.statementType.tax") == "de-gcd_genInfo.report.id.statementType.tax.statementTypeTax.SB" ||
                GetSingleChoiceValue("de-gcd_genInfo.report.id.statementType.tax") == "de-gcd_genInfo.report.id.statementType.tax.statementTypeTax.EB";

            if (isSpecialBalance) {
                result &= CheckShareholder4Bilanzart();
            }

            result &= CheckShareholderGeneral();

            // v
            XbrlElementValue_Tuple accordingToTuple = Root.Values["de-gcd_genInfo.report.id.accordingTo"] as XbrlElementValue_Tuple;
            foreach (var child in accordingToTuple.Items[0].Values) {
                if (child.Key == "de-gcd_genInfo.report.id.accordingTo.name" && child.Value.HasValue && !isSpecialBalance) {
                    ValidationError(child.Value, "Name der Gesamthand darf nur bei Sonderbilanz oder Ergänzungsbilanz gesetzt werden.");
                    result = false;
                }
            }

            //((XbrlElementValue_Tuple)Root.Values["de-gcd_genInfo.company.id.shareholder"]).Items.Count

            //if (isSpecialBalance) {
            //    foreach (var shareholder in ((XbrlElementValue_Tuple)Root.Values["de-gcd_genInfo.company.id.shareholder"]).Items) {
            //        if (!shareholder.Values["de-gcd_genInfo.company.id.shareholder.currentnumber"].HasValue) {
            //            result = false;
            //            ValidationError(shareholder.Values["de-gcd_genInfo.company.id.shareholder.currentnumber"], "Es muss ein Wert angegeben werden.");
            //        }
            //    }
            //    if (((XbrlElementValue_Tuple)Root.Values["de-gcd_genInfo.company.id.shareholder"]).Items.Count == 0) {
            //        AddGeneralError("Bei Auswahl einer Eröffnungs- oder Sonderbilanz muss mindestens ein Gesellschafter angegeben werden.");
            //        result = false;
            //    }
            //}
            //XbrlElementValue_Tuple accordingToTuple = Root.Values["de-gcd_genInfo.report.id.accordingTo"] as XbrlElementValue_Tuple;
            //foreach (var child in accordingToTuple.Items[0].Values) {
            //    if (isSpecialBalance && child.Key == "de-gcd_genInfo.report.id.accordingTo.name" && !child.Value.HasValue) {
            //        ValidationError(child.Value, "Es muss ein Wert angegeben werden.");
            //        result = false;
            //    }
            //    if (GetSingleChoiceValue("de-gcd_genInfo.report.id.statementType.tax") == "de-gcd_genInfo.report.id.statementType.tax.statementTypeTax.GHB" && 
            //        child.Key == "de-gcd_genInfo.report.id.accordingTo.name" && child.Value.HasValue) {
            //        ValidationError(child.Value, "Bei Auswahl einer Gesamthandsbilanz unter \"Bilanzart steuerlich\" darf hier kein Wert eingetragen werden.");
            //        result = false;
            //    }

            //    if (child.Key == "de-gcd_genInfo.report.id.accordingTo.idNo") {
            //        XbrlElementValue_Tuple idNoTuple = child.Value as XbrlElementValue_Tuple;
            //        result &= CheckSteuernummer(
            //            idNoTuple.Items[0].Values["de-gcd_genInfo.company.id.idNo.type.companyId.ST13"],
            //            idNoTuple.Items[0].Values["de-gcd_genInfo.company.id.idNo.type.companyId.BF4"],
            //            null,
            //            //Always check Steuernummer if isSpecialBalance
            //            !isSpecialBalance
            //        );
            //    }
            //}

            // ToDo check if it's neccesary to check this because we could just not send this information to the "Finanzverwaltung"
            result &= ValidatePositionsNotAllowedForFinancial();


            // Check for consistency of information about industry sector - validation regarding ERIC Release 06/2012
            var tupelIndustrysector = Root.Values["de-gcd_genInfo.company.id.industry"] as XbrlElementValue_Tuple;
            if (tupelIndustrysector != null) {
                foreach (var tupelEntry in tupelIndustrysector.Items) {

                    var allWithValue = true;
                    var allWithoutValue = true;

                    // all Values or none has to be filled
                    foreach (var value in tupelEntry.Values) {
                        allWithValue &= value.Value.HasValue;
                        allWithoutValue &= !value.Value.HasValue;
                    }

                    if (!(allWithValue || allWithoutValue)) {
                        foreach (var value in tupelEntry.Values) {
                            ValidationError(value.Value, ResourcesValidationErrors.IndustrySectorAllOrNone);
                        }
                        result = false;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks for entries in "NotPermittedForFinancial" fields
        /// </summary>
        private bool ValidatePositionsNotAllowedForFinancial() {
            bool result = true;
            string x1 = string.Empty;
            foreach (var value in Root.Values) {
                if (value.Value is XbrlElementValue_SingleChoice && !value.Value.Element.IsSelectionListEntry) {
                    x1 = value.Key;
                }
                if (value.Value.Element.NotPermittedForFinancial) {
                    if (value.Value.Element.IsSelectionListEntry && GetSingleChoiceValue(x1) != value.Key) {
                        continue;
                    }

                    ValidationError(x1,
                                    value.Value.Element.IsSelectionListEntry
                                        ? ResourcesValidationErrors.NotPermittedForFinancial_Selection
                                        : ResourcesValidationErrors.NotPermittedForFinancial);
                    result = false;
                    
                }
            }
            return result;
        }

        // q
        private bool CheckLegalStatus() {

            switch (GetSingleChoiceValue("de-gcd_genInfo.company.id.legalStatus")) {
                case "de-gcd_genInfo.company.id.legalStatus.RHRA":
                case "de-gcd_genInfo.company.id.legalStatus.RHRB":
                case "de-gcd_genInfo.company.id.legalStatus.RAG":
                case "de-gcd_genInfo.company.id.legalStatus.RAP":
                case "de-gcd_genInfo.company.id.legalStatus.FB43":
                case "de-gcd_genInfo.company.id.legalStatus.G43":
                case "de-gcd_genInfo.company.id.legalStatus.SR":
                    ValidationError("de-gcd_genInfo.company.id.legalStatus", ResourcesValidationErrors.legalStatusNotAllowed);
                    return false;
                default:
                    return true;
            }
        }

        // r
        private bool CheckSteuernummer(IValueTreeEntry steuerNummerEntry, IValueTreeEntry bufaEntry, IValueTreeEntry idMerkmalEntry, bool CheckSteuernummerOnlyIfNotNull = false) {
            bool result = true;
            bool CheckSteuernummer = true;
            Eric ericWrapper = new Eric();
            string steuernummer = steuerNummerEntry.HasValue ? steuerNummerEntry.Value as string : string.Empty;
            
            //var statementType =
            //    Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.statementType"] as XbrlElementValue_SingleChoice;
            

            //if (statementType != null && statementType.SelectedValue != null &&
            //    statementType.SelectedValue.Id == "de-gcd_genInfo.report.id.statementType.statementType.EB" && !steuerNummerEntry.HasValue) {
            //        if (bufaEntry == null || !bufaEntry.HasValue || string.IsNullOrEmpty(bufaEntry.Value as string) || ericWrapper.PruefeBufa(bufaEntry.Value as string)) {
            //            result = false;
            //            ValidationError(bufaEntry, "Bei Übermittlung einer Eröffnungsbilanz und Fehlen der 13stelligen Steuernummer muss eine gültige Bundesfinanzamtsnummer angegeben werden.");
            //        }
            //}

            //if (bufaEntry != null && !string.IsNullOrEmpty(bufaEntry.Value as string) && !ericWrapper.PruefeBufa(bufaEntry.Value as string)) {
            //    result = false;
            //    ValidationError(bufaEntry, "Keine gültige Bundesfinanzamtsnummer.");
            //}

            if (idMerkmalEntry != null && !string.IsNullOrEmpty(idMerkmalEntry.Value as string) && !ericWrapper.PruefeIdentifikationsNr(idMerkmalEntry.Value as string)) {
                result = false;
                ValidationError(idMerkmalEntry, "Keine gültige steuerliche IdNr.");
            }

            if (GetSingleChoiceValue("de-gcd_genInfo.report.id.statementType") == "de-gcd_genInfo.report.id.statementType.statementType.EB") {
                if(string.IsNullOrEmpty(steuernummer)){
                    CheckSteuernummer = false;
                    //Check Bundesfinanzamtsnummer
                    if(bufaEntry != null && !ericWrapper.PruefeBufa(bufaEntry.Value as string)){
                        result = false;
                        ValidationError(bufaEntry, "Bei Übermittlung einer Eröffnungsbilanz und Fehlen der 13stelligen Steuernummer muss eine gültige Bundesfinanzamtsnummer angegeben werden.");
                    }
                }
            }
            if (steuerNummerEntry != null && CheckSteuernummer && !ericWrapper.PruefeSteuernummer(steuernummer)) {
                if (!CheckSteuernummerOnlyIfNotNull || (CheckSteuernummerOnlyIfNotNull && !string.IsNullOrEmpty(steuernummer))) {
                    result = false;
                    ValidationError(steuerNummerEntry, ericWrapper.LastError);
                }
            }

            return result;
        }

        // t
        private bool CheckShareholder4Bilanzart() {

            //if (GetSingleChoiceValue("de-gcd_genInfo.report.id.statementType.tax") !=
            //        "de-gcd_genInfo.report.id.statementType.tax.statementTypeTax.SB" &&
            //        GetSingleChoiceValue("de-gcd_genInfo.report.id.statementType.tax") !=
            //        "de-gcd_genInfo.report.id.statementType.tax.statementTypeTax.EB" &&
            

            bool result = true;

            result &= CheckValueExists("de-gcd_genInfo.report.id.accordingTo.name");

                foreach (var shareholder in ((XbrlElementValue_Tuple)Root.Values["de-gcd_genInfo.company.id.shareholder"]).Items) {
                    if (!shareholder.Values["de-gcd_genInfo.company.id.shareholder.currentnumber"].HasValue) {
                        result = false;
                        ValidationError(shareholder.Values["de-gcd_genInfo.company.id.shareholder.currentnumber"], "Es muss ein Wert angegeben werden.");
                    }
                }
            
            XbrlElementValue_Tuple accordingToTuple = Root.Values["de-gcd_genInfo.report.id.accordingTo"] as XbrlElementValue_Tuple;
            foreach (var child in accordingToTuple.Items[0].Values) {
                if (child.Key == "de-gcd_genInfo.report.id.accordingTo.name" && !child.Value.HasValue) {
                    ValidationError(child.Value, "Es muss ein Wert angegeben werden.");
                    result = false;
                }
                if (GetSingleChoiceValue("de-gcd_genInfo.report.id.statementType.tax") == "de-gcd_genInfo.report.id.statementType.tax.statementTypeTax.GHB" &&
                    child.Key == "de-gcd_genInfo.report.id.accordingTo.name" && child.Value.HasValue) {
                    ValidationError(child.Value, "Bei Auswahl einer Gesamthandsbilanz unter \"Bilanzart steuerlich\" darf hier kein Wert eingetragen werden.");
                    result = false;
                }

                if (child.Key == "de-gcd_genInfo.report.id.accordingTo.idNo") {
                    XbrlElementValue_Tuple idNoTuple = child.Value as XbrlElementValue_Tuple;
                    result &= CheckSteuernummer(
                        idNoTuple.Items[0].Values["de-gcd_genInfo.company.id.idNo.type.companyId.ST13"],
                        idNoTuple.Items[0].Values["de-gcd_genInfo.company.id.idNo.type.companyId.BF4"],
                        null,
                        //Always check Steuernummer if isSpecialBalance
                        false
                    );
                }
            }

            return result;
        }

        // u
        private bool CheckShareholderGeneral() {

            //if (GetSingleChoiceValue("de-gcd_genInfo.report.id.statementType.tax") !=
            //        "de-gcd_genInfo.report.id.statementType.tax.statementTypeTax.SB" &&
            //        GetSingleChoiceValue("de-gcd_genInfo.report.id.statementType.tax") !=
            //        "de-gcd_genInfo.report.id.statementType.tax.statementTypeTax.EB" &&



            bool result = true;
            XbrlElementValue_Tuple tuple = (XbrlElementValue_Tuple) Root.Values["de-gcd_genInfo.company.id.shareholder"];
            int shareholderId = 0;
            List<Tuple<IValueTreeEntry, IValueTreeEntry>> shareholderKeys =
                new List<Tuple<IValueTreeEntry, IValueTreeEntry>>();

            foreach (var shareholder in tuple.Items) {
                long zaehler, nenner;

                //if (CheckValueExists("de-gcd_genInfo.company.id.shareholder.ShareDivideKey") &&
                //    !HasValue(denominatorKey)) {

                //}

                //IValueTreeEntry numerator = null, denominator = null;
                foreach (var item in shareholder.Values) {

                    
                //string taxId=null, taxNumber=null, wid=null;
                //IValueTreeEntry taxIdTreeEntry = null, taxNumberTreeEntry = null;


                switch (item.Key) {
                    case "de-gcd_genInfo.company.id.shareholder.ShareDivideKey":
                        XbrlElementValue_Tuple localTuple = (XbrlElementValue_Tuple)item.Value;
                        string denominatorKey = "de-gcd_genInfo.company.id.shareholder.ShareDivideKey.denominator";
                        string numeratorKey = "de-gcd_genInfo.company.id.shareholder.ShareDivideKey.numerator";
                        if (
                            localTuple.Items[0].Values[numeratorKey].HasValue &&
                            !localTuple.Items[0].Values[denominatorKey].HasValue) {
                            string message =
                                "Wenn ein Wert für den Nenner angegeben wird, muss auch ein Wert für den Zähler angegeben werden.";
                            ValidationErrorTuple(
                                denominatorKey, message,
                                "de-gcd_genInfo.company.id.shareholder", shareholderId);
                            result = false;
                        }
                        else if (
                          !localTuple.Items[0].Values[numeratorKey].HasValue &&
                          localTuple.Items[0].Values[denominatorKey].HasValue) {
                            string message =
                                "Wenn ein Wert für den Zähler angegeben wird, muss auch ein Wert für den Nenner angegeben werden.";
                            ValidationErrorTuple(
                                numeratorKey, message,
                                "de-gcd_genInfo.company.id.shareholder", shareholderId);
                            result = false;
                        }

                        if (localTuple.Items[0].Values[numeratorKey].HasValue && localTuple.Items[0].Values[denominatorKey].HasValue &&
                            long.TryParse(
                                localTuple.Items[0].Values[numeratorKey].Value.ToString(),
                                out zaehler) &&
                            long.TryParse(
                                localTuple.Items[0].Values[denominatorKey].Value.
                                    ToString(), out nenner)) {

                            if (zaehler > nenner) {
                                string message =
                                    "Zähler darf nicht größer als der Nenner sein.";
                                ValidationErrorTuple(
                                    denominatorKey, message,
                                    "de-gcd_genInfo.company.id.shareholder", shareholderId);
                                ValidationErrorTuple(
                                    numeratorKey, message,
                                    "de-gcd_genInfo.company.id.shareholder", shareholderId);
                                result = false;
                            }
                        }
                        break;
                //}    
                    //    case "de-gcd_genInfo.company.id.shareholder.taxid":
                    //        if (item.Value.HasValue) taxId = item.Value.Value.ToString();
                    //        taxIdTreeEntry = item.Value;
                    //        break;
                    case "de-gcd_genInfo.company.id.shareholder.taxnumber":
                        if (item.Value.HasValue && !item.Value.ValidationError)
                            result &= CheckSteuernummer(item.Value, null, null);
                            break;
                    }

                    //result &= CheckSteuernummer(
                    //    taxNumberTreeEntry,
                    //    null,
                    //    taxIdTreeEntry
                    //);
                }
                ++shareholderId;
            }

            return result;
        }

        // s
        private bool CheckShareholder4PersonenGesellschaft() {

            //if (GetSingleChoiceValue("de-gcd_genInfo.report.id.statementType.tax") !=
            //        "de-gcd_genInfo.report.id.statementType.tax.statementTypeTax.SB" &&
            //        GetSingleChoiceValue("de-gcd_genInfo.report.id.statementType.tax") !=
            //        "de-gcd_genInfo.report.id.statementType.tax.statementTypeTax.EB" &&



            bool result = true;
            XbrlElementValue_Tuple tuple = (XbrlElementValue_Tuple)Root.Values["de-gcd_genInfo.company.id.shareholder"];
            int shareholderId = 0;
            List<Tuple<IValueTreeEntry, IValueTreeEntry>> shareholderKeys = new List<Tuple<IValueTreeEntry, IValueTreeEntry>>();
            
            foreach (var shareholder in tuple.Items) {
                string taxId=null, taxNumber=null, wid=null;
                IValueTreeEntry taxIdTreeEntry = null, taxNumberTreeEntry = null;
                //IValueTreeEntry numerator = null, denominator = null;
                foreach (var item in shareholder.Values) {
                    switch (item.Key) {
                        case "de-gcd_genInfo.company.id.shareholder.name":
                        case "de-gcd_genInfo.company.id.shareholder.currentnumber":
                        case "de-gcd_genInfo.company.id.shareholder.SpecialBalanceRequired":
                        case "de-gcd_genInfo.company.id.shareholder.extensionRequired":
                        case "de-gcd_genInfo.company.id.shareholder.legalStatus":
                            if (!item.Value.HasValue) {
                                ValidationError(item.Value, "Es muss ein Wert angegeben werden.");
                                result = false;
                            }
                            break;
                        case "de-gcd_genInfo.company.id.shareholder.WID": //until WID was introduced
                            if (item.Value.HasValue) { 
                                wid = item.Value.Value.ToString();
                                ValidationError(item.Value, "Bis zur Einführung der W-IdNr ist noch keine Angabe zulässig.");
                            }
                            break;
                        case "de-gcd_genInfo.company.id.shareholder.taxid":
                            if (item.Value.HasValue) taxId = item.Value.Value.ToString();
                            taxIdTreeEntry = item.Value;
                            break;
                        case "de-gcd_genInfo.company.id.shareholder.taxnumber":
                            if (item.Value.HasValue) taxNumber = item.Value.Value.ToString();
                            taxNumberTreeEntry = item.Value;
                            break;

                        case "de-gcd_genInfo.company.id.shareholder.ShareDivideKey":
                            XbrlElementValue_Tuple localTuple = (XbrlElementValue_Tuple)item.Value;
                            shareholderKeys.Add(new Tuple<IValueTreeEntry, IValueTreeEntry>(
                                localTuple.Items[0].Values["de-gcd_genInfo.company.id.shareholder.ShareDivideKey.numerator"],
                                localTuple.Items[0].Values["de-gcd_genInfo.company.id.shareholder.ShareDivideKey.denominator"]
                            ));
                            break;
                    }
                }
                if (string.IsNullOrEmpty(taxId) && string.IsNullOrEmpty(taxNumber)) { // later: && string.IsNullOrEmpty(wid)
                    string message = "Es muss mindestens eines der beiden Felder \"Steuernummer\" oder \"steuerliche IdNr\" angegeben werden.";
                    ValidationErrorTuple("de-gcd_genInfo.company.id.shareholder.taxnumber", message, "de-gcd_genInfo.company.id.shareholder", shareholderId);
                    ValidationErrorTuple("de-gcd_genInfo.company.id.shareholder.taxid", message, "de-gcd_genInfo.company.id.shareholder", shareholderId);
                    result = false;
                }
                result &= CheckSteuernummer(
                    taxNumberTreeEntry,
                    null,
                    taxIdTreeEntry
                );

                ++shareholderId;
            }

            //Check shareholder sum < 1.0
            double shareHolderKeySum = 0.0;
            foreach (var keyTuple in shareholderKeys) {
                if (keyTuple.Item1.HasValue && keyTuple.Item2.HasValue && (long)keyTuple.Item2.Value != 0)
                    shareHolderKeySum += (long)keyTuple.Item1.Value / Convert.ToDouble(keyTuple.Item2.Value);
            }
            // item1 = Zähler , item2 = Nenner
            foreach (var keyTuple in shareholderKeys) {
                if (!keyTuple.Item1.HasValue) {
                    ValidationError(keyTuple.Item1, "Es muss ein Wert angegeben werden.");
                    result = false;
                }
                if (!keyTuple.Item2.HasValue) {
                    ValidationError(keyTuple.Item2, "Es muss ein Wert angegeben werden.");
                    result = false;
                } else if ((long) keyTuple.Item2.Value == 0) {
                    ValidationError(keyTuple.Item2, "Der Nenner darf nicht 0 sein.");
                    result = false;
                }

                // check sum ShareDivideKey < 1 only if statementType.tax (Bilanzart) != statementTypeTax.SB (Sonderbilanz) and statementType.tax (Bilanzart) != statementTypeTax.EB (Ergänzungsbilanz)
                if (GetSingleChoiceValue("de-gcd_genInfo.report.id.statementType.tax") !=
                    "de-gcd_genInfo.report.id.statementType.tax.statementTypeTax.SB" &&
                    GetSingleChoiceValue("de-gcd_genInfo.report.id.statementType.tax") !=
                    "de-gcd_genInfo.report.id.statementType.tax.statementTypeTax.EB" &&
                    shareHolderKeySum < 1.0) {
                    ValidationError(keyTuple.Item1,
                                    "Die Summe der Beteiligungsschlüssel aller Gesellschafter" + Environment.NewLine +
                                    " darf nicht kleiner als 1 sein.");
                    result = false;
                }
            }

            return result;
        }

        
        string GetTupleValue(string tupleName, string elementName){
            //XbrlElementValue_Tuple tuple = Root.Values[tupleName] as XbrlElementValue_Tuple;
            //if(tuple == null) return "";
            //foreach (var parent in tuple.Items)
            //    foreach (var item in parent.Values)
            //        if (item.Key == elementName)
            //            return item.Value.Value as string;
            //return "";
            IValueTreeEntry result = GetTupleEntry(tupleName, elementName);
            return result == null ? "" : Convert.ToString(result.Value);
        }

        IValueTreeEntry GetTupleEntry(string tupleName, string elementName) {
            XbrlElementValue_Tuple tuple = Root.Values[tupleName] as XbrlElementValue_Tuple;
            if (tuple == null) return null;
            foreach (var parent in tuple.Items)
                foreach (var item in parent.Values)
                    if (item.Key == elementName)
                        return item.Value;
            return null;
        }
    }
}
