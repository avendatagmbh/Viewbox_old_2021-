using System;
using System.Windows;
using AvdCommon.Rules.Gui.Models;

namespace AvdCommon.Rules.Gui
{
    /// <summary>
    ///   Interaktionslogik für DlgEditRule.xaml
    /// </summary>
    public partial class DlgEditRule : Window
    {
        public DlgEditRule()
        {
            InitializeComponent();
        }

        private EditRuleModel Model
        {
            get { return DataContext as EditRuleModel; }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Model.ParameterError) && string.IsNullOrEmpty(Model.ExecutionError))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(this,
                                "Bitte beheben Sie zuerst die folgenden Fehler: " + Environment.NewLine +
                                Model.ParameterError, "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}