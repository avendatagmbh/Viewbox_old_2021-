// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-01-03
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Taxonomy.Enums;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKit.Models;
using eBalanceKit.Structures;
using eBalanceKit.Windows;
using eBalanceKit.Windows.EditPresentationTreeDetails;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.Presentation;
using IPresentationTree = eBalanceKitBusiness.IPresentationTree;
using IPresentationTreeNode = eBalanceKitBusiness.Interfaces.PresentationTree.IPresentationTreeNode;

namespace eBalanceKit.Controls {
    /// <summary>
    /// Interaktionslogik für CtlTaxonomyTreeView.xaml
    /// </summary>
    public partial class CtlTaxonomyTreeView : UserControl {
        #region constructor
        public CtlTaxonomyTreeView() {
            InitializeComponent();
            DataContextChanged += CtlTaxonomyTreeView_DataContextChanged;
        }

        private void CtlTaxonomyTreeView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != null) 
            {
                var oldModel = e.OldValue as TaxonomyViewModel;
                oldModel.RoleURI = null;
                oldModel.PropertyChanged -= Model_PropertyChanged;
                
            }
            
            if (Model != null) {
                Model.PropertyChanged += Model_PropertyChanged;
                var temp = Model.RoleURI;
                var main = Model.Owner as MainWindow;
                if (main != null) {
                    DragDropPopup = main.DragDropPopup;
                    GiveFeedback += main.dragItem_GiveFeedback;
                }

                Model.TreeView = tvBalance;

            }
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "PresentationTree") {
                if (Dispatcher.CheckAccess()) {
                    if (Model.PresentationTree == null) tvBalance.ItemsSource = null;
                    else if (!string.IsNullOrEmpty(PresentationRootName))
                        tvBalance.ItemsSource = Model.PresentationTree.GetNode(PresentationRootName);
                    else tvBalance.ItemsSource = Model.PresentationTree;
                } else {
                    Dispatcher.Invoke(
                        DispatcherPriority.Background,
                        new Action(
                            delegate {
                                if (Model.PresentationTree == null)
                                    tvBalance.ItemsSource = null;
                                if (!string.IsNullOrEmpty(PresentationRootName) && Model.PresentationTree!=null)
                                    tvBalance.ItemsSource = Model.PresentationTree.GetNode(PresentationRootName);
                                else
                                    tvBalance.ItemsSource = Model.PresentationTree;
                            }));
                }
            }
        }
        #endregion

        #region fields
        private bool _cntrlPressed;

        /// <summary>
        /// Start position of mouse drag event, needed to init drag&drop process.
        /// </summary>
        private Point? _startPoint;
        #endregion fields

        #region properties
        private TaxonomyViewModel Model {
            get {
                if (Dispatcher.CheckAccess()) return DataContext as TaxonomyViewModel;
                TaxonomyViewModel obj = null;
                Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new Action(
                        delegate { obj = DataContext as TaxonomyViewModel; }));
                return obj;
            }
        }

        public Popup DragDropPopup { get; set; }

        #endregion properties

        #region eventHandler

        #region tvBalance_SelectedItemChanged
        private void tvBalance_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (!_cntrlPressed) {
                UnselectAllTreeItems(tvBalance.SelectedItem);
            }

            if (tvBalance.SelectedItem is IsSelectable)
                (tvBalance.SelectedItem as IsSelectable).IsSelected = true;           

            if (tvBalance.SelectedItem is PresentationTreeNode) {
                // presentation tree node object                
                GlobalResources.Info.SelectedElement = ((PresentationTreeNode) tvBalance.SelectedItem).Element;
            } else {
                // account node object
                GlobalResources.Info.SelectedElement = null;
            }
        }
        #endregion

        #region tvBalance_PreviewMouseLeftButtonDown
        private void tvBalance_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {

            var position = e.GetPosition(tvBalance);

            var scrollBar = UIHelpers.TryFindFromPoint<ScrollBar>(tvBalance, position);
            if (scrollBar != null) {
                _startPoint = null;
                return;
            } 
            _startPoint = position;
        }
        #endregion

        #region tvBalance_PreviewMouseMove
        private void tvBalance_PreviewMouseMove(object sender, MouseEventArgs e) {
            if (DragDropPopup == null) return; // disables drag&drop

            if (e.LeftButton == MouseButtonState.Pressed && _startPoint != null && !DragDropPopup.IsVisible) {
                Point position = e.GetPosition(tvBalance);

                var tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(tvBalance, position);
                if (tvi == null) return;
                if (!(tvi.Header is IBalanceListEntry)) return;

                if (Math.Abs(position.X - _startPoint.Value.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Value.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    if (!RightManager.HasDocumentRestWriteRight(Model.Document, UIHelpers.TryFindParent<Window>(this)))
                        return;

                    List<IBalanceListEntry> accounts = GetSelectedBalanceListItems();

                    //foreach (IBalanceListEntry account in accounts) {
                    //    Model.Document.RemoveAssignment(account);
                    //    account.RemoveFromParents();
                    //}

                    var data = new AccountListDragDropData(accounts);
                    var dragData = new DataObject("AccountListDragDropData", data);

                    DragDropPopup.DataContext = data;
                    if (!DragDropPopup.IsOpen) DragDropPopup.IsOpen = true;

                    //RemoveFromTreeView();
                    try {
                        DragDrop.DoDragDrop(tvBalance, dragData, DragDropEffects.Move);
                    }
                    catch (eBalanceKitBusiness.Exceptions.AssignmentNotAllowedException ex) {
                        MessageBox.Show(ex.Message, ex.Header, MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    if (DragDropPopup.IsOpen) DragDropPopup.IsOpen = false;
                }
            }
        }
        #endregion

        #region tvBalance_DragEnter
        private void tvBalance_DragEnter(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent("AccountListDragDropData") || sender == e.Source) {
                e.Effects = DragDropEffects.None;
                return;
            }
        }
        #endregion

        #region tvBalance_DragOver
        private void tvBalance_DragOver(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent("AccountListDragDropData") ||
                !RightManager.WriteRestDocumentAllowed(Model.Document)) {
                e.Effects = DragDropEffects.None;
                return;
            }

            var data = e.Data.GetData("AccountListDragDropData") as AccountListDragDropData;

            var tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(tvBalance, e.GetPosition(tvBalance));
            if (tvi != null &&
                (tvi.Header is IBalanceListEntry ||
                 (tvi.Header is IPresentationTreeNode) && ((IPresentationTreeNode) tvi.Header).IsValueEditingAllowed)) {
                e.Effects = DragDropEffects.Move;
                data.AllowDrop = true;
            } else {
                e.Effects = DragDropEffects.None;
                data.AllowDrop = false;
            }
        }
        #endregion

        #region tvBalance_Drop
        private void tvBalance_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent("AccountListDragDropData")) {
                if (!RightManager.WriteRestDocumentAllowed(Model.Document)) return;
                var data = e.Data.GetData("AccountListDragDropData") as AccountListDragDropData;

                var tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(tvBalance, e.GetPosition(tvBalance));
                if (!(tvi != null &&
                      (tvi.Header is IBalanceListEntry ||
                       (tvi.Header is IPresentationTreeNode) &&
                       ((IPresentationTreeNode) tvi.Header).IsValueEditingAllowed))) {
                    return;
                }

                IPresentationTreeNode node = null;
                if ((tvi.Header is IPresentationTreeNode) && ((IPresentationTreeNode) tvi.Header).IsValueEditingAllowed) {
                    node = (IPresentationTreeNode) tvi.Header;
                } else if (tvi.Header is IBalanceListEntry) {
                    var dropAccount = (IBalanceListEntry) tvi.Header;
                    node = (IPresentationTreeNode) (dropAccount).Parents.First();
                }

                if (node == null) return;

                // remove existing assignments
                foreach (var account in data.Accounts) {
                    Model.Document.RemoveAssignment(account);
                    account.RemoveFromParents();
                }

                foreach (var assignedNode in node.Element.PresentationTreeNodes) {
                    foreach (var account in data.Accounts) {
                        if(account is IAccount && !(account as IAccount).CheckAssignmentAllowed(assignedNode))
                            continue;
                        assignedNode.AddChildren(account);
                        ((IPresentationTreeNode) assignedNode).IsExpanded = true;
                    }
                }

                var assignments = data.Accounts.Select(account => new Tuple<IBalanceListEntry, string>(account, node.Element.Id)).ToList();
                Model.Document.AddAccountAssignments(assignments, null);
            }
        }
        #endregion

        #region tvBalance_KeyDown
        private void tvBalance_KeyDown(object sender, KeyEventArgs e) {
            if (tvBalance.SelectedItem is IAccount && e.Key == Key.Delete) {
                if (!RightManager.HasDocumentRestWriteRight(
                    Model.Document, UIHelpers.TryFindParent<Window>(this))) return;

                var items = GetSelectedBalanceListItems();
                foreach (var item in items) RemoveAssignment(item);

                e.Handled = true;
                return;
            }

            var node = tvBalance.SelectedItem as IPresentationTreeNode;
            if (node == null) return;
            var owner = UIHelpers.TryFindParent<Window>(this);

            switch (e.Key) {
                case Key.Delete: {
                    if (!RightManager.HasDocumentRestWriteRight(
                        Model.Document, UIHelpers.TryFindParent<Window>(this))) return;

                    switch (node.Element.ValueType) {
                        case XbrlElementValueTypes.Monetary:
                            var items = GetSelectedBalanceListItems();
                            foreach (var item in items) RemoveAssignment(item);
                            break;
                    } 
                    
                    e.Handled = true;
                    break;
                }

                case Key.F2: {

                    if (node != null) {
                        
                        switch (node.Element.ValueType) {
                            case XbrlElementValueTypes.HyperCubeContainerItem:
                                if (!node.Document.ExistsHyperCube(node.Element.Id)) return;
                                
                                // preload cube data
                                var progress = new DlgProgress(owner)
                                               {ProgressInfo = {Caption = "Lade Tabelle...", IsIndeterminate = true}};
                                progress.ExecuteModal(node.Document.GetHyperCube(node.Element.Id).PreloadItems);

                                new DlgEditHyperCubeDetails(node) { Owner = owner }.ShowDialog();
                                return;

                            case XbrlElementValueTypes.Abstract:
                            case XbrlElementValueTypes.None:
                            case XbrlElementValueTypes.SingleChoice:
                            case XbrlElementValueTypes.MultipleChoice:
                            case XbrlElementValueTypes.Boolean:
                            case XbrlElementValueTypes.Date:
                            case XbrlElementValueTypes.Int:
                            case XbrlElementValueTypes.Numeric:
                                // no detail editor implemented
                                break;

                            case XbrlElementValueTypes.Tuple:
                                if (node.Element.IsList) {
                                    new DlgEditListDetails(node) {
                                        Owner = owner,
                                        DataContext = new PresentationTreeDetailModel(node.Document, node.Value)
                                    }.ShowDialog();
                                }
                                break;

                            case XbrlElementValueTypes.Monetary:
                                new DlgEditMonetaryDetails {
                                    Owner = owner,
                                    DataContext = new PresentationTreeDetailModel(node.Document, node.Value)
                                }.ShowDialog();
                                break;

                            case XbrlElementValueTypes.String:
                                new DlgEditTextDetails {
                                    Owner = owner,
                                    DataContext =
                                        new PresentationTreeDetailModel(node.Document,
                                                                        node.Value)
                                }.ShowDialog();
                                break;
                        }

                        e.Handled = true;
                    }
                    break;
                }

                case Key.LeftCtrl:
                case Key.RightCtrl: {
                    _cntrlPressed = true;
                    e.Handled = true;
                    break;
                }
            }
        }
        #endregion

        #region txtFilter_KeyDown
        //private void txtFilter_KeyDown(object sender, KeyEventArgs e) {
        //    if (e.Key == Key.Return) {
        //        txtFilter.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        //        e.Handled = true;
        //    }
        //}
        #endregion

        //--------------------------------------------------------------------------------

        private void tvBalance_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) {
                _cntrlPressed = false;
                e.Handled = true;
            }
        }


        //--------------------------------------------------------------------------------
        #endregion eventHandler

        #region methods

        #region RemoveFromTreeView
        /// <summary>
        /// Removes the selected item from tree view.
        /// </summary>
        private void RemoveFromTreeView() {
            List<IBalanceListEntry> accounts = ((AccountListDragDropData) DragDropPopup.DataContext).Accounts;
            foreach (var account in accounts) RemoveAssignment(account);
        }

        private void RemoveAssignment(IBalanceListEntry account) {
            Model.Document.RemoveAssignment(account);
            account.RemoveFromParents();
        }
        #endregion

        //--------------------------------------------------------------------------------

       // private void btnFilter_Click(object sender, RoutedEventArgs e) { txtFilter.GetBindingExpression(TextBox.TextProperty).UpdateSource(); }

        //private void btnResetFilter_Click(object sender, RoutedEventArgs e) {
        //    txtFilter.Clear();
        //    txtFilter.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        //    Model.PresentationTree.ResetFilter();
        //}

        private List<IBalanceListEntry> GetSelectedBalanceListItems() {
            var accounts = new List<IBalanceListEntry>();

            Action<IPresentationTreeEntry> getSelectedBalanceListItems = null;
            getSelectedBalanceListItems = root => {
                if (root is IBalanceListEntry) {
                    var item = root as IBalanceListEntry;
                    if (item.IsSelected) {
                        accounts.Add(root as IBalanceListEntry);
                    }
                } else if (root is IPresentationTreeNode) {
                    var item = root as IPresentationTreeNode;
                    foreach (var node in item.Children) {
                        getSelectedBalanceListItems(node);
                    }
                }
            };

            foreach (var node in Model.PresentationTree.RootEntries) {
                getSelectedBalanceListItems(node);
            }
            return accounts;
        }

        #region UnselectAllTreeItems
        private void UnselectAllTreeItems(object selectedItem) {
            if (Model.PresentationTree != null)
                Model.PresentationTree.UnselectAll(selectedItem);
        }
        #endregion

        //--------------------------------------------------------------------------------
        #endregion methods

        #region PresentationRootName
        public string PresentationRootName { get; set; }
        #endregion PresentationRootName

        private void BtnExpandAllNodesClick(object sender, RoutedEventArgs e) {
            if (Model != null)
                new DlgProgress(UIHelpers.TryFindParent<Window>(this)) {
                    ProgressInfo = { IsIndeterminate = true, Caption = eBalanceKitResources.Localisation.ResourcesCommon.LoadingPositions }
                }.ExecuteModal(Model.PresentationTree.ExpandAllNodes);
        }
        private void BtnCollapseAllNodesClick(object sender, RoutedEventArgs e) { Model.PresentationTree.CollapseAllNodes(); }
    }
}