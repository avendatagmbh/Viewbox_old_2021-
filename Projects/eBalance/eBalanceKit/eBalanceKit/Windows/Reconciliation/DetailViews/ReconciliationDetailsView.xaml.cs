// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-04-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKit.Windows.Reconciliation.DragDropHelperItems;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;

namespace eBalanceKit.Windows.Reconciliation.DetailViews {
    public partial class ReconciliationDetailsView {
        public ReconciliationDetailsView() { InitializeComponent(); }

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

                    // check for drop in source position
                    {
                        var subItem = UIHelpers.TryFindFromPoint<ReclassificationSource>(this, e.GetPosition(this));
                        if (subItem != null) {
                            reclassification.SourceElement = data.Item;
                            return;
                        }
                    }

                    // check for drop in destination position
                    {
                        var subItem = UIHelpers.TryFindFromPoint<ReclassificationDest>(this, e.GetPosition(this));
                        if (subItem != null) {
                            reclassification.DestinationElement = data.Item;
                            return;
                        }
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

                string result;
                if (!reconciliation.IsAssignmentAllowed(data.Item, out result)) {
                    e.Effects = DragDropEffects.None;
                    data.Message = result;
                    data.AllowDrop = false;
                    e.Handled = true;
                    return;
                }

                if (reconciliation is IReclassification) {
                    #region reclassification

                    // check for drop in source position
                    {
                        var subItem = UIHelpers.TryFindFromPoint<ReclassificationSource>(this, e.GetPosition(this));
                        if (subItem != null) {
                            e.Effects = DragDropEffects.Move;
                            data.AllowDrop = true;
                            e.Handled = true;
                            return;
                        }
                    }

                    // check for drop in destination position
                    {
                        var subItem = UIHelpers.TryFindFromPoint<ReclassificationDest>(this, e.GetPosition(this));
                        if (subItem != null) {
                            e.Effects = DragDropEffects.Move;
                            data.AllowDrop = true;
                            e.Handled = true;
                            return;
                        }
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