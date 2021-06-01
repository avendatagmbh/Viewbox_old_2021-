// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-04-16
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Linq;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Validators {
    /// <summary>
    /// This class validates the GAAP taxonomy.
    /// </summary>
    public sealed class ValidatorGAAP : ValidatorBase, IValidator {
        public ValidatorGAAP(Document document)
            : base(document) {
        }

        private bool ContainsReportElement_IncomeStatement {
            get {
                var value =
                    Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.reportElement"] as
                    XbrlElementValue_MultipleChoice;
                bool? isChecked = value.IsChecked["de-gcd_genInfo.report.id.reportElement.reportElements.GuV"].BoolValue;
                return isChecked.HasValue && isChecked.Value;
            }
        }

        private bool ContainsReportElement_BalanceSheet {
            get {
                var value =
                    Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.reportElement"] as
                    XbrlElementValue_MultipleChoice;
                bool? isChecked = value.IsChecked["de-gcd_genInfo.report.id.reportElement.reportElements.B"].BoolValue;
                return isChecked.HasValue && isChecked.Value;
            }
        }

        #region IValidator Members

        /// <summary>
        /// Processes the validation.
        /// </summary>
        public override bool Validate() {
            bool result = base.Validate();
            //bool result = true;

            // kke must have a value, if at least one sublist contains a value
            if (Root.Values.ContainsKey("kke")) {
                var value = Root.Values["kke"];
                if (value.Value == null && (HasValue("kke.unlimitedPartners") || HasValue("kke.limitedPartners"))) {
                    ValidationError(value,
                                    "Es muss ein Wert eingetragen werden, wenn in einer der untergeordneten Listen ein Wert Eintrag existiert.");
                    result = false;
                }
            }

            // w & x
            result &= ValidateBalanceSheet();

            // y
            if (!StaticCompanyMethods.IsRKV(Document) && !StaticCompanyMethods.IsRVV(Document)) {
                result &= CheckY();
            }

            result &= ValidateIncomeStatement();

            //if (Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.incomeStatementendswithBalProfit"].HasValue &&
            //    Convert.ToBoolean(
            //        Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.incomeStatementendswithBalProfit"].Value))
            //    result &= ValidateIncomeUse();

            result &= ValidateTransferValues();
            result &= ValidatePositionsNotAllowedForTax();

            result &= CheckBB();

            return result;
        }

        private bool CheckBB() {
            bool result = true;

            if (StaticCompanyMethods.IsPersonenGesellschaft(Document)) {
                // don't know why but they (the Leitfaden) treads it as different cases
                if (StaticCompanyMethods.IsRVV(Document)) {
                    //fplgm.net=fpl
                    result &= DecimalValuesEqualChecker("de-gaap-ci_fplgm.net", "de-gaap-ci_fpl");
                }
                else if (StaticCompanyMethods.IsRKV(Document)) {
                    //fplgm.net=fpl
                    result &= DecimalValuesEqualChecker("de-gaap-ci_fplgm.net", "de-gaap-ci_fpl");
                }
                else {
                    //fplgm.net=fpl
                    result &= DecimalValuesEqualChecker("de-gaap-ci_fplgm.net", "de-gaap-ci_fpl");
                }
            }

            if (StaticCompanyMethods.IsPersonenGesellschaft(Document) || StaticCompanyMethods.IsEinzelunternehmer(Document)) {
                if (StaticCompanyMethods.IsRVV(Document)) {
                    //is.netIncome == fpl.netIncome
                    result &= DecimalValuesEqualChecker("de-gaap-ci_is.netIncome", "de-gaap-ci_fpl.netIncome");
                }
                else if (StaticCompanyMethods.IsRKV(Document)) {
                    //isBanks.27 == fpl.netIncome
                    result &= DecimalValuesEqualChecker("de-gaap-ci_is.Banks.27", "de-gaap-ci_fpl.netIncome");
                }
                else {
                    //is.netIncome == fpl.netIncome
                    result &= DecimalValuesEqualChecker("de-gaap-ci_is.netIncome", "de-gaap-ci_fpl.netIncome");
                }
            }

            return result;
        }
        #endregion

        private bool DecimalValuesEqualChecker(string element1, string element2) {
            var result = false;

            var number1 = (HasValue(element1))
                              ? Root.Values[element1].
                                    DecimalValue
                              : 0;

            var number2 = (HasValue(element2))
                              ? Root.Values[element2].
                                    DecimalValue
                              : 0;
            result = decimal.Equals(number1, number2);

            if (!result) {
                ValidationError(element1, string.Format(ResourcesValidationErrors.HasToBeEqual, element1, element2));
                ValidationError(element2, string.Format(ResourcesValidationErrors.HasToBeEqual, element2, element1));
            }

            return result;
        }

        private bool ValidateIncomeUse() {
            bool result = true;
            foreach (var value in Root.Values) {
                //Fordere nur ein Pflichtfeld bei nicht steuerlich unzulässigen Feldern.
                if (value.Key.StartsWith("de-gaap-ci_incomeUse.") && !value.Value.Element.NotPermittedForFiscal &&
                    !value.Value.Element.NotPermittedForCommercial
                    && value.Value.Element.IsMandatoryField && !value.Value.HasValue) {
                    if (result)
                        ValidationError(value.Value,
                                        "Es muss ein Wert angegeben werden," + Environment.NewLine +
                                        "da das Feld \"Bilanz enthält Ausweis des Bilanzgewinns\" ausgewählt wurde.");
                    else
                        ValidationError(value.Value, "Es muss ein Wert angegeben werden.");
                    result = false;
                }
            }
            return result;
        }


        private bool ValidateIncomeStatement() {
            bool result = true;
            if (IsFinalAnnualBalanceSheet()) {
                if (!HasValue(TaxonomyPart.IncomeStatementSumPosition)) {
                    ValidationError(TaxonomyPart.IncomeStatementSumPosition,
                                    "In der Gewinn- und Verlustrechnung wurden noch keine Daten eingegeben.");
                    result = false;
                }
            }
            return result;
        }

        private bool ValidateBalanceSheet() {
            bool result = true;


            if (StaticCompanyMethods.IsRKV(Document)) {
                // w
                if (!HasValue(TaxonomyPart.BalanceSheetAss)) {
                    ValidationError(TaxonomyPart.BalanceSheetAss,
                                    "Bei Auswahl der Bankentaxonomie muss Summe Aktiva ein Wert zugewiesen werden");
                    result = false;
                }
                // ToDo check if "else" would be better instead of new check - case Aktiva has value and Passiva doesn't
                //x
                if (HasValue(TaxonomyPart.BalanceSheetAss) && HasValue(TaxonomyPart.BalanceSheetEqLiab)) {
                    var valueBS = Root.Values[TaxonomyPart.BalanceSheetAss];
                    var valueEqLiab = Root.Values[TaxonomyPart.BalanceSheetEqLiab];
                    if (valueBS.DecimalValue != valueEqLiab.DecimalValue) {
                        ValidationError(TaxonomyPart.BalanceSheetAss, ResourcesValidationErrors.Aktiva_differs_Passiva);
                        ValidationError(TaxonomyPart.BalanceSheetEqLiab, ResourcesValidationErrors.Aktiva_differs_Passiva);
                        result = false;
                    }
                }
            }
            else if (StaticCompanyMethods.IsRVV(Document)) {
                // w
                if (!HasValue(TaxonomyPart.BalanceSheetAss)) {
                    ValidationError(TaxonomyPart.BalanceSheetAss, "Bei Auswahl der Versicherungstaxonomie muss Summe Aktiva ein Wert zugewiesen werden");
                    result = false;
                }

                //x
                if (HasValue(TaxonomyPart.BalanceSheetAss) && HasValue(TaxonomyPart.BalanceSheetEqLiab)) {
                    var valueBS = Root.Values[TaxonomyPart.BalanceSheetAss];
                    var valueEqLiab = Root.Values[TaxonomyPart.BalanceSheetEqLiab];
                    if (valueBS.DecimalValue != valueEqLiab.DecimalValue) {
                        ValidationError(TaxonomyPart.BalanceSheetAss, ResourcesValidationErrors.Aktiva_differs_Passiva);
                        ValidationError(TaxonomyPart.BalanceSheetEqLiab, ResourcesValidationErrors.Aktiva_differs_Passiva);
                        result = false;
                    }
                }
            }
            else {
                //w
                if (!HasValue(TaxonomyPart.BalanceSheetAss)) {
                    ValidationError(TaxonomyPart.BalanceSheetAss, "Der Summe Aktiva muss ein Wert zugewiesen werden");
                    result = false;
                }

                //x
                if (HasValue(TaxonomyPart.BalanceSheetAss) && HasValue(TaxonomyPart.BalanceSheetEqLiab)) {
                    var valueBS = Root.Values[TaxonomyPart.BalanceSheetAss];
                    var valueEqLiab = Root.Values[TaxonomyPart.BalanceSheetEqLiab];
                    if (valueBS.DecimalValue != valueEqLiab.DecimalValue) {
                        ValidationError(TaxonomyPart.BalanceSheetAss, ResourcesValidationErrors.Aktiva_differs_Passiva);
                        ValidationError(TaxonomyPart.BalanceSheetEqLiab, ResourcesValidationErrors.Aktiva_differs_Passiva);
                        result = false;
                    }
                }
            }

            // balance sheet: balance "total assets" (Aktiva) must be equal to balance "total equity and liabilities" (Passiva)
            if (HasValue(TaxonomyPart.BalanceSheetAss) && HasValue(TaxonomyPart.BalanceSheetEqLiab)) {
                var valueBS = Root.Values[TaxonomyPart.BalanceSheetAss];
                var valueEqLiab = Root.Values[TaxonomyPart.BalanceSheetEqLiab];
                //if (valueBS.DecimalValue != valueEqLiab.DecimalValue) {
                //    ValidationError(TaxonomyPart.BalanceSheetAss, ResourcesValidationErrors.Aktiva_differs_Passiva);
                //    ValidationError(TaxonomyPart.BalanceSheetEqLiab, ResourcesValidationErrors.Aktiva_differs_Passiva);
                //    result = false;
                //}

                if (Document.IsCommercialBalanceSheet && valueBS.ReconciliationInfo.STValue != valueEqLiab.ReconciliationInfo.STValue) {
                    ValidationError(TaxonomyPart.BalanceSheetAss,
                                    ResourcesValidationErrors.Aktiva_differs_Passiva_Recon);
                    ValidationError("de-gaap-ci_hbst.transfer",
                                    ResourcesValidationErrors.Aktiva_differs_Passiva_Recon);
                    ValidationError(TaxonomyPart.BalanceSheetEqLiab,
                                    ResourcesValidationErrors.Aktiva_differs_Passiva_Recon);
                    result = false;
                }
                /*
                // ToDo uncomment if balance sum has to be equal before and after reconciliation
                if (Document.IsCommercialBalanceSheet && valueBS.ReconciliationInfo.STValue != valueBS.DecimalValue) {
                    ValidationError(TaxonomyPart.BalanceSheetAss,
                                    "Aktiva vor und nach Anwendung der Überleitungsrechnung muss überein stimmen.");
                    result = false;
                }

                if (Document.IsCommercialBalanceSheet && valueEqLiab.DecimalValue != valueEqLiab.ReconciliationInfo.STValue) {
                    ValidationError(TaxonomyPart.BalanceSheetEqLiab,
                                    "Aktiva vor und nach Anwendung der Überleitungsrechnung muss überein stimmen.");
                    result = false;
                }
                */
            }
            else //if (IsFinalAnnualBalanceSheet()) 
            {
                if (!HasValue(TaxonomyPart.BalanceSheetEqLiab)) {
                    ValidationError(TaxonomyPart.BalanceSheetAss, "Im Passiva wurden noch keine Daten eingegeben.");
                    ValidationError(TaxonomyPart.BalanceSheetEqLiab, "Im Passiva wurden noch keine Daten eingegeben.");
                    result = false;
                }

                if (!HasValue(TaxonomyPart.BalanceSheetAss)) {
                    ValidationError(TaxonomyPart.BalanceSheetAss, "Im Aktiva wurden noch keine Daten eingegeben.");
                    ValidationError(TaxonomyPart.BalanceSheetEqLiab, "Im Aktiva wurden noch keine Daten eingegeben.");
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Jahresüberschuss/-fehlbetrag / Bilanzgewinn/-verlust
        /// </summary>
        /// <ToDo>Extend for special taxonomies</ToDo>
        private bool CheckY(bool isTaxEntryFromUeberleitung = false) {
            bool result = true;


            var x1 =
                Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.statementType"] as XbrlElementValue_SingleChoice;
            var x2 =
                Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.incomeStatementendswithBalProfit"] as XbrlElementValue_Boolean;

            if (x1 != null && x1.SelectedValue != null && x1.SelectedValue.Id == "de-gcd_genInfo.report.id.statementType.statementType.E" && x2 != null && x2.BoolValue != null) {
                // case 1
                if (x2.BoolValue.Value == false) {
                    if (!StaticCompanyMethods.IsPersonenGesellschaft(Document) && !isTaxEntryFromUeberleitung) {
                        result &= Calculate4NoIncomeStatementWithBalProfit();
                    }
                    // case 2
                }
                else {
                    if (!isTaxEntryFromUeberleitung) {
                        result &= Calculate4IncomeStatementWithBalProfit();
                    }
                }
            }

            var x3 = "de-gaap-ci_DeterminationOfTaxableIncomeSpec.forProfitOrganization.taxableIncome";
            var x4 = "de-gaap-ci_DeterminationOfTaxableIncomeSpec.forProfitOrganization.determinationOfTaxableIncome";

            if (HasValue(x3) && !HasValue(x4)) {
                result = false;
                ValidationError(x4, string.Format(ResourcesValidationErrors.IfThenRequired, Root.Values[x3].DisplayString, Root.Values[x4].DisplayString));
            }

            return result;
        }

        /// <summary>
        /// This methode is doing the calculation stuff for case 2 where genInfo.report.id.incomeStatementendswithBalProfit = YES
        /// </summary>
        /// <returns>Is it valid?</returns>
        private bool Calculate4IncomeStatementWithBalProfit() {
            bool result1, result2;

            var val1 = "de-gaap-ci_bs.eqLiab.equity.profitLoss";
            var val2 = "de-gaap-ci_incomeUse.gainLoss";
            var val3 = "de-gaap-ci_incomeUse.gainLoss.netIncome";
            var val4 = "de-gaap-ci_is.netIncome";

            var number1 = (HasValue(val1))
                              ? Root.Values[val1].
                                    DecimalValue
                              : 0;

            var number2 = (HasValue(val2))
                              ? Root.Values[val2].
                                    DecimalValue
                              : 0;

            var number3 = (HasValue(val3))
                              ? Root.Values[val3].
                                    DecimalValue
                              : 0;

            var number4 = (HasValue(val4))
                              ? Root.Values[val4].
                                    DecimalValue
                              : 0;

            result1 = (number1 == number2);

            if (!result1) {
                ValidationError(val1, ResourcesValidationErrors.profitLoss_unequal_gainLoss);
                ValidationError(val2, ResourcesValidationErrors.profitLoss_unequal_gainLoss);
            }

            result2 = (number3 == number4);

            if (!result2) {
                ValidationError(val3, ResourcesValidationErrors.netIncome_GuV_differs_IncomeUse);
                ValidationError(val4, ResourcesValidationErrors.netIncome_GuV_differs_IncomeUse);
            }



            return (result1 && result2);
        }

        /// <summary>
        /// This methode is doing the calculation stuff for case 2 where genInfo.report.id.incomeStatementendswithBalProfit = NO
        /// </summary>
        /// <returns>Is it valid?</returns>
        private bool Calculate4NoIncomeStatementWithBalProfit() {
            bool result = true;

            var number1 = (HasValue("de-gaap-ci_bs.ass.deficitNotCoveredByCapital.privateAccountSP.netIncome"))
                              ? Root.Values["de-gaap-ci_bs.ass.deficitNotCoveredByCapital.privateAccountSP.netIncome"].
                                    DecimalValue
                              : 0;
            // div
            var number2 = (HasValue("de-gaap-ci_bs.ass.deficitNotCoveredByCapital.lossUnlimitedLiablePartnerS.netIncome"))
                              ? Root.Values["de-gaap-ci_bs.ass.deficitNotCoveredByCapital.lossUnlimitedLiablePartnerS.netIncome"].
                                    DecimalValue
                              : 0;
            // div
            var number3 = (HasValue("de-gaap-ci_bs.ass.deficitNotCoveredByCapital.lossLimitedLiablePartnerS.netIncome"))
                              ? Root.Values["de-gaap-ci_bs.ass.deficitNotCoveredByCapital.lossLimitedLiablePartnerS.netIncome"].
                                    DecimalValue
                              : 0;
            // +
            var number4 = (HasValue("de-gaap-ci_bs.eqLiab.equity.subscribed.privateAccountSP.netIncome"))
                              ? Root.Values["de-gaap-ci_bs.eqLiab.equity.subscribed.privateAccountSP.netIncome"].
                                    DecimalValue
                              : 0;
            // +
            var number5 = (HasValue("de-gaap-ci_bs.eqLiab.equity.subscribed.unlimitedLiablePartners.netIncome"))
                              ? Root.Values["de-gaap-ci_bs.eqLiab.equity.subscribed.unlimitedLiablePartners.netIncome"].
                                    DecimalValue
                              : 0;
            // +
            var number6 = (HasValue("de-gaap-ci_bs.eqLiab.equity.subscribed.limitedLiablePartners.netIncome"))
                              ? Root.Values["de-gaap-ci_bs.eqLiab.equity.subscribed.limitedLiablePartners.netIncome"].
                                    DecimalValue
                              : 0;
            // +
            var number7 = (HasValue("de-gaap-ci_bs.eqLiab.equity.netIncome"))
                              ? Root.Values["de-gaap-ci_bs.eqLiab.equity.netIncome"].
                                    DecimalValue
                              : 0;
            // =
            var shouldBe = (HasValue("de-gaap-ci_is.netIncome"))
                              ? Root.Values["de-gaap-ci_is.netIncome"].
                                    DecimalValue
                              : 0;

            result = decimal.Equals(decimal.Round((- number1 - number2 - number3 + number4 + number5 + number6 + number7), 2),
                                  shouldBe);
            //result = true;

            if (!result) {
                ValidationError("de-gaap-ci_is.netIncome", "Jahresüberschuss/-fehlbetrag in Bilanz und GuV stimmt nicht überein.");
            }

            return result;
        }

        /// <summary>
        /// Bei steuerlich unzulässigen Positionen im Bilanzbaum dürfen...
        /// ...bei einer Steuerbilanz weder Konten zugeordnet werden noch manuelle Werte eingetragen werden
        /// ...bei einer Handelsbilanz muss der Steuerbilanzwert (=Handelbilanzwert + Überleitungsrechnung) 0€ ergeben.
        /// </summary>
        private bool ValidatePositionsNotAllowedForTax() {
            bool result = true;
            foreach (var value in Root.Values) {
                if (value.Value.Element.NotPermittedForCommercial || value.Value.Element.NotPermittedForFiscal) {
                    var monetary = (value.Value as XbrlElementValue_Monetary);
                    if (Document.IsCommercialBalanceSheet) {
                        if (monetary != null && monetary.ReconciliationInfo != null) {
                            if (monetary.ReconciliationInfo.STValue != null && monetary.ReconciliationInfo.STValue != 0) {
                                ValidationError(value.Value,
                                                ResourcesValidationErrors.NotAllowedForTaxCommercialBalanceSheet);
                                result = false;
                            }
                        }
                    }
                    else {
                        if (monetary != null && monetary.HasValue) {
                            ValidationError(value.Value,
                                            ResourcesValidationErrors.NotAllowedForTax);
                            result = false;
                        }
                    }
                }
            }
            return result;
        }

        private bool ValidateTransferValues() {
            bool result = true;
            if (Document.IsCommercialBalanceSheet) {

                var reconciliations = Root.Values.Where(
                    value =>
                    value.Value.ReconciliationInfo != null &&
                    (value.Value.ReconciliationInfo.TransferValue.HasValue ||
                     value.Value.ReconciliationInfo.TransferValuePreviousYear.HasValue) ).ToList();

                result = reconciliations.Any();


                if (!result) {
                    ValidationError(TaxonomyPart.BalanceSheetAss,
                                    ResourcesValidationErrors.CommercialBalanceSheetWithoutReconciliations);
                } else {
                    // validation is not allowed in parent AND child
                    foreach (var reconciliationEntry in reconciliations) {
                        if (!reconciliationEntry.Value.ReconciliationInfo.IsValid) {
                            ValidationError(reconciliationEntry.Value, ResourcesValidationErrors.ReconciliationNotAllowed);
                            result = false;
                        }
                    }
                }
            }
            return result;
        }
    }
}