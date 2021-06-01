// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-04-25
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Reconciliation.Interfaces {
    public interface IReferenceListManipulator {
        /// <summary>
        /// Adds an item to the item collection.
        /// </summary>
        /// <param name="item"></param>
        void AddItemToReferenceList(IReferenceListItem item);
        /// <summary>
        /// Removes an item from the item collection.
        /// </summary>
        /// <param name="item"></param>
        void RemoveItemFromReferenceList(IReferenceListItem item);
        /// <summary>
        /// Checks whether an element with the given id is contained in the Items collection 
        /// </summary>
        /// <param name="elementId">The elementId to check for</param>
        /// <returns>True if the element with the given id is contained in the collection otherwise false</returns>
        bool IsElementContainedInReferenceList(int elementId);
    }
}
