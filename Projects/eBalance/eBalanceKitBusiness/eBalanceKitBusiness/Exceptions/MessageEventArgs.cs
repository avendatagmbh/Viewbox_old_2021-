// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-02-29
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.Exceptions {
    public class MessageEventArgs : System.EventArgs {
        public MessageEventArgs(string message, string caption = null) {
            Message = message;
            Caption = caption;
        }

        /// <summary>
        /// Message string.
        /// </summary>
        public string Message { get; private set; }
        
        /// <summary>
        /// Caption, e.g. for message boxes.
        /// </summary>
        public string Caption{ get; private set; }
    }
}