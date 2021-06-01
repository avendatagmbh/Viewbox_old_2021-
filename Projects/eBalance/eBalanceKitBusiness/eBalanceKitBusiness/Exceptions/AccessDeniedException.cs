// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-02-27
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.Exceptions {
    public class AccessDeniedException : System.Exception {
        internal AccessDeniedException(string message) : base(message) { }
    }
}