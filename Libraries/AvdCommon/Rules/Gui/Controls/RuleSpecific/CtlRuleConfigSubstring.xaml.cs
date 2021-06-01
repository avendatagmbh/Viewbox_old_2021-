using System;
using System.Windows;
using System.Windows.Controls;
using AvdCommon.Rules.ExecuteRules;
using AvdCommon.Rules.Gui.Models;

namespace AvdCommon.Rules.Gui.Controls.RuleSpecific
{
    /// <summary>
    ///   Interaktionslogik für CtlRuleConfigSubstring.xaml
    /// </summary>
    public partial class CtlRuleConfigSubstring : UserControl
    {
        public CtlRuleConfigSubstring()
        {
            InitializeComponent();
        }

        private EditRuleModel Model
        {
            get { return DataContext as EditRuleModel; }
        }

        public bool HasErrors(ref string errorMessage)
        {
            int startIndex, endIndex;

            if (!Int32.TryParse(txtStartIndex.Text, out startIndex))
            {
                errorMessage = "Startindex ist kein gültiger Integer";
                return true;
            }
            if (!Int32.TryParse(txtEndIndex.Text, out endIndex))
            {
                errorMessage = "Endindex ist kein gültiger Integer";
                return true;
            }
            if (startIndex < 0)
            {
                errorMessage = "Startindex darf nicht negativ sein";
                return true;
            }
            if (endIndex < 0)
            {
                errorMessage = "Endindex darf nicht negativ sein";
                return true;
            }
            if (startIndex > endIndex)
            {
                errorMessage = "Startindex darf nicht größer als Endindex sein.";
                return true;
            }
            return false;
        }

        public Rule CreateRuleWithParameters()
        {
            string errorMessage = String.Empty;
            if (HasErrors(ref errorMessage)) throw new ArgumentException(errorMessage);

            int startIndex = Convert.ToInt32(txtStartIndex.Text);
            int endIndex = Convert.ToInt32(txtEndIndex.Text);
            return new RuleSubstring {FromPos = startIndex, ToPos = endIndex};
        }

        private void UpdateParameters()
        {
            if (Model == null) return;
            if (Model.RuleWithParameters is RuleSubstring)
            {
                RuleSubstring rule = Model.RuleWithParameters as RuleSubstring;
                string errorMessage = "";
                if (!HasErrors(ref errorMessage))
                {
                    //    rule.FromPos = Convert.ToInt32(txtStartIndex.Text);
                    //    rule.ToPos = Convert.ToInt32(txtEndIndex.Text);
                }
                Model.ParameterError = errorMessage;
                Model.ParametersChanged();
            }
        }

        private void txtStartIndex_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateParameters();
        }

        private void txtEndIndex_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateParameters();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateParameters();
        }
    }
}