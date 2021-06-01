// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Taxonomy.Enums;
using Utils;
using eBalanceKit.Models.Document;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Add.Models {
    public class AddReportModel : NotifyPropertyChangedBase {
        public AddReportModel(Window owner) {
            Owner = owner;

            //Elements =
            //    TaxonomyManager.GetTaxonomy(TaxonomyManager.GetLatestTaxonomyInfo(TaxonomyType.GCD)).Elements.Values;
            Elements = TaxonomyManager.GCD_Taxonomy.Elements.Values;
            //OnPropertyChanged("ChoiceElementsAccountingStandard");
            //OnPropertyChanged("ChoiceElementsSpecialAccountingStandard");
        }

        public Window Owner { get; private set; }

        private IEnumerable<Taxonomy.IElement> Elements { get { return _elements; } set{
            if (_elements == value) {
                return;
            }
            _elements = value;
            SelectedSpecialAccountingStandard =
                ChoiceElementsSpecialAccountingStandard.FirstOrDefault(
                    element => element.Id.Equals("de-gcd_genInfo.report.id.specialAccountingStandard.K"));

            
            SelectedAccountingStandard =
                ChoiceElementsAccountingStandard.FirstOrDefault(
                    element => element.Id.Equals("de-gcd_genInfo.report.id.accountingStandard.accountingStandard.HGBM"));

            OnPropertyChanged("ChoiceElementsAccountingStandard");
            OnPropertyChanged("ChoiceElementsSpecialAccountingStandard");
        } }
        private IEnumerable<Taxonomy.IElement> _elements;

        #region SpecialAccountingStandard
        private Taxonomy.IElement _selectedSpecialAccountingStandard;

        public Taxonomy.IElement SelectedSpecialAccountingStandard {
            get { return _selectedSpecialAccountingStandard; }
            set {
                if (_selectedSpecialAccountingStandard != value) {
                    _selectedSpecialAccountingStandard = value;
                    OnPropertyChanged("SelectedSpecialAccountingStandard");
                }
            }
        }

        public IEnumerable<Taxonomy.IElement> ChoiceElementsSpecialAccountingStandard {
            get {
                return Elements.Where(
                    elm =>
                    elm.Id.StartsWith("de-gcd_genInfo.report.id.specialAccountingStandard.") &&
                    !elm.Id.EndsWith("head") && !elm.Id.EndsWith("ERL") && Taxonomy.Taxonomy.IsAllowedListEntry(elm)).ToList();
            }
        }
        #endregion SpecialAccountingStandard

        #region AccountingStandard
        private Taxonomy.IElement _selectedAccountingStandard;

        public Taxonomy.IElement SelectedAccountingStandard {
            get { return _selectedAccountingStandard; }
            set {
                if (_selectedAccountingStandard != value) {
                    _selectedAccountingStandard = value;
                    OnPropertyChanged("SelectedAccountingStandard");
                }
            }
        }
        
        public IEnumerable<Taxonomy.IElement> ChoiceElementsAccountingStandard {
            get {
                var list = Elements.Where(
                    elm =>
                    elm.Id.StartsWith("de-gcd_genInfo.report.id.accountingStandard.accountingStandard") &&
                    !elm.Id.EndsWith("head") && !elm.Id.EndsWith("ERL") && Taxonomy.Taxonomy.IsAllowedListEntry(elm)).ToList();

                return list;
            }
        }
        #endregion AccountingStandard

    }
}