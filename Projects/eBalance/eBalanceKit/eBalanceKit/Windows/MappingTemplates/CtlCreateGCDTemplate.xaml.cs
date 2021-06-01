using System.Windows;

namespace eBalanceKit.Windows.MappingTemplates
{
    /// <summary>
    /// Interaction logic for CtlCreateGCDTemplate.xaml
    /// </summary>
    public partial class CtlCreateGCDTemplate
    {
        public CtlCreateGCDTemplate()
        {
            InitializeComponent();
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e) { UIHelpers.TryFindParent<Window>(this).DialogResult = false; }
        private void BtnCreateNewTemplateClick(object sender, RoutedEventArgs e) {
            UIHelpers.TryFindParent<Window>(this).DialogResult = true;
        }
    }
}
