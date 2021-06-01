using System;
using System.Collections.Generic;
using System.Xml;
using Taxonomy.Enums;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;

namespace Taxonomy {
    
    public interface IElement {
        
        /// <summary>
        /// Balance type (debit or credit).
        /// </summary>
        string Balance { get; set; }
        
        string Documentation { get; }
        ILabel GetLabel(LabelRoles role);
        string Id { get; set; }
        string IdDisplayString { get; }
        bool IsAbstract { get; }
        bool IsList { get; set; }
        bool IsListField { get; set; }
        bool IsMandatoryField { get; }
        bool IsPersistant { get; set; }
        bool IsSelectionListEntry { get; }
        bool IsTuple { get; }
        string Label { get; }
        string MandatoryLabel { get; }
        MandatoryType MandatoryType { get; }
        string Name { get; set; }
        XmlQualifiedName TargetNamespace { get; }
        bool Nillable { get; }
        
        bool NotPermittedForCommercial { get; }
        bool NotPermittedForFinancial { get; }
        bool NotPermittedForFiscal { get; }
        
        double Order { get; set; }
        List<IElement> Parents { get; }
        string PeriodType { get; set; }
        string PositionNumber { get; set; }
        bool PreferTerseLabels { get; set; }
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        string ReferenceString { get; }
        string ShortDocumentation { get; }
        string SubstitutionGroup { get; set; }
        Type TableType { get; set; }
        string TerseLabel { get; }
        string Type { get; set; }
        XbrlElementValueTypes ValueType { get; set; }

        List<string> ChildReferences { get; }
        List<IElement> Children { get; }
        IEnumerable<IReference> References { get; }
        List<IPresentationTreeNode> PresentationTreeNodes { get; }
        
        string ToString();

        bool HasComputationSources { get; }
        bool HasPositiveComputationSources { get; }
        bool HasNegativeComputationSources { get; }

        bool HasComputationTargets { get; }
        bool HasComputationItems { get; }

        IEnumerable<SummationItem> SummationTargets { get; }
        IEnumerable<IElement> SummationSources { get; }
        
        /// <summary>
        /// Used as DisplayMemberPath in comboboxes to display items in the following style: "Label (id)".
        /// </summary>
        string ComboBoxLabel { get; }

        bool IsHypercubeItem { get; }
        bool IsDimensionItem { get; }

        bool LegalFormEU { get; }
        bool LegalFormKSt { get; }
        bool LegalFormPG { get; }
        bool LegalFormSEAG { get; }
        bool LegalFormVVaG { get; }
        bool LegalFormOerV { get; }
        bool LegalFormBNaU { get; }

        bool IsDebitBalance { get; }
        bool IsCreditBalance { get; }


        bool IsReconciliationPosition { get; }
        bool IsBalanceSheetAssetsPosition { get; }
        bool IsBalanceSheetLiabilitiesPosition { get; }
        bool IsIncomeStatementPosition { get; }

        /// <summary>
        /// True, if the element specifies the balance sheet assets sum position.
        /// </summary>
        bool IsBalanceSheetAssetsSumPosition { get; }

        /// <summary>
        /// True, if the element specifies the balance sheet liabilities sum position.
        /// </summary>
        bool IsBalanceSheetLiabilitiesSumPosition { get; }

        /// <summary>
        /// True, if the element specifies the income statement sum position.
        /// </summary>
        bool IsIncomeStatementSumPosition { get; }

        //bool HasChildWithValue { get; set; }

        bool TypeOperatingResultGKV { get; }
        bool TypeOperatingResultUKV { get; }
        bool TypeOperatingResultAll { get; }
        bool IsMonetaryTree { get; }


        /// <summary>
        /// Get the information if relevant for "keineBranche" (Kerntaxonomie).
        /// </summary>
        bool TaxonomieNEIN { get; }
        /// <summary>
        /// Get the information if relevant for "PBV".
        /// </summary>
        bool TaxonomiePBV { get; }
        /// <summary>
        /// Get the information if relevant for "KHBV".
        /// </summary>
        bool TaxonomieKHBV { get; }
        /// <summary>
        /// Get the information if relevant for "EBV".
        /// </summary>
        bool TaxonomieEBV { get; }
        /// <summary>
        /// Get the information if relevant for "VUV".
        /// </summary>
        bool TaxonomieVUV { get; }
        /// <summary>
        /// Get the information if relevant for "WUV".
        /// </summary>
        bool TaxonomieWUV { get; }
        /// <summary>
        /// Get the information if relevant for "LUF".
        /// </summary>
        bool TaxonomieLUF { get; }
    }
}
