using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using eBalanceKit.Windows.Reconciliation.DragDropHelperItems;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation {
    /// <summary>
    /// Interaction logic for CtlReconciliationDetailsView.xaml
    /// </summary>
    public partial class CtlReconciliationDetailsView : UserControl {
        public CtlReconciliationDetailsView() {
            InitializeComponent();
        }
        
        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Handled) return;

            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = UIElement.MouseWheelEvent, Source = sender };
            var parent = ((Control)sender).Parent as UIElement;
            if (parent == null) return;
            parent.RaiseEvent(eventArg);
        }


        #region ListBoxDrop
        private void ControlDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent("DragDropData")) {
                var data = e.Data.GetData("DragDropData") as DragDropData;
                if (data == null) return;

                var reconciliation = (IReconciliation)DataContext;
                if (reconciliation is IReclassification) {
                    #region reclassification
                    var reclassification = reconciliation as IReclassification;
                    
                    // check for drop in source or destination position
                    object subItemSource = UIHelpers.TryFindFromPoint<ReclassificationSource>(this, e.GetPosition(this));
                    object subItemDestination = UIHelpers.TryFindFromPoint<ReclassificationDest>(this, e.GetPosition(this));

                    // if destination is not ReclassificationSource and ReclassificationDest then fallback to Drag&Drop helper area
                    if (subItemSource == null && subItemDestination == null) {
                        subItemSource = UIHelpers.TryFindFromPoint<Border>(this, e.GetPosition(this));
                        if (subItemSource != null && (subItemSource as Border).Name != "ReclassificationSourceHelper") subItemSource = null;
                    }
                    // if destination is not ReclassificationSource and ReclassificationDest then fallback to Drag&Drop helper area
                    if (subItemSource == null && subItemDestination == null) {
                        subItemDestination = UIHelpers.TryFindFromPoint<Border>(this, e.GetPosition(this));
                        if (subItemDestination != null && (subItemDestination as Border).Name != "ReclassificationDestinationHelper") subItemDestination = null;
                    }

                    if (subItemSource != null) {
                        reclassification.SourceElement = data.Item;
                        return;
                    }
                    if (subItemDestination != null) {
                        reclassification.DestinationElement = data.Item;
                        return;
                    }

                    #endregion // reclassification

                } else if (reconciliation is IValueChange) {
                    #region value change
                    var delta = reconciliation as IDeltaReconciliation;
                    var transaction = UIHelpers.TryFindFromPoint<DeltaReconciliationItem>(this, e.GetPosition(this));
                    if (transaction != null) {
                        var t = (IReconciliationTransaction)transaction.DataContext;
                        t.Position = data.Item;
                    } else {
                        delta.AddTransaction(data.Item);
                    }

                    #endregion value change

                } else if (reconciliation is IDeltaReconciliation) {
                    #region delta
                    var delta = reconciliation as IDeltaReconciliation;
                    var transaction = UIHelpers.TryFindFromPoint<DeltaReconciliationItem>(this, e.GetPosition(this));
                    if (transaction != null) {
                        var t = (IReconciliationTransaction)transaction.DataContext;
                        t.Position = data.Item;
                    } else {
                        delta.AddTransaction(data.Item);
                    }

                    #endregion delta
                }
            }
        }
        #endregion // ListBoxDrop

        #region ListBoxDragOver
        private void ControlDragOver(object sender, DragEventArgs e) {
            if (DataContext == null /* no reconciliation selected */ || !e.Data.GetDataPresent("DragDropData")) {
                e.Effects = DragDropEffects.None;
            } else {
                var data = e.Data.GetData("DragDropData") as DragDropData;
                if (data == null) return;

                data.ShowReplacePositionSign = false;
                data.ShowAddSign = false;
                data.Message = null;

                var reconciliation = (IReconciliation)DataContext;

                bool isNonModifiableReconciliation = reconciliation is IImportedValues || reconciliation is IPreviousYearValues;

                string result;
                if (!reconciliation.IsAssignmentAllowed(data.Item, out result) || isNonModifiableReconciliation) {
                    e.Effects = DragDropEffects.None;
                    if (isNonModifiableReconciliation)
                        data.Message = ResourcesReconciliation.AssignmentNotPossibleInformationCaption;
                    else
                        data.Message = result;
                    data.AllowDrop = false;
                    e.Handled = true;
                    return;
                }

                if (reconciliation is IReclassification) {
                    #region reclassification
                    
                    // check for drop in source or destination position
                    object subItemSource = UIHelpers.TryFindFromPoint<ReclassificationSource>(this, e.GetPosition(this));
                    object subItemDestination = UIHelpers.TryFindFromPoint<ReclassificationDest>(this, e.GetPosition(this));

                    // if destination is not ReclassificationSource and ReclassificationDest then fallback to Drag&Drop helper area
                    if (subItemSource == null && subItemDestination == null) {
                        subItemSource = UIHelpers.TryFindFromPoint<Border>(this, e.GetPosition(this));
                        if (subItemSource != null && (subItemSource as Border).Name != "ReclassificationSourceHelper") subItemSource = null;
                    }
                    // if destination is not ReclassificationSource and ReclassificationDest then fallback to Drag&Drop helper area
                    if (subItemSource == null && subItemDestination == null) {
                        subItemDestination = UIHelpers.TryFindFromPoint<Border>(this, e.GetPosition(this));
                        if (subItemDestination != null && (subItemDestination as Border).Name != "ReclassificationDestinationHelper") subItemDestination = null;
                    }

                    if (subItemSource != null || subItemDestination != null) {
                        e.Effects = DragDropEffects.Move;
                        data.AllowDrop = true;
                        e.Handled = true;
                        return;
                    }

                    #endregion // reclassification

                } else if (reconciliation is IDeltaReconciliation) {
                    #region delta
                    var transaction = UIHelpers.TryFindFromPoint<DeltaReconciliationItem>(this, e.GetPosition(this));
                    if (transaction != null) {
                        e.Effects = DragDropEffects.Move;
                        data.ShowReplacePositionSign = true;
                        data.ShowAddSign = false;
                        data.Message = null;
                        data.AllowDrop = true;
                        e.Handled = true;
                    } else {
                        e.Effects = DragDropEffects.Move;
                        data.ShowReplacePositionSign = false;
                        data.ShowAddSign = true;
                        data.Message = null;
                        data.AllowDrop = true;
                        e.Handled = true;
                    }

                    return;
                    #endregion // delta
                }

                e.Effects = DragDropEffects.None;
                data.AllowDrop = false;
                e.Handled = true;
            }
        }
        #endregion // ListBoxDragOver
    }
}
