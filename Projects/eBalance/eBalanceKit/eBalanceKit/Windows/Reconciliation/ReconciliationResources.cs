// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-29
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AvdWpfControls;
using eBalanceKitBase.Structures;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Reconciliation;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation {
    public partial class ReconciliationResources {

        private void BtnAssignPositionClick(object sender, RoutedEventArgs e) {
            var btn = (Button) sender;

            var node = btn.DataContext as IReconciliationTreeNode;
            if (node == null) return;

            var model = (ReconciliationsModel) UIHelpers.TryFindParent<CtlReconciliations>(btn).DataContext;
            if (model == null) return;
            
            var reconciliation = model.SelectedReconciliation;
            if (reconciliation == null) return;

            string result;
            if (!reconciliation.IsAssignmentAllowed(node.Element, out result)) {
                if (result != null) MessageBox.Show(result, ResourcesReconciliation.AssignmentNotPossibleInformationCaption, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (reconciliation is IDeltaReconciliation) {
                var deltaReconciliation = reconciliation as IDeltaReconciliation;
                deltaReconciliation.AddTransaction(node.Element);            
            } 
        }

        private void BtnAssignSourcePositionClick(object sender, RoutedEventArgs e) {
            var btn = (Button)sender;

            var node = btn.DataContext as IReconciliationTreeNode;
            if (node == null) return;

            var model = (ReconciliationsModel)UIHelpers.TryFindParent<CtlReconciliations>(btn).DataContext;
            if (model == null) return;

            var reconciliation = model.SelectedReconciliation;
            if (reconciliation == null) return;
            
            string result;
            if (!reconciliation.IsAssignmentAllowed(node.Element, out result)) {
                if (result != null) MessageBox.Show(result, ResourcesReconciliation.AssignmentNotPossibleInformationCaption, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!(reconciliation is IReclassification)) return;
            var reclassification = reconciliation as IReclassification;
            reclassification.SourceElement = node.Element;
        }

        private void BtnAssignDestPositionClick(object sender, RoutedEventArgs e) {
            var btn = (Button)sender;

            var node = btn.DataContext as IReconciliationTreeNode;
            if (node == null) return;

            var model = (ReconciliationsModel)UIHelpers.TryFindParent<CtlReconciliations>(btn).DataContext;
            if (model == null) return;

            var reconciliation = model.SelectedReconciliation;
            if (reconciliation == null) return;
            
            string result;
            if (!reconciliation.IsAssignmentAllowed(node.Element, out result)) {
                if (result != null) MessageBox.Show(result, ResourcesReconciliation.AssignmentNotPossibleInformationCaption, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!(reconciliation is IReclassification)) return;
            var reclassification = reconciliation as IReclassification;
            reclassification.DestinationElement = node.Element;
        }

        private void BtnDeletePreviousYearValue_OnClick(object sender, RoutedEventArgs e) {
            var btn = sender as Button;
            if (btn == null) return;

            var node = btn.DataContext as IReconciliationTreeNode;
            if (node != null)
                if (MessageBox.Show(
                    string.Format(ResourcesReconciliation.RequestDeletePreviousYearValue, node.Element.Label),
                    ResourcesReconciliation.DeletePosition,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes) == MessageBoxResult.Yes) {
                    node.Value.ReconciliationInfo.TransferValueInputPreviousYear = null;
                }
        }

        private void BtnDeletePreviousYearCorrectionValue_OnClick(object sender, RoutedEventArgs e) {
            var btn = sender as Button;
            if (btn == null) return;

            var node = btn.DataContext as IReconciliationTreeNode;
            if (node != null)
                if (MessageBox.Show(
                    string.Format(ResourcesReconciliation.RequestDeletePreviousYearCorrectionValue, node.Element.Label),
                    ResourcesReconciliation.DeletePosition,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes) == MessageBoxResult.Yes) {
                    node.Value.ReconciliationInfo.TransferValueInputPreviousYearCorrection = null;
                }
        }

        private void BtnDeleteReconciliation_OnClick(object sender, RoutedEventArgs e) {
            var btn = sender as Button;
            if (btn == null) return;

            var transaction = btn.DataContext as IReconciliationTransaction;
            if (transaction == null) return;

            switch (transaction.Reconciliation.ReconciliationType) {
                case ReconciliationTypes.AuditCorrectionPreviousYear:
                case ReconciliationTypes.PreviousYearValues:
                    if (MessageBox.Show(
                        string.Format(
                            transaction.Reconciliation.ReconciliationType == ReconciliationTypes.PreviousYearValues ? 
                            ResourcesReconciliation.RequestDeletePreviousYearValue : 
                            ResourcesReconciliation.RequestDeletePreviousYearCorrectionValue,
                            transaction.Position.Label),
                        ResourcesReconciliation.DeletePosition,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.Yes) == MessageBoxResult.Yes) {

                            switch (transaction.Reconciliation.ReconciliationType) {
                                case ReconciliationTypes.ImportedValues:
                                case ReconciliationTypes.Reclassification:
                                case ReconciliationTypes.ValueChange:
                                case ReconciliationTypes.Delta:
                                case ReconciliationTypes.TaxBalanceValue:
                                case ReconciliationTypes.AuditCorrection:
                                    // nothing to do
                                    break;
                                
                                case ReconciliationTypes.PreviousYearValues:
                                    transaction.ReconciliationInfo.TransferValueInputPreviousYear = null;
                                    break;
                                
                                case ReconciliationTypes.AuditCorrectionPreviousYear:
                                    transaction.ReconciliationInfo.TransferValueInputPreviousYearCorrection = null;
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        
                    }
                    break;

                case ReconciliationTypes.ImportedValues:
                    MessageBox.Show(
                        string.Format(ResourcesReconciliation.RequestDeleteImportedValueNotAllowed),
                        ResourcesReconciliation.DeletePosition,
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation,
                        MessageBoxResult.Yes);
                    break;

                case ReconciliationTypes.Reclassification:
                case ReconciliationTypes.ValueChange:
                case ReconciliationTypes.AuditCorrection:
                case ReconciliationTypes.TaxBalanceValue:
                case ReconciliationTypes.Delta:
                    if (MessageBox.Show(
                        string.Format(ResourcesReconciliation.RequestDeletePosition, transaction.Position.Label),
                        ResourcesReconciliation.DeletePosition,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.Yes) == MessageBoxResult.Yes) {
                        transaction.Remove();
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ClickableControlMouseClick(object sender, RoutedEventArgs e) {
            var control = (ClickableControl) sender;
            var transaction = (IReconciliationTransaction)control.DataContext;

            var parent = UIHelpers.TryFindParent<CtlReconciliations>(control);
            if (parent == null) return;
            var model = (ReconciliationsModel) parent.DataContext;
            
            if (model.SelectedReconciliation != null)
                model.SelectedReconciliation.IsSelected = false;
            
            transaction.Reconciliation.IsSelected = true;
            transaction.IsSelected = true;
        }

        private void ChkSelectedChanged(object sender, RoutedEventArgs e) {
            CheckBox chk = (CheckBox) sender;

            // get data context as IReconciliationTreeNode
            IReconciliationTreeNode node = chk.DataContext as IReconciliationTreeNode;
            if (node == null) return;

            // return if already set by walking through children
            if (node.IsAssignedToReferenceList == (chk.IsChecked ?? false)) return;

            node.IsAssignedToReferenceList = chk.IsChecked ?? false;

            // get the ReconciliationsModel
            ReconciliationsModel model = (ReconciliationsModel) UIHelpers.TryFindParent<CtlReferenceList>(chk).DataContext;
            if (model == null) return;

            int elementId = model.Document.TaxonomyIdManager.GetId(node.Element.Id);
            // save selection changes
            if (node.IsAssignedToReferenceList) {
                //model.Document.AddItemToReferenceList(new ReferenceListItem(elementId));

                DlgReferenceList owner = UIHelpers.TryFindParent<DlgReferenceList>(chk);
                new DlgProgress(owner) {
                    ProgressInfo = {IsIndeterminate = true, Caption = ResourcesCommon.LoadingPositions}
                }.ExecuteModal(() => {
                    SelectAllChildren(node, model);
                    owner.Dispatcher.Invoke(
                        DispatcherPriority.Send,
                        new Action(
                            () => { UIHelpers.TryFindParent<CtlReferenceListTreeView>(chk).ExpandAllSelected(node); }));
                });
                model.Document.AddItemToReferenceList(new ReferenceListItem(elementId));
            } else {
                //model.Document.RemoveItemFromReferenceList(new ReferenceListItem(elementId));

                new DlgProgress(UIHelpers.TryFindParent<DlgReferenceList>(chk)) {
                    ProgressInfo = {IsIndeterminate = true, Caption = ResourcesCommon.LoadingPositions}
                }.ExecuteModal(() => UnSelectAllChildren(node, model));
                model.Document.RemoveItemFromReferenceList(new ReferenceListItem(elementId));
            }
        }

        private void SelectAllChildren(IReconciliationTreeNode node, ReconciliationsModel model) {
            if (node != null) {
                foreach (IReconciliationTreeNode child in node.Children.OfType<IReconciliationTreeNode>()) {
                    SelectAllChildren(child, model);
                    int elementId = model.Document.TaxonomyIdManager.GetId(child.Element.Id);
                    model.Document.AddItemToReferenceList(new ReferenceListItem(elementId));
                    child.IsAssignedToReferenceList = true;
                }
            }
        }

        private void UnSelectAllChildren(IReconciliationTreeNode node, ReconciliationsModel model) {
            if (node != null) {
                foreach (IReconciliationTreeNode child in node.Children.OfType<IReconciliationTreeNode>()) {
                    UnSelectAllChildren(child, model);
                    int elementId = model.Document.TaxonomyIdManager.GetId(child.Element.Id);
                    model.Document.RemoveItemFromReferenceList(new ReferenceListItem(elementId));
                    child.IsAssignedToReferenceList = false;
                }
            }
        }
    }
}