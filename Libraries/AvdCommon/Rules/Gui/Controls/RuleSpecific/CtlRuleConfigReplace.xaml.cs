using System.Windows;
using System.Windows.Controls;
using AvdCommon.Rules.ExecuteRules;
using AvdCommon.Rules.Gui.Models;

namespace AvdCommon.Rules.Gui.Controls.RuleSpecific
{
    /// <summary>
    ///   Interaktionslogik für CtlRuleConfigReplace.xaml
    /// </summary>
    public partial class CtlRuleConfigReplace : UserControl
    {
        public CtlRuleConfigReplace()
        {
            InitializeComponent();
        }

        private EditRuleModel Model
        {
            get { return DataContext as EditRuleModel; }
        }

        public bool HasErrors(ref string errorMessage)
        {
            if (string.IsNullOrEmpty(txtOldString.Text))
            {
                errorMessage = "Der zu ersetzende String darf nicht leer sein";
                return true;
            }
            return false;
        }

        private void UpdateParameters()
        {
            if (Model == null) return;
            if (Model.RuleWithParameters is RuleReplace)
            {
                RuleReplace rule = Model.RuleWithParameters as RuleReplace;
                string errorMessage = "";
                if (!HasErrors(ref errorMessage))
                {
                }
                Model.ParameterError = errorMessage;
                Model.ParametersChanged();
            }
        }

        private void txtNewString_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateParameters();
        }

        private void txtOldString_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateParameters();
        }

        private void cbUseRegex_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateParameters();
        }

        private void cbUseRegex_Checked(object sender, RoutedEventArgs e)
        {
            UpdateParameters();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateParameters();
        }
    }
}