// --------------------------------------------------------------------------------
// author: Attila Papp
// since: 2012-09-20
// copyright 2011 AvenDATA GmbH
// comment: i cannot see how to unit test this class, however it improves the readability of other classes
// --------------------------------------------------------------------------------
using System;
using Part = SystemDb.SystemDb.Part;

namespace ViewboxAdmin.Models.ProfileRelated
{
    /// <summary>
    /// This class hide some hard-to-read enum related operations extracted from legacy code 
    /// </summary>
    public class ProfilePartLoadingEnumHelper : IProfilePartLoadingEnumHelper
    {
        private Type _enumtype = typeof(Part);
        /// <summary>
        /// Get the length of the enumeration
        /// </summary>
        /// <returns></returns>
        public int LengthOfEnum() {
            return Enum.GetValues(_enumtype).Length;
        }
        /// <summary>
        /// get the next item in the enumeration
        /// </summary>
        /// <param name="part">the current element in enumeration</param>
        /// <returns></returns>
        public Part GetNextPartEnum(Part part) {
            string nextitem = ((int) part + 1).ToString();
            return (Part)Enum.Parse(_enumtype, nextitem);
        }
        /// <summary>
        /// Check if it is the last element
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public bool IsNotLast(Part part) {
            int itemindex = (int) part + 1; 
            return itemindex < Enum.GetValues(_enumtype).Length;
        }

        
    }
}
