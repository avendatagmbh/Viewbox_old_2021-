// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Taxonomy.Exceptions.Resources;

namespace Taxonomy.Exceptions {
    /// <summary>
    /// Exception class for invalid taxonomy files. Thrown if an unhandled child has been found in reference nodes.
    /// </summary>
    [Serializable]
    public class UnknownReferenceNodeChildType : Exception {
        internal UnknownReferenceNodeChildType(string attribute)
            : base(String.Format(XbrlExceptions.UnknownAttribute, attribute)) { }
    }
}
