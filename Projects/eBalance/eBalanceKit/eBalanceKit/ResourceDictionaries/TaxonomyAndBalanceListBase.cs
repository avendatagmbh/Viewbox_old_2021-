using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using AvdWpfControls;
using Taxonomy.PresentationTree;
using eBalanceKitBusiness;
using eBalanceKit.Windows;
using eBalanceKit.Windows.BalanceList;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Manager;

namespace eBalanceKit.ResourceDictionaries {
    partial class TaxonomyAndBalanceListBase {
        private void btnCreateSplitAccountGroup_Click(object sender, RoutedEventArgs e) {


            var btn = sender as Button;
            var parent = UIHelpers.TryFindParent<Window>(btn);
            var account = btn.DataContext as IAccount;
            if (!RightManager.HasDocumentRestWriteRight(account.BalanceList.Document, parent))
                return;

            var btnPos = btn.PointToScreen(new Point(0, 0));
            var sag = account.CreateSplitAccountGroup();
            DlgSplitAccount dlg = new DlgSplitAccount(parent) { DataContext = sag };

            //ComputeDlgSplitAccountGroupPos(btnPos, dlg);

            bool? result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true) {
                sag.Save();
                account.BalanceList.AddSplitAccountGroup(sag);
            }
        }

        private void btnDeleteSplitAccountGroup_Click(object sender, RoutedEventArgs e) {
            var btn = sender as Button;
            var sag = btn.DataContext as ISplitAccountGroup;
            var parent = UIHelpers.TryFindParent<Window>(btn);
            if (!RightManager.HasDocumentRestWriteRight(sag.BalanceList.Document, parent))
                return;

            string msg =
                "Möchten Sie die Aufteilung für das Konto \"" + sag.Account.Label + "\" wirklich Löschen?";
            if (MessageBox.Show(parent, msg, "Aufteilung Löschen?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes) {
                sag.BalanceList.RemoveSplitAccountGroup(sag);
            }
        }

        private void btnEditSplitAccountGroup_Click(object sender, RoutedEventArgs e) {
            var btn = sender as Button;
            var parent = UIHelpers.TryFindParent<Window>(btn);
            var sag = btn.DataContext as ISplitAccountGroup;
            if (!RightManager.HasDocumentRestWriteRight(sag.BalanceList.Document, parent))
                return;
            var btnPos = btn.PointToScreen(new Point(0, 0));
            DlgSplitAccount dlg = new DlgSplitAccount(parent);
            var clone = sag.Clone();
            dlg.DataContext = clone;

            //ComputeDlgSplitAccountGroupPos(btnPos, dlg);

            bool? result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true) {
                sag.Update(clone);
                sag.Save();
            }
        }

        private void BalanceListAssignedAccountText_Clicked(object sender, RoutedEventArgs e) {
            var border = sender as Border;
            var owner = UIHelpers.TryFindParent<MainWindow>(border);
            var model = owner.Model;
            var ptrees = model.CurrentDocument.GaapPresentationTrees;
            
            var account = border.DataContext as IBalanceListEntry;
            var id = account.AssignedElement.Id;

            foreach (var ptree in ptrees.Values) {
                ptree.UnselectAll();
            }

            account.IsSelected = true;

            foreach (var ptree in ptrees.Values) {
                if (ptree.HasNode(id)) {
                    var node = ptree.GetNode(id);
                    (node as IPresentationTreeNode).IsSelected = true;
                    ptree.ExpandSelected(node as IPresentationTreeNode);
                }
            }
        }

        private void AccountGroupDoubleClick(object sender, RoutedEventArgs e) {
            var ctl = sender as ClickableControl;
            var parent = UIHelpers.TryFindParent<Window>(ctl);
            var group = ctl.DataContext as IAccountGroup;
            var btnPos = ctl.PointToScreen(new Point(0, 0));
            DlgGroupAccount dlg = new DlgGroupAccount { Owner = parent };
            var clone = group.Clone();
            dlg.DataContext = clone;

            ComputeDlgAccountGroupPos(btnPos, dlg);

            bool? result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true) {
                group.DoDbUpdate = false;
                group.Update(clone);
                group.DoDbUpdate = true;
                group.Save();
            }
        }

        private static void ComputeDlgAccountGroupPos(Point pos, DlgGroupAccount dlg) {
            var h = SystemParameters.PrimaryScreenHeight;
            var w = SystemParameters.PrimaryScreenWidth;

            if (pos.Y + dlg.Height > h) dlg.Top = Math.Max(0, pos.Y - dlg.Height + 30);
            else dlg.Top = pos.Y;

            if (pos.X + dlg.Width > w) dlg.Left = Math.Max(0, pos.X - dlg.Width + 30);
            else dlg.Left = pos.X;
        }

    }
}
