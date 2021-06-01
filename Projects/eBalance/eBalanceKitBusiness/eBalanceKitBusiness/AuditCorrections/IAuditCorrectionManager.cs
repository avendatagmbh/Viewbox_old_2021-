// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-15
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.AuditCorrections {
    public interface IAuditCorrectionManager : INotifyPropertyChanged {
        IEnumerable<IAuditCorrection> PositionCorrections { get; }
        IAuditCorrection SelectedPositionCorrection { get; set; }
        bool AddPositionsCorrectionValueAllowed { get; }
        bool DeletePositionsCorrectionValueAllowed { get; }
        IAuditCorrection AddPositionCorrection();
        void DeleteSelectedPositionCorrection();
        void DeletePositionCorrection(IAuditCorrection correction);
        void DeleteAllCorrections();

        /// <summary>
        /// Transfers the specified audit corrections to the specified target document. This process replaces all existing audit corrections in the target document.
        /// </summary>
        /// <param name="progressInfo"> </param>
        /// <param name="targetDocuments"> </param>
        /// <param name="auditCorrectionTransactions"></param>
        /// <param name="reconciliationTransactions"></param>
        bool TransferAuditCorrectionValues(
            ProgressInfo progressInfo,
            IEnumerable<Document> targetDocuments,
            IEnumerable<IAuditCorrectionTransaction> auditCorrectionTransactions,
            IEnumerable<IReconciliationTransaction> reconciliationTransactions);

        Dictionary<string, System.Exception> Errors { get; set; }

        /// <summary>
        /// Checks if the specified audit corrections are compatible with the specified target document.
        /// </summary>
        /// <returns>A list of problems</returns>
        ProblemCategory CheckAuditCorrectionValues(
            ProgressInfo progressInfo,
            Document targetDocument,
            List<IAuditCorrectionTransaction> auditCorrectionTransactions,
            List<IReconciliationTransaction> reconciliationTransactions);
    }
}