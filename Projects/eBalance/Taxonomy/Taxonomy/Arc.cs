// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-11-04
// copyright 2010-2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Threading;
using System.Xml;
using Taxonomy.Enums;
using Taxonomy.Exceptions;
using Taxonomy.Interfaces;

namespace Taxonomy {
    internal class Arc : IArc {
        public Arc(XmlNode node) {
            Usable = true;

            if (node.Attributes != null)
                foreach (XmlAttribute attr in node.Attributes) {
                    switch (attr.LocalName) {
                        case "type":
                            Type = attr.Value;
                            break;

                        case "arcrole":
                            Arcrole = attr.Value;
                            break;

                        case "from":
                            From = attr.Value;
                            break;

                        case "to":
                            To = attr.Value;
                            break;

                        case "use":
                            Use = attr.Value;
                            break;

                        case "title":
                            Title = attr.Value;
                            break;

                        case "priority":
                            Priority = int.Parse(attr.Value);
                            break;

                        case "order":
                            //Order = double.Parse(attr.Value.Replace(".", ","));
                            Order = double.Parse(attr.Value);
                            break;

                        case "preferredLabel":
                            PreferredLabel = attr.Value;
                            break;

                        case "weight":
                            //Weight = double.Parse(attr.Value.Replace(".", ","));
                            Weight = double.Parse(attr.Value);
                            break;

                        case "presentationArc":
                            // ignored    
                            break;

                        case "xbrldt":
                            // ignored    
                            break;

                        case "usable": // xbrl dimensions extension
                            Usable = bool.Parse(attr.Value);
                            break;

                        case "contextElement": // xbrl dimensions extension
                            switch (attr.Value) {
                                case "segment":
                                    ContextElement = ContextElement.Segment;
                                    break;
                                
                                case "scenario":
                                    ContextElement = ContextElement.Scenario;
                                    break;
                                
                                default:
                                    throw new Exception("Found invalid contextElement value in arc node: " + attr.Value);
                            }
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
        public string Arcrole { get; private set; }
        public string From { get; private set; }
        public string To { get; private set; }
        public string Use { get; private set; }
        public string Title { get; private set; }
        public int Priority { get; private set; }
        public double Order { get; private set; }
        public string PreferredLabel { get; private set; }
        public double Weight { get; private set; }
        public ILocator FromLocator { get; internal set; }
        public ILocator ToLocator { get; internal set; }
        public ContextElement ContextElement { get; private set; }
        public bool Usable { get; private set; }
        #endregion properties
    }
}