// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-10-09
// copyright 2010 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Xml;
using Taxonomy.Enums;
using Taxonomy.Exceptions;
using Taxonomy.Interfaces;

namespace Taxonomy {
    /// <summary>
    /// XBRL linkbase.
    /// </summary>
    internal class LinkbaseRef : ILinkbaseRef {
        internal LinkbaseRef(XmlNode node, string path) {
            Path = path;

            if (node.Attributes != null)
                foreach (XmlAttribute attr in node.Attributes) {
                    switch (attr.LocalName) {
                        case "type":
                            Type = attr.Value;
                            if (!attr.Value.Equals("simple")) {
                                // other values are not allowed
                                throw new InvalidAttributeException(attr.Name);
                            }
                            break;

                        case "arcrole":
                            Arcrole = attr.Value;
                            if (!attr.Value.Equals("http://www.w3.org/1999/xlink/properties/linkbase")) {
                                // other values are not allowed
                                throw new InvalidAttributeException(attr.Name);
                            }
                            break;

                        case "href":
                            HREF = attr.Value;
                            break;

                        case "title":
                            Title = attr.Value;
                            break;

                        case "role":
                            RoleURI = attr.Value;
                            switch (RoleURI) {
                                case "http://www.xbrl.org/2003/role/labelLinkbaseRef":
                                    Role = LinkbaseRoles.Label;
                                    break;

                                case "http://www.xbrl.org/2003/role/definitionLinkbaseRef":
                                    Role = LinkbaseRoles.Definition;
                                    break;

                                case "http://www.xbrl.org/2003/role/referenceLinkbaseRef":
                                    Role = LinkbaseRoles.Reference;
                                    break;

                                case "http://www.xbrl.org/2003/role/presentationLinkbaseRef":
                                    Role = LinkbaseRoles.Presentation;
                                    break;

                                case "http://www.xbrl.org/2003/role/calculationLinkbaseRef":
                                    Role = LinkbaseRoles.Calculation;
                                    break;

                                default:
                                    // unknown role
                                    throw new UnknownRoleException(attr.Value);
                            }
                            break;

                        case "link":
                        case "xlink":
                            // ignored
                            break;

#if DEBUG
                        default:
                            throw new UnknownAttributeException(attr.Name, node.Name);
#endif
                    }
                }

            if (string.IsNullOrEmpty(Type)) throw new MissingAttributeException("type", node.Name);
            if (string.IsNullOrEmpty(Arcrole)) throw new MissingAttributeException("arcrole", node.Name);
            if (string.IsNullOrEmpty(HREF)) throw new MissingAttributeException("href", node.Name);
        }

        #region properties
        public string Type { get; private set; }
        public string HREF { get; private set; }
        public string Title { get; private set; }
        public string RoleURI { get; private set; }
        public LinkbaseRoles Role { get; private set; }
        public string Arcrole { get; private set; }
        public string Path { get; private set; }
        public string File { get { return Path != null ? Path + "\\" + HREF : HREF; } }
        #endregion properties
    }
}