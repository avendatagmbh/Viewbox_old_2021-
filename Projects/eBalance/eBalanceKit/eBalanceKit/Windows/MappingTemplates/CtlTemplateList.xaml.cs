// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-11-22
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKit.Windows.MappingTemplates.Models;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates {
    public partial class CtlTemplateList : UserControl {
        public CtlTemplateList() {
            InitializeComponent();
        }

        private TemplateListModel Model { get { return DataContext as TemplateListModel; }}

        private void LstTemplatesMouseDoubleClick(object sender, MouseButtonEventArgs e) { Model.Parent.EditTemplate(); }
        private void BtnUpdateClick(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            var template = button.DataContext as MappingTemplateHead;
            Model.Parent.UpdateTemplate(template);
        }
    }
}