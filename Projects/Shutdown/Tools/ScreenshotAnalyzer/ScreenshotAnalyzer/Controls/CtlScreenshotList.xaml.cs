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
using Microsoft.Win32;
using ScreenshotAnalyzer.Models;

namespace ScreenshotAnalyzer.Controls {
    /// <summary>
    /// Interaktionslogik für CtlScreenshotList.xaml
    /// </summary>
    public partial class CtlScreenshotList : UserControl {
        public CtlScreenshotList() {
            InitializeComponent();
        }

        ScreenshotListModel Model { get { return DataContext as ScreenshotListModel; } }

        private void btnLoadImages_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "Bilddateien (*.png, *.jpg)|*.png;*.jpg";
            if (dialog.ShowDialog().Value) {
                Model.AddImages(dialog.FileNames);
            }
        }

        private void ListBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            //if (e.Key == Key.Delete || e.Key == Key.Back) {
            //    Model.DeleteImages(lbScreenshots.SelectedItems);
            //}
        }

        private void btnClearImages_Click(object sender, RoutedEventArgs e) {
            if (Model.Screenshots != null)
                Model.DeleteImages(Model.Screenshots.Screenshots);
        }

        private void btnDeleteImage_Click(object sender, RoutedEventArgs e) {
            Model.DeleteImages(lbScreenshots.SelectedItems);
        }
    }
}
