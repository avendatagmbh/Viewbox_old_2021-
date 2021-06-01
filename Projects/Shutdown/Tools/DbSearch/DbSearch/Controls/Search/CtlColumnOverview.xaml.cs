// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-10-17
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DbSearch.Models.Search;
using DbSearch.Structures.Results;
using DbSearchLogic.Structures.TableRelated;
using Utils;

namespace DbSearch.Controls.Search {
    /// <summary>
    /// Interaktionslogik für CtlColumnOverview.xaml
    /// </summary>
    public partial class CtlColumnOverview : UserControl {
        public CtlColumnOverview() {
            InitializeComponent();
        }

        ColumnOverviewModel Model { get { return DataContext as ColumnOverviewModel; } }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            Column column = ((Button)sender).DataContext as Column;
            if (column == null) return;
            column.IsUsedInSearch = !column.IsUsedInSearch;
        }

        private void btnChangeVisibility_Click(object sender, RoutedEventArgs e) {
            Column column = ((Button) sender).DataContext as Column;
            if (column == null) return;
            column.IsVisible = !column.IsVisible;
        }

        private void miCopyColumn_Click(object sender, RoutedEventArgs e) {
            DataGridRow row = UIHelpers.TryFindFromPoint<DataGridRow>(dgColumnOverview, _lastClick);
            if (row == null) return;
            Column column = row.Item as Column;
            Model.DuplicateColumn(column);
        }

        private Point _lastClick;
        private void dgColumnOverview_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            _lastClick = e.GetPosition(dgColumnOverview);

        }
    }
}
