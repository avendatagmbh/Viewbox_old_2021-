/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2011-04-16      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taxonomy;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Validators {

    /// <summary>
    /// This class validates the GCD taxonomy.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-04-16</since>
    public sealed class ValidatorGCD : ValidatorBase, IValidator {

        public ValidatorGCD(Document document)
            : base(document) {
            this.Root = document.ValueTreeGcd.Root;
        }

        /// <summary>
        /// Processes the validation.
        /// </summary>
        public override bool Validate() {
            bool result = base.Validate();

            // a
            result &= CheckValueExists("de-gcd_genInfo.report.id.reportStatus"); // disallowed elements was removed in Taxonomie.IsAllowedListEntry()
            // b
            result &= CheckValueExists("de-gcd_genInfo.report.id.revisionStatus");
            // c
            result &= CheckValueExists("de-gcd_genInfo.report.id.statementType");
            // d
            result &= CheckValueExists("de-gcd_genInfo.report.period.balSheetClosingDate");
            // g
            result &= CheckValueExists("de-gcd_genInfo.report.id.incomeStatementendswithBalProfit");
            // h
            result &= CheckValueExists("de-gcd_genInfo.report.id.consolidationRange"); // disallowed elements was removed in Taxonomie.IsAllowedListEntry()
            
            if (IsFinalAnnualBalanceSheet()) {
                if (!HasValue("de-gcd_genInfo.report.period.fiscalYearBegin")) {
                    ValidationError("de-gcd_genInfo.report.period.fiscalYearBegin", ResourcesValidationErrors.GcdFiscalYearBeginNotExists);
                    result = false;
                }

                if (!HasValue("de-gcd_genInfo.report.period.fiscalYearEnd")) {
                    ValidationError("de-gcd_genInfo.report.period.fiscalYearEnd", ResourcesValidationErrors.GcdFiscalYearEndNotExists);
                    result = false;
                }
                if (HasValue("de-gcd_genInfo.report.period.fiscalYearEnd") && HasValue("de-gcd_genInfo.report.period.balSheetClosingDate") &&
                    !Root.Values["de-gcd_genInfo.report.period.fiscalYearEnd"].Value.ToString().Equals(Root.Values["de-gcd_genInfo.report.period.balSheetClosingDate"].Value.ToString())
                ) {
                    ValidationError("de-gcd_genInfo.report.period.balSheetClosingDate", ResourcesValidationErrors.balSheetClosingDate_not_equals_fiscalYearEnd);
                    result = false;
                }
            }
            // l
            result &= CheckValueExists("de-gcd_genInfo.report.id.accountingStandard");

            result &= CheckValueExists("de-gcd_genInfo.report.id.incomeStatementFormat");
            result &= CheckValueExists("de-gcd_genInfo.report.id.reportType");
            //result &= CheckValueExists("de-gcd_genInfo.report.id.specialAccountingStandard");

            // m
            if (StaticCompanyMethods.IsRKV(Document) || StaticCompanyMethods.IsRVV(Document)) {
                // ToDo check that only one (1) specialAccountingStandard is selected
            }
            

            //result &= CheckValueExists("de-gcd_genInfo.report.id.statementType.tax"); // laut Doku nur für Personengesellschaften, muss aber laut ERIC für alle angegeben werden?!?!

            // o (GUV)
            if (GetReportElement("de-gcd_genInfo.report.id.reportElement.reportElements.GuV").HasValue && GetReportElement("de-gcd_genInfo.report.id.reportElement.reportElements.GuV").Value) {
                if (GetSingleChoiceValue("de-gcd_genInfo.report.id.incomeStatementFormat") != "de-gcd_genInfo.report.id.incomeStatementFormat.incomeStatementFormat.GKV" &&
                    GetSingleChoiceValue("de-gcd_genInfo.report.id.incomeStatementFormat") != "de-gcd_genInfo.report.id.incomeStatementFormat.incomeStatementFormat.UKV") {
                    ValidationError("de-gcd_genInfo.report.id.incomeStatementFormat", ResourcesValidationErrors.unallowedIncomeStatementFormat);
                    result = false;
                }
            }

            // o (GKV)
            if (GetSingleChoiceValue("de-gcd_genInfo.report.id.incomeStatementFormat") == "de-gcd_genInfo.report.id.incomeStatementFormat.incomeStatementFormat.GKV" && 
                HasValue("de-gaap-ci_is.netIncome.regular.operatingCOGS")) {
                ValidationError("de-gcd_genInfo.report.id.incomeStatementFormat", ResourcesValidationErrors.GKV_and_operatingCOGS);
                result = false;
            }

            // o (UKV)
            if (GetSingleChoiceValue("de-gcd_genInfo.report.id.incomeStatementFormat") == "de-gcd_genInfo.report.id.incomeStatementFormat.incomeStatementFormat.UKV" && 
                HasValue("de-gaap-ci_is.netIncome.regular.operatingTC")) {
                    ValidationError("de-gcd_genInfo.report.id.incomeStatementFormat", ResourcesValidationErrors.UKV_and_operatingTC);
                result = false;
            }


            // validate report elements
            result &= ValidateReportElements();

            //if (this.Root.Values.ContainsKey("de-gcd_genInfo.company.id.idNo")) {
            //    var idNoTuple = Root.Values["de-gcd_genInfo.company.id.idNo"] as XbrlElementValue_Tuple;
            //    if (idNoTuple.Items[0].Values.ContainsKey("de-gcd_genInfo.company.id.idNo.type.companyId.BF4")) {
            //        var value = idNoTuple.Items[0].Values["de-gcd_genInfo.company.id.idNo.type.companyId.BF4"] as XbrlElementValue_String;
            //        string BF4 = value.StringValue;
            //    }
            //}
            

            //CheckValueExists("de-gcd_genInfo.report.id.period.balSheetClosingDate");
            //CheckValueExists("de-gcd_genInfo.report.id.period.fiscalPreciousYearBegin");

            //// endgültiger Jahresabschluss --> NIL nicht erlaubt, Ende-WJ muss Bilanzstichtag entsprechen
            //if (HasValue("de-gcd_genInfo.report.id.statementType") &&
            //    Root.Values["de-gcd_genInfo.report.id.statementType"].Value.ToString() == "de-gcd_genInfo.report.id.statementType.E") {

            //    CheckValueExists("de-gcd_genInfo.report.period.fiscalYearBegin");
            //    CheckValueExists("de-gcd_genInfo.report.period.fiscalYearEnd");
            //}

            result &= CheckStartDate();


            return result;
        }

        private bool CheckContainsValue(Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode presentationTreeNode) {
            var result = false;
            
            var valueTreeEntry = Document.ValueTreeMain.GetValue(presentationTreeNode.Element.Id);
            if (valueTreeEntry != null) {
                result |= valueTreeEntry.HasValue;
            }
            foreach (Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode child in presentationTreeNode.Children) {
                result |= CheckContainsValue(child);
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="presentationTreeNode"></param>
        /// <param name="element">The element an account is assigned to</param>
        /// <returns></returns>
        private bool CheckContainsAccountWithSendFlag(Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode presentationTreeNode, IElement element) {
            var result = false;
            
            var valueTreeEntry = Document.ValueTreeMain.GetValue(presentationTreeNode.Element.Id);
            if (valueTreeEntry != null && valueTreeEntry.Element.Id.Equals(element.Id)) {
                result |= valueTreeEntry.SendAccountBalances;
            }

            if (result) {
                return result;
            }

            foreach (Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode child in presentationTreeNode.Children) {
                result |= CheckContainsAccountWithSendFlag(child, element);
            }
            return result;
        }

        /// <summary>
        /// Validates the fiscalYearBegin or balSheetClosingDate (depending on statementType) - [Check 12.12]
        /// </summary>
        private bool CheckStartDate() {

                DateTime eBalanceStartDate = new DateTime(2011, 12, 31);

                var nameOfDateValueElement = string.Empty;

                var balanceKind =
                    Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.statementType"] as XbrlElementValue_SingleChoice;

                if (balanceKind != null && balanceKind.SelectedValue != null &&
                    balanceKind.SelectedValue.Id == "de-gcd_genInfo.report.id.statementType.statementType.EB") {
                    nameOfDateValueElement = "de-gcd_genInfo.report.period.fiscalYearBegin";
                }
                else {
                    nameOfDateValueElement = "de-gcd_genInfo.report.period.balSheetClosingDate";
                }

            try {
                var startDate =
                    Document.ValueTreeGcd.Root.Values[nameOfDateValueElement] as XbrlElementValue_Date;

                var comp = DateTime.Compare(startDate.DateValue.Value, eBalanceStartDate);

                if (comp < 0) {
                    ValidationError(startDate, "Datum muss nach dem " + eBalanceStartDate.ToString("d") + "liegen");
                    return false;
                }
                else {
                    return true;
                }
            }
            catch (Exception ex) {
                ValidationError(nameOfDateValueElement, ex.Message);
                return false;
            }
        }

        private bool? GetReportElement(string name) {
            if (this.Root.Values.ContainsKey("de-gcd_genInfo.report.id.reportElement")) {
                var reportElements = Root.Values["de-gcd_genInfo.report.id.reportElement"] as XbrlElementValue_MultipleChoice;
                return reportElements.IsChecked[name].BoolValue;
            }
            return null;

        }
        /// <summary>
        /// The XbrlElementValue_MultipleChoice for the report parts.
        /// </summary>
        private XbrlElementValue_MultipleChoice reportElements { get { return Root.Values["de-gcd_genInfo.report.id.reportElement"] as XbrlElementValue_MultipleChoice; } }

        private bool ValidateReportElements() {

            bool result = true;
            if (this.Root.Values.ContainsKey("de-gcd_genInfo.report.id.reportElement")) {
                
                foreach (var elem in reportElements.Elements) {
                    reportElements.IsChecked[elem.Id].ValidationError = false;
                    reportElements.IsChecked[elem.Id].ValidationErrorMessage = null;
                    reportElements.IsChecked[elem.Id].ValidationWarning = false;
                    reportElements.IsChecked[elem.Id].ValidationWarningMessage = null;

                    if (elem.IsMandatoryField && !reportElements.IsChecked[elem.Id].BoolValue.HasValue) {
                        reportElements.IsChecked[elem.Id].ValidationError = true;
                        reportElements.IsChecked[elem.Id].ValidationErrorMessage = "Pflichtfeld, bitte treffen Sie eine Auswahl.";
                        result = false;
                    } else if (elem.Id == "de-gcd_genInfo.report.id.reportElement.reportElements.B" || elem.Id == "de-gcd_genInfo.report.id.reportElement.reportElements.GuV") {
                        if (IsFinalAnnualBalanceSheet() && reportElements.IsChecked[elem.Id].BoolValue.Value == false) {
                            reportElements.IsChecked[elem.Id].ValidationError = true;
                            reportElements.IsChecked[elem.Id].ValidationErrorMessage = "Dieser Berichtsbestandteil muss in einem endgültigen Jahresabschluss enthalten sein.";
                            result = false;
                        }
                    }

                    //if (elem.Id.Equals(Document.TaxonomyPart.PartBalance)) {
                    //    if (reportElements.IsChecked[elem.Id].BoolValue != true) {
                    //        reportElements.IsChecked[elem.Id].ValidationError = true;
                    //        reportElements.IsChecked[elem.Id].ValidationErrorMessage = "Dieser Berichtsbestandteil muss übermittelt werden.";
                    //        result = false;
                    //    }
                    //}

                    if (elem.Id.Equals(Document.TaxonomyPart.PartReconciliation.Path)) {
                        if (Document.IsCommercialBalanceSheet && reportElements.IsChecked[elem.Id].BoolValue != true) {
                            reportElements.IsChecked[elem.Id].ValidationError = true;
                            reportElements.IsChecked[elem.Id].ValidationErrorMessage = "Bei einer vorliegenden Handelsbilanz muss eine Überleitungsrechnung mit übermittelt werden.";
                            result = false;
                        }
                    }

                    //if IncomeStatementendswithBalProfit the report part IncomeUse is required
                    if (elem.Id.Equals(Document.TaxonomyPart.PartIncomeUse.Path)) {
                        
                        const string incomeStatementendswithBalProfit = "de-gcd_genInfo.report.id.incomeStatementendswithBalProfit";

                        if (GetBoolValue(incomeStatementendswithBalProfit) == true && reportElements.IsChecked[elem.Id].BoolValue != true)
                        {
                            ValidationError(incomeStatementendswithBalProfit, ResourcesValidationErrors.IncomeStatementendswithBalProfitRequiresIncomeUse);
                            reportElements.IsChecked[elem.Id].ValidationError = true;
                            reportElements.IsChecked[elem.Id].ValidationErrorMessage = ResourcesValidationErrors.IncomeStatementendswithBalProfitRequiresIncomeUse;
                            result = false;
                        }
                    }

                    // send account balance details (Kontensalden zu einer oder mehreren Positionen)
                    if (elem.Id.Equals(Document.TaxonomyPart.PartAccountDetails.Path)) {
                        result &= CheckAccountDetails(elem);
                    }

                    if (Document.TaxonomyPart.ReportParts[elem.Id].IsSelected.HasValue && Document.TaxonomyPart.ReportParts[elem.Id].TaxonomyElements != null && Document.TaxonomyPart.ReportParts[elem.Id].TaxonomyElements.Any(element => !string.IsNullOrEmpty(element))) {
                        bool partFilled = true;

                        // for each entry in reportPart.TaxonomyElements
                        foreach (var elementId in Document.TaxonomyPart.ReportParts[elem.Id].TaxonomyElements) {

                            // get the ValueTreeNode
                            var valueTreeNodes =
                                Document.ValueTreeMain.Root.Values.Values.Where(
                                    element => element.Element.Id.Equals(elementId)).ToList();
                            foreach (var treeNode in valueTreeNodes) {
                                // check if they have a value
                                partFilled &= treeNode.HasValue;
                            }

                            // get the PresentationTreeNodes

                            var presentationTrees =
                                Document.MainTaxonomy.PresentationTrees.Where(
                                    pTree => pTree.Nodes.Any(node => node.Element.Id.Equals(elementId))).ToList();


                            if (Document.TaxonomyPart.ReportParts[elem.Id] == Document.TaxonomyPart.PartReconciliation) {
                                partFilled = presentationTrees.Any();
                            }
                            else {
                                foreach (var presentationTree in presentationTrees) {
                                    foreach (var rootEntry in presentationTree.Nodes.Where(node => node.Element.Id.Equals(elementId)).ToList()) {
                                        // check if they (or one of the childs) HasValue
                                        partFilled &= CheckContainsValue(rootEntry);
                                    }
                                }
                            }
                        }


                        if (!partFilled && Document.TaxonomyPart.ReportParts[elem.Id].IsSelected.Value) {
                            reportElements.IsChecked[elem.Id].ValidationError = true;
                            reportElements.IsChecked[elem.Id].ValidationErrorMessage = ResourcesValidationErrors.EmptyReportPart;
                        }
                        else if (partFilled && !Document.TaxonomyPart.ReportParts[elem.Id].IsSelected.Value && !reportElements.IsChecked[elem.Id].ValidationError) {
                            // show warning only if no validation error is shown
                            reportElements.IsChecked[elem.Id].ValidationWarning = true;
                            reportElements.IsChecked[elem.Id].ValidationWarningMessage = ResourcesValidationErrors.ReportPartFilledButNotSelected;
                        }

                        if (Document.TaxonomyPart.ReportParts[elem.Id].IsSelected.Value) {
                            result &= partFilled;
                        }
                    }

                }
            }
    
            return result;
        }

        public bool ValidateReconciliationElements() {
             bool result = true;
             if (this.Root.Values.ContainsKey("de-gcd_genInfo.report.id.reportElement")) {

                 foreach (var elem in reportElements.Elements) {
                     
                     if (elem.Id.Equals(Document.TaxonomyPart.PartReconciliation.Path)) {
                         if (Document.IsCommercialBalanceSheet && reportElements.IsChecked[elem.Id].BoolValue != true) {
                             reportElements.IsChecked[elem.Id].ValidationError = true;
                             reportElements.IsChecked[elem.Id].ValidationErrorMessage = "Bei einer vorliegenden Handelsbilanz muss eine Überleitungsrechnung mit übermittelt werden.";
                             result = false;
                         }
                     }

                     if (Document.TaxonomyPart.ReportParts[elem.Id].IsSelected.HasValue && Document.TaxonomyPart.ReportParts[elem.Id].TaxonomyElements != null && Document.TaxonomyPart.ReportParts[elem.Id].TaxonomyElements.Any(element => !string.IsNullOrEmpty(element))) {
                         bool partFilled = true;

                         // for each entry in reportPart.TaxonomyElements
                         foreach (var elementId in Document.TaxonomyPart.ReportParts[elem.Id].TaxonomyElements) {
                             
                             // get the PresentationTreeNodes

                             var presentationTrees =
                                 Document.MainTaxonomy.PresentationTrees.Where(
                                     pTree => pTree.Nodes.Any(node => node.Element.Id.Equals(elementId))).ToList();


                             if (Document.TaxonomyPart.ReportParts[elem.Id] == Document.TaxonomyPart.PartReconciliation) {
                                 partFilled = presentationTrees.Any();
                             } else {
                                 foreach (var presentationTree in presentationTrees) {
                                     foreach (var rootEntry in presentationTree.Nodes.Where(node => node.Element.Id.Equals(elementId)).ToList()) {
                                         // check if they (or one of the childs) HasValue
                                         partFilled &= CheckContainsValue(rootEntry);
                                     }
                                 }
                             }
                         }


                         if (!partFilled && Document.TaxonomyPart.ReportParts[elem.Id].IsSelected.Value) {
                             reportElements.IsChecked[elem.Id].ValidationError = true;
                             reportElements.IsChecked[elem.Id].ValidationErrorMessage = ResourcesValidationErrors.EmptyReportPart;
                         } else if (partFilled && !Document.TaxonomyPart.ReportParts[elem.Id].IsSelected.Value && !reportElements.IsChecked[elem.Id].ValidationError) {
                             // show warning only if no validation error is shown
                             reportElements.IsChecked[elem.Id].ValidationWarning = true;
                             reportElements.IsChecked[elem.Id].ValidationWarningMessage = ResourcesValidationErrors.ReportPartFilledButNotSelected;
                         }

                         if (Document.TaxonomyPart.ReportParts[elem.Id].IsSelected.Value) {
                             result &= partFilled;
                         }
                     }
                 }
             }
             return result;
        }


        /// <summary>
        /// Checks if there is an inconsistency between report part "send account details" and marked taxonomy positions "send account details for this position"
        /// </summary>
        /// <param name="elem">Should be the element with Id=de-gcd_genInfo.report.id.reportElement.reportElements.KS</param>
        /// <returns>Valid?</returns>
        private bool CheckAccountDetails(IElement elem) {
            bool result = false;

            // report part is checked "send account balance details" (Kontensalden zu einer oder mehreren Positionen)
            if (reportElements.IsChecked[elem.Id].BoolValue.HasValue &&
                reportElements.IsChecked[elem.Id].BoolValue.Value) {

                if (!ContainsSendAccountDetailElement()) {
                    reportElements.IsChecked[elem.Id].ValidationError = true;
                    reportElements.IsChecked[elem.Id].ValidationErrorMessage = ResourcesValidationErrors.EmptyReportPart;
                }
                else {
                    result = true;
                }

            }
            else {
                // Marked taxonomy positions as send account details but no marked report part to send account details
                if (ContainsSendAccountDetailElement()) {
                    reportElements.IsChecked[elem.Id].ValidationError = true;
                    reportElements.IsChecked[elem.Id].ValidationErrorMessage =
                        ResourcesValidationErrors.SendAccountDetailsInconsistency;
                } else {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if one of the selected report parts contains taxonomy positions marked with the flag "send account details"
        /// </summary>
        /// <returns></returns>
        private bool ContainsSendAccountDetailElement() {
            bool result = false;

            // for each part that will be send
            foreach (var reportParts in Document.TaxonomyPart.ReportPartsToSend) {
                
                if (reportParts.TaxonomyElements != null) {

                    // for each entry in reportPart.Value
                    foreach (var elementId in reportParts.TaxonomyElements) {

                        var presentationTrees =
                            Document.MainTaxonomy.PresentationTrees.Where(
                                pTree => pTree.Nodes.Any(node => node.Element.Id.Equals(elementId)));
                        foreach (var presentationTree in presentationTrees) {
                            foreach (
                                var rootEntry in
                                    presentationTree.Nodes.Where(node => node.Element.Id.Equals(elementId))) {


                                // check if they (or one of the childs) HasValue
                                foreach (var balanceList in Document.BalanceListsImported) {
                                    foreach (var assignedItem in balanceList.AssignedItems.OfType<IAccount>()) {
                                        var element = assignedItem.AssignedElement ??
                                                      assignedItem.AccountGroup.AssignedElement;

                                        result |= CheckContainsAccountWithSendFlag(rootEntry, element);
                                    }
                                }
                            }
                        }
                    }

                }
            }
            return result;
        }

        /// <summary>
        /// Get the BoolValue from the XbrlElementValue_Boolean element with the specified id
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        private bool? GetBoolValue(string elementId) {
            if (Root.Values.ContainsKey(elementId) &&
                            Root.Values[elementId] is XbrlElementValue_Boolean &&
                            (Root.Values[elementId] as XbrlElementValue_Boolean).BoolValue != null) {
                return (Root.Values[elementId] as XbrlElementValue_Boolean).BoolValue;
            }

            System.Diagnostics.Debug.Assert(Root.Values.ContainsKey(elementId) &&
                            Root.Values[elementId] is XbrlElementValue_Boolean);

            return null;

        }


    }
}