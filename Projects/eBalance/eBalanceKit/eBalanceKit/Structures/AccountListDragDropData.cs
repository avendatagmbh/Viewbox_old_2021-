/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-12-12      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness;

namespace eBalanceKit.Structures {

    /// <summary>
    /// This class contains the accounts, which should be dragged from the list view into the tree view or vice versa.
    /// </summary>
    internal class AccountListDragDropData {

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountListDragDropData"/> class.
        /// </summary>
        /// <param name="accounts">The accounts.</param>
        public AccountListDragDropData(List<IBalanceListEntry> accounts) {
            this.Accounts = accounts;
        }

        /// <summary>
        /// Gets or sets the accounts.
        /// </summary>
        /// <value>The accounts.</value>
        public List<IBalanceListEntry> Accounts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow drop].
        /// </summary>
        /// <value><c>true</c> if [allow drop]; otherwise, <c>false</c>.</value>
        public bool AllowDrop { get; set; }
    }
}
