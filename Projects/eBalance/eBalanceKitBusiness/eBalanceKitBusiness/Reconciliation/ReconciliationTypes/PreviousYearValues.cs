// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using Taxonomy;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Reconciliation.ReconciliationTypes {
    internal class PreviousYearValues : DeltaReconciliation, IPreviousYearValues {
        
        #region Constructors
        /// <summary>
        /// Constructor for new reconciliation.
        /// </summary>
        /// <param name="document">Assigned document.</param>
        internal PreviousYearValues(Document document)
            : base(document, Enums.ReconciliationTypes.PreviousYearValues) {

            Comment = "Vorjahreswerte";
            DocumentManager.Instance.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "HasPreviousYearReports")
                    OnPropertyChanged(args.PropertyName);
            };
        }

        /// <summary>
        /// Constructor for existing reconciliation.
        /// </summary>
        /// <param name="dbEntityReconciliation">Assigned reconciliation.</param>
        internal PreviousYearValues(DbEntityReconciliation dbEntityReconciliation)
            : base(dbEntityReconciliation) { }
        #endregion // constructors           
  
        public override bool IsAssignmentAllowed(IElement position, out string result) {
            result = string.Empty;
            if (Transactions.Any(t => t.Position.Id == position.Id)) return false;
            return true;
        }

        public bool HasPreviousYearReports { get { return DocumentManager.Instance.HasPreviousYearReports; } }

        /// <summary>
        /// True, if transactions could be assigned to presentation tree nodes.
        /// </summary>
        internal override bool IsAssignable { get { return false; } }

    }
}