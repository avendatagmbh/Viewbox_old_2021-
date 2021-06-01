// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-03-24
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Input;
using eBalanceKit.Windows.MappingTemplates.Models;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.MappingTemplates {
    public partial class DlgTemplates {
        public DlgTemplates(Document document) {
            InitializeComponent();
            Model = new DlgTemplatesModel(document, this);
            DataContext = Model;

        }

        private DlgTemplatesModel Model { get; set; }

        private void WindowKeyDown(object sender, KeyEventArgs e) {
            if (e.Key != Key.Escape) return;
            e.Handled = true;
            Close();
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) { Close(); }
    }
}