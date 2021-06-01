using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DbSearch.Models.SearchStatus;
using DbSearchLogic.ProcessQueue;
using DbSearchLogic.SearchCore.Threading;
using Utils;

namespace DbSearch.Controls.SearchStatus {
    /// <summary>
    /// Interaktionslogik für CtlSearchStatus.xaml
    /// </summary>
    public partial class CtlSearchStatus : UserControl {
        public CtlSearchStatus() {
            InitializeComponent();
        }

        private void btnStopThreads_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show(UIHelpers.TryFindParent<Window>(this), "Möchten Sie wirklich alle Suchen abbrechen?", "", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;
            ThreadManager.AbortThreads();
        }

        private void btnRemoveSelectedSearches_Click(object sender, RoutedEventArgs e) {
            SearchStatusModel model = DataContext as SearchStatusModel;
            if (model != null) model.DeleteSelectedKeySearches(dgKeySearch.SelectedItems);
        }
    }
}
