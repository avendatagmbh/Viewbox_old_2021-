// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-03-15
// copyright 2011 AvenDATA
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using eBalanceKit.Windows.MappingTemplates.Models;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates {
    public partial class CtlTemplateTreeView {
        private Point _startPoint;
        public CtlTemplateTreeView() { InitializeComponent(); }

        internal Popup DragDropPopup { get; set; }

        private IPresentationTree PresentationTree { get { return DataContext as IPresentationTree; } }

        private void TvBalanceSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            //assignmentList.Focus();
            var tvi = FindSelectedNode(tvBalance);
            if (tvi == null) return;

            tvi.BringIntoView(); 
            
            if (tvBalance.SelectedItem is IPresentationTreeNode) {
                // presentation tree node object
                GlobalResources.Info.SelectedElement =
                    ((IPresentationTreeNode) tvBalance.SelectedItem).Element;
            } else {
                // account node object
                GlobalResources.Info.SelectedElement = null;
            }
        }

        public static TreeViewItem FindSelectedNode(DependencyObject parent) {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child == null) continue;
                if (child is TreeViewItem && ((TreeViewItem)child).IsSelected) return (TreeViewItem)child;
                TreeViewItem result = FindSelectedNode(child);
                if (result != null) return result;
            }
            return null;
        }
        private void TvBalancePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) { _startPoint = e.GetPosition(tvBalance); }

        private void TvBalancePreviewMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed && tvBalance.SelectedItem != null &&
                tvBalance.SelectedItem is MappingLineGui) {
                var position = e.GetPosition(tvBalance);

                var tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(tvBalance, position);
                if (tvi == null) return;
                if (!(tvi.Header is MappingLineGui)) return;

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    var assignments = new List<MappingLineGui> {(MappingLineGui) tvBalance.SelectedItem};

                    var data = new TemplateGuiDragDropData(assignments);
                    var dragData = new DataObject("TemplateGUIDragDropData", data);

                    DragDropPopup.DataContext = data;
                    if (!DragDropPopup.IsOpen) DragDropPopup.IsOpen = true;

                    DragDrop.DoDragDrop(tvBalance, dragData, DragDropEffects.Move);

                    if (DragDropPopup.IsOpen) DragDropPopup.IsOpen = false;
                }
            }
        }

        private void TvBalanceDragEnter(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent("TemplateGUIDragDropData") || sender == e.Source) {
                e.Effects = DragDropEffects.None;
                return;
            }
        }

        private void TvBalanceDragOver(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent("TemplateGUIDragDropData")) {
                e.Effects = DragDropEffects.None;
                return;
            }

            var data = e.Data.GetData("TemplateGUIDragDropData") as TemplateGuiDragDropData;
            if (data == null) return;

            var tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(tvBalance, e.GetPosition(tvBalance));
            if (tvi != null) {
                if (tvi.Header is MappingLineGui) {
                    e.Effects = DragDropEffects.Move;
                    data.AllowDrop = true;
                } else if (tvi.Header is IPresentationTreeNode) {
                    var node = tvi.Header as IPresentationTreeNode;
                    if (node.IsValueEditingAllowed) {
                        e.Effects = DragDropEffects.Move;
                        data.AllowDrop = true;
                    } else {
                        // tvi.Header is no allowed drop node
                        e.Effects = DragDropEffects.None;
                        data.AllowDrop = false;
                    }
                }
            } else {
                // tvi.Header == null
                e.Effects = DragDropEffects.None;
                data.AllowDrop = false;
            }
        }

        private void TvBalanceDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent("TemplateGUIDragDropData")) {
                var data = e.Data.GetData("TemplateGUIDragDropData") as TemplateGuiDragDropData;
                if (data == null) return;

                var tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(tvBalance, e.GetPosition(tvBalance));
                if (!(tvi != null &&
                      (tvi.Header is MappingLineGui ||
                       (tvi.Header is IPresentationTreeNode &&
                        ((IPresentationTreeNode) tvi.Header).IsValueEditingAllowed)))) {
                    return;
                }

                IPresentationTreeNode node = null;
                if ((tvi.Header is IPresentationTreeNode) &&
                    ((IPresentationTreeNode) tvi.Header).IsValueEditingAllowed) {
                    node = (IPresentationTreeNode) tvi.Header;
                } else if (tvi.Header is MappingLineGui) {
                    var assignment = (MappingLineGui) tvi.Header;
                    node = (IPresentationTreeNode) (assignment).Parents.First();
                }

                RemoveFromTreeView();

                if (node != null) {
                    foreach (MappingLineGui assignment in data.Items) {
                        node.AddChildren(assignment);
                        (node).IsExpanded = true;
                        (assignment).ElementId = node.Element.Id;
                    }
                }
            }
        }

        private void TvBalanceKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                if (tvBalance.SelectedItem == null) return;
                if (tvBalance.SelectedItem is MappingLineGui) {
                    var mapping = tvBalance.SelectedItem as MappingLineGui;
                    mapping.ElementId = null;
                    mapping.RemoveFromParents();
                }
            }
        }

        private void BtnExpandAllNodesClick(object sender, RoutedEventArgs e) {
            new DlgProgress(UIHelpers.TryFindParent<Window>(this)) {
                ProgressInfo = { IsIndeterminate = true, Caption = eBalanceKitResources.Localisation.ResourcesCommon.LoadingPositions }
            }.ExecuteModal(PresentationTree.ExpandAllNodes);
            
        }
        
        private void BtnCollapseAllNodesClick(object sender, RoutedEventArgs e) { PresentationTree.CollapseAllNodes(); }

        private void RemoveFromTreeView() {
            List<MappingLineGui> assignments = ((TemplateGuiDragDropData)DragDropPopup.DataContext).Items;
            foreach (MappingLineGui assignment in assignments) {
                assignment.RemoveFromParents();
            }
        }
    }
}