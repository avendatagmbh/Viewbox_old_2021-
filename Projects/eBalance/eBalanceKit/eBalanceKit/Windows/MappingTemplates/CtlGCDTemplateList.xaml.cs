using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKit.Windows.MappingTemplates.Models;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates
{
    public partial class CtlGCDTemplateList : UserControl
    {
        public CtlGCDTemplateList()
        {
            InitializeComponent();
        }

        private GCDTemplateListModel Model { get { return DataContext as GCDTemplateListModel; } }

        private void LstTemplatesMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            //Model.Parent.EditTemplate();
        }
        private void BtnUpdateClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var template = button.DataContext as MappingTemplateHeadGCD;
            Model.Parent.UpdateTemplate(template);
        }
    }
}
