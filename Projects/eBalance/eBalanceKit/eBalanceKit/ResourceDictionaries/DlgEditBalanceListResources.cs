using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using eBalanceKitBusiness;
using eBalanceKit.Windows;

namespace eBalanceKit.ResourceDictionaries {
    partial class DlgEditBalanceListResources {

        private void btnDeleteAccountGroup_Click(object sender, RoutedEventArgs e) {
            var btn = sender as Button;
            var group = btn.DataContext as IAccountGroup;
            var parent = UIHelpers.TryFindParent<Window>(btn);

            string msg =
                "Möchten Sie die Kontogruppe \"" + group.Label + "\" wirklich Löschen?";
            if (MessageBox.Show(parent, msg, "Kontogruppe Löschen?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes) {
                group.BalanceList.RemoveAccountGroup(group);
            }
        }

        private void btnEditAccountGroup_Click(object sender, RoutedEventArgs e) {
            var btn = sender as Button;
            var parent = UIHelpers.TryFindParent<Window>(btn);
            var group = btn.DataContext as IAccountGroup;
            var btnPos = btn.PointToScreen(new Point(0, 0));
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
            var h = System.Windows.SystemParameters.PrimaryScreenHeight;
            var w = System.Windows.SystemParameters.PrimaryScreenWidth;

            if (pos.Y + dlg.Height > h) dlg.Top = Math.Max(0, pos.Y - dlg.Height + 30);
            else dlg.Top = pos.Y;

            if (pos.X + dlg.Width > w) dlg.Left = Math.Max(0, pos.X - dlg.Width + 30);
            else dlg.Left = pos.X;
        }

    }
}
