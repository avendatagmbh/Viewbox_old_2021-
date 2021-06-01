using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AvdCommon.Rules;
using AvdCommon.Rules.Gui.DragDrop;
using AvdWpfControls;
using AvdWpfControls.Converters;
using DbAccess.Structures;
using DbSearch.Converter;
using DbSearch.Models.Search;
using DbSearchBase.Enums;
using DbSearchLogic.Structures.TableRelated;
using Utils;
using log4net;
using AV.Log;

namespace DbSearch.Controls.Search {


    /// <summary>
    /// Interaktionslogik für CtlSearchMatrix.xaml
    /// </summary>
    public partial class CtlSearchMatrix : UserControl {

        internal static ILog _log = LogHelper.GetLogger();

        #region RowSorter
        public class RowSorter : IComparer {
            public delegate int TwoArgDelegate(Row arg1, Row arg2);

            private TwoArgDelegate compareDelegate;
            private Query _query;

            public RowSorter(ListSortDirection direction, DataGridColumn column, Query query) {
                _query = query;
                int dir = (direction == ListSortDirection.Ascending) ? 1 : -1;

                //set a delegate to be called by IComparer.Compare
                int index = _query.DisplayIndexToIndex[CtlSearchMatrix.ConvertDisplayIndex(column.DisplayIndex)];

                switch (_query.Columns[index].DbColumnInfo.Type) {
                    case DbColumnTypes.DbNumeric:
                    case DbColumnTypes.DbInt:
                    case DbColumnTypes.DbBigInt:
                        compareDelegate = (a, b) => {
                            double d1, d2;
                            if (Double.TryParse(a.RowEntries[index].DisplayString, out d1) && Double.TryParse(b.RowEntries[index].DisplayString, out d2))
                                return d1.CompareTo(d2) * dir;
                            return a.RowEntries[index].DisplayString.CompareTo(
                                b.RowEntries[index].DisplayString) * dir;
                        };
                        break;
                    case DbColumnTypes.DbDate:
                    case DbColumnTypes.DbTime:
                    case DbColumnTypes.DbDateTime:
                        compareDelegate = (a, b) => {
                            DateTime d1, d2;
                            if (DateTime.TryParse(a.RowEntries[index].DisplayString, out d1) && DateTime.TryParse(b.RowEntries[index].DisplayString, out d2))
                                return d1.CompareTo(d2) * dir;
                            return a.RowEntries[index].DisplayString.CompareTo(
                                b.RowEntries[index].DisplayString) * dir;
                        };
                        break;
                    default:
                        compareDelegate = (a, b) => {
                            return a.RowEntries[index].DisplayString.CompareTo(
                                b.RowEntries[index].DisplayString) * dir;
                        };
                        break;
                }
            }
            int IComparer.Compare(object X, object Y) {
                return compareDelegate((Row) X, (Row) Y);
            }
        }
        #endregion RowSorter

        SearchModel Model { get { return DataContext as SearchModel; } }

        public CtlSearchMatrix() {
            InitializeComponent();
        }

        #region CreateColumns

        internal void CreateColumns() {
            dgvSearchMatrix.Columns.Clear();
            if (Model == null) return;

            int i = 0;
            //Create column with delete image to remove rows
            {
                DataGridTemplateColumn gridColumn = new DataGridTemplateColumn() {
                    Header = "" 
                };
                FrameworkElementFactory imageFactory = new FrameworkElementFactory(typeof(ImageButton));
                BitmapImage deleteImage = new BitmapImage(new Uri(@"pack://application:,,,/DbSearch;component/Resources/delete.png"));

                imageFactory.SetValue(ImageButton.ImageSourceProperty, deleteImage);
                RoutedEventHandler clickHandler = DeleteRow_Click;
                imageFactory.AddHandler(ImageButton.ClickEvent, clickHandler);

                DataTemplate cellTemplate = new DataTemplate();
                cellTemplate.VisualTree = imageFactory;
                gridColumn.CellTemplate = cellTemplate;

                dgvSearchMatrix.Columns.Add(gridColumn);
                
            }
            foreach (var column in Model.Query.Columns) {
                DataGridTemplateColumn gridColumn = new DataGridTemplateColumn();
                Style headerStyle = new Style(typeof(DataGridColumnHeader));
                headerStyle.Setters.Add(new Setter(TemplateProperty, Application.Current.TryFindResource("ColumnHeaderTemplate")));
                gridColumn.HeaderStyle = headerStyle;
                gridColumn.Header = new ColumnHeaderModel(column, Model.Query);
                
                gridColumn.SortMemberPath = string.Format("RowEntries[{0}].RuleDisplayString", i);
                gridColumn.Visibility = column.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                
                //Set cell template
                FrameworkElementFactory textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
                Binding b = new Binding(string.Format("RowEntries[{0}].RuleDisplayString", i));
                textBlockFactory.SetValue(TextBlock.TextProperty, b);
                textBlockFactory.SetValue(TextBlock.BackgroundProperty, new Binding(string.Format("RowEntries[{0}].Status", i)) { Converter = new StatusToBackgroundConverter() });
                textBlockFactory.SetValue(TextBlock.ForegroundProperty, Brushes.Black);
                textBlockFactory.SetValue(ToolTipProperty, new Binding(string.Format("RowEntries[{0}].DisplayString", i)));

                DataTemplate cellTemplate = new DataTemplate();
                cellTemplate.VisualTree = textBlockFactory;
                gridColumn.CellTemplate = cellTemplate;

                //Set editing template
                FrameworkElementFactory textBoxFactory = new FrameworkElementFactory(typeof(TextBox));
                textBoxFactory.SetValue(TextBox.TextProperty, new Binding(string.Format("RowEntries[{0}].EditedValue", i)){Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged});
                textBoxFactory.SetValue(TextBox.ForegroundProperty, Brushes.Black);
                //textBoxFactory.AddHandler(UIElement.IsVisibleChanged, );
                //textBoxFactory.AddHandler(TextBox.GotFocusEvent, new RoutedEventHandler(GotFucos));
                //textBoxFactory.AddHandler(TextBox.);
                
                
                DataTemplate cellEditingTemplate = new DataTemplate();
                
                cellEditingTemplate.VisualTree = textBoxFactory;
                gridColumn.CellEditingTemplate = cellEditingTemplate;

                column.PropertyChanged += column_PropertyChanged;

                dgvSearchMatrix.Columns.Add(gridColumn);
                ++i;
                
            }
            for (i = 1; i <= Model.Query.Columns.Count; ++i)
                dgvSearchMatrix.Columns[i].DisplayIndex = Model.Query.Columns[i-1].DisplayIndex+1;

        }

        #endregion CreateColumns

        #region EventHandler
        private void DeleteRow_Click(object sender, RoutedEventArgs e) {
            var row = UIHelpers.TryFindParent<DataGridRow>((DependencyObject) sender);

            if (row == null) return;
            Model.Query.RemoveRow((Row) row.Item);
        }


        void column_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "IsVisible") {
                Column column = sender as Column;
                dgvSearchMatrix.Columns[ConvertToColumnIndex(column.DisplayIndex)].Visibility = column.IsVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            CreateColumns();
        }

        #region Sorting and Reordering
        private void dgvSearchMatrix_Sorting(object sender, DataGridSortingEventArgs e) {
            e.Handled = true;   // prevent the built-in sort from sorting

            var column = e.Column;

            ListSortDirection direction = (column.SortDirection != ListSortDirection.Ascending) ?
                         ListSortDirection.Ascending : ListSortDirection.Descending;
            column.SortDirection = direction;

            ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(dgvSearchMatrix.ItemsSource);

            CtlSearchMatrix.RowSorter mySort = new CtlSearchMatrix.RowSorter(direction, column, Model.Query);
            lcv.CustomSort = mySort; 
        }

        private void dgvSearchMatrix_ColumnReordered(object sender, DataGridColumnEventArgs e) {
            if (e.Column.DisplayIndex == 0) {
                e.Column.DisplayIndex = _displayIndexBeforeReordering;
                return;
            }

            for (int i = 1; i < dgvSearchMatrix.Columns.Count; ++i) {
                int displayIndex = ConvertDisplayIndex(dgvSearchMatrix.Columns[i].DisplayIndex);

                //Console.WriteLine(i + ": " + displayIndex);
                Model.Query.DisplayIndexToIndex[displayIndex] = i - 1;
                Model.Query.Columns[ConvertDisplayIndex(i)].DisplayIndex = displayIndex;
            }
        }
        private int _displayIndexBeforeReordering;
        private void dgvSearchMatrix_ColumnReordering(object sender, DataGridColumnReorderingEventArgs e) {
            _displayIndexBeforeReordering = e.Column.DisplayIndex;

        }

        #endregion

        private void dgvSearchMatrix_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            //search the object hierarchy for a datagrid row
            DependencyObject source = (DependencyObject)e.OriginalSource;
            var cell = UIHelpers.TryFindParent<DataGridCell>(source);
            var dataGridRow = UIHelpers.TryFindParent<DataGridRow>(source);
            //the user did not click on a row
            if (cell == null || dataGridRow == null) return;
            Column column = ((ColumnHeaderModel)cell.Column.Header).Column;
            Row row = dataGridRow.Item as Row;
            RowEntry rowEntry = row.RowEntries[column.Query.IndexOfColumn(column)];           
            if (string.IsNullOrEmpty(rowEntry.EditedValue))
                rowEntry.EditedValue = rowEntry.DisplayString;
            cell.IsEditing = true;
            
            //cell.Focus();
            cell.IsSelected = true;
        }


        private void dgvSearchMatrix_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == Key.Delete) {

                //Check whether the user is in edit mode and do not delete rows then
                if (e.OriginalSource is TextBox) return;
                DataGridCell cell = e.OriginalSource as DataGridCell;
                if (cell != null && cell.IsEditing) return;

                if (MessageBox.Show(UIHelpers.TryFindParent<Window>(this), "Wollen Sie die markierten Zeilen wirklich löschen?", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.No)
                    return;

                List<Row> rowsToDelete = new List<Row>();
                for (int i = 0; i < dgvSearchMatrix.SelectedItems.Count; ++i) {
                    var row = dgvSearchMatrix.SelectedItems[i] as Row;
                    rowsToDelete.Add(row);
                }

                foreach (var row in rowsToDelete)
                    Model.Query.RemoveRow(row);

                e.Handled = true;
            }
        }
        #endregion EventHandler

        private static int ConvertDisplayIndex(int index) {
            return index - 1;
        }

        private int ConvertToColumnIndex(int displayIndex) {
            return Model.Query.DisplayIndexToIndex[displayIndex] + 1;
        }

        private DataGridCell _lastClickedCell;
        private void dgvSearchMatrix_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            // iteratively traverse the visual tree
            while ((dep != null) &&
                    !(dep is DataGridCell) &&
                    !(dep is DataGridColumnHeader)) {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
                return;


            if (dep is DataGridCell) {
                _lastClickedCell = dep as DataGridCell;
            }

        }

        private void miExcludeValue_Click(object sender, RoutedEventArgs e) {
            if (_lastClickedCell == null ) {
                MessageBox.Show("Keine korrekte Zelle ausgewählt.");
                return;
            }
            //string text = ((TextBlock) (_lastClickedCell.Content)).Text;
            //if (_lastClickedCell.Column.Header.ToString() == "Verprobung") {
            //}
        }

        private void dgvSearchMatrix_Drop(object sender, DragEventArgs e) {
            RuleListDragDropData data = e.Data.GetData(typeof(RuleListDragDropData)) as RuleListDragDropData;
            if (data != null) {
                DataGridCell lbi = UIHelpers.TryFindFromPoint<DataGridCell>(dgvSearchMatrix, e.GetPosition(dgvSearchMatrix));
                
                if (lbi != null) {
                    ColumnHeaderModel headerModel = lbi.Column.Header as ColumnHeaderModel;
                    if (headerModel == null) {
                        _log.Log(LogLevelEnum.Error, "Konnte den Drag/Drop nicht ausführen");
                        return;
                    }
                    headerModel.Column.RuleSet.AddRule((Rule) data.Rules[0].Clone());
                }

                e.Handled = true;
            }
        }

        private void dgvSearchMatrix_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e) {
            ContentPresenter cp = (ContentPresenter)e.EditingElement;

            TextBox destinationTextBox = VisualTreeHelper.GetChild(cp, 0) as TextBox;

            if (destinationTextBox != null) {
                destinationTextBox.Focus();
                destinationTextBox.SelectAll();
            }
            //Model.IsInEditMode = true;
        }

    }
}
