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
using System.Windows.Shapes;
using TransDATA.Models;

namespace TransDATA.Windows
{
    /// <summary>
    /// Interaction logic for DlgHistorySelector.xaml
    /// </summary>
    public partial class DlgHistorySelector : Window
    {
        public DlgHistorySelector()
        {
            InitializeComponent();

        }

        private HistorySelectorModel Model
        {
            get { return DataContext as HistorySelectorModel; }
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                e.Handled = true;
                //Owner.Close();
            }
        }

    }
}
