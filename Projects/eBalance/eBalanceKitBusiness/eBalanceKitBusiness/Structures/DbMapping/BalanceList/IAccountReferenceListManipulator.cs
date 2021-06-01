// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-05-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness.Structures.DbMapping.BalanceList {
    public interface IAccountReferenceListManipulator {
        /// <summary>
        /// Adds an item to the item collection.
        /// </summary>
        /// <param name="item"></param>
        void AddItemToReferenceList(IAccountReferenceListItem item);
        /// <summary>
        /// Removes an item from the item collection.
        /// </summary>
        /// <param name="item"></param>
        void RemoveItemFromReferenceList(IAccountReferenceListItem item);
        /// <summary>
        /// Checks whether an account with the given id is contained in the Items collection 
        /// </summary>
        /// <param name="accountType">The account type to check for</param>
        /// <param name="accountId">The accountId to check for</param>
        /// <returns>True if the account with the given id is contained in the collection otherwise false</returns>
        bool IsAccountContainedInReferenceList(AccountTypeEnum accountType, long accounttId);
        /// <summary>
        /// Checks whether an account with the given item contained in the Items collection 
        /// </summary>
        /// <param name="item"></param>
        /// <returns>True if the account with the given item is contained in the collection otherwise false</returns>
        bool IsAccountContainedInReferenceList(IAccountReferenceListItem item);
    }
}
