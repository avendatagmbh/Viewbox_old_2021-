// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-10-09
// copyright 2010 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Taxonomy.Exceptions.Resources;

namespace Taxonomy.Exceptions {
    /// <summary>
    /// Exception class for invalid taxonomy files. Thrown, if an invalid attribute value was found.
    /// </summary>
    [Serializable]
    public class InvalidAttributeException : Exception {
        internal InvalidAttributeException(string attribute)
            : base(String.Format(XbrlExceptions.InvalidAttribute, attribute)) { }
    }
}