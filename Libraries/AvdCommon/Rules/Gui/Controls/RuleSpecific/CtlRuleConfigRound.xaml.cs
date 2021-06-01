using System;
using System.Windows.Controls;
using AvdCommon.Rules.ExecuteRules;
using AvdCommon.Rules.Gui.Models;

namespace AvdCommon.Rules.Gui.Controls.RuleSpecific
{
    /// <summary>
    ///   Interaktionslogik für CtlRuleConfigTrim.xaml
    /// </summary>
    public partial class CtlRuleConfigRound : UserControl
    {
        public CtlRuleConfigRound()
        {
            InitializeComponent();
        }

        private EditRuleModel Model
        {
            get { return DataContext as EditRuleModel; }
        }

        public bool HasErrors(ref string errorMessage)
        {
            int roundToDecimal;
            if (!Int32.TryParse(txtRoundToDecimal.Text, out roundToDecimal))
            {
                errorMessage = "Die Stelle auf die gerundet wird, muss eine Zahl sein";
                return true;
            }
            if (roundToDecimal <= 0)
            {
                errorMessage = "Die Stelle auf die gerundet wird, muss positiv sein.";
                return true;
            }
            return false;
        }

        private void UpdateParameters()
        {
            if (Model == null) return;
            if (Model.RuleWithParameters is RuleRound)
            {
                RuleRound rule = Model.RuleWithParameters as RuleRound;
                string errorMessage = "";
                if (!HasErrors(ref errorMessage))
                {
                    rule.RoundToDecimal = Convert.ToInt32(txtRoundToDecimal.Text);
                }
                Model.ParameterError = errorMessage;
                Model.ParametersChanged();
            }
        }

        private void txtRoundToDecimal_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateParameters();
        }
    }
}