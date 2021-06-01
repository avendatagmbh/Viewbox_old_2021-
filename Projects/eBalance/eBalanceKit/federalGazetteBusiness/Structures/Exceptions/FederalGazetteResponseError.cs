// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-17
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace federalGazetteBusiness.Structures.Exceptions {
    public class FederalGazetteResponseError: eBalanceKitBusiness.Exceptions.ExceptionBase {
        
        public FederalGazetteResponseError(string message) : base(message) {
        }

        public FederalGazetteResponseError(string message, string header) : base(message, header) {
        }
    }
}