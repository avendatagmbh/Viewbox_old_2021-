// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-04-13
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using eBalanceKitBusiness.Localisation;

namespace eBalanceKitBusiness.Exceptions {
    public abstract class ExceptionBase : Exception {
        
        /// <summary>
        /// Sets the Message for this Exception. DefaultHeader "Error" is used.
        /// </summary>
        /// <param name="message">Message that will be visualized in a MessageBox</param>
        protected ExceptionBase(string message) : base(message) { Header = DefaultHeader; }

        /// <summary>
        /// Sets the Header and the message for this Exception
        /// </summary>
        /// <param name="message">The content of the exception message that will be vizualised for the user.</param>
        /// <param name="header">Header that can be visualized in the MessageBox</param>
        protected ExceptionBase(string message, string header) : base(message) { Header = header; }
        

        public string Header { get; set; }
        protected static string DefaultMessage { get { return "There was an exception."; } }
        protected static string DefaultHeader { get { return "Error"; } }
    }
}