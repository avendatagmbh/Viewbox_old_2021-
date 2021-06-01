// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-04-16
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Taxonomy.Enums;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.XbrlElementValue;

namespace eBalanceKitBusiness.Validators {
    /// <summary>
    /// Base class for validator classes.
    /// </summary>
    public class ValidatorBase {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorBase"/> class.
        /// </summary>
        /// <param name="root">The root.</param>
        protected ValidatorBase(Document document) { Document = document; }

        #region Properties
        private List<string> _generalErrorMessages = new List<string>();
        protected Document Document { get; set; }
        protected ValueTreeNode Root { get; set; }

        public List<string> GeneralErrorMessages { get { return _generalErrorMessages; } private set { _generalErrorMessages = value; } }
        protected Structures.ReportPartPath TaxonomyPart { get { return Document.TaxonomyPart; } }
        #endregion

        #region interface methods
        /// <summary>
        /// Processes the validation. This method should be overridden from inheritted classes to provide the respective validation methods.
        /// Returns true, if no validation errors has been occured, and false otherwhise.
        /// </summary>
        public virtual bool Validate() {
            if (this is ValidatorGCD || this is ValidatorGCD_Company) Root = Document.ValueTreeGcd.Root;
            else Root = Document.ValueTreeMain.Root;

            ResetValidationValues(Root);
            ResetValidationValues(Root);
            return Validate(Root);
        }

        public virtual void ResetValidationValues() {
            ResetValidationValues(Document.ValueTreeGcd.Root);
            ResetValidationValues(Document.ValueTreeMain.Root);
            //ResetValidationValues(Root);
        }

        private void ResetValidationValues(ValueTreeNode Root) {
            GeneralErrorMessages.Clear();
            foreach (IValueTreeEntry value in Root.Values.Values) {
                // reset warning
                value.ValidationWarning = false;
                value.ValidationWarningMessage = null;

                // reset error
                value.ValidationError = false;
                value.ValidationErrorMessage = null;

                // process sub elements
                if (value is XbrlElementValue_Tuple) {
                    foreach (ValueTreeNode item in (value as XbrlElementValue_Tuple).Items) ResetValidationValues(item);
                }
                //else if (value is XbrlElementValue_List) {
                //    foreach(ValueTreeNode item in (value as XbrlElementValue_List).Items) ResetValidationValues(item);
                //}
            }
        }

        /// <summary>
        /// Returns true, if no validation errors has been occured, and false otherwhise.
        /// </summary>
        /// <param name="Root"></param>
        /// <returns></returns>
        private bool Validate(ValueTreeNode Root) {
            bool result = true;
            foreach (IValueTreeEntry value in Root.Values.Values) {
                //value.ValidationWarningMessage = "";
                //value.ValidationErrorMessage = "";
                //value.ValidationWarning = false;
                //value.ValidationError = false;


                if (value.Element.IsSelectionListEntry) continue;

                if (value.Element.IsMandatoryField) {
                    if (!value.HasValue) {
                        // set warning message
                        switch (value.Element.ValueType) {
                            case XbrlElementValueTypes.Boolean:
                            case XbrlElementValueTypes.SingleChoice:
                                ValidationWarning(value, "Pflichtfeld, bitte wählen Sie eine Option aus.");
                                break;

                            case XbrlElementValueTypes.Date:
                                ValidationWarning(value, "Pflichtfeld, bitte wählen Sie ein Datum aus.");
                                break;

                            case XbrlElementValueTypes.MultipleChoice:
                                break;

                            case XbrlElementValueTypes.Int:
                            case XbrlElementValueTypes.Numeric:
                            case XbrlElementValueTypes.Monetary:
                            case XbrlElementValueTypes.String:
                                ValidationWarning(value, "Pflichtfeld, bitte geben Sie einen Wert ein.");
                                break;

                            case XbrlElementValueTypes.Tuple:
                                ValidationWarning(value, "Pflichtfeld, bitte legen Sie mindestens einen Datensatz an.");
                                break;
                        }
                    }

                    if (value is XbrlElementValue_Tuple) {
                        foreach (ValueTreeNode item in (value as XbrlElementValue_Tuple).Items) Validate(item);
                    }
                }
            }

            return result;
        }
        #endregion interface methods

        #region internal helper methods
        /// <summary>
        /// Validates, that the specified element has a value.
        /// </summary>
        /// <param name="elementName"></param>
        protected bool HasValue(string elementName) { return HasValue(elementName, Root); }

        protected bool HasValue(string elementName, ValueTreeNode root) {
            if (root.Values.ContainsKey(elementName)) return root.Values[elementName].HasValue;
            else return false;
        }

        /// <summary>
        /// Sets the specified validation error message.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        protected void ValidationError(IValueTreeEntry value, string message) {
            // reset warning if an error occured
            value.ValidationWarning = false;
            value.ValidationWarningMessage = null;

            value.ValidationError = true;

            if (!string.IsNullOrEmpty(value.ValidationErrorMessage)) value.ValidationErrorMessage += Environment.NewLine + message;
            else value.ValidationErrorMessage = message;
        }

        protected void ValidationError(string elementName, string message) { if (Root.Values.ContainsKey(elementName)) ValidationError(Root.Values[elementName], message); }

        protected void ValidationErrorTuple(string elementName, string message, string rootName, int parentId = 0) {
            if (Root.Values.ContainsKey(rootName)) {
                var tuple = Root.Values[rootName] as XbrlElementValue_Tuple;
                if (tuple == null) return;

                int current = 0;
                foreach (ValueTreeNode parent in tuple.Items) {
                    if (parentId != current) continue;
                    foreach (var item in parent.Values) {
                        if (item.Key == elementName) {
                            ValidationError(item.Value, message);
                            return;
                        }
                    }
                    ++current;
                }
            }
        }

        /// <summary>
        /// Sets the specified validation warning message.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        protected void ValidationWarning(IValueTreeEntry value, string message) {
            value.ValidationWarning = true;

            if (!string.IsNullOrEmpty(value.ValidationWarningMessage)) value.ValidationErrorMessage += Environment.NewLine + message;
            else value.ValidationWarningMessage = message;
        }

        protected bool CheckValueExists(string elementName) {
            if (!HasValue(elementName)) {
                ValidationError(elementName, "Es muss ein Wert angegeben werden.");
                return false;
            }
            return true;
        }

        protected string GetSingleChoiceValue(string name) {
            var element = Root.Values[name] as XbrlElementValue_SingleChoice;
            if (element == null || element.SelectedValue == null) return "";
            return element.SelectedValue.Id;
        }

        protected bool IsFinalAnnualBalanceSheet() {
            var x1 =
                Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.statementType"] as XbrlElementValue_SingleChoice;
            var x2 =
                Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.reportStatus"] as XbrlElementValue_SingleChoice;

            if (x1 != null && x1.SelectedValue != null && x2 != null && x2.SelectedValue != null &&
                x1.SelectedValue.Id == "de-gcd_genInfo.report.id.statementType.statementType.E" &&
                x2.SelectedValue.Id == "de-gcd_genInfo.report.id.reportStatus.reportStatus.E") {
                return true;
            } else return false;
        }

        protected void AddGeneralError(string error) { GeneralErrorMessages.Add(error); }
        #endregion internal helper methods
    }
}