// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace Taxonomy.Interfaces {
    /// <summary>
    /// Xbrl reference element.
    /// </summary>
    public interface IReference {
        string Type { get; }
        string Content { get; }
        string Id { get; }
        string Role { get; }
        string Title { get; }
        string Name { get; }
        string Paragraph { get; }
        string Subparagraph { get; }
        string Clause { get; }
        string Chapter { get; }
        string ValidSince { get; }
        string ValidThrough { get; }
        
        bool? LegalFormEU { get; }
        bool? LegalFormKSt { get; }
        bool? LegalFormPG { get; }
        bool? LegalFormSEAG { get; }
        bool? LegalFormVVaG { get; }
        bool? LegalFormOerV { get; }
        bool? LegalFormBNaU { get; }

        string TypeOperatingResult { get; }
        string NotPermittedFor { get; }
        string FiscalRequirement { get; }
        string Section { get; }
        string Subsection { get; }
        string Subclause { get; }
        string Article { get; }
        string Note { get; }
        string IssueDate { get; }
        string Number { get; }
        string FiscalReference { get; }
        string FiscalValidSince { get; }
        string FiscalValidThrough { get; }
        string ConsistencyCheck { get; }

        bool? kindOfFinancialInstitutionKreditgenossenschWarengeschaeft { get; }
        bool? kindOfFinancialInstitutionFinanzdienstl { get; }
        bool? kindOfFinancialInstitutionPfandbriefbanken { get; }
        bool? kindOfFinancialInstitutionBauspar { get; }
        bool? kindOfFinancialInstitutionKreditgenossensch { get; }
        bool? kindOfFinancialInstitutionSparkassen { get; }
        bool? kindOfFinancialInstitutionKapitalanlagegesellschaften { get; }
        bool? kindOfFinancialInstitutionSkontrofuehrer { get; }
        bool? kindOfFinancialInstitutionPfandbriefbankenOERA { get; }
        bool? kindOfFinancialInstitutionGirozentralen { get; }
        bool? kindOfFinancialInstitutiongenossZentrB { get; }
        bool? kindOfFinancialInstitutionDtGenB { get; }
        bool? kindOfFinancialInstitutionPfandBG { get; }
        bool? kindOfBusinessSUV { get; }
        bool? kindOfBusinessRV { get; }
        bool? kindOfBusinessLKV { get; }
        bool? kindOfBusinessPSK { get; }
        bool? kindOfBusinessLUV { get; }
        bool? kindOfBusinessSUK { get; }
        bool? kindOfBusinessPF { get; }
        bool? keineBranche { get; }

        bool? LUF { get; }
        bool? VUV { get; }
        bool? EBV { get; }
        bool? WUV { get; }
        bool? KHBV { get; }
        bool? PBV { get; }
    }
}