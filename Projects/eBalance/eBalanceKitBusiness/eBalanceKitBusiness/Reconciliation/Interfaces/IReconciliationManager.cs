// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Reconciliation.Interfaces {
    public interface IReconciliationManager {

        /// <summary>
        /// Assigned document.
        /// </summary>
        Document Document { get; }

        /// <summary>
        /// Special reconciliation to hold all previous year values.
        /// </summary>
        IPreviousYearValues PreviousYearValues { get; }

        IDeltaReconciliation AuditCorrectionsPreviousYear { get; }
        /// <summary>
        /// List of all reconciliations in the assigned document.
        /// </summary>
        IEnumerable<IReconciliation> Reconciliations { get; }

        /// <summary>
        /// List of all reclassifications in the assigned document (subset of Reconciliations).
        /// </summary>
        IEnumerable<IReclassification> Reclassifications { get; }

        /// <summary>
        /// List of all value changes in the assigned document (subset of Reconciliations).
        /// </summary>
        IEnumerable<IValueChange> ValueChanges { get; }

        /// <summary>
        /// List of all delta reconciliations in the assigned document (subset of Reconciliations).
        /// </summary>
        IEnumerable<IDeltaReconciliation> DeltaReconciliations { get; }

        /// <summary>
        /// List of all imported values in the assigned document (subset of Reconciliations).
        /// </summary>
        IEnumerable<IImportedValues> ImportedValues { get; }

        /// <summary>
        /// List of all audit corrections in the assigned document (subset of Reconciliations).
        /// </summary>
        IEnumerable<IDeltaReconciliation> AuditCorrections { get; }

        /// <summary>
        /// List of all TaxBalanceValues in the assigned document (subset of Reconciliations).
        /// </summary>
        IEnumerable<IDeltaReconciliation> TaxBalanceValues { get; }

        IReconciliation SelectedReconciliation { get; }

        /// <summary>
        /// Creates a new reconciliation of the specified type.
        /// </summary>
        /// <param name="type">Type of the new reconciliation.</param>
        IReconciliation AddReconciliation(Enums.ReconciliationTypes type);

        /// <summary>
        /// Deletes the specified reconciliation from the reconciliation list and removes it's persistant copy.
        /// </summary>
        /// <param name="reconciliation">The reconciliation which should be deleted.</param>
        /// <param name="deleteFromDb"> </param>
        void DeleteReconciliation(IReconciliation reconciliation, bool deleteFromDb = true);
        
        #region [ Reference list ]

        /// <summary>
        /// The list of elements selected for referencelist
        /// </summary>
        ReferenceList ReferenceList { get; }

        #endregion [ Reference list ]

        void RefreshValues();
    }
}