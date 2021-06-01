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
using DbAccess.Structures;
using DbSearchBase.Enums;
using DbSearchLogic.SearchCore.Keys;

namespace DbSearch.Controls.Keys {
    /// <summary>
    /// Interaction logic for CtlKeyResults.xaml
    /// </summary>
    public partial class CtlKeyResults : UserControl {
        public CtlKeyResults() { InitializeComponent(); }

        private KeyManager Model { get { return this.DataContext as KeyManager; } }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Model.ResultFilter.ColumnTypesToShow = icColumnTypes.Items.OfType<BindableDbColumnType>();
            Model.RefreshVisibleKeyList();
        }
    }
}
