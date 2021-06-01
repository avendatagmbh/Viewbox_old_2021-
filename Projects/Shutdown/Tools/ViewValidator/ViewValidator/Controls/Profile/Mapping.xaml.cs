using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using AvdWpfControls;
using ViewValidator.Models.Profile;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidator.Controls.Profile {
    /// <summary>
    /// Interaktionslogik für Mapping.xaml
    /// </summary>
    public partial class CtlMapping : UserControl {

        #region Model
        private ColumnMappingModel Model { get { return this.DataContext as ColumnMappingModel; } }
        #endregion

        public CtlMapping() {
            InitializeComponent();
        }

        private const string DRAG = "DRAG";

        private void lbSource_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                ListBoxItem listBoxItem = UIHelpers.FindAnchestor<ListBoxItem>((DependencyObject)e.OriginalSource);
                if (listBoxItem == null) return;
                DragDrop.DoDragDrop(listBoxItem, new DataObject(DRAG, listBoxItem.Content), DragDropEffects.Move);
            }
        }

        private void lbDestination_DragEnter(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DRAG) || sender == e.Source) {
                e.Effects = DragDropEffects.None;
            }
        }

        private void lbDestination_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DRAG)) {
                string source = e.Data.GetData(DRAG) as string;
                ListBoxItem dest = UIHelpers.FindAnchestor<ListBoxItem>((DependencyObject)e.OriginalSource);
                if (dest == null) return;

                ColumnMapping newMapping = new ColumnMapping(source, dest.Content.ToString());
                if (Model.ContainsMapping(newMapping))
                    MessageBox.Show("Bereits vorhanden");
                else
                    Model.AddMapping(newMapping);
                //Model.ObsSource.Remove(source);
                //Model.ObsDestination.Remove(dest.Content.ToString());                
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            Button btn = (Button)sender;
            ColumnMapping map = (ColumnMapping)btn.DataContext;
            Model.RemoveMapping(map);
            //Model.ObsSource.Add(map.Source);
            //Model.ObsDestination.Add(map.Destination);
        }




        #region Helper 
 
        private static T FindVisualParent<T>(UIElement element) where T : UIElement 
        { 
            UIElement parent = element; 
            while (parent != null) 
            { 
                T correctlyTyped = parent as T; 
                if (correctlyTyped != null) 
                { 
                    return correctlyTyped; 
                } 
 
                parent = VisualTreeHelper.GetParent(parent) as UIElement; 
            } 
 
            return null; 
        } 
 
        #endregion 

        #region Drag and Drop
        private ColumnMapping _targetMapping; 

        private void dgvMapping_MouseMove(object sender, MouseEventArgs e) {
            // This is what we're using as a cue to start a drag, but this can be 
            // customized as needed for an application. 
            if (e.LeftButton == MouseButtonState.Pressed) {
                // Find the row and only drag it if it is already selected. 
                DataGridRow row = FindVisualParent<DataGridRow>(e.OriginalSource as FrameworkElement);
                if ((row != null) && row.IsSelected) {
                    // Perform the drag operation 
                    ColumnMapping selectedMapping = (ColumnMapping)row.Item;
                    DragDropEffects finalDropEffect = DragDrop.DoDragDrop(row, selectedMapping, DragDropEffects.Move);
                    if ((finalDropEffect == DragDropEffects.Move) && (_targetMapping != null)) {
                        // A Move drop was accepted 

                        // Determine the index of the item being dragged and the drop 
                        // location. If they are difference, then move the selected 
                        // item to the new location. 
                        
                        int oldIndex = Model.Mapping.IndexOf(selectedMapping);
                        int newIndex = Model.Mapping.IndexOf(_targetMapping);
                        if (oldIndex != newIndex) {
                            Model.Mapping.Move(oldIndex, newIndex);
                        }

                        _targetMapping = null;
                    }
                }
            } 
        }

        private void dgvMapping_Drop(object sender, DragEventArgs e) {
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
        #endregion

        
        private void AddForAdditionalMapping_Click(object sender, RoutedEventArgs e) {
            if (_lastClickedCell == null || !(_lastClickedCell.Content is TextBlock)) {
                MessageBox.Show("Keine korrekte Zelle ausgewählt.");
                return;
            }
            string text = ((TextBlock) (_lastClickedCell.Content)).Text;
            if (_lastClickedCell.Column.Header.ToString() == "Verprobung") {
                Model.ObsSource.Add(text);
            } else if (_lastClickedCell.Column.Header.ToString() == "View") {
                Model.ObsDestination.Add(text);
            }
        }

        private DataGridCell _lastClickedCell;
        private void dgvMapping_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
 
            // iteratively traverse the visual tree
            while ((dep != null) &&
                    !(dep is DataGridCell) &&
                    !(dep is DataGridColumnHeader))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
 
            if (dep == null)
                return;
 
            //if (dep is DataGridColumnHeader)
            //{
            //    DataGridColumnHeader columnHeader = dep as DataGridColumnHeader;
            //    // do something
            //}
 
            if (dep is DataGridCell)
            {
                _lastClickedCell = dep as DataGridCell;
            }

        }

        private void btnChangeVisibility_Click(object sender, RoutedEventArgs e) {
            Button btn = (Button)sender;
            ColumnMapping mapping = (ColumnMapping)btn.DataContext;
            mapping.IsVisible = !mapping.IsVisible;
        }

    }
}
