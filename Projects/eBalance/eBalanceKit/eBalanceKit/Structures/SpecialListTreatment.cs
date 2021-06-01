using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using eBalanceKit.Controls.XbrlVisualisation;
using eBalanceKit.Models;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Structures {
    class SpecialListTreatment {
        public SpecialListTreatment(NavigationTreeEntry entry, XbrlBasePanel panel, UserControl control, string elementId) { 
            Entry = entry;
            Panel = panel;
            ElementId = elementId;
            Control = control;
            Entry.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Entry_PropertyChanged);
        }

        void Entry_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "IsSelected" && (sender as NavigationTreeEntry).IsSelected) {
                ExpandXbrlListView();
            }
        }

        public void ExpandXbrlListView() {
            if (_allowedGenInfoItems.Contains(ElementId)) {
                var elements = Panel.ContentPanel.Children.OfType<XbrlListView>();
                if (elements != null) {
                    var xbrlListView = elements.First();
                    if (xbrlListView.Data != null) {
                        if (xbrlListView.Data.Items.Count > 0) xbrlListView.Data.SelectedItem = xbrlListView.Data.Items[0];
                        else if (xbrlListView.Data.AddItemAllowed) {
                            xbrlListView.Data.AddValue();
                            var elem = xbrlListView.Data.Items[0].Values.First().Value;
                            elem.Value = ResourcesCommon.ListElementNoDescription;
                        }
                    }
                }
            }else if(_allowedKkeItems.Contains(ElementId)) {
                var listBoxData = eBalanceKitBusiness.Manager.DocumentManager.Instance.CurrentDocument.ValueTreeMain.GetValue(ElementId) as XbrlElementValue_List;

                if (listBoxData != null) {
                    if (listBoxData.Items.Count > 0) listBoxData.SelectedItem = listBoxData.Items[0];
                    else if (listBoxData.AddItemAllowed) {
                        listBoxData.AddValue();
                        var elem = listBoxData.Items[0].Values.First().Value;
                        elem.Value = ResourcesCommon.ListElementNoDescription;
                    }
                }
            }
        }

        private NavigationTreeEntry Entry { get; set; }
        private XbrlBasePanel Panel { get; set; }
        private UserControl Control { get; set; }
        private string ElementId { get; set; }
        List<string> _allowedGenInfoItems = new List<string> {
            "de-gcd_genInfo.company.id.contactAddress", 
            "de-gcd_genInfo.company.id.shareholder", 
            "de-gcd_genInfo.doc.author"
        };

        List<string> _allowedKkeItems = new List<string> {
            "de-gaap-ci_kke.unlimitedPartners",
            "de-gaap-ci_kke.limitedPartners"
        }; 
    }
}
