// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-02-01
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;

namespace TransDATA.ResourceDictionaries {
    public partial class AppResources {
        private void TableItemChecked(object sender, RoutedEventArgs e) {
            var checkbox = sender as CheckBox;
            var listBox = UIHelpers.TryFindParent<ListBox>(checkbox);
            var selectedProfileModel = listBox.DataContext as TransDATA.Models.SelectedProfileModel;
            if (selectedProfileModel != null) selectedProfileModel.TableSelectionChanged();
        }

        private void TableItemUnchecked(object sender, RoutedEventArgs e) {
            var checkbox = sender as CheckBox;
            var listBox = UIHelpers.TryFindParent<ListBox>(checkbox);
            var selectedProfileModel = listBox.DataContext as TransDATA.Models.SelectedProfileModel;
            if (selectedProfileModel != null) selectedProfileModel.TableSelectionChanged();
        }

        private void ColumnItemChecked(object sender, RoutedEventArgs e) {
            var checkbox = sender as CheckBox;
            var listBox = UIHelpers.TryFindParent<ListBox>(checkbox);
            var selectedProfileModel = listBox.DataContext as TransDATA.Models.SelectedProfileModel;
            if (selectedProfileModel != null) selectedProfileModel.ColumnSelectionChanged();
        }

        private void ColumnItemUnchecked(object sender, RoutedEventArgs e) {
            var checkbox = sender as CheckBox;
            var listBox = UIHelpers.TryFindParent<ListBox>(checkbox);
            var selectedProfileModel = listBox.DataContext as TransDATA.Models.SelectedProfileModel;
            if (selectedProfileModel != null) selectedProfileModel.ColumnSelectionChanged();
        }
    }
}