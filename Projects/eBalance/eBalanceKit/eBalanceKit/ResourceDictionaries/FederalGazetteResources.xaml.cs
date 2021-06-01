// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-10-29
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using Taxonomy.Enums;
using eBalanceKit.Windows.EditPresentationTreeDetails;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Interfaces.PresentationTree;

namespace eBalanceKit.ResourceDictionaries {
    public partial class FederalGazetteResources {
        
        private void BtnEditDetails_Click(object sender, RoutedEventArgs e) {
            var btn = (Button)sender;
            var node = btn.DataContext as IPresentationTreeNode;
            var owner = UIHelpers.TryFindParent<Window>(btn);
            if (node == null) return;

            if (node.IsHypercubeContainer) {
                // preload cube data
                var progress = new DlgProgress(owner) { ProgressInfo = { Caption = "Lade Tabelle...", IsIndeterminate = true } };
                progress.ExecuteModal(node.Document.GetHyperCube(node.Element.Id).PreloadItems);

                new DlgEditHyperCubeDetails(node) { Owner = owner }.ShowDialog();
                return;
            }

            switch (node.Element.ValueType) {
                case XbrlElementValueTypes.Abstract:
                case XbrlElementValueTypes.None:
                case XbrlElementValueTypes.SingleChoice:
                case XbrlElementValueTypes.MultipleChoice:
                case XbrlElementValueTypes.Tuple:
                    if (node.Element.IsList) {
                        new DlgEditListDetails(node) {
                            Owner = owner,
                            DataContext = new PresentationTreeDetailModel(node.Document, node.Value)
                        }.ShowDialog();
                    }
                    return;
                
                case XbrlElementValueTypes.Boolean:
                case XbrlElementValueTypes.Date:
                case XbrlElementValueTypes.Int:
                case XbrlElementValueTypes.Numeric:
                    // no detail editor implemented
                    return;

                case XbrlElementValueTypes.Monetary:
                    new DlgEditMonetaryDetails {
                        Owner = owner,
                        DataContext = new PresentationTreeDetailModel(node.Document, node.Value)
                    }.ShowDialog();
                    return;

                case XbrlElementValueTypes.String:
                    new DlgEditTextDetails {
                        Owner = owner,
                        DataContext = new PresentationTreeDetailModelRtf(node.Document, node.Value)
                    }.ShowDialog();
                    return;
            }
        }

        private void btnResetIconClick(object sender, RoutedEventArgs e) {
            Button btn = sender as Button;

            DependencyObject grid = System.Windows.Media.VisualTreeHelper.GetParent(btn);
            var element = System.Windows.Media.VisualTreeHelper.GetChild(grid, 0);
            var cb = element as ComboBox;
            if (cb != null) {
                cb.SelectedIndex = -1;
                return;
            }
            var dp = element as DatePicker;
            if (dp != null) {
                dp.SelectedDate = null;
            }
        } 
    }
}