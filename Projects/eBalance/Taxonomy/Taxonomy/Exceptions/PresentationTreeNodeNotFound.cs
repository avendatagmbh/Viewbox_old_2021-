// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-06-06
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Taxonomy.Exceptions.Resources;

namespace Taxonomy.Exceptions {
    /// <summary>
    /// Thrown if an expected presentation tree node could not be found.
    /// </summary>
    [Serializable]
    public class PresentationNodeTreeNotFound : Exception {
        internal PresentationNodeTreeNotFound(string nodeName)
            : base(String.Format(XbrlExceptions.InvalidAttribute, nodeName)) { }
    }
}