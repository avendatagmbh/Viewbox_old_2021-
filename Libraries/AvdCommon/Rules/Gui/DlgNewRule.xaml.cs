using System;
using System.Windows;
using AvdCommon.Rules.Gui.Models;

namespace AvdCommon.Rules.Gui
{
    /// <summary>
    ///   Interaktionslogik für DlgNewRule.xaml
    /// </summary>
    public partial class DlgNewRule : Window
    {
        public DlgNewRule()
        {
            InitializeComponent();
        }

        private NewRuleModel Model
        {
            get { return DataContext as NewRuleModel; }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Model.EditRuleModel.ParameterError) &&
                string.IsNullOrEmpty(Model.EditRuleModel.ExecutionError))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(this,
                                "Bitte beheben Sie zuerst die folgenden Fehler: " + Environment.NewLine +
                                Model.EditRuleModel.ParameterError, "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}