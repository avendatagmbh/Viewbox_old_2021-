using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AvdCommon.Rules.ExecuteRules;
using AvdCommon.Rules.Gui.Factories;
using AvdCommon.Rules.Gui.Models;

namespace AvdCommon.Rules.Gui.Controls
{
    /// <summary>
    ///   Interaktionslogik für CtlEditRule.xaml
    /// </summary>
    public partial class CtlEditRule : UserControl
    {
        public CtlEditRule()
        {
            InitializeComponent();
        }

        private EditRuleModel Model
        {
            get { return DataContext as EditRuleModel; }
        }

        private UserControl CurrentRuleControl { get; set; }

        private void SetControl()
        {
            if (CurrentRuleControl != null)
            {
                //Set to null in order to avoid binding errors
                CurrentRuleControl.DataContext = null;
                mainGrid.Children.Remove(CurrentRuleControl);
            }
            CurrentRuleControl = RuleControlFactory.UserControlFromRule(Model.Rule);
            CurrentRuleControl.Margin = new Thickness(0, 5, 0, 0);

            Grid.SetRow(CurrentRuleControl, 1);
            CurrentRuleControl.DataContext = Model;
            mainGrid.Children.Add(CurrentRuleControl);
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Rule")
            {
                SetControl();
            }
            if (e.PropertyName == "Parameters")
            {
                UpdateAfterRuleExecution();
            }
        }

        private void UpdateAfterRuleExecution()
        {
            if (Model != null && Model.RuleWithParameters != null && string.IsNullOrEmpty(Model.ParameterError))
            {
                try
                {
                    Model.ExecutionError = string.Empty;
                    txtAfterRuleExecution.Text =
                        ((ExecuteRule) (Model.RuleWithParameters)).Execute(txtBeforeRuleExecution.Text);
                }
                catch (Exception ex)
                {
                    Model.ExecutionError = "Fehler beim Anwenden: " + ex.Message;
                }
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Model != null)
            {
                Model.PropertyChanged += Model_PropertyChanged;
                SetControl();
            }
        }

        private void txtBeforeRuleExecution_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAfterRuleExecution();
        }
    }
}