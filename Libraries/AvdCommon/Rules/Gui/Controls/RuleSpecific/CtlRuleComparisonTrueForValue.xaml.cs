using System.Windows.Controls;
using AvdCommon.Rules.ExecuteRules;
using AvdCommon.Rules.Gui.Models;

namespace AvdCommon.Rules.Gui.Controls.RuleSpecific
{
    /// <summary>
    ///   Interaktionslogik für CtlRuleComparisonTrueForValue.xaml
    /// </summary>
    public partial class CtlRuleComparisonTrueForValue : UserControl
    {
        public CtlRuleComparisonTrueForValue()
        {
            InitializeComponent();
        }

        private EditRuleModel Model
        {
            get { return DataContext as EditRuleModel; }
        }

        private void UpdateParameters()
        {
            if (Model == null) return;
            if (Model.RuleWithParameters is RuleComparisonTrueForValue)
            {
                Model.ParameterError = "";
                Model.ParametersChanged();
            }
        }

        private void txtTrueValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateParameters();
        }
    }
}