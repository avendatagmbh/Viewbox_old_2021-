// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-11-04
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Xml;
using Taxonomy.Exceptions;
using Taxonomy.Interfaces;

namespace Taxonomy {
    /// <summary>
    /// Xbrl reference element.
    /// </summary>
    internal class Reference : IReference {
        internal Reference(XmlNode node) {
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

                        case "xlink:title":
                            Title = attr.Value;
                            break;

#if DEBUG
                        default:
                            throw new UnknownAttributeException(attr.Name, node.Name);
#endif
                    }
                }

            foreach (XmlNode child in node.ChildNodes) {
                switch (child.LocalName) {
                    case "Name":
                        Name = child.InnerText;
                        break;

                    case "Paragraph":
                        Paragraph = child.InnerText;
                        break;

                    case "Subparagraph":
                        Subparagraph = child.InnerText;
                        break;

                    case "Clause":
                        Clause = child.InnerText;
                        break;

                    case "Chapter":
                        Chapter = child.InnerText;
                        break;

                    case "ValidSince":
                        ValidSince = child.InnerText;
                        break;

                    case "ValidThrough":
                        ValidThrough = child.InnerText;
                        break;

                    case "legalFormEU": // Einzelunternehmen
                        LegalFormEU = (child.InnerText == "true");
                        break;

                    case "legalFormKSt": // Körperschaften
                        LegalFormKSt = (child.InnerText == "true");
                        break;

                    case "legalFormPG": // Personengesellschaften
                        LegalFormPG = (child.InnerText == "true");
                        break;

                    case "legalFormSEAG":
                        LegalFormSEAG = (child.InnerText == "true");
                        break;

                    case "legalFormVVaG":
                        LegalFormVVaG = (child.InnerText == "true");
                        break;

                    case "legalFormOerV":
                        LegalFormOerV = (child.InnerText == "true");
                        break;

                    case "legalFormBNaU":
                        LegalFormBNaU = (child.InnerText == "true");
                        break;

                    case "typeOperatingResult":
                        TypeOperatingResult = child.InnerText;
                        break;

                    case "fiscalRequirement":
                        FiscalRequirement = child.InnerText;
                        break;

                    case "notPermittedFor":
                        NotPermittedFor = child.InnerText;
                        break;

                    case "Section":
                        Section = child.InnerText;
                        break;

                    case "Subsection":
                        Subsection = child.InnerText;
                        break;

                    case "Subclause":
                        Subclause = child.InnerText;
                        break;

                    case "Article":
                        Article = child.InnerText;
                        break;

                    case "Note":
                        Note = child.InnerText;
                        break;

                    case "IssueDate":
                        IssueDate = child.InnerText;
                        break;

                    case "Number":
                        Number = child.InnerText;
                        break;

                    case "fiscalReference":
                        FiscalReference = child.InnerText;
                        break;

                    case "fiscalValidSince":
                        FiscalValidSince = child.InnerText;
                        break;

                    case "fiscalValidThrough":
                        FiscalValidThrough = child.InnerText;
                        break;

                    case "consistencyCheck":
                        ConsistencyCheck = child.InnerText;
                        break;

                    case "Publisher":
                        Publisher = child.InnerText;
                        break;

                    case "kindOfFinancialInstitutionKreditgenossenschWarengeschaeft":
                        kindOfFinancialInstitutionKreditgenossenschWarengeschaeft = (child.InnerText == "true");
                        break;
                    
                    case "kindOfFinancialInstitutionFinanzdienstl":
                        kindOfFinancialInstitutionFinanzdienstl = (child.InnerText == "true");
                        break;
                    
                    case "kindOfFinancialInstitutionPfandbriefbanken":
                        kindOfFinancialInstitutionPfandbriefbanken = (child.InnerText == "true");
                        break;
                    
                    case "kindOfFinancialInstitutionBauspar":
                        kindOfFinancialInstitutionBauspar = (child.InnerText == "true");
                        break;
                    
                    case "kindOfFinancialInstitutionKreditgenossensch":
                        kindOfFinancialInstitutionKreditgenossensch = (child.InnerText == "true");
                        break;
                    
                    case "kindOfFinancialInstitutionSparkassen":
                        kindOfFinancialInstitutionSparkassen = (child.InnerText == "true");
                        break;
                    
                    case "kindOfFinancialInstitutionKapitalanlagegesellschaften":
                        kindOfFinancialInstitutionKapitalanlagegesellschaften = (child.InnerText == "true");
                        break;
                    
                    case "kindOfFinancialInstitutionSkontrofuehrer":
                        kindOfFinancialInstitutionSkontrofuehrer = (child.InnerText == "true");
                        break;
                    
                    case "kindOfFinancialInstitutionPfandbriefbankenOERA":
                        kindOfFinancialInstitutionPfandbriefbankenOERA = (child.InnerText == "true");
                        break;
                    
                    case "kindOfFinancialInstitutionGirozentralen":
                        kindOfFinancialInstitutionGirozentralen = (child.InnerText == "true");
                        break;
                    
                    case "kindOfFinancialInstitutiongenossZentrB":
                        kindOfFinancialInstitutiongenossZentrB = (child.InnerText == "true");
                        break;
                    
                    case "kindOfFinancialInstitutionDtGenB":
                        kindOfFinancialInstitutionDtGenB = (child.InnerText == "true");
                        break;
                    
                    case "kindOfFinancialInstitutionPfandBG":
                        kindOfFinancialInstitutionPfandBG = (child.InnerText == "true");
                        break;
                    
                    case "kindOfBusinessSUV":
                        kindOfBusinessSUV = (child.InnerText == "true");
                        break;
                    
                    case "kindOfBusinessRV":
                        kindOfBusinessRV = (child.InnerText == "true");
                        break;
                    
                    case "kindOfBusinessLKV":
                        kindOfBusinessLKV = (child.InnerText == "true");
                        break;
                    
                    case "kindOfBusinessPSK":
                        kindOfBusinessPSK = (child.InnerText == "true");
                        break;
                    
                    case "kindOfBusinessLUV":
                        kindOfBusinessLUV = (child.InnerText == "true");
                        break;
                    
                    case "kindOfBusinessSUK":
                        kindOfBusinessSUK = (child.InnerText == "true");
                        break;
                    
                    case "kindOfBusinessPF":
                        kindOfBusinessPF = (child.InnerText == "true");
                        break;
                    
                    case "keineBranche":
                        keineBranche = (child.InnerText == "true");
                        break;
                    
                    case "LUF":
                        LUF = child.InnerText=="true";
                        break;
                    
                    case "VUV":
                        VUV = child.InnerText=="true";
                        break;
                    
                    case "EBV":
                        EBV = child.InnerText=="true";
                        break;
                    
                    case "WUV":
                        WUV = child.InnerText=="true";
                        break;
                    
                    case "KHBV":
                        KHBV = child.InnerText=="true";
                        break;
                    
                    case "PBV":
                        PBV = child.InnerText=="true";
                        break;

#if DEBUG
                    default:
                        throw new UnknownReferenceNodeChildType(child.LocalName);
#endif
                }
            }
        }

        #region properties
        public string Type { get; private set; }
        public string Content { get; private set; }
        public string Id { get; private set; }
        public string Role { get; private set; }
        public string Title { get; private set; }
        public string Name { get; private set; }
        public string Paragraph { get; private set; }
        public string Subparagraph { get; private set; }
        public string Clause { get; private set; }
        public string Chapter { get; private set; }
        public string ValidSince { get; private set; }
        public string ValidThrough { get; private set; }
        
        public bool? LegalFormEU { get; private set; }
        public bool? LegalFormKSt { get; private set; }
        public bool? LegalFormPG { get; private set; }
        public bool? LegalFormSEAG { get; private set; }
        public bool? LegalFormVVaG { get; private set; }
        public bool? LegalFormOerV { get; private set; }
        public bool? LegalFormBNaU { get; private set; }

        public string TypeOperatingResult { get; private set; }
        public string NotPermittedFor { get; private set; }
        public string FiscalRequirement { get; private set; }
        public string Section { get; private set; }
        public string Subsection { get; private set; }
        public string Subclause { get; private set; }
        public string Article { get; set; }
        public string Note { get; private set; }
        public string IssueDate { get; private set; }
        public string Number { get; private set; }
        public string FiscalReference { get; private set; }
        public string FiscalValidSince { get; private set; }
        public string FiscalValidThrough { get; private set; }
        public string ConsistencyCheck { get; private set; }

        public string Publisher { get; private set; }
        
        public bool? kindOfFinancialInstitutionKreditgenossenschWarengeschaeft { get; private set; }
        public bool? kindOfFinancialInstitutionFinanzdienstl { get; private set; }
        public bool? kindOfFinancialInstitutionPfandbriefbanken { get; private set; }
        public bool? kindOfFinancialInstitutionBauspar { get; private set; }
        public bool? kindOfFinancialInstitutionKreditgenossensch { get; private set; }
        public bool? kindOfFinancialInstitutionSparkassen { get; private set; }
        public bool? kindOfFinancialInstitutionKapitalanlagegesellschaften { get; private set; }
        public bool? kindOfFinancialInstitutionSkontrofuehrer { get; private set; }
        public bool? kindOfFinancialInstitutionPfandbriefbankenOERA { get; private set; }
        public bool? kindOfFinancialInstitutionGirozentralen { get; private set; }
        public bool? kindOfFinancialInstitutiongenossZentrB { get; private set; }
        public bool? kindOfFinancialInstitutionDtGenB { get; private set; }
        public bool? kindOfFinancialInstitutionPfandBG { get; private set; }
        public bool? kindOfBusinessSUV { get; private set; }
        public bool? kindOfBusinessRV { get; private set; }
        public bool? kindOfBusinessLKV { get; private set; }
        public bool? kindOfBusinessPSK { get; private set; }
        public bool? kindOfBusinessLUV { get; private set; }
        public bool? kindOfBusinessSUK { get; private set; }
        public bool? kindOfBusinessPF { get; private set; }
        public bool? keineBranche { get; private set; }

        public bool? LUF { get; private set; }
        public bool? VUV { get; private set; }
        public bool? EBV { get; private set; }
        public bool? WUV { get; private set; }
        public bool? KHBV { get; private set; }
        public bool? PBV { get; private set; }

        #endregion properties
    }
}