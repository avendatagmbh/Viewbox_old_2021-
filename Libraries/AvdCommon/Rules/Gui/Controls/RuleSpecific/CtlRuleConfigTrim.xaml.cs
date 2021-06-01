using System;
using System.Windows;
using System.Windows.Controls;
using AvdCommon.Rules.ExecuteRules;
using AvdCommon.Rules.Gui.Models;

namespace AvdCommon.Rules.Gui.Controls.RuleSpecific
{
    /// <summary>
    ///   Interaktionslogik für CtlRuleConfigTrim.xaml
    /// </summary>
    public partial class CtlRuleConfigTrim : UserControl
    {
        public CtlRuleConfigTrim()
        {
            InitializeComponent();
        }

        private EditRuleModel Model
        {
            get { return DataContext as EditRuleModel; }
        }

        public bool HasErrors(ref string errorMessage)
        {
            int trimCount;
            if (!Int32.TryParse(txtTrimCount.Text, out trimCount))
            {
                errorMessage = "Die Anzahl der zu trimmenden Zeichen muss eine Zahl sein.";
                return true;
            }
            if (trimCount <= 0)
            {
                errorMessage = "Die Anzahl der zu trimmenden Zeichen muss positiv sein.";
                return true;
            }
            return false;
        }

        private void UpdateParameters()
        {
            if (Model == null) return;
            if (Model.RuleWithParameters is RuleTrim)
            {
                RuleTrim rule = Model.RuleWithParameters as RuleTrim;
                string errorMessage = "";
                if (!HasErrors(ref errorMessage))
                {
                    rule.FromLeft = cbFromLeft.IsChecked.Value;
                    rule.TrimCount = Convert.ToInt32(txtTrimCount.Text);
                }
                Model.ParameterError = errorMessage;
                Model.ParametersChanged();
            }
        }

        private void txtTrimCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateParameters();
        }

        private void cbFromLeft_Checked(object sender, RoutedEventArgs e)
        {
            UpdateParameters();
        }

        private void cbFromLeft_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateParameters();
        }
    }
}