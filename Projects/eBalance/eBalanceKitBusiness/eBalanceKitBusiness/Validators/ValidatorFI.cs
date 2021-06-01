// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-21
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Validators {
    public class ValidatorFi : ValidatorBase, IValidator {

        public ValidatorFi(Document document)
            : base(document) { Root = document.ValueTreeMain.Root; }
    }
}