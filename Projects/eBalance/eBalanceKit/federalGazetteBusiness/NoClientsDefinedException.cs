// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-11-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Exceptions;

namespace federalGazetteBusiness {
    public class NoClientsDefinedException : ExceptionBase {
        public NoClientsDefinedException() : base(_defaultMessage, _defaultHeader) {
            
        }

        public NoClientsDefinedException(string message) : base(message, _defaultHeader) {

        }

        public NoClientsDefinedException(string message, string header) : base(message, header) {
        }

        private const string _defaultMessage = "Es wurden noch keine Einsenderkunden angelegt.";
        private const string _defaultHeader = "Keine Einsenderkunden";
    }
}