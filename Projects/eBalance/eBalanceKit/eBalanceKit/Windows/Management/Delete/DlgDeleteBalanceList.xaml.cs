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
using eBalanceKit.Models.Document;
using Utils;
using eBalanceKitBusiness;
using eBalanceKit.Windows.Management.Delete.Models;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;
using eBalanceKitBusiness.Manager;

namespace eBalanceKit.Windows.Management.Delete
{
    /// <summary>
    /// Interaction logic for DlgDeleteBalanceList.xaml
    /// </summary>
    public partial class DlgDeleteBalanceList : Window
    {
        private Window _parent;

        public DeleteBalanceListModel Model { get; private set; }

        public DlgDeleteBalanceList(Document parentDocument, Window parent)
        {
            Model = new DeleteBalanceListModel(new EditDocumentModel(parentDocument, this).Document);
            _parent = parent;
            DataContext = Model;
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(ResourcesBalanceList.RequestDeleteBalanceList, "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (!RightManager.HasDocumentRestWriteRight(Model.Document, _parent)) return;

                Model.DeleteBalanceList();
            }

            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
