using System.Windows;
using System.Windows.Controls;
using AvdCommon.Rules.Gui.Models;

namespace AvdCommon.Rules.Gui.Controls.RuleSpecific
{
    /// <summary>
    ///   Interaktionslogik für CtlRuleConfigEliminateLeadingZeros.xaml
    /// </summary>
    public partial class CtlRuleConfigEliminateLeadingZeros : UserControl
    {
        public CtlRuleConfigEliminateLeadingZeros()
        {
            InitializeComponent();
        }

        private EditRuleModel Model
        {
            get { return DataContext as EditRuleModel; }
        }

        private void cbDeleteLastZero_Click(object sender, RoutedEventArgs e)
        {
            Model.ParameterError = "";
            Model.ParametersChanged();
        }
    }
}