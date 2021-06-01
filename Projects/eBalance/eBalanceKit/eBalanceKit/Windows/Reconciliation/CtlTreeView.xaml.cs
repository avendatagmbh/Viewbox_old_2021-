// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-08
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AvdWpfControls.Localisation;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces;

namespace eBalanceKit.Windows.Reconciliation {
    public partial class CtlTreeView {
        /// <summary>
        /// Start position of mouse drag event, needed to init drag&drop process.
        /// </summary>
        private Point? _startPoint;

        public CtlTreeView() { InitializeComponent(); }

        private static DataTemplate DefaultItemTemplate {
            get {
                var dt = new HierarchicalDataTemplate { ItemsSource = new Binding()};
                var labelFactory = new FrameworkElementFactory(typeof(TextBlock));
                labelFactory.SetBinding(TextBlock.TextProperty, new Binding("Element.Label"));
                dt.VisualTree = labelFactory;
                return dt;
            }
        }

        private ITaxonomyTree Model { get { return DataContext as ITaxonomyTree; } }

        private void BtnExpandAllNodesClick(object sender, RoutedEventArgs e) {
            if (Model != null)
                new DlgProgress(UIHelpers.TryFindParent<Window>(this)) {
                    ProgressInfo = { IsIndeterminate = true, Caption = eBalanceKitResources.Localisation.ResourcesCommon.LoadingPositions }
                }.ExecuteModal(Model.ExpandAllNodes);
        }

        private void BtnCollapseAllNodesClick(object sender, RoutedEventArgs e) { if (Model != null) Model.CollapseAllNodes(); }
        
        #region ItemTemplate
        public DataTemplate ItemTemplate { get { return (DataTemplate) GetValue(ItemTemplateProperty); } set { SetValue(ItemTemplateProperty, value); } }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof (DataTemplate), typeof (CtlTreeView),
                                        new UIPropertyMetadata(null));
        #endregion ItemTemplate

        #region ItemTemplateSelector
        public DataTemplateSelector ItemTemplateSelector {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateSelectorProperty =
            DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(CtlTreeView), new UIPropertyMetadata(null));       
        #endregion ItemTemplateSelector

        private Popup DragDropPopup { get { return UIHelpers.TryFindParent<CtlReconciliations>(this).DragDropPopup; } }
        
        private void TreeView_OnPreviewMouseMove(object sender, MouseEventArgs e) {
            var tree = (IReconciliationTree) DataContext;
            if (!tree.Document.ReportRights.WriteTransferValuesAllowed) return;

            if (e.LeftButton == MouseButtonState.Pressed && _startPoint != null && !DragDropPopup.IsVisible) {
                Point position = e.GetPosition(treeView);

                if (Math.Abs(position.X - _startPoint.Value.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Value.Y) > SystemParameters.MinimumVerticalDragDistance) {

                    var tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(treeView, position);
                    if (tvi == null) return;

                    var node = tvi.Header as IReconciliationTreeNode;
                    if (node == null) return;
                    
                    if (!node.Element.IsReconciliationPosition) return;

                    var data = new DragDropData {Item = node.Element};
                    var dragData = new DataObject("DragDropData", data);

                    DragDropPopup.DataContext = data;
                    if (!DragDropPopup.IsOpen) DragDropPopup.IsOpen = true;

                    DragDrop.DoDragDrop(treeView, dragData, DragDropEffects.Move);

                    if (DragDropPopup.IsOpen) DragDropPopup.IsOpen = false;
                }
            }
        }

        private void TreeView_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var position = e.GetPosition(treeView);
            
            var tb = UIHelpers.TryFindFromPoint<TextBoxBase>(treeView, position);
            if (tb != null) {
                _startPoint = null;
                return;
            }

            var scrollBar = UIHelpers.TryFindFromPoint<ScrollBar>(treeView, position);
            if (scrollBar != null) {
                _startPoint = null;
                return;
            }

            _startPoint = position;
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            var tvi = FindSelectedNode(treeView);
            if (tvi == null) return;

            tvi.BringIntoView();

            if (treeView.SelectedItem is IReconciliationTreeNode) {
                GlobalResources.Info.SelectedElement =
                    (treeView.SelectedItem as IReconciliationTreeNode).Element;
            } else {
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

        private void Border_SizeChanged(object sender, SizeChangedEventArgs e) {
            treeViewScrollViewer.HorizontalScrollBarVisibility = ActualWidth < 600
                                                                     ? ScrollBarVisibility.Visible
                                                                     : ScrollBarVisibility.Disabled;
        }
    }
}