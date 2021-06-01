/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-12      initial implementation
 *************************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace ViewBuilderBusiness.EventArgs {
    /// <summary>
    /// 
    /// </summary>
    public class MessageEventArgs : global::System.EventArgs {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        internal MessageEventArgs(string message) {
            this.Message = message;
        }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; private set; }
    }
}
