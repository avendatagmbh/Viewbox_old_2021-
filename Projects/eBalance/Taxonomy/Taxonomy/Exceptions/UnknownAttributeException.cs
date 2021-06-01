// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Taxonomy.Exceptions.Resources;

namespace Taxonomy.Exceptions {
    /// <summary>
    /// Exception class for invalid taxonomy files. Thrown if an unhandled attribute has been found.
    /// </summary>
    [Serializable]
    public class UnknownAttributeException : Exception {
        internal UnknownAttributeException(string attribute, string node)
            : base(String.Format(XbrlExceptions.UnknownAttribute, attribute, node)) { }
    }
}