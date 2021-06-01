using System.Windows;
using System.Windows.Controls;
using ScreenshotAnalyzer.Models.ListView;

namespace ScreenshotAnalyzer.Controls {

    /// <summary>
    /// Interaktionslogik für CtlListView.xaml
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2010-01-11</since>
    public partial class CtlListView : UserControl {

        internal CtlListView(IListViewModel model) {
            InitializeComponent();
            
            Model = model;
            DataContext = Model;
            Model.InitUiElements(dataPanel, listPanel);
        }

        private IListViewModel Model { get; set; }
        private void btnAddItem_Click(object sender, RoutedEventArgs e) { this.Model.AddItem(); }
        private void btnDeleteItem_Click(object sender, RoutedEventArgs e) { Model.DeleteSelectedItem(); }
    }
}
