// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-11-04
// copyright 2010-2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Xml;
using Taxonomy.Exceptions;
using Taxonomy.Interfaces;

namespace Taxonomy {
    internal class RoleType : IRoleType {
        internal RoleType(XmlNode node) {
            foreach (XmlNode child in node.ChildNodes) {
                switch (child.LocalName) {
                    case "definition":
                        Name = child.InnerText;
                        break;
                    
                    case "usedOn":
                        if (child.InnerText.EndsWith("presentationLink")) HasPresentationLink = true;
                        else if (child.InnerText.EndsWith("calculationLink")) HasCalculationLink = true;
                        else if (child.InnerText.EndsWith("definitionLink")) HasDefinitionLink = true;
#if DEBUG
                        else throw new Exception("Unknown usedOn Type in RoleType.ctor: " + child.InnerText);
#endif
                        break;
                }
            }

            if (node.Attributes != null)
                foreach (XmlAttribute attr in node.Attributes) {
                    switch (attr.LocalName) {
                        case "id":
                            Id = attr.Value;
                            break;

                        case "roleURI":
                            RoleUri = attr.Value;
                            break;

#if DEBUG
                        default:
                            throw new UnknownAttributeException(attr.Name, node.Name);
#endif
                    }
                }

            if (string.IsNullOrEmpty(Id)) throw new Exception("Missing Id attribute for RoleType node.");
            if (string.IsNullOrEmpty(RoleUri)) throw new Exception("Missing RoleURI attribute for RoleType node.");
        }

        #region properties
        public string Name { get; private set; }
        public string Id { get; private set; }
        public string RoleUri { get; private set; }
        public bool HasPresentationLink { get; private set; }
        public bool HasCalculationLink { get; private set; }
        public bool HasDefinitionLink { get; private set; }

        #region
        private XbrlDisplayStyle _style = new XbrlDisplayStyle();
        public XbrlDisplayStyle Style { get { return _style; } }
        #endregion
        
        #endregion properties

        public int CompareTo(object obj) { return (obj is RoleType) ? Style.CompareTo((obj as RoleType).Style) : 0; }
        public override string ToString() { return Id + " [" + Name + "]"; }
    }
}