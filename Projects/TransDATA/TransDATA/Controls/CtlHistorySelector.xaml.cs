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
using Base.Localisation;
using TransDATA.Models;

namespace TransDATA.Controls
{
    /// <summary>
    /// Interaction logic for CtlHistorySelector.xaml
    /// </summary>
    public partial class CtlHistorySelector : UserControl
    {
        public CtlHistorySelector()
        {
            InitializeComponent();
        }

        private HistorySelectorModel Model
        {
            get { return DataContext as HistorySelectorModel; }
        }

        private Window Owner
        {
            get { return UIHelpers.TryFindParent<Window>(this); }
        }

        private void BtnCloseClick(object sender, RoutedEventArgs e)
        {
            Owner.Close();
        }

        private void LbItemsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BtnSelectClick(object sender, RoutedEventArgs e)
        {
            Model.CopyProfile();
            Owner.Close();
        }

    }
}
