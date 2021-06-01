// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Reconciliation {
    internal class ReconciliationTypeInfo : IReconciliationTypeInfo {

        internal ReconciliationTypeInfo(Enums.ReconciliationTypes type, string shortLabel, string label) {
            ReconciliationType = type;
            ShortLabel = shortLabel;
            Label = label;
        }

        public Enums.ReconciliationTypes ReconciliationType { get; private set; }
        public string ShortLabel { get; private set; }
        public string Label { get; private set; }
        public override string ToString() { return Label; }

        public static IDictionary<Enums.ReconciliationTypes, IReconciliationTypeInfo> ReconciliationTypeInfos {
            get {
                var reconciliationTypeInfos = new Dictionary<Enums.ReconciliationTypes, IReconciliationTypeInfo>();

                reconciliationTypeInfos[Enums.ReconciliationTypes.Reclassification] = new ReconciliationTypeInfo(
                    Enums.ReconciliationTypes.Reclassification,
                    ResourcesReconciliation.ReconciliationReclassificationShort,
                    ResourcesReconciliation.ReconciliationReclassification);

                reconciliationTypeInfos[Enums.ReconciliationTypes.ValueChange] = new ReconciliationTypeInfo(
                    Enums.ReconciliationTypes.ValueChange,
                    ResourcesReconciliation.ReconciliationValueChangeShort,
                    ResourcesReconciliation.ReconciliationValueChange);

                reconciliationTypeInfos[Enums.ReconciliationTypes.Delta] = new ReconciliationTypeInfo(
                    Enums.ReconciliationTypes.Delta,
                    ResourcesReconciliation.ReconciliationDeltaShort,
                    ResourcesReconciliation.ReconciliationDelta);

                reconciliationTypeInfos[Enums.ReconciliationTypes.PreviousYearValues] = new ReconciliationTypeInfo(
                    Enums.ReconciliationTypes.PreviousYearValues,
                    ResourcesReconciliation.ReconciliationPreviousYearValuesShort,
                    ResourcesReconciliation.ReconciliationPreviousYearValues);

                reconciliationTypeInfos[Enums.ReconciliationTypes.ImportedValues] = new ReconciliationTypeInfo(
                    Enums.ReconciliationTypes.ImportedValues,
                    ResourcesReconciliation.ReconciliationImportedValuesShort,
                    ResourcesReconciliation.ReconciliationImportedValues);

                reconciliationTypeInfos[Enums.ReconciliationTypes.AuditCorrection] = new ReconciliationTypeInfo(
                    Enums.ReconciliationTypes.AuditCorrection,
                    ResourcesReconciliation.ReconciliationAuditCorrectionShort,
                    ResourcesReconciliation.ReconciliationAuditCorrection);

                reconciliationTypeInfos[Enums.ReconciliationTypes.AuditCorrectionPreviousYear] = new ReconciliationTypeInfo(
                    Enums.ReconciliationTypes.AuditCorrectionPreviousYear,
                    ResourcesReconciliation.ReconciliationAuditCorrectionPreviousYearShort,
                    ResourcesReconciliation.ReconciliationAuditCorrectionPreviousYear);

                reconciliationTypeInfos[Enums.ReconciliationTypes.TaxBalanceValue] = new ReconciliationTypeInfo(
                    Enums.ReconciliationTypes.TaxBalanceValue,
                    ResourcesReconciliation.ReconciliationTaxBalanceValueShort,
                    ResourcesReconciliation.ReconciliationTaxBalanceValue);

                return reconciliationTypeInfos;
            }
        }
    }
}