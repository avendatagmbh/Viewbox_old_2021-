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
using AvdWpfControls;
using DbSearch.Models.Search;

namespace DbSearch.Controls.Search {
    /// <summary>
    /// Interaktionslogik für CtlSearch.xaml
    /// </summary>
    public partial class CtlSearch : UserControl {
        public CtlSearch() {
            InitializeComponent();
        }

        SearchModel Model { get { return DataContext as SearchModel; } }

        private void nudRows_ValueChanged(object sender, ValueChangedEventArgs e) {
            if(Model != null) Model.ChangeRows(e.NewValue);
        }

        private void nudColumns_ValueChanged(object sender, ValueChangedEventArgs e) {
            if (Model != null)
                Model.ChangeColumns(e.NewValue);
                
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model != null) {
                Model.Query.Columns.CollectionChanged -= Columns_CollectionChanged;
                Model.Query.Columns.CollectionChanged += Columns_CollectionChanged;
            }
        }

        void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            CtlSearchMatrix.CreateColumns();
        }
    }
}
