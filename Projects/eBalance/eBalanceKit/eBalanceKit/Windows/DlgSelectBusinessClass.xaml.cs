// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-28
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Taxonomy;
using Taxonomy.Enums;
using eBalanceKitBusiness.Manager;

namespace eBalanceKit.Windows {
    /// <summary>
    /// Interaktionslogik für DlgSelectBusinessClass.xaml
    /// </summary>
    public partial class DlgSelectBusinessClass : Window {
        public DlgSelectBusinessClass() {
            InitializeComponent();

            ITaxonomy taxonomy = TaxonomyManager.GetTaxonomy(TaxonomyManager.GetLatestTaxonomyInfo(TaxonomyType.GCD));
            IElement element = taxonomy.Elements["de-gdc_genInfo.report.id.specialAccountingStandard"];
            Items = element.Children[0].Children;
            SelectedItem = Items.Last();

            DataContext = this;
        }

        public IElement SelectedItem { get; set; }
        public List<IElement> Items { get; private set; }

        private void btnOk_Click(object sender, RoutedEventArgs e) { DialogResult = true; }
    }
}