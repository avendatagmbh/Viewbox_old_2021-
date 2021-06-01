// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-10-09
// copyright 2010 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Xml;
using Taxonomy.Exceptions.Resources;

namespace Taxonomy.Exceptions {
    /// <summary>
    /// Exception class for invalid taxonomy files.
    /// </summary>
    [Serializable]
    public class InvalidTaxonomyFileException : Exception {
        internal InvalidTaxonomyFileException(string filename)
            : base(String.Format(XbrlExceptions.InvalidTaxonomyFile, filename)) { FileName = filename; }

        internal InvalidTaxonomyFileException(string filename, XmlNode node, string attributeName)
            : base(String.Format(XbrlExceptions.InvalidTaxonomyFileAttributeError, filename, node.Name, attributeName)) {
            Node = node;
            FileName = filename;
        }

        internal InvalidTaxonomyFileException(string filename, XmlNode node, Exception innerException)
            : base(String.Format(XbrlExceptions.InvalidTaxonomyFileNodeError, filename, node.Name), innerException) {
            Node = node;
            FileName = filename;
        }

        /// <summary>
        /// Gets the name of the taxonomy file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets or sets the xml-node, which raised the error.
        /// </summary>
        /// <value>The node.</value>
        public XmlNode Node { get; private set; }
    }
}