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
using AvdCommon.Rules.Gui.DragDrop;
using AvdCommon.Rules.Gui.TreeNodes;
using ViewValidator.Models.Rules;

namespace ViewValidator.Controls.Rules {
    /// <summary>
    /// Interaktionslogik für RuleToColumnsAssignment.xaml
    /// </summary>
    public partial class RuleToColumnsAssignment : UserControl {
        public RuleToColumnsAssignment() {
            InitializeComponent();
        }

        RuleToColumnsAssignmentModel Model { get { return DataContext as RuleToColumnsAssignmentModel; } }

        private void TreeView_KeyDown(object sender, KeyEventArgs e) {
             if(e.Key == Key.Delete) {
                if (treeView.SelectedItem != null && treeView.SelectedItem is RuleTreeNode) {
                    RuleTreeNode node = (RuleTreeNode) treeView.SelectedItem;
                    ColumnTreeNode columnNode = (ColumnTreeNode) node.Parent;
                    columnNode.RemoveRule(node);
                }
            }
             //if (Keyboard.Modifiers & ModifierKeys.Control == ModifierKeys.Control) {
             //}
        }

        private void treeView_Drop(object sender, DragEventArgs e) {
            TreeViewItem tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(treeView, e.GetPosition(treeView));
            if (tvi != null) {
                TreeNodeBase treeNode = tvi.Header as TreeNodeBase;
                RuleListDragDropData data = e.Data.GetData(typeof (RuleListDragDropData)) as RuleListDragDropData;
                if (treeNode != null && data != null) {
                    e.Handled = true;
                    data.Dragging = false;

                    if (e.KeyStates == DragDropKeyStates.ControlKey) {
                        treeNode.AddRules(data.Rules.ToList());
                    } else {
                        if (data.DragSource != null && treeNode.Root == data.DragSource.Root)
                            //Move rules
                            data.DragSource.MoveRule(treeNode);
                            
                        else {
                            treeNode.AddRules(data.Rules.ToList());
                            if (data.DragSource != null)
                                data.DragSource.DeleteRule();
                                
                            
                        }
                            
                    }

                }
            }

        }

        private Point _startPoint;

        private void treeView_MouseMove(object sender, MouseEventArgs e) {
                if (e.LeftButton == MouseButtonState.Pressed) {
                    if (!Model.DragDropData.Dragging) {
                        TreeViewItem tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(treeView, _startPoint);
                        if (tvi != null && tvi.Header is RuleTreeNode) {
                            RuleTreeNode ruleTreeNode = tvi.Header as RuleTreeNode;

                            e.Handled = true;
                            Model.DragDropData.Clear();
                            Model.DragDropData.Rules.Add(ruleTreeNode.Rule);
                            Model.DragDropData.DragSource = ruleTreeNode;
                            Model.DragDropData.Dragging = true;
                            DragDrop.DoDragDrop(treeView, Model.DragDropData, DragDropEffects.Move | DragDropEffects.Copy);
                        }
                    }

                }
        }

        private void treeView_PreviewMouseLeftButtonDown_1(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(treeView);
        }


        //private void UserControl_DragOver(object sender, DragEventArgs e) {
        //    if (e.KeyStates == DragDropKeyStates.ControlKey)
        //        e.Effects = DragDropEffects.Copy;
        //    else
        //        e.Effects = DragDropEffects.Move;

        //}

        //private void treeView_DragOver(object sender, DragEventArgs e) {
        //    if (e.KeyStates == DragDropKeyStates.ControlKey)
        //        e.Effects = DragDropEffects.Copy;
        //    else
        //        e.Effects = DragDropEffects.Move;
        //}

    }
}
