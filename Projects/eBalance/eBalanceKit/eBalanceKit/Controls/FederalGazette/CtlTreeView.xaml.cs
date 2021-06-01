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
using Taxonomy.Enums;
using eBalanceKit.Windows.EditPresentationTreeDetails;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Interfaces.BalanceList;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;
using eBalanceKitBusiness.Structures.Presentation;
using eBalanceKitBusiness;
using eBalanceKit.Models.FederalGazette;
using eBalanceKit.Models;

namespace eBalanceKit.Controls.FederalGazette {
    /// <summary>
    /// Interaktionslogik für CtlTreeView.xaml
    /// </summary>
    public partial class CtlTreeView : UserControl {
        public CtlTreeView() {
            InitializeComponent();
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            //if (!_cntrlPressed) {
                UnselectAllTreeItems(treeView.SelectedItem);
            //}
            if (treeView.SelectedItem is IsSelectable)
                (treeView.SelectedItem as IsSelectable).IsSelected = true;   
        }

        #region UnselectAllTreeItems
        private void UnselectAllTreeItems(object selectedItem) {
            if (Model != null)
                Model.UnselectAll(selectedItem);
        }
        #endregion

        #region tvBalance_KeyDown
        private void treeView_KeyDown(object sender, KeyEventArgs e) {
            
            var node = treeView.SelectedItem as IPresentationTreeNode;
            if (node == null) return;
            var owner = UIHelpers.TryFindParent<Window>(this);

            switch (e.Key) {
                case Key.F2: {
                    switch (node.Element.ValueType) {
                        case XbrlElementValueTypes.HyperCubeContainerItem:
                            if (!node.Document.ExistsHyperCube(node.Element.Id)) return;

                            // preload cube data
                            var progress = new DlgProgress(owner)
                                           {ProgressInfo = {Caption = "Lade Tabelle...", IsIndeterminate = true}};
                            progress.ExecuteModal(node.Document.GetHyperCube(node.Element.Id).PreloadItems);

                            new DlgEditHyperCubeDetails(node) {Owner = owner}.ShowDialog();
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
                                    new PresentationTreeDetailModelRtf(node.Document,
                                                                       node.Value)
                            }.ShowDialog();
                            break;
                    }

                    e.Handled = true;
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

        private bool _cntrlPressed;
        private eBalanceKitBusiness.IPresentationTree Model { get { return DataContext as eBalanceKitBusiness.IPresentationTree; } }

        /// <summary>
        /// Collapse all node in the treeView.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCollapseAllNodes_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext != null) {
                FederalGazetteTreeViewModel fgTrm = (DataContext as FederalGazetteTreeViewModel);

                if (fgTrm != null
                    && fgTrm.PresentationTree != null) {
                    new DlgProgress(UIHelpers.TryFindParent<Window>(this)) {
                        ProgressInfo = {
                            IsIndeterminate = true,
                            Caption = eBalanceKitResources.Localisation.ResourcesCommon.LoadingPositions
                        }
                    }.ExecuteModal(fgTrm.CollapseAllNodes);
                }
            }
        }

        /// <summary>
        /// Expand all node in the treeView.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExpandAllNodes_Click(object sender, RoutedEventArgs e) {
            if (DataContext != null) {
                FederalGazetteTreeViewModel fgTrm = (DataContext as FederalGazetteTreeViewModel);

                if (fgTrm != null
                    && fgTrm.PresentationTree != null) {
                    new DlgProgress(UIHelpers.TryFindParent<Window>(this)) {
                        ProgressInfo = {
                            IsIndeterminate = true,
                            Caption = eBalanceKitResources.Localisation.ResourcesCommon.LoadingPositions
                        }
                    }.ExecuteModal(fgTrm.ExpandAllNodes);
                }
            }
        }
    }
}