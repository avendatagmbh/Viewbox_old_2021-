// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-10-09
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Taxonomy.Enums;
using Taxonomy.Exceptions;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using Taxonomy.PresentationTree;
using System.Linq;

namespace Taxonomy {

    /// <summary>
    /// This class represents a XBRL taxonomy schema.
    /// </summary>
    public class Taxonomy : ITaxonomy {

        #region constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Taxonomy"/> class.
        /// </summary>
        public Taxonomy(ITaxonomyInfo taxonomyInfo) {
            TaxonomyInfo = taxonomyInfo;
            Path =
                System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    taxonomyInfo.Path);
            
            Filename = taxonomyInfo.Filename;

            Init();
            ReadReferences();

            foreach (var element in Elements) {
                ((Element) element.Value).SetReferences();
            }

            if (taxonomyInfo.Type != TaxonomyType.Unknown) ApplyStyle();

            // DEBUG: create default style
            //System.Diagnostics.Debug.WriteLine("default style for taxonomy " + taxonomyInfo.Name);
            //System.Diagnostics.Debug.WriteLine("--------------------------------------------------------------------------------");
            //System.Diagnostics.Debug.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8"" ?>");
            //System.Diagnostics.Debug.WriteLine("");
            //System.Diagnostics.Debug.WriteLine("<style>");
            //System.Diagnostics.Debug.WriteLine("  <roles>");
            //foreach (var role in Roles) {
            //    System.Diagnostics.Debug.WriteLine("");
            //    System.Diagnostics.Debug.WriteLine("    <!-- " + role.Name + "-->");
            //    System.Diagnostics.Debug.WriteLine("    <role name=\"" + role.Id + "\" isVisible=\"true\" showBalanceList=\"true\"/>");
            //}
            //System.Diagnostics.Debug.WriteLine("  </roles>");
            //System.Diagnostics.Debug.WriteLine("</style>");

            // DEBUG: show available tuples
            //System.Diagnostics.Debug.WriteLine("available tuple elements");
            //System.Diagnostics.Debug.WriteLine("--------------------------------------------------------------------------------");
            //foreach (var element in Elements.Values) {
            //    if (element.IsTuple) {
            //        System.Diagnostics.Debug.WriteLine(element.Label);
            //        System.Diagnostics.Debug.WriteLine(element.Name);
            //        System.Diagnostics.Debug.WriteLine(element.Id);
            //        System.Diagnostics.Debug.WriteLine("");
            //    }
            //}
            //System.Diagnostics.Debug.WriteLine("--------------------------------------------------------------------------------");
        }
        #endregion constructor

        #region properties

        #region LinkbaseRefs
        private Dictionary<LinkbaseRoles, List<ILinkbaseRef>> _linkbaseRefs =
            new Dictionary<LinkbaseRoles, List<ILinkbaseRef>>();

        public Dictionary<LinkbaseRoles, List<ILinkbaseRef>> LinkbaseRefs { get { return _linkbaseRefs; } }
        #endregion LinkbaseRefs

        #region Roles
        private Dictionary<string, RoleType> _roleTypes = new Dictionary<string, RoleType>();
        public IEnumerable<IRoleType> Roles { get { return _roleTypes.Values; } }

        public IRoleType GetRole(string id) {
            if (_roleTypes.ContainsKey(id)) return _roleTypes[id];
            else throw new Exception("Role '" + id + "' does not exist in taxonomy file " + Filename + ".");
        }

        /// <summary>
        /// Check if the specified role exists in the current taxonomie
        /// </summary>
        /// <param name="id">The role that should be looked for.</param>
        /// <returns>If the current taxonomie contains this role.</returns>
        public bool RoleExists(string id) { return _roleTypes.ContainsKey(id); }
        
        #endregion Roles

        #region Elements
        private Dictionary<string, IElement> _elements = new Dictionary<string, IElement>();
        public Dictionary<string, IElement> Elements { get { return _elements; } }
        #endregion Elements

        #region ElementsByName
        private readonly Dictionary<string, IElement> _elementsByName = new Dictionary<string, IElement>();
        private Dictionary<string, IElement> ElementsByName { get { return _elementsByName; } }
        #endregion ElementsByName

        #region PresentationTrees
        private readonly Dictionary<string, IPresentationTree> _presentationTreeDict =
            new Dictionary<string, IPresentationTree>();

        private readonly List<IPresentationTree>  _presentationTrees = new List<IPresentationTree>();

        public IEnumerable<IPresentationTree> PresentationTrees { get { return _presentationTrees; } }

        public IPresentationTree GetPresentationTree(string roleUri) {
            IPresentationTree ptree;
            _presentationTreeDict.TryGetValue(roleUri, out ptree);
            return ptree;
        }
        #endregion

        public ITaxonomyInfo TaxonomyInfo { get; private set; }
        public string Path { get; private set; }
        public string Filename { get; private set; }
        public string SchemaRef { get; private set; }

        private List<string> LoadedFiles { get; set; }

        #endregion properties

        #region methods

        #region InitSelectionLists
        public void InitSelectionLists() {
            foreach (var element in Elements.Values) {
                if ((element.ValueType == XbrlElementValueTypes.SingleChoice ||
                     element.ValueType == XbrlElementValueTypes.MultipleChoice)) {

                    foreach (var subelement in element.Children) {
                        subelement.Children.Clear();
                        foreach (var valueElement in Elements.Values) {
                            string sg = valueElement.SubstitutionGroup;
                            if (sg.Equals(subelement.Name)) {
                                if (!IsAllowedListEntry(valueElement)) continue;
                                subelement.Children.Add(valueElement);
                            }
                        }
                    }
                }
            }
        }
        #endregion InitSelectionLists

        #region IsAllowedListEntry
        /// <summary>
        /// Hide members, which are not allowed in the specified selection list).
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        public static bool IsAllowedListEntry(IElement elem) {
            if (elem.NotPermittedForFinancial) return false;
            string elemId = elem.Id;

            if (elemId.StartsWith("de-gcd_genInfo.report.id.accountingStandard.accountingStandard.")) {
                return elemId.EndsWith(".head") ||
                       elemId.EndsWith(".HGBM") ||
                       elemId.EndsWith(".HAOE") ||
                       elemId.EndsWith(".AO");
            }

            return !elemId.StartsWith("de-gcd_genInfo.report.id.consolidationRange.") || elemId.EndsWith(".EA");
        }
        #endregion IsAllowedListEntry

        #region Init
        private void Init() {
            LoadedFiles = new List<string>();
            Load(System.IO.Path.Combine(Path, Filename));

            // init element hierarchy structure
            foreach (IElement element in Elements.Values) {
                foreach (string reference in element.ChildReferences) {
                    ElementsByName[reference].Parents.Add(element);
                    element.Children.Add(ElementsByName[reference]);
                }
            }

            foreach (RoleType role in Roles) {
                try {
                    if (role.HasCalculationLink) {
                        // key1: "from id", key2: "to id"
                        var calculationArcDict = new Dictionary<string, Dictionary<string, Arc>>();

                        // load presentation locator tree
                        foreach (LinkbaseRef linkbase in LinkbaseRefs[LinkbaseRoles.Calculation]) {
                            var tmp = LoadCalulationFile(linkbase.File, role);
                            if (tmp != null) {
                                foreach (var arc in tmp) {
                                    if (calculationArcDict.ContainsKey(arc.From)) {
                                        var subDict = calculationArcDict[arc.From];
                                        if (subDict.ContainsKey(arc.To)) {
                                            // resolve conflict
                                            if (arc.Priority > subDict[arc.To].Priority) {
                                                // replace arc
                                                subDict[arc.To] = arc;
                                            }
                                        } else {
                                            subDict[arc.To] = arc;
                                        }
                                    } else {
                                        calculationArcDict[arc.From] = new Dictionary<string, Arc>();
                                        calculationArcDict[arc.From][arc.To] = arc;
                                    }
                                }
                            }
                        }

                        // collect allowed arc values
                        var calculationArcList = calculationArcDict.Values.SelectMany(
                            subDict => subDict.Values.Where(arc => arc.Use != "prohibited")).ToList();

                        foreach (var arc in calculationArcList) {

                            if (Elements.ContainsKey(arc.FromLocator.Element) &&
                                Elements.ContainsKey(arc.ToLocator.Element)) {

                                var element = Elements[arc.FromLocator.Element];

                                ((Element) element).AddSummationItem(Elements[arc.ToLocator.Element], arc.Weight, arc);
                            } else {
                                // computation source or target not in presentation layer!
                                if (!Elements.ContainsKey(arc.FromLocator.Element))
                                    Debug.WriteLine("Missing calculation locator source: " + arc.FromLocator.Element);
                                else
                                    Debug.WriteLine("Missing calculation locator dest: " + arc.ToLocator.Element);
                            }
                        }

                        // process definition linkbases
                        if (LinkbaseRefs.ContainsKey(LinkbaseRoles.Definition)) {
                            foreach (LinkbaseRef linkbaseRef in LinkbaseRefs[LinkbaseRoles.Definition]) {
                                foreach (Arc definitionArc in LoadDefinitionFile(linkbaseRef.File, role)) {
                                    switch (definitionArc.Arcrole) {
                                        case "http://xbrl.org/int/dim/arcrole/all":
                                        case "http://xbrl.org/int/dim/arcrole/notAll":
                                            // TODO
                                            break;

                                        case "http://xbrl.org/int/dim/arcrole/dimension-domain":
                                        case "http://xbrl.org/int/dim/arcrole/hypercube-dimension":
                                        case "http://xbrl.org/int/dim/arcrole/domain-member":
                                            //IElement element;
                                            //Elements.TryGetValue(definitionArc.To, out element);
                                            //if (element != null) ((Element)element).IsDimensionItem = true;
                                            // TODO
                                            break;

                                        default:
                                            break;

                                    }
                                }
                            }
                        }


                    }
                } catch (Exception) {
                    throw;
                }
            }

            InitSelectionLists();
            LoadLabels();
            ComputePresentationTrees();
        }
        #endregion Init

        #region Load
        private void Load(string filename) {
            if (LoadedFiles.Contains(filename)) return;
            LoadedFiles.Add(filename);

            // load schema from file
            XmlSchema schema;
            using (var reader = new XmlTextReader(filename)) {
                schema = XmlSchema.Read(reader, null);
            }

            // set target namespace and target namespace name
            var targetNamespace =
                schema.Namespaces.ToArray().FirstOrDefault(ns => ns.Namespace == schema.TargetNamespace);

            if (targetNamespace == null) 
                throw new Exception("Target namespace not found in schema namespaces!");

            if (LoadedFiles.Count == 1) SchemaRef = targetNamespace.Namespace + "/" + Filename;

            foreach (var schemaObject in schema.Items) {
                if (schemaObject is XmlSchemaAnnotation) {
                    ParseXmlSchemaObject(schemaObject as XmlSchemaAnnotation, filename);

                } else if (schemaObject is XmlSchemaElement) {
                    ParseXmlSchemaObject(schemaObject as XmlSchemaElement, targetNamespace);

                } else {
                    System.Diagnostics.Debug.WriteLine("Found unknown SchemaObject type in Taxonomy.Load: " +
                                                       schemaObject.GetType().Name);

                }
            }

            // load include and import references
            foreach (var include in schema.Includes) {
                if (!(include is XmlSchemaExternal)) continue;
                Uri uri = new Uri(new Uri(include.SourceUri),
                                  new Uri((include as XmlSchemaExternal).SchemaLocation, UriKind.RelativeOrAbsolute));
                if (uri.IsFile) Load(uri.LocalPath.Replace("%20", " "));
            }
        }
        #endregion Load

        private void ParseXmlSchemaObject(XmlSchemaAnnotation annotation, string filename) {
            foreach (var annotationObject in annotation.Items) {
                if (annotationObject is XmlSchemaAppInfo) {
                    var appInfo = annotationObject as XmlSchemaAppInfo;
                    foreach (var node in appInfo.Markup) {
                        try {
                            if (node.LocalName == "linkbaseRef") {
                                LinkbaseRef lbr = new LinkbaseRef(node, new FileInfo(filename).DirectoryName);
                                if (!LinkbaseRefs.ContainsKey(lbr.Role)) LinkbaseRefs[lbr.Role] = new List<ILinkbaseRef>();
                                LinkbaseRefs[lbr.Role].Add(lbr);
                            } else if (node.LocalName == "roleType") {
                                RoleType roleType = new RoleType(node);
                                _roleTypes[roleType.RoleUri] = roleType;
                            } else {
#if DEBUG
                                throw new Exception("Found unhandled element type in Taxonomy.Load / AppInfo: " +
                                                    node.GetType().Name);
#endif
                            }
                        } catch (Exception ex) {
                            throw new InvalidTaxonomyFileException(filename, node, ex);
                        }
                    }
                } else {
#if DEBUG
                    throw new Exception("Found unhandled SchemaObject type in Taxonomy.Load: " +
                                        annotationObject.GetType().Name);
#endif
                }
            }
        }

        public void ParseXmlSchemaObject(XmlSchemaElement element, XmlQualifiedName targetNamespace) {
            try {
                Element elem = new Element(element, targetNamespace);
                Elements[elem.Id] = elem;
                ElementsByName[elem.TargetNamespace.Namespace + ":" + elem.Name] = elem;
            } catch (Exception) {
                throw;
            }
        }

        #region ReadLabels

        private void LoadLabels() { foreach (var labelRef in LinkbaseRefs[LinkbaseRoles.Label]) LoadLabels(labelRef.File); }

        private void LoadLabels(string file) {

            XmlDocument doc = new XmlDocument();

            // Create an XmlNamespaceManager to resolve the default namespace.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("base", "http://www.xbrl.org/2003/linkbase");
            nsmgr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
            nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            try {
                doc.Load(file);

                foreach (
                    XmlNode labelLinkNode in doc.DocumentElement.SelectNodes("/base:linkbase/base:labelLink", nsmgr)) {

                    Dictionary<string, Locator> locators = new Dictionary<string, Locator>();
                    Dictionary<string, List<Label>> labels = new Dictionary<string, List<Label>>();

                    // collect locators
                    foreach (XmlNode locatorNode in labelLinkNode.SelectNodes("base:loc", nsmgr)) {
                        Locator loc = new Locator(locatorNode);

                        if (!locators.ContainsKey(loc.Label)) {
                            locators[loc.Label] = loc;
                        } else {
                            //throw new Exception("Locator Element '" + loc.Label + "' mehrfach gefunden!");
                        }
                    }

                    // collect labels
                    foreach (XmlNode labelNode in labelLinkNode.SelectNodes("base:label", nsmgr)) {
                        Label label = new Label(labelNode);

                        if (!labels.ContainsKey(label.Content)) {
                            labels[label.Content] = new List<Label>();
                        }

                        labels[label.Content].Add(label);
                    }

                    // collect label arcs
                    foreach (XmlNode labelArcNode in labelLinkNode.SelectNodes("base:labelArc", nsmgr)) {
                        Arc arc = new Arc(labelArcNode);

                        if (!labels.ContainsKey(arc.To)) {
                            continue;
                        }

                        if (!locators.ContainsKey(arc.From)) {
                            continue;
                        }

                        if (!this.Elements.ContainsKey(locators[arc.From].Element)) {
                            continue;
                        }

                        foreach (Label label in labels[arc.To]) {
                            ((Element) (Elements[locators[arc.From].Element])).AddLabel(label);
                        }
                    }

                }

            } catch (Exception) {
                throw;
            }
        }
        #endregion ReadLabels

        #region ReadReferences
        private void ReadReferences() {

            if (!this.LinkbaseRefs.ContainsKey(LinkbaseRoles.Reference)) return;

            foreach (LinkbaseRef linkbaseRef in this.LinkbaseRefs[LinkbaseRoles.Reference]) {

                XmlDocument doc = new XmlDocument();

                // Create an XmlNamespaceManager to resolve the default namespace.
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("base", "http://www.xbrl.org/2003/linkbase");
                nsmgr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
                nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");

                try {
                    doc.Load(linkbaseRef.File);

                    foreach (
                        XmlNode referenceLinkNode in
                            doc.DocumentElement.SelectNodes("/base:linkbase/base:referenceLink", nsmgr)) {

                        Dictionary<string, Locator> locators = new Dictionary<string, Locator>();
                        Dictionary<string, Reference> references = new Dictionary<string, Reference>();

                        // collect locators
                        foreach (XmlNode locatorNode in referenceLinkNode.SelectNodes("base:loc", nsmgr)) {
                            Locator loc = new Locator(locatorNode);

                            if (!locators.ContainsKey(loc.Label)) {
                                locators[loc.Label] = loc;
                            } else {
                                //throw new Exception("Locator Element '" + loc.Label + "' mehrfach gefunden!");
                            }
                        }

                        // collect references
                        foreach (XmlNode referenceNode in referenceLinkNode.SelectNodes("base:reference", nsmgr)) {
                            Reference reference = new Reference(referenceNode);
                            references[reference.Content] = reference;
                        }

                        // collect label arcs
                        foreach (XmlNode referenceArcNode in referenceLinkNode.SelectNodes("base:referenceArc", nsmgr)) {
                            Arc arc = new Arc(referenceArcNode);

                            if (!references.ContainsKey(arc.To)) {
                                continue;
                            }

                            if (!locators.ContainsKey(arc.From)) {
                                continue;
                            }

                            if (!this.Elements.ContainsKey(locators[arc.From].Element)) {
                                continue;
                            }

                            ((Element) Elements[locators[arc.From].Element]).AddReference(references[arc.To]);

                            if (!string.IsNullOrEmpty(references[arc.To].FiscalRequirement)) {

                                var elem = Elements[locators[arc.From].Element];

                                switch (references[arc.To].FiscalRequirement) {
                                    case "Rechnerisch notwendig, soweit vorhanden":
                                        (elem as Element).MandatoryType = MandatoryType.Computed;
                                        break;

                                    case "Summenmussfeld":
                                        (elem as Element).MandatoryType = MandatoryType.Sum;
                                        break;

                                    case "Mussfeld, Kontennachweis erwünscht":
                                        (elem as Element).MandatoryType = MandatoryType.AccountBalance;
                                        break;

                                    case "Mussfeld":
                                        (elem as Element).MandatoryType = MandatoryType.Default;
                                        break;

                                    default:
                                        break;
                                }

                                ((Element) elem).IsMandatoryField = true;
                            }

                            if (!string.IsNullOrEmpty(references[arc.To].NotPermittedFor)) {
                                switch (references[arc.To].NotPermittedFor) {
                                    case "steuerlich":
                                        ((Element) Elements[locators[arc.From].Element]).NotPermittedForFiscal = true;
                                        break;
                                    case "handelsrechtlicher Einzelabschluss":
                                        ((Element) Elements[locators[arc.From].Element]).NotPermittedForCommercial =
                                            true;
                                        break;
                                    case "Einreichung an Finanzverwaltung":
                                        ((Element) Elements[locators[arc.From].Element]).NotPermittedForFinancial = true;
                                        break;
                                }
                            }


                        }

                    }

                } catch (Exception) {
                    throw;
                }
            }

        }
        #endregion ReadReferences

        #region LoadPresentationFile
        private List<Arc> LoadPresentationFile(string file, RoleType role) { return LoadArcFile("presentation", file, role); }
        #endregion LoadPresentationFile

        #region LoadCalulationFile
        private List<Arc> LoadCalulationFile(string file, RoleType role) { return LoadArcFile("calculation", file, role); }
        #endregion LoadCalulationFile

        #region LoadCalulationFile
        private List<Arc> LoadDefinitionFile(string file, RoleType role) { return LoadArcFile("definition", file, role); }
        #endregion LoadCalulationFile

        #region LoadArcFile
        private List<Arc> LoadArcFile(string type, string file, RoleType role) {
            XmlDocument doc = new XmlDocument();
            List<Arc> locatorList = new List<Arc>();

            try {
                // Create an XmlNamespaceManager to resolve the default namespace.
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("base", "http://www.xbrl.org/2003/linkbase");
                nsmgr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
                nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");

                doc.Load(file);

                Dictionary<string, Locator> locators = new Dictionary<string, Locator>();

                foreach (
                    XmlNode linkNode in doc.DocumentElement.SelectNodes("/base:linkbase/base:" + type + "Link", nsmgr)) {

                    if (!linkNode.Attributes["xlink:role"].Value.Equals(role.RoleUri)) continue;

                    // collect locators
                    foreach (XmlNode locatorNode in linkNode.SelectNodes("base:loc", nsmgr)) {
                        Locator loc = new Locator(locatorNode);

                        if (!locators.ContainsKey(loc.Label)) {
                            locators[loc.Label] = loc;
                        } else {
                            //throw new Exception("Locator Element '" + loc.Label + "' mehrfach gefunden!");
                        }
                    }

                    // collect presentation arcs
                    foreach (XmlNode arcNode in linkNode.SelectNodes("base:" + type + "Arc", nsmgr)) {
                        Arc arc = new Arc(arcNode);

                        if (!locators.ContainsKey(arc.From)) continue;
                        if (!locators.ContainsKey(arc.To)) continue;
                        if (!this.Elements.ContainsKey(locators[arc.From].Element)) continue;
                        if (!this.Elements.ContainsKey(locators[arc.To].Element)) continue;

                        arc.FromLocator = locators[arc.From];
                        arc.ToLocator = locators[arc.To];

                        locatorList.Add(arc);
                    }
                }

                return locatorList;

            } catch (Exception) {
                throw;
            }
        }
        #endregion LoadArcFile

        #region ComputePresentationTrees
        private void ComputePresentationTrees() {

            Debug.Assert(_presentationTrees.Count == 0, "Presentation tree list is not empty!");
            Debug.Assert(_presentationTreeDict.Count == 0, "Presentation tree dictionary is not empty!");

            foreach (RoleType role in Roles) {
                try {
                    if (!role.HasPresentationLink) continue;                   
                    var ptree = ComputePresentationTree(role);

                    // (ptree == null) means that the presentation tree could not be found for a specific role, which
                    // is usually be caused by a missing linkbaseRef definition, e.g. to replace the balance_sheet 
                    // presentation tree from the base taxonomy with the balance_sheet presentation tree of a industry
                    // sector specific taxonomy.
                    if (ptree == null) continue;

                    _presentationTreeDict[role.RoleUri] = ptree;
                    _presentationTrees.Add(ptree);
                
                } catch (Exception ex) {
                    throw new Exception(ex.Message, ex);
                }
            }
        }
        #endregion ComputePresentationTrees

        #region ComputePresentationTree
        private PresentationTree.PresentationTree ComputePresentationTree(RoleType role) {
            try {
                Debug.Assert(role.HasPresentationLink, "Function ComputePresentationTree was called for a role without presentation links!");

                // key1: "from id", key2: "to id"
                var presentationArcDict = new Dictionary<string, Dictionary<string, Arc>>();

                // load presentation locator tree
                foreach (LinkbaseRef linkbase in LinkbaseRefs[LinkbaseRoles.Presentation]) {
                    var tmp = LoadPresentationFile(linkbase.File, role);
                    if (tmp != null) {
                        foreach (var arc in tmp) {

                            if (presentationArcDict.ContainsKey(arc.From)) {
                                var subDict = presentationArcDict[arc.From];
                                if (subDict.ContainsKey(arc.To)) {
                                    // resolve conflict
                                    if (arc.Priority > subDict[arc.To].Priority) {
                                        // replace arc
                                        subDict[arc.To] = arc;
                                    }
                                } else {
                                    subDict[arc.To] = arc;
                                }
                            } else {
                                presentationArcDict[arc.From] = new Dictionary<string, Arc>();
                                presentationArcDict[arc.From][arc.To] = arc;
                            }
                        }
                    }
                }

                // collect allowed arc values
                var presentationArcList = presentationArcDict.Values.SelectMany(
                    subDict => subDict.Values.Where(arc => arc.Use != "prohibited")).ToList();

                // no presentation tree entries found
                if (!presentationArcList.Any()) return null;

                var pt = new PresentationTree.PresentationTree();
                var presentationTreeNodes = new Dictionary<string, PresentationTreeNode>();

                // create presentation tree nodes
                foreach (var presentationLoc in presentationArcList) {
                    var reference = presentationLoc.FromLocator.Element;
                    if (Elements.ContainsKey(reference) && !presentationTreeNodes.ContainsKey(reference)) {
                        var elem = Elements[reference];
                        presentationTreeNodes[reference] = new PresentationTreeNode(pt, elem);
                        elem.PresentationTreeNodes.Add(presentationTreeNodes[reference]);
                    }

                    reference = presentationLoc.ToLocator.Element;
                    if (Elements.ContainsKey(reference) && !presentationTreeNodes.ContainsKey(reference)) {
                        var elem = Elements[reference];
                        presentationTreeNodes[reference] = new PresentationTreeNode(pt, elem);
                        elem.PresentationTreeNodes.Add(presentationTreeNodes[reference]);
                    }
                }

                // create presentation tree structure
                foreach (var presentationArc in presentationArcList) {

                    var fromNode = presentationTreeNodes[presentationArc.FromLocator.Element];
                    var toNode = presentationTreeNodes[presentationArc.ToLocator.Element];

                    toNode.Order = presentationArc.Order;

                    fromNode.AddChildren(toNode);
                }

                pt.InitNodes(presentationTreeNodes.Values);
                pt.SetIsHypercubeFlag(presentationTreeNodes.Values);

                pt.Role = role;
                return pt;

            } catch (Exception ex) {
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion ComputePresentationTree

        private void ApplyStyle() {
            ApplyGlobalStyle();
            ApplySpecialStyle();
        }

        private void ApplyGlobalStyle() {
            XmlDocument doc = new XmlDocument();
            //doc.Load(@"taxonomy\styles\global.xml");
            doc.Load(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "taxonomy", "styles", "global.xml"));
            var typedefNode = doc.SelectSingleNode("style/typedefs");
            foreach (XmlNode typedef in typedefNode.ChildNodes) {
                switch (typedef.Name) {
                    case "list": {
                        IElement elem;
                        Elements.TryGetValue(typedef.InnerText, out elem);
                        if (elem != null) elem.IsList = true;
                    }
                        break;

                    case "multiple_choice": {
                        IElement elem;
                        Elements.TryGetValue(typedef.InnerText, out elem);
                        if (elem != null) elem.ValueType = XbrlElementValueTypes.MultipleChoice;
                    }
                        break;

#if DEBUG
                    default:
                        throw new Exception("Found unknown typedef in style definition.");
#endif
                }
            }
        }

        private void ApplySpecialStyle() {
            //var file = @"taxonomy\styles\" + TaxonomyInfo.Name + ".xml";
            var file = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "taxonomy", "styles",  TaxonomyInfo.Name + ".xml");
            if (File.Exists(file)) ApplySpecialStyle(file);
        }

        private void ApplySpecialStyle(string file) {

            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            Dictionary<string, IRoleType> rolesById = new Dictionary<string, IRoleType>();
            foreach (var role in _roleTypes.Values) rolesById[role.Id] = role;

            int ordinal = 0;
            var roleNode = doc.SelectSingleNode("style/roles");
            if (roleNode != null)
                foreach (XmlNode role in roleNode.ChildNodes) {
                    if (role.Attributes == null) continue;
                    
                    string name = null;
                    bool display = true;
                    bool showBalanceList = true;

                    foreach (XmlAttribute attr in role.Attributes) {
                        switch (attr.Name) {
                            case "name":
                                name = attr.Value;
                                break;
                                
                            case "isVisible":
                                bool.TryParse(attr.Value, out display);
                                break;
                                
                            case "showBalanceList":
                                bool.TryParse(attr.Value, out showBalanceList);
                                break;

#if DEBUG
                            default:
                                throw new Exception("Found unknown role in style definition.");
#endif
                        }
                    }

                    if (string.IsNullOrEmpty(name) || !rolesById.ContainsKey(name)) continue;
                    var r = rolesById[name];
                    r.Style.Taxonomy = this;
                    r.Style.Ordinal = ordinal;
                    r.Style.ShowBalanceList = showBalanceList;
                    r.Style.IsVisible = display;

                    ordinal++;
                }

            var elementNode = doc.SelectSingleNode("style/elements");
            if (elementNode != null) {
                foreach (XmlNode node in elementNode.ChildNodes) {
                    if (node.Attributes == null) continue;

                    string name = null;
                    bool isPersistant = true;
                    bool legalFormPersG = true, legalFormEU = true, legalFormOther = true;
                    bool foundPersG = false, foundEu = false, foundLegalFormOther = false;

                    foreach (XmlAttribute attr in node.Attributes) {
                        switch (attr.Name) {
                            case "name":
                                name = attr.Value;
                                break;

                            case "isPersistant":
                                bool.TryParse(attr.Value, out isPersistant);
                                break;
                            case "legalFormPersG":
                                foundPersG = true;
                                bool.TryParse(attr.Value, out legalFormPersG);
                                break;
                            case "legalFormEU":
                                foundEu = true;
                                bool.TryParse(attr.Value, out legalFormEU);
                                break;
                            case "legalFormOther":
                                foundLegalFormOther = true;
                                bool.TryParse(attr.Value, out legalFormOther);
                                break;
#if DEBUG
                            default:
                                throw new Exception("Found unknown element in style/elements definition.");
#endif
                        }
                    }

                    if (string.IsNullOrEmpty(name) || !Elements.ContainsKey(name)) continue;

                    Elements[name].IsPersistant = isPersistant;
                    if (foundPersG) {
                        (Elements[name] as Element).SetLegalFormPG(legalFormPersG);
                    }
                    if (foundEu) {
                        (Elements[name] as Element).SetLegalFormEU(legalFormEU);
                    }
                    if (foundLegalFormOther) {
                        (Elements[name] as Element).SetLegalFormOther(legalFormOther);
                    }
                }
            }
        }
        #endregion methods
    }
}