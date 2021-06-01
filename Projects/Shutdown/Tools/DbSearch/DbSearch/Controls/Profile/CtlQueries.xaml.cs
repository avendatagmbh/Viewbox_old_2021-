using System;
using System.Collections;
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
using DbSearch.Models.Profile;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearch.Controls.Profile {
    /// <summary>
    /// Interaktionslogik für CtlQueries.xaml
    /// </summary>
    public partial class CtlQueries : UserControl {
        public CtlQueries() {
            InitializeComponent();
        }

        QueriesModel Model { get { return DataContext as QueriesModel; } }

        private void btnDeleteItem_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Wollen Sie die markierten Abfragen wirklich löschen?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                List<Query> itemsToDelete = new List<Query>();
                foreach (var query in dgQueries.SelectedItems.Cast<Query>())
                    itemsToDelete.Add(query);
                Model.DeleteQueries(itemsToDelete);
            }
        }
    }
}
