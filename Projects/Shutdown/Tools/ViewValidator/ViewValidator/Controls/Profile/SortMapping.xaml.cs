using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ViewValidator.Models.Profile;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidator.Controls.Profile {
    /// <summary>
    /// Interaktionslogik für SortMapping.xaml
    /// </summary>
    public partial class SortMapping : UserControl {

        #region Model
        private ColumnMappingModel Model { get { return this.DataContext as ColumnMappingModel; } }
        #endregion

        public SortMapping() {
            InitializeComponent();
        }

        private const string DRAG = "DRAG";

        private void dgvColumnMapping_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                if (dgvColumnMapping.SelectedItem != null)
                    DragDrop.DoDragDrop(dgvColumnMapping, new DataObject(DRAG, dgvColumnMapping.SelectedItem), DragDropEffects.Move);
            }
        }

        private void dgvColumnSort_DragEnter(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DRAG) || sender == e.Source) {
                e.Effects = DragDropEffects.None;
            }
        }

        private void AddSortMapping(ColumnMapping mapping) {
            if (mapping == null) return;
            if (Model.HasSortMapping(mapping)) {
                MessageBox.Show("Bereits vorhanden");
            } else {
                Model.Sort.Add(mapping);
            }
        }

        private void dgvColumnSort_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DRAG)) {
                ColumnMapping source = e.Data.GetData(DRAG) as ColumnMapping;
                AddSortMapping(source);
            } else {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                
                // Verify that this is a valid drop and then store the drop target 
                DataGridRow row = FindVisualParent<DataGridRow>(e.OriginalSource as UIElement);
                if (row != null) {
                    _targetMapping = row.Item as ColumnMapping;
                    if (_targetMapping != null) {
                        e.Effects = DragDropEffects.Move;
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            Button btn = (Button)sender;
            ColumnMapping map = (ColumnMapping)btn.DataContext;
            Model.Sort.Remove(map);
            //Model.AddMapping(map);
        }

        ColumnMapping _targetMapping;
        private void dgvColumnSort_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                DataGridRow row = FindVisualParent<DataGridRow>(e.OriginalSource as FrameworkElement);
                if ((row != null) && row.IsSelected) {
                    // Perform the drag operation 
                    ColumnMapping selectedMapping = (ColumnMapping)row.Item;
                    DragDropEffects finalDropEffect = DragDrop.DoDragDrop(row, selectedMapping, DragDropEffects.Move);
                    if ((finalDropEffect == DragDropEffects.Move) && (_targetMapping != null)) {
                        int oldIndex = Model.Sort.IndexOf(selectedMapping);
                        int newIndex = Model.Sort.IndexOf(_targetMapping);
                        if (oldIndex != newIndex) {
                            Model.Sort.Move(oldIndex, newIndex);
                        }

                        _targetMapping = null;
                    }
                }
            } 

        }
        #region Helper

        private static T FindVisualParent<T>(UIElement element) where T : UIElement {
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

        #endregion

        private void dgvColumnMapping_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            DependencyObject source = (DependencyObject)e.OriginalSource;
            var row = UIHelpers.TryFindParent<DataGridRow>(source);
            if (row == null) return;
            AddSortMapping(row.Item as ColumnMapping);
        }
    }
}
