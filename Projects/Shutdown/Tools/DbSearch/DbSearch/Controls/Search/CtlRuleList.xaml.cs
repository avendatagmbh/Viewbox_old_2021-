using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AvdCommon.Rules;
using AvdCommon.Rules.Gui.Models;
using DbSearch.Models.Rules;
using DbSearch.Structures;
using DbSearch.Windows;
using Utils;

namespace DbSearch.Controls.Search {
    /// <summary>
    /// Interaktionslogik für CtlRuleList.xaml
    /// </summary>
    public partial class CtlRuleList : UserControl {
        public CtlRuleList() {
            InitializeComponent();
        }

        RuleListBoxModel Model { get { return (DataContext as RuleListModel).RuleListBoxModel; } }

        private void AddRule() {
            AvdCommon.Rules.Gui.DlgNewRule dlgNewRule = new AvdCommon.Rules.Gui.DlgNewRule() { Owner = UIHelpers.TryFindParent<Window>(this) };
            NewRuleModel model = new NewRuleModel();
            dlgNewRule.DataContext = model;

            bool? dialogResult = dlgNewRule.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value && Model.Rules != null) {
                Model.Rules.AddRule(model.EditRuleModel.RuleWithParameters, null, true);
            }
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e) {
            AddRule();
        }

        private void btnDeleteItem_Click(object sender, RoutedEventArgs e) {
            if (ctlRuleListBox.ruleList.SelectedIndex != -1) {
                Model.Rules.RemoveRule((Rule)ctlRuleListBox.ruleList.SelectedItem);
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            ctlRuleListBox.DataContext = Model;
        }

        //private void Window_Drop(object sender, DragEventArgs e) {
        //    Model.DragDropData.Dragging = false;
        //    //If a rule is dropped somewhere in the window where it should not be dropped, then the user most certainly wanted to delete the rule
        //    if (Model.DragDropData.DragSource != null) {
        //        Model.DragDropData.DragSource.DeleteRule();
        //    }
        //}


    }
}
