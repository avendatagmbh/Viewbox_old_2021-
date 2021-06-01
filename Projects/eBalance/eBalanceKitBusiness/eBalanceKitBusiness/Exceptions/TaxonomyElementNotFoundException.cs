// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-09-17
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.Exceptions {
    public class TaxonomyElementNotFoundException :ExceptionBase {
        public TaxonomyElementNotFoundException(string message) : base(message) {
        }

        public TaxonomyElementNotFoundException(string message, string header) : base(message, header) {
        }
    }
}
