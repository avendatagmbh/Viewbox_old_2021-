// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-04-13
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using eBalanceKitBusiness.Localisation;

namespace eBalanceKitBusiness.Exceptions {
    /// <summary>
    /// An ExceptionClass that contains a hint that because of to less rights the user is not able to execute the command (ExceptionMessages.ActionNotAllowedMessage)
    /// </summary>
    public class ActionNotAllowed : ExceptionBase {
        /// <summary>
        /// Throws an Exception with a hint that because of to less rights the user is not able to execute the command (ExceptionMessages.ActionNotAllowedMessage)
        /// </summary>
        public ActionNotAllowed()
            : base(ExceptionMessages.ActionNotAllowedMessage, ExceptionMessages.ActionNotAllowedHeader) { }

        /// <summary>
        /// Throws an Exception with the specified message and the default Header (ExceptionMessages.ActionNotAllowedHeader)
        /// </summary>
        /// <param name="message">The message that can be displayed in a MessageBox</param>
        public ActionNotAllowed(string message)
            : base(ExceptionMessages.ActionNotAllowedHeader, message) { }
        
    }
}