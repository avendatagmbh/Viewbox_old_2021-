// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-11-04
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Taxonomy.Enums;
using Taxonomy.Exceptions;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;

namespace Taxonomy {
    public class SummationItem {
        public IElement Element { get; internal set; }
        public double Weight { get; internal set; }
        public IArc CalculationArc { get; internal set; }
    }

    /// <summary>
    /// This class represents a XBRL element.
    /// </summary>
    internal class Element : IElement, INotifyPropertyChanged {
        internal Element(XmlSchemaElement element, XmlQualifiedName targetNamespace) {
            PresentationTreeNodes = new List<IPresentationTreeNode>();
            ChildReferences = new List<string>();
            Children = new List<IElement>();
            _labels = new Dictionary<string, Dictionary<LabelRoles, ILabel>>();

            PreferTerseLabels = true;
            IsPersistant = true;

            TargetNamespace = targetNamespace;
            Id = element.Id;
            Name = element.Name;
            Nillable = element.IsNillable;
            IsAbstract = element.IsAbstract;
            SubstitutionGroup = element.SubstitutionGroup.Name;
            Type = element.SchemaTypeName.Name;
            
            LegalFormEU = false;
            LegalFormKSt = false;
            LegalFormPG = false;
            LegalFormSEAG = false;
            LegalFormVVaG = false;
            LegalFormOerV = false;
            LegalFormBNaU = false;

            // Initialise everything with true -> NULL is true
            TaxonomieNEIN = true;
            TaxonomiePBV = true;
            TaxonomieKHBV = true;
            TaxonomieEBV = true;
            TaxonomieVUV = true;
            TaxonomieWUV = true;
            TaxonomieLUF = true;

            if (element.UnhandledAttributes != null) {
                foreach (var attr in element.UnhandledAttributes) {
                    switch (attr.LocalName) {

                        case "balance":
                            Balance = attr.Value;
                            break;

                        case "periodType":
                            PeriodType = attr.Value;
                            break;

#if DEBUG
                        default:
                            throw new UnknownAttributeException(attr.Name, element.Name);
#endif
                    }
                }
            }

            if (element.SchemaType is XmlSchemaComplexType)
                AnalyseComplexType(element.SchemaType as XmlSchemaComplexType);

            //foreach (XmlNode childNode in node.ChildNodes) 
            //if (childNode.Name.Equals("complexType")) 
            //    AnalyseComplexType(childNode);

            if (ValueType == XbrlElementValueTypes.None)
                ValueType = GetValueType();
        }

        #region AnalyseComplexType
        private void AnalyseComplexType(XmlSchemaComplexType complexType) {
            if (complexType.ContentModel is XmlSchemaSimpleContent)
                AnalyseComplexTypeSimpleContent(complexType.ContentModel as XmlSchemaSimpleContent);
            else
                AnalyseComplexTypeComplexContent(complexType);
        }

        private void AnalyseComplexTypeSimpleContent(XmlSchemaSimpleContent content) {
            if (content.Content is XmlSchemaSimpleContentRestriction) {
                var restriction = content.Content as XmlSchemaSimpleContentRestriction;
                Type = restriction.BaseTypeName.Name;
            } else if (content.Content is XmlSchemaSimpleContentExtension) {
#if DEBUG
                throw new Exception("Taxonomy.Element: extensions for simple content are not yet supported.");
#endif
            } else {
#if DEBUG
                throw new Exception("Taxonomy.Element: unhandled XmlSchemaSimpleContent.Content type: " +
                                    content.Content.GetType().Name);
#endif
            }
        }

        private void AnalyseComplexTypeComplexContent(XmlSchemaComplexType complexType) {
            if (complexType.Particle is XmlSchemaChoice)
                AnalyseXmlSchemaChoice(complexType.Particle as XmlSchemaChoice);
            else if (complexType.Particle is XmlSchemaAll)
                AnalyseXmlSchemaAll(complexType.Particle as XmlSchemaAll);
            else if (complexType.Particle is XmlSchemaSequence)
                AnalyseXmlSchemaSequence(complexType.Particle as XmlSchemaSequence);
            else if (complexType.Particle is XmlSchemaGroupRef)
                AnalyseXmlSchemaGroupRef(complexType.Particle as XmlSchemaGroupRef);
            else {
#if DEBUG
                throw new Exception("Unhandled particle type when parsing complex type: " +
                                    complexType.Particle.GetType().Name);
#endif
            }
        }

        private void AnalyseXmlSchemaChoice(XmlSchemaChoice choice) {
            // The <choice> indicator specifies that either one child element or another can occur.
            ValueType = XbrlElementValueTypes.SingleChoice;
            foreach (var item in choice.Items) {
                if (item is XmlSchemaElement) {
                    var refName = (item as XmlSchemaElement).RefName.ToString();
                    ChildReferences.Add(refName);
                }
            }
        }

        private void AnalyseXmlSchemaAll(XmlSchemaAll xmlSchemaAll) {
#if DEBUG
            // The <all> indicator specifies that the child elements can appear in any order, and that each child element must occur only once.
            throw new NotImplementedException("Handling of complex type particle XmlSchemaAll is not yet implemented.");
#endif
        }

        private void AnalyseXmlSchemaSequence(XmlSchemaSequence sequence) {
            // The <sequence> indicator specifies that the child elements must appear in a specific order.
            foreach (var item in sequence.Items) {
                if (item is XmlSchemaElement) {
                    var refName = (item as XmlSchemaElement).RefName.ToString();
                    ChildReferences.Add(refName);
                } else {
#if DEBUG
                    throw new NotImplementedException(
                        "Found unhandled item type in complex type / sequence element: " + item.GetType().Name +
                        Environment.NewLine + item.SourceUri + " / " + item.LineNumber);
#endif

                }
            }           
        }

        private void AnalyseXmlSchemaGroupRef(XmlSchemaGroupRef groupRef) {
#if DEBUG
            throw new NotImplementedException("Handling of complex type particle XmlSchemaGroupRef is not yet implemented.");
#endif
        }
        #endregion AnalyseComplexType

        private XbrlElementValueTypes GetValueType() {
            if (IsTuple) return XbrlElementValueTypes.Tuple;
            if (IsAbstract) return XbrlElementValueTypes.Abstract;
            switch (Type) {
                case "monetaryItemType":
                    return XbrlElementValueTypes.Monetary;

                case "dateItemType":
                    return XbrlElementValueTypes.Date;

                case "booleanItemType":
                    return XbrlElementValueTypes.Boolean;

                case "stringItemType":
                case "gYearItemType":
                case "anyURIItemType":
                case "languageItemType":
                case "QNameItemType":

                    return XbrlElementValueTypes.String;

                case "nonNegativeIntegerItemType":
                case "positiveIntegerItemType":
                    return XbrlElementValueTypes.Int;

                case "decimalItemType":
                    return XbrlElementValueTypes.Numeric;

                case "":
                    return XbrlElementValueTypes.String;

                default:
#if DEBUG
                    throw new Exception("Unhandled value type: " + Type);
#else
                    return XbrlElementValueTypes.String;
#endif
            }
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events

        private readonly Dictionary<string, Dictionary<LabelRoles, ILabel>> _labels;

        private string _typeOperatingResult;

        #region properties

        #region summation rules
        private readonly List<IElement> _summationSources = new List<IElement>();
        private readonly List<SummationItem> _summationTargets = new List<SummationItem>();
        public IEnumerable<SummationItem> SummationTargets { get { return _summationTargets; } }

        public IEnumerable<IElement> SummationSources { get { return _summationSources; } }
        #endregion

        #region Label properties
        #region PreferTerseLabels
        public bool PreferTerseLabels {
            get { return _preferTerseLabels; }
            set {
                if (_preferTerseLabels == value) return;
                _preferTerseLabels = value;
                OnPropertyChanged("PreferTerseLabels");
                OnPropertyChanged("Label");
            }
        }
        private bool _preferTerseLabels;
        #endregion PreferTerseLabels

        public string Label {
            get {
                string label;

                if (PreferTerseLabels) {
                    if (GetLabel(LabelRoles.TerseLabel) != null) label = GetLabel(LabelRoles.TerseLabel).Caption;
                    else if (GetLabel(LabelRoles.Label) != null) label = GetLabel(LabelRoles.Label).Caption;
                    else label = Name;
                } else {
                    if (GetLabel(LabelRoles.Label) != null) label = GetLabel(LabelRoles.Label).Caption;
                    else if (GetLabel(LabelRoles.TerseLabel) != null) label = GetLabel(LabelRoles.TerseLabel).Caption;
                    else label = Name;
                }

                return label;
            }
        }

        public string TerseLabel {
            get {
                string label;

                if (GetLabel(LabelRoles.TerseLabel) != null) label = GetLabel(LabelRoles.TerseLabel).Caption;
                else if (GetLabel(LabelRoles.Label) != null) label = GetLabel(LabelRoles.Label).Caption;
                else label = Name;

                return label;
            }
        }

        public string MandatoryLabel {
            get {
                string label;

                var lbl = GetLabel(LabelRoles.TerseLabel);
                if (lbl != null && !string.IsNullOrEmpty(lbl.Caption)) label = lbl.Caption;
                else if (GetLabel(LabelRoles.Label) != null) label = GetLabel(LabelRoles.Label).Caption;
                else label = Name;
                
                return (IsMandatoryField ? "*" : string.Empty) + label;
            }
        }
        #endregion Label properties

        public string Name { get; set; }
        public string Id { get; set; }
        public double Order { get; set; }
        public string Type { get; set; }

        #region SubstitutionGroup
        private string _substitutionGroup;
        public string SubstitutionGroup {
            get { return _substitutionGroup; } 
            set {
                _substitutionGroup = value;
                switch (value) {
                    case "hypercubeItem":
                        //Debug.WriteLine("hypercubeItem: " + ToString());
                        IsHypercubeItem = true;
                        break;

                    case "dimensionItem":
                        //Debug.WriteLine("dimensionItem: " + ToString());
                        IsDimensionItem = true;
                        break;
                }
            }
        }
        #endregion
        
        public bool IsTuple { get { return SubstitutionGroup.Equals("tuple"); } }
        public bool IsSelectionListEntry { get { return SubstitutionGroup != null && SubstitutionGroup.EndsWith(".head"); } }

        public bool Nillable { get; private set; }
        public bool IsAbstract { get; private set; }
        public XbrlElementValueTypes ValueType { get; set; }
        
        public bool IsPersistant { get; set; }

        #region References
        private readonly List<IReference> _references = new List<IReference>();
        public IEnumerable<IReference> References { get { return _references; } }
        #endregion

        public bool IsMandatoryField { get; internal set; }
        public MandatoryType MandatoryType { get; internal set; }
        
        public bool NotPermittedForCommercial { get; internal set; }
        public bool NotPermittedForFiscal { get; internal set; }
        public bool NotPermittedForFinancial { get; internal set; }
        
        public bool IsListField { get; set; }
        public bool IsList { get; set; }
        public List<string> ChildReferences { get; private set; }
        public XmlQualifiedName TargetNamespace { get; private set; }

        public List<IElement> Children { get; private set; }
        public string PeriodType { get; set; }
        public List<IPresentationTreeNode> PresentationTreeNodes { get; private set; }
        public Type TableType { get; set; }

        public string PositionNumber { get; set; }
        public string Balance { get; set; }     

        public string IdDisplayString {
            get {
                if (string.IsNullOrEmpty(PositionNumber)) return "??? (" + Name + ")";
                return PositionNumber + " (" + Name + ")";
            }
        }

        public bool HasComputationSources { get { return _summationSources.Count > 0; } }
        
        public bool HasPositiveComputationSources { get; private set; }
        public bool HasNegativeComputationSources { get; private set; }

        public bool HasComputationTargets { get { return _summationTargets.Count > 0; } }
        public bool HasComputationItems { get { return HasComputationSources || HasComputationTargets; } }

        /// <summary>
        /// Used as DisplayMemberPath in comboboxes to display items in the following style: "Label (id)".
        /// </summary>
        public string ComboBoxLabel {
            get {
                var tmp = Name.Split('.');
                var doc = Documentation;
                if (!string.IsNullOrEmpty(doc) &&
                    doc != "Konkrete Ausprägung einer Aufzählung" &&
                    doc != "Value of an enumeration type" &&
                    doc != "-") {
                    return Label + " (" + doc + " / " + tmp[tmp.Length - 1] + ")";
                }

                return Label + " (" + tmp[tmp.Length - 1] + ")";
            }
        }

        #region Parents
        private readonly List<IElement> _parents = new List<IElement>();
        public List<IElement> Parents { get { return _parents; } }
        #endregion Parents

        #region Documentation
        public string Documentation {
            get {
                var label = GetLabel(LabelRoles.Documentation);
                return label != null ? label.Caption : "-";
            }
        }
        #endregion Documentation

        #region ShortDocumentation
        public string ShortDocumentation {
            get {
                string caption;
                var docLabel = GetLabel(LabelRoles.Documentation);

                var terseLabel = GetLabel(LabelRoles.TerseLabel);
                if (terseLabel != null) caption = terseLabel.Caption;
                else {
                    var label = GetLabel(LabelRoles.Label);
                    caption = label != null ? label.Caption : Name;
                }

                return docLabel != null ? caption + ": " + docLabel.Caption : caption;
            }
        }
        #endregion ShortDocumentation

        #region ReferenceString
        public string ReferenceString {
            get {
                if (_references.Count == 0) return "-";
                var sb = new StringBuilder();
                foreach (var reference in References) {
                    var parts = new List<string>();

                    if (reference.Name != null) parts.Add(reference.Name);
                    if (reference.Paragraph != null) parts.Add("Paragraph " + reference.Paragraph);
                    if (reference.Subparagraph != null) parts.Add("Unterparagraph " + reference.Subparagraph);
                    if (reference.Clause != null) parts.Add("Absatz " + reference.Clause);
                    if (reference.ValidSince != null) parts.Add("gültig von: " + reference.ValidSince);
                    if (reference.ValidThrough != null) parts.Add("gültig bis: " + reference.ValidThrough);
                    if (reference.ValidSince != null) parts.Add("steuerlich erforderlich seit: " + reference.FiscalValidSince);
                    if (reference.Section != null) parts.Add("Abschnitt: " + reference.Section);
                    if (reference.Chapter != null) parts.Add("Kapitel: " + reference.Chapter);
                    if (reference.Subsection != null) parts.Add("Unterabschnitt: " + reference.Subsection);
                    if (reference.LegalFormEU != null) parts.Add("relevant für Einzelunternehmen: " + reference.LegalFormEU);
                    if (reference.LegalFormKSt != null) parts.Add("relevant für Körperschaften: " + reference.LegalFormKSt);
                    if (reference.LegalFormPG != null) parts.Add("relevant für Personengesellschaften: " + reference.LegalFormPG);
                    if (reference.Note != null) parts.Add("Anmerkung: " + reference.Note);
                    if (reference.NotPermittedFor != null) parts.Add("nicht erlaubt für: " + reference.Name);
                    if (reference.FiscalRequirement != null) parts.Add(reference.FiscalRequirement);

                    if (parts.Count > 0) sb.Append(string.Join(", ", parts)).Append(Environment.NewLine);
                }

                return sb.Length > 0 ? sb.ToString() : "-";
            }
        }
        #endregion ReferenceString

        public bool IsHypercubeItem { get; private set; }
        public bool IsDimensionItem { get; internal set; }

        public bool LegalFormEU { get; private set; }
        public bool LegalFormKSt { get; private set; }
        public bool LegalFormPG { get; private set; }
        public bool LegalFormSEAG { get; private set; }
        public bool LegalFormVVaG { get; private set; }
        public bool LegalFormOerV { get; private set; }
        public bool LegalFormBNaU { get; private set; }

        public bool TaxonomieNEIN { get; private set; }
        public bool TaxonomiePBV { get; private set; }
        public bool TaxonomieKHBV { get; private set; }
        public bool TaxonomieEBV { get; private set; }
        public bool TaxonomieVUV { get; private set; }
        public bool TaxonomieWUV { get; private set; }
        public bool TaxonomieLUF { get; private set; }

        public bool IsDebitBalance { get { return Balance != null && Balance == "debit"; } }
        public bool IsCreditBalance { get { return Balance != null && Balance == "credit"; } }

        public bool TypeOperatingResultGKV { get { return _typeOperatingResult != null && _typeOperatingResult.Equals("GKV"); } }
        public bool TypeOperatingResultUKV { get { return _typeOperatingResult != null && _typeOperatingResult.Equals("UKV"); } }
        /// <summary>
        /// This element is for all TypeOperatingResults (GKV, UKV, neutral, null).
        /// </summary>
        public bool TypeOperatingResultAll { get { return _typeOperatingResult == null || string.IsNullOrEmpty(_typeOperatingResult) || (!_typeOperatingResult.Equals("GKV") && !_typeOperatingResult.Equals("UKV")); } }

        public bool IsMonetaryTree { get {
            return Name.StartsWith("bs.eqLiab.liab") || Name.StartsWith("bs.ass") || Name.StartsWith("bs.contingLiab") ||
                   Name.StartsWith("ci_incomeUse.gainLoss") ||
                   Name.StartsWith("ci_kke.unlimitedPartners.sumEquityAccounts.kinds.sumYearEnd") ||
                   Name.StartsWith("ci_cfs.cashEqu") || Name.StartsWith("ci_cfs.cashEqu.compound") ||
                   Name.StartsWith("ci_cfs.cashEqu.cashChanges") || Name.StartsWith("ci_cfs.cashEqu.nonCashChanges") ||
                   Name.StartsWith("ci_fpl") ||
                   //Name.StartsWith("is.netIncome") ||
                   Name.StartsWith("ci_fplgm.netmethod");
        } }

        public bool IsReconciliationPosition { get {
            return ValueType == XbrlElementValueTypes.Monetary &&
                   (IsBalanceSheetAssetsPosition || IsIncomeStatementPosition || IsBalanceSheetLiabilitiesPosition);
        } }

        public bool IsBalanceSheetAssetsPosition { get { return Name.StartsWith("bs.ass") || Name.StartsWith("bsBanks.ass") || Name.StartsWith("bsIns.ass"); } }
        public bool IsBalanceSheetLiabilitiesPosition { get { return Name.StartsWith("bs.eqLiab") || Name.StartsWith("bsBanks.eqLiab") || Name.StartsWith("bsIns.eqLiab"); } }
        public bool IsIncomeStatementPosition { get { return Name.StartsWith("is.netIncome") || Name.StartsWith("isBanks") || Name.StartsWith("isIns"); } }

        /// <summary>
        /// True, if the element specifies the balance sheet assets sum position.
        /// </summary>
        public bool IsBalanceSheetAssetsSumPosition { get { return Id == "de-gaap-ci_bs.ass" || Id == "de-fi_bsBanks.ass" || Id == "de-ins_bsIns.ass"; } }

        /// <summary>
        /// True, if the element specifies the balance sheet liabilities sum position.
        /// </summary>
        public bool IsBalanceSheetLiabilitiesSumPosition { get { return Id == "de-gaap-ci_bs.eqLiab" || Id == "de-fi_bsBanks.eqLiab" || Id == "de-ins_bsIns.eqLiab"; } }

        /// <summary>
        /// True, if the element specifies the income statement sum position.
        /// </summary>
        public bool IsIncomeStatementSumPosition { get { return Id == "de-gaap-ci_is.netIncome"; } }

        #endregion properties

        #region internal methods
        internal void AddSummationItem(IElement elem, double weight, IArc calculationArc) {
            var si = new SummationItem { Element = elem, Weight = weight, CalculationArc = calculationArc };
            _summationTargets.Add(si);
            var element = (Element)elem;
            element._summationSources.Add(this);

            // set HasPositiveComputationSources  and HasNegativeComputationSources flags
            // (used in gui to display the respective sum icons)
            // TODO: attach these properties to the respective presentation tree nodes instead of elements.
            if (si.Weight > 0) element.HasPositiveComputationSources = true;
            if (si.Weight < 0) element.HasNegativeComputationSources = true;
        }

        /// <summary>
        /// Creates a clone of the element, whereas all lists except of PresentationTreeNodes are only flat copies, since
        /// these values will never be changed. For PresentationTreeNodes a new empty list will be created.
        /// </summary>
        internal IElement PartialClone() {
            var elem = (Element)MemberwiseClone();
            elem.PresentationTreeNodes = new List<IPresentationTreeNode>();
            return elem;
        }

        internal void AddLabel(ILabel label) {
            // get language specific label dictionary or create new one, if not exists.
            Dictionary<LabelRoles, ILabel> labelDict;
            _labels.TryGetValue(label.Language, out labelDict);
            if (labelDict == null) {
                labelDict = new Dictionary<LabelRoles, ILabel>();
                _labels[label.Language] = labelDict;
            }

            // get label role
            LabelRoles role;
            switch (label.Role) {
                case "http://www.xbrl.org/2003/role/label":
                case "http://www.xbrl.org/2008/role/label":
                    role = LabelRoles.Label;
                    break;

                case "http://www.xbrl.org/2003/role/positiveLabel":
                case "http://www.xbrl.org/2008/role/positiveLabel":
                    role = LabelRoles.PositiveLabel;
                    break;

                case "http://www.xbrl.org/2003/role/negativeLabel":
                case "http://www.xbrl.org/2008/role/negativeLabel":
                    role = LabelRoles.NegativeLabel;
                    break;

                case "http://www.xbrl.org/2003/role/verboseLabel":
                case "http://www.xbrl.org/2008/role/verboseLabel":
                    role = LabelRoles.VerboseLabel;
                    break;

                case "http://www.xbrl.org/2003/role/terseLabel":
                case "http://www.xbrl.org/2008/role/terseLabel":
                    role = LabelRoles.TerseLabel;
                    break;

                case "http://www.xbrl.org/2003/role/positiveTerseLabel":
                case "http://www.xbrl.org/2008/role/positiveTerseLabel":
                    role = LabelRoles.PositiveTerseLabel;
                    break;

                case "http://www.xbrl.org/2003/role/negativeTerseLabel":
                case "http://www.xbrl.org/2008/role/negativeTerseLabel":
                    role = LabelRoles.NegativeTerseLabel;
                    break;

                case "http://www.xbrl.org/2003/role/documentation":
                case "http://www.xbrl.org/2008/role/documentation":
                    role = LabelRoles.Documentation;
                    break;

                case "http://www.xbrl.org/2003/role/periodStartLabel":
                case "http://www.xbrl.org/2008/role/periodStartLabel":
                    role = LabelRoles.PeriodStartLabel;
                    break;

                case "http://www.xbrl.org/2003/role/periodEndLabel":
                case "http://www.xbrl.org/2008/role/periodEndLabel":
                    role = LabelRoles.PeriodEndLabel;
                    break;

                default:
                    throw new Exception("Unknown label role: " + label.Role);
            }

            labelDict[role] = label;

        }
        #endregion internal methods

        #region methods

        #region GetLabel
        public ILabel GetLabel(LabelRoles role) {
            // get language specific label dictionary
            Dictionary<LabelRoles, ILabel> labelDict;

            if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de")) {
                _labels.TryGetValue("de", out labelDict);
                if (labelDict == null) _labels.TryGetValue("en", out labelDict);
            } else {
                _labels.TryGetValue("en", out labelDict);
                if (labelDict == null)
                    _labels.TryGetValue("de", out labelDict);
            }

            if (labelDict == null) return null;
            return labelDict.ContainsKey(role) ? labelDict[role] : null;
        }
        #endregion GetLabel

        public override string ToString() { return Label + " [" + Name + "]"; }

        #endregion methods


        public void AddReference(Reference reference) {

            _references.Add(reference);
            if (reference.LegalFormEU.HasValue) LegalFormEU = reference.LegalFormEU.Value;
            if (reference.LegalFormKSt.HasValue) LegalFormKSt = reference.LegalFormKSt.Value;
            if (reference.LegalFormPG.HasValue) LegalFormPG = reference.LegalFormPG.Value;
            if (reference.LegalFormSEAG.HasValue) LegalFormSEAG = reference.LegalFormSEAG.Value;
            if (reference.LegalFormVVaG.HasValue) LegalFormVVaG = reference.LegalFormVVaG.Value;
            if (reference.LegalFormOerV.HasValue) LegalFormOerV = reference.LegalFormOerV.Value;
            if (reference.LegalFormBNaU.HasValue) LegalFormBNaU = reference.LegalFormBNaU.Value;
            _typeOperatingResult = reference.TypeOperatingResult;
            
            if (reference.EBV.HasValue) TaxonomieEBV &= reference.EBV.Value;
            if (reference.KHBV.HasValue) TaxonomieKHBV &= reference.KHBV.Value;
            if (reference.LUF.HasValue) TaxonomieLUF &= reference.LUF.Value;
            if (reference.PBV.HasValue) TaxonomiePBV &= reference.PBV.Value;
            if (reference.VUV.HasValue) TaxonomieVUV &= reference.VUV.Value;
            if (reference.WUV.HasValue) TaxonomieWUV &= reference.WUV.Value;
            if (reference.keineBranche.HasValue) TaxonomieNEIN = reference.keineBranche.Value;
        }

        public void SetReferences() {
            if (LegalFormBNaU || LegalFormEU || LegalFormKSt || LegalFormOerV || LegalFormPG || LegalFormSEAG ||
                LegalFormVVaG) return;
            LegalFormBNaU = true;
            LegalFormEU = true;
            LegalFormKSt = true;
            LegalFormOerV = true;
            LegalFormPG = true;
            LegalFormSEAG = true;
            LegalFormVVaG = true;
        }

        public void SetLegalFormEU(bool value) {
            if (!this.Id.StartsWith("de-gcd")) {
                Debug.Assert(this.Id.StartsWith("de_gcd"), "Please don't set the allowed legal form for gaap stuff");
                return;
            }
            LegalFormEU = value;
        }
        public void SetLegalFormPG(bool value) {
            if (!this.Id.StartsWith("de-gcd")) {
                Debug.Assert(this.Id.StartsWith("de_gcd"), "Please don't set the allowed legal form for gaap stuff");
                return;
            }
            LegalFormPG = value;
        }
        public void SetLegalFormOther(bool value) {
            if (!this.Id.StartsWith("de-gcd")) {
                Debug.Assert(this.Id.StartsWith("de_gcd"), "Please don't set the allowed legal form for gaap stuff");
                return;
            }

            LegalFormBNaU = value;
            LegalFormKSt = value;
            LegalFormOerV = value;
            LegalFormSEAG = value;
            LegalFormVVaG = value;
        }

    }
}