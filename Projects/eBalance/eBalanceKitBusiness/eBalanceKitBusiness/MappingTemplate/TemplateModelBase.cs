// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.ObjectModel;
using System.Linq;
using Utils;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKitBusiness.MappingTemplate {
    public class TemplateModelBase : NotifyPropertyChangedBase {
        public TemplateModelBase(Document document, MappingTemplateHead template) {
            BalanceLists = new ObservableCollection<MappingTemplateBalanceList>();
            foreach (
                var bl in
                    document.BalanceListsImported.Select(
                        balanceList =>
                        new MappingTemplateBalanceList { BalanceList = balanceList, IsChecked = (balanceList == document.SelectedBalanceList) })) {
                bl.PropertyChanged += BalanceListPropertyChanged;
                BalanceLists.Add(bl);
            }

            SelectedBalanceLists = new ObservableCollection<IBalanceList>();

            Document = document;
            Template = template;

            UpdateBalanceListCollection();
        }

        void BalanceListPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "IsChecked")
                UpdateBalanceListCollection();
        }

        public Document Document { get; private set; }
        public MappingTemplateHead Template { get; private set; }
        public ObservableCollection<MappingTemplateBalanceList> BalanceLists { get; private set; }
        public ObservableCollection<IBalanceList> SelectedBalanceLists { get; private set; }

        private void UpdateBalanceListCollection() {
            SelectedBalanceLists.Clear();
            foreach (var balanceList in BalanceLists.Where(balanceList => balanceList.IsChecked))
                SelectedBalanceLists.Add(balanceList.BalanceList);
        }         

    }
}