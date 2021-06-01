// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-11-04
// copyright 2010-2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Xml;
using Taxonomy.Exceptions;
using Taxonomy.Interfaces;

namespace Taxonomy {
    /// <summary>
    /// XBRL locator element.
    /// </summary>
    internal class Locator : ILocator {
        public Locator(XmlNode node) {
            if (node.Attributes != null)
                foreach (XmlAttribute attr in node.Attributes) {
                    switch (attr.Name) {
                        case "xlink:type":
                            Type = attr.Value;
                            break;

                        case "xlink:href":
                            HREF = attr.Value;
                            break;

                        case "xlink:label":
                            Label = attr.Value;
                            break;

                        case "xlink:title":
                            Title = attr.Value;
                            break;

#if DEBUG
                        default:
                            throw new UnknownAttributeException(attr.Name, node.Name);
#endif
                    }
                }
        }

        #region properties
        public string Type { get; private set; }
        public string HREF { get; private set; }
        public string File { get { return HREF.Substring(0, HREF.IndexOf("#")); } }
        public string Element { get { return HREF.Substring(HREF.IndexOf("#") + 1); } }
        public string Label { get; private set; }
        public string Title { get; private set; }
        #endregion properties
    }
}