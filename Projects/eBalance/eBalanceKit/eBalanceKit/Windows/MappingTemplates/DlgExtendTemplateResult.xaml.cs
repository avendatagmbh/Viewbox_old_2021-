// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-18
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using eBalanceKitBusiness.MappingTemplate;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.MappingTemplates {
    public partial class DlgExtendTemplateResult {
        public DlgExtendTemplateResult() {
            InitializeComponent();
        }

        private ExtendTemplateModel Model { get { return DataContext as ExtendTemplateModel; } }

        private void BtnSaveClick(object sender, RoutedEventArgs e) {
            string message = null;
            if (Model.SaveTemplate(out message))
                DialogResult = true;
            else {
                DialogResult = false;
                MessageBox.Show(
                    message,
                    ResourcesCommon.ExtendTemplate,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e) { DialogResult = false; }
    }
}