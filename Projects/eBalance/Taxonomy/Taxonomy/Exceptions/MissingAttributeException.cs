// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-10-09
// copyright 2010 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Taxonomy.Exceptions.Resources;

namespace Taxonomy.Exceptions {
    /// <summary>
    /// Exception class for invalid taxonomy files. Thrown if an expected attribute could not be found.
    /// </summary>
    [Serializable]
    internal class MissingAttributeException : Exception {
        internal MissingAttributeException(string attribute, string node)
            : base(String.Format(XbrlExceptions.MissingAttribute, attribute, node)) { }
    }
}