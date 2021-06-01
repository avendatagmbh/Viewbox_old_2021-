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
using eBalanceKit.Windows.MappingTemplates.Models;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.MappingTemplates
{
    /// <summary>
    /// Interaction logic for DlgGCDTemplates.xaml
    /// </summary>
    public partial class DlgGCDTemplates : Window
    {
        public DlgGCDTemplates(Document document)
        {
            InitializeComponent();
            Model = new DlgGCDTemplatesModel(document, this);
            DataContext = Model;
        }

        private DlgGCDTemplatesModel Model { get; set; }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;
            e.Handled = true;
            Close();
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) { Close(); }
    }
}
