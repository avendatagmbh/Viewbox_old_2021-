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
using System.Windows.Controls.Primitives;
using eBalanceKit.Models.Assistants;
using eBalanceKitBusiness.Import;

namespace eBalanceKit.Controls.BalanceList {
    /// <summary>
    /// Interaktionslogik für ImportDataPreview.xaml
    /// </summary>
    public partial class ImportDataPreview : UserControl {
        public ImportDataPreview() {
            InitializeComponent();
        }

        public event EventHandler ColumnSelected;
        private void OnColumnSelected() {
            if (ColumnSelected != null) ColumnSelected(this, new System.EventArgs());
        }
        
        public string SelectedColumn {
            get { return (string)GetValue(SelectedColumnProperty); }
            set { SetValue(SelectedColumnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColumn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColumnProperty =
            DependencyProperty.Register("SelectedColumn", typeof(string), typeof(ImportDataPreview), new UIPropertyMetadata(null));
        
        /// <summary>
        /// Handles the MouseDown event of the dgCsvData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void dgCsvData_MouseDown(object sender, MouseButtonEventArgs e) {
            Point p = e.GetPosition(dgCsvData);
            DataGridColumnHeader header = UIHelpers.TryFindFromPoint<DataGridColumnHeader>(dgCsvData, p);
            if (header != null) {
                this.SelectedColumn = header.Column.Header.ToString();
                OnColumnSelected();
            } else {
                DataGridCell cell = UIHelpers.TryFindFromPoint<DataGridCell>(dgCsvData, p);
                if (cell != null) {
                    this.SelectedColumn = cell.Column.Header.ToString();
                    OnColumnSelected();
                }
            }
        }

    }
}
