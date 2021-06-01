using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using eBalanceKitBusiness.HyperCubes.Import;

namespace eBalanceKit.Windows.Import {
    /// <summary>
    /// Interaktionslogik für Ctl3dOrder.xaml
    /// </summary>
    public partial class Ctl3dOrder : UserControl {

        ObservableCollection<string> zoneList = new ObservableCollection<string>();
        ListBox dragSource = null; 


        public Ctl3dOrder() { InitializeComponent(); }


        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            ListBox parent = (ListBox)sender;
            dragSource = parent;
            object data = GetDataFromListBox(dragSource, e.GetPosition(parent));

            if (data != null) {
                DragDrop.DoDragDrop(parent, data, DragDropEffects.Move);
            }
        }

        private void ListBox_Drop(object sender, DragEventArgs e) {
            ListBox parent = (ListBox)sender;
            object data = e.Data.GetData(typeof(eBalanceKitBusiness.HyperCubes.Import.ImportConfig.DimensionInfo));
            ((IList)dragSource.ItemsSource).Remove(data);
            lbDimensionsAvailable.Items.Refresh(); //.Remove(data);
            parent.Items.Add(data);
        }

        #region GetDataFromListBox(ListBox,Point)
        private static object GetDataFromListBox(ListBox source, Point point) {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null) {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue) {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);

                    if (data == DependencyProperty.UnsetValue) {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }

                    if (element == source) {
                        return null;
                    }
                }

                if (data != DependencyProperty.UnsetValue) {
                    return data;
                }
            }

            return null;
        }
        #endregion

        private void lbDimensionsAvailable_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                if (lbDimensionsAvailable.SelectedIndex < 0) {
                    return;
                }
                lbDimensionsAvailable.Items.Remove(lbDimensionsAvailable.SelectedItem);
            }
        }

        private void btDown_Click(object sender, RoutedEventArgs e) {
            //var dims = (DataContext as ImportConfig).Dimensions;
            var currentItem = (DataContext as ImportConfig).Dimensions[lbDimensionsAvailable.SelectedIndex];
            if (lbDimensionsAvailable.SelectedIndex >= lbDimensionsAvailable.Items.Count - 1 || lbDimensionsAvailable.SelectedIndex < 0) {
                return;
            }
            currentItem.Index = lbDimensionsAvailable.SelectedIndex + 1;
            (DataContext as ImportConfig).Dimensions.Move(lbDimensionsAvailable.SelectedIndex, lbDimensionsAvailable.SelectedIndex + 1);

            //(DataContext as ImportConfig).Dimensions.Remove(currentItem);

            //lbDimensionsAvailable.Items.MoveCurrentToNext(); lbDimensionsAvailable.Items.Refresh();
            PrintOrder();
        }

        private void btUp_Click(object sender, RoutedEventArgs e) {
            //lbDimensionsAvailable.Items.MoveCurrentToPrevious();
            
            var oldIndex = lbDimensionsAvailable.SelectedIndex;
            if (lbDimensionsAvailable.SelectedIndex <= 0 ){ //>= lbDimensionsAvailable.Items.Count - 1) {
                return;
            }
            (DataContext as ImportConfig).Dimensions[oldIndex].Index = lbDimensionsAvailable.SelectedIndex - 1;
            
            (DataContext as ImportConfig).Dimensions.Move(lbDimensionsAvailable.SelectedIndex,
                                                          lbDimensionsAvailable.SelectedIndex - 1);
            PrintOrder();
        }

        private void btRemove_Click(object sender, RoutedEventArgs e) { (DataContext as ImportConfig).Dimensions.RemoveAt(lbDimensionsAvailable.SelectedIndex); PrintOrder(); }

        private void PrintOrder() {
            foreach (ImportConfig.DimensionInfo dimensionInfo in (DataContext as ImportConfig).Dimensions) {
                System.Diagnostics.Debug.Print(dimensionInfo.DimensionName);
            }

            //System.Diagnostics.Debug.Print(string.Join(" , ", (DataContext as ImportConfig).Dimensions));
        }

    }
}
