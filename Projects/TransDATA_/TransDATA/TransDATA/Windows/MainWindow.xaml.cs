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
using System.Windows.Shapes;
using TransDATA.Models;
using TransDATABusiness.ConfigDb.Tables;
using System.Collections.ObjectModel;

namespace TransDATA.Windows {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow() {
            InitializeComponent();
            
            _model = new MainWindowModel(this);
            this.DataContext = _model;
            chkIsTableCheckedHeader.DataContext = _model;
            chkIsColumnCheckedHeader.DataContext = _model;
            chkIsEmptyTableCheckedHeader.DataContext = _model;
        }

        private MainWindowModel _model;


        /// <summary>
        /// Handles the PreviewMouseLeftButtonDown event of the DataGridCell control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly) {
                if (!cell.IsFocused) {
                    cell.Focus();
                }
                DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                if (dataGrid != null) {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow) {
                        if (!cell.IsSelected)
                            cell.IsSelected = true;
                    } else {
                        DataGridRow row = FindVisualParent<DataGridRow>(cell);
                        if (row != null && !row.IsSelected) {
                            row.IsSelected = true;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Handles the PreviewMouseKeyDown event of the DataGridCell control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void DataGridCell_PreviewMouseKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Space) {
                DataGridCell cell = (DataGridCell)sender;
                chkIsTableCheckedHeader.IsChecked = null;
                DataGridRow row = FindVisualParent<DataGridRow>(cell);
                // TODO
                e.Handled = true;
            }
        }

        /// <summary>
        /// Finds the visual parent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        static T FindVisualParent<T>(UIElement element) where T : UIElement {
            UIElement parent = element;
            while (parent != null) {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null) {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        private void chkIsTableCheckedHeader_Checked(object sender, RoutedEventArgs e) {
            if (_model.CurrentProfile == null) return;
            foreach (TransDATABusiness.ConfigDb.Tables.Table tab in _model.CurrentProfile.Tables) {
                tab.IsChecked = true;
            }
        }

        /// <summary>
        /// Handles the Unchecked event of the chkIsTableCheckedHeader control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void chkIsTableCheckedHeader_Unchecked(object sender, RoutedEventArgs e) {
            if (_model.CurrentProfile == null) return;
            foreach (TransDATABusiness.ConfigDb.Tables.Table tab in _model.CurrentProfile.Tables) {
                tab.IsChecked = false;
            }
        }

        /// <summary>
        /// Handles the Click event of the chkIsChecked control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void chkIsTableChecked_Click(object sender, RoutedEventArgs e) {
            chkIsTableCheckedHeader.IsChecked = null;
        }

        /// <summary>
        /// Handles the Checked event of the chkIsColumnCheckedHeader control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void chkIsColumnCheckedHeader_Checked(object sender, RoutedEventArgs e) {
            if (dgColumns.ItemsSource == null) return;
            DataGrid dg = FindVisualParent<DataGrid>((CheckBox)sender);
            foreach (Column colm in (ObservableCollection<Column>)dg.ItemsSource) {
                colm.IsChecked = true;
            }
        }

        /// <summary>
        /// Handles the Unchecked event of the chkIsColumnCheckedHeader control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void chkIsColumnCheckedHeader_Unchecked(object sender, RoutedEventArgs e) {
            if (_model.CurrentProfile == null) return;
            DataGrid dg = FindVisualParent<DataGrid>((CheckBox)sender);
            foreach (Column colm in (ObservableCollection<Column>)dg.ItemsSource) {
                colm.IsChecked = false;
            }
        }

        /// <summary>
        /// Handles the Click event of the chkIsColumnChecked control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void chkIsColumnChecked_Click(object sender, RoutedEventArgs e) {
            chkIsColumnCheckedHeader.IsChecked = null;

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (_model.CurrentProfile == null) return;
            if ((sender as TabControl) == null) return;
            TabItem tbi = (TabItem)((sender as TabControl).SelectedItem);
            if (tbi == null) return;
            _model.GetIsTabChecked(tbi.Name);
        }

        /// <summary>
        /// Handles the Checked event of the chkIsEmptyTableCheckedHeader control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void chkIsEmptyTableCheckedHeader_Checked(object sender, RoutedEventArgs e) {
            if (_model.CurrentProfile == null) return;
            foreach (TransDATABusiness.ConfigDb.Tables.Table tab in _model.EmptyTables) {
                tab.IsChecked = true;
            }
        }

        /// <summary>
        /// Handles the Unchecked event of the chkIsEmptyTableCheckedHeader control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void chkIsEmptyTableCheckedHeader_Unchecked(object sender, RoutedEventArgs e) {
            if (_model.CurrentProfile == null) return;
            foreach (TransDATABusiness.ConfigDb.Tables.Table tab in _model.EmptyTables) {
                tab.IsChecked = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the chkIsEmptyTableChecked control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void chkIsEmptyTableChecked_Click(object sender, RoutedEventArgs e) {
            chkIsColumnCheckedHeader.IsChecked = null;
        }


        /// <summary>
        /// Handles the Click event of the btnStartExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStartExport_Click(object sender, RoutedEventArgs e) {
            // TODO
        }


        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            // TODO
        }


        /// <summary>
        /// Handles the Click event of the btnEditProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnEditProfile_Click(object sender, RoutedEventArgs e) {
            DlgEditProfile DlgEditProfile =new DlgEditProfile(_model.CurrentProfile);
            DlgEditProfile.Owner = this;
            DlgEditProfile.Ok += new EventHandler(DlgEditProfile_Ok);
            DlgEditProfile.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            DlgEditProfile.ShowDialog();
        }

        void DlgEditProfile_Ok(object sender, EventArgs e) {
            _model.CreateProfile();
            rdbAllIsChecked.IsChecked = true;
            tableControl.SelectedIndex = 0;
            _model.IsTabChecked = true;
        }
 
        /// <summary>
        /// Handles the Click event of the btnSelectProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectProfile_Click(object sender, RoutedEventArgs e) {
            // TODO
        }

        /// <summary>
        /// Handles the Click event of the btnSelectDataBase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectDataBase_Click(object sender, RoutedEventArgs e) {
            if (_model.CurrentProfile == null) return;
            DlgDatabase DlgDatabase = new DlgDatabase(_model.CurrentProfile.ConfigDatabase);
            DlgDatabase.Owner = this;
            DlgDatabase.Ok += new EventHandler(DlgDatabase_Ok);
            DlgDatabase.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            DlgDatabase.ShowDialog();

        }

        /// <summary>
        /// Handles the Ok event of the DlgDatabase control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void DlgDatabase_Ok(object sender, EventArgs e) {
            //if (_model.CurrentProfile.ConfigDatabase.DbName.Equals(((DlgDatabase)sender).ConfigDatabase.DbName)) {
            //    string message = "Datenbank '" + ((DlgDatabase)sender).ConfigDatabase.DbName + "' wird neu geladen. Alle bisherigen Änderungen gehen verloren.";
            //    MessageBox.Show(
            //    message,
            //    "Fehler",
            //    MessageBoxButton.OK,
            //    MessageBoxImage.Exclamation);
            //}
            _model.CreateProfile();
            _model.CurrentProfile.ConfigDatabase = ((DlgDatabase)sender).ConfigDatabase;

            rdbAllIsChecked.IsChecked = true;
            _model.IsTabChecked = true;
        }

        /// <summary>
        /// Handles the Closed event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Window_Closed(object sender, EventArgs e) {
            _model.AppConfig.Dispose();
        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e) {
            //_model.CreateProfile("Driver={MySQL ODBC 5.1 Driver};Server = chip; Database = viewbuilder_test; User = root;Password = avendata; Option=3");
            //txtSearchItem.Clear();
            //rdbAllIsChecked.IsChecked= true;
            //_model.IsTabChecked = true;
        }

        private void expander_Expanded(object sender, RoutedEventArgs e) {
            cdInfoPanel.Width = new GridLength(250);
        }

        private void expander_Collapsed(object sender, RoutedEventArgs e) {
            cdInfoPanel.Width = new GridLength(20);
        }

        private void RibbonButton_Click_1(object sender, RoutedEventArgs e) {

        }

        private void rdbAllIsChecked_Checked(object sender, RoutedEventArgs e) {
            if (_model.CurrentProfile == null) rdbAllIsChecked.IsChecked = false;
            txtSearchItem.Clear();
            _model.GetListTables("rdbAllIsChecked");

        }

        private void rdbSelectedIsChecked_Checked(object sender, RoutedEventArgs e) {
            if (_model.CurrentProfile == null) return;
            txtSearchItem.Clear();
            _model.SelectedTables.Clear();
            foreach (TransDATABusiness.ConfigDb.Tables.Table tab in _model.AllTables) {
                if (tab.IsChecked) {
                    _model.SelectedTables.Add(tab);
                }

            }
            _model.GetListTables("rdbSelectedIsChecked");
        }

        private void rdbDeselectedIsChecked_Checked(object sender, RoutedEventArgs e) {
            if (_model.CurrentProfile == null) return;
            txtSearchItem.Clear();
            _model.DeselectedTables.Clear();
            foreach (TransDATABusiness.ConfigDb.Tables.Table tab in _model.AllTables) {
                if (!tab.IsChecked) {
                    _model.DeselectedTables.Add(tab);
                }

            }
            _model.GetListTables("rdbDeselectedIsChecked");
        }

        private void rdbAllColumnsIsChecked_Checked(object sender, RoutedEventArgs e) {
            if (_model.CurrentProfile == null) rdbAllColumnsIsChecked.IsChecked = false;
            _model.GetListColumns("rdbAllColumnsIsChecked");
        }

        private void rdbSelectedColumnsIsChecked_Checked(object sender, RoutedEventArgs e) {
            if (_model.CurrentProfile == null) return;
            if (_model.Table == null) return;
            _model.SelectedColumns.Clear();
            foreach (Column colm in _model.Table.Columns) {
                if (colm.IsChecked) {
                    _model.SelectedColumns.Add(colm);
                }
            }
            _model.GetListColumns("rdbSelectedColumnsIsChecked");
        }

        private void rdbDeselectedColumnsIsChecked_Checked(object sender, RoutedEventArgs e) {
            if (_model.CurrentProfile == null) return;
            _model.DeselectedColumns.Clear();
            foreach (Column colm in _model.Table.Columns) {
                if (!colm.IsChecked) {
                    _model.DeselectedColumns.Add(colm);
                }
            }
            _model.GetListColumns("rdbDeselectedColumnsIsChecked");
        }

        private void dgTablescripts_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            _model.Table = (TransDATABusiness.ConfigDb.Tables.Table)dgTablescripts.SelectedItem;
            rdbAllColumnsIsChecked.IsChecked = true;
            _model.GetListColumns("rdbAllColumnsIsChecked");
        }

        private void dgTablescriptsEmpty_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            _model.Table = (TransDATABusiness.ConfigDb.Tables.Table)dgTablescriptsEmpty.SelectedItem;
            rdbAllColumnsIsChecked.IsChecked = true;
            _model.GetListColumns("rdbAllColumnsIsChecked");
        }

        private void txtSearchItem_TextChanged(object sender, TextChangedEventArgs e) {
            if (_model.CurrentProfile == null) return;
            char[] _ch = txtSearchItem.Text.ToCharArray();
            _model.GetSearch(_ch, _ch.Length);
        }
    }
}