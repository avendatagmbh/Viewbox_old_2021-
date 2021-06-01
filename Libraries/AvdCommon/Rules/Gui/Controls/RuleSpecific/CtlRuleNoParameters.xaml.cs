using System.Windows;
using System.Windows.Controls;
using AvdCommon.Rules.Gui.Models;

namespace AvdCommon.Rules.Gui.Controls.RuleSpecific
{
    /// <summary>
    ///   Interaktionslogik für CtlRuleNoParameters.xaml
    /// </summary>
    public partial class CtlRuleNoParameters : UserControl
    {
        public CtlRuleNoParameters()
        {
            InitializeComponent();
        }

        private EditRuleModel Model
        {
            get { return DataContext as EditRuleModel; }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Model != null)
                Model.ParametersChanged();
        }
    }
}