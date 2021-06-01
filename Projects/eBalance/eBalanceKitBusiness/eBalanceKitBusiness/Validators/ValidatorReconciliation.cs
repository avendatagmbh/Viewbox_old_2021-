// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-17
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Validators {
    public class ValidatorReconciliation : ValidatorGAAP {
        internal ValidatorReconciliation(Document document) : base(document) {
        }

        public override bool Validate() {
            return ExecuteBaseValidation() && ValidateIncomeStatement() && ValidateBalanceSheet() &&
                         ValidateTransferValues() && ValidatePositionsNotAllowedForTax();
        }
    }
}