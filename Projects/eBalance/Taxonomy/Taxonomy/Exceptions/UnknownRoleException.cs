// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-10-09
// copyright 2010 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Taxonomy.Exceptions.Resources;

namespace Taxonomy.Exceptions {
    /// <summary>
    /// Exception class for invalid taxonomy files. Thrown, if an unexpected rolename has been found.
    /// </summary>
    [Serializable]
    public class UnknownRoleException : Exception {
        internal UnknownRoleException(string role)
            : base(String.Format(XbrlExceptions.UnknownRole, role)) { }
    }
}