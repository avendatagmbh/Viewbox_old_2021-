// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-11-04
// copyright 2010 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Xml;
using Taxonomy.Exceptions;
using Taxonomy.Interfaces;

namespace Taxonomy {
    /// <summary>
    /// Xbrl Label element.
    /// </summary>
    internal class Label : ILabel {
        internal Label(XmlNode node) {
            Caption = node.InnerText;

            if (node.Attributes != null)
                foreach (XmlAttribute attr in node.Attributes) {
                    switch (attr.Name) {
                        case "xlink:type":
                            Type = attr.Value;
                            break;

                        case "xlink:label":
                            Content = attr.Value;
                            break;

                        case "id":
                            Id = attr.Value;
                            break;

                        case "xlink:role":
                            Role = attr.Value;
                            break;

                        case "xml:lang":
                            Language = attr.Value;
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
        public string Content { get; private set; }
        public string Id { get; private set; }
        public string Role { get; private set; }
        public string Language { get; private set; }
        public string Caption { get; private set; }
        public string Title { get; private set; }
        #endregion properties
   }
}