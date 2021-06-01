// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;

namespace eBalanceKit.Windows.Reconciliation {
    public partial class CtlReconciliations {
        public CtlReconciliations() {
            InitializeComponent();
        }

        public CtlReconciliations(string caption) {
            InitializeComponent();
            txtCaption.Text = caption;
        }

        private void CtlReconciliations_OnPreviewDragOver(object sender, DragEventArgs e) {
            if (!DragDropPopup.IsOpen) return;
            var popupSize = new Size(DragDropPopup.ActualWidth, DragDropPopup.ActualHeight);
            DragDropPopup.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);
        }

        private void CtlReconciliations_OnGiveFeedback(object sender, GiveFeedbackEventArgs e) {
            var data = DragDropPopup.DataContext as DragDropData;
            if (data == null) return;
            popupBorder.Opacity = data.AllowDrop ? 1.0 : 0.5;
            popupAddItemBorder.Visibility = data.ShowAddSign ? Visibility.Visible : Visibility.Collapsed;
            popupReplaceItemBorder.Visibility = data.ShowReplacePositionSign ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UserControl_DragOver(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent("DragDropData")) {
                e.Effects = DragDropEffects.None;
            } else {
                var data = e.Data.GetData("DragDropData") as DragDropData;
                if (data == null) return;
                data.AllowDrop = false;
                data.ShowReplacePositionSign = false;
                data.ShowAddSign = false;
                data.Message = null;
                e.Handled = true;
            }
        }
    }
}