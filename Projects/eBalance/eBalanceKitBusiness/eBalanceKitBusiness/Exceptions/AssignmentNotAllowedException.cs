// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-04-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using eBalanceKitBusiness.Localisation;

namespace eBalanceKitBusiness.Exceptions {
    /// <summary>
    /// Exception class to throw unexpected (or expected Exceptions) during the assignment process
    /// </summary>
    public class AssignmentNotAllowedException : ExceptionBase {
        
        /// <summary>
        /// Sets the DefaultMessage as Message
        /// </summary>
        public AssignmentNotAllowedException() : base(DefaultMessage, DefaultHeader) { }
        
        /// <summary>
        /// Sets the Header for this Exception
        /// </summary>
        /// <param name="header"></param>
        public AssignmentNotAllowedException(string header) : base(DefaultMessage, header) { }
        
        public AssignmentNotAllowedException(string header, IEnumerable<string> placeHolderReplacements) : base(string.Format(DefaultMessage, placeHolderReplacements), header) { }

        public AssignmentNotAllowedException(string[] placeHolderReplacements)
            : base(string.Format(DefaultMessage, placeHolderReplacements), DefaultHeader) { }

        private new static string DefaultMessage { get { return ExceptionMessages.AssignmentNotAllowedExceptionMessage; } }
        private new static string DefaultHeader { get { return ExceptionMessages.AssignmentNotAllowedExceptionHeader; } }
    }
}