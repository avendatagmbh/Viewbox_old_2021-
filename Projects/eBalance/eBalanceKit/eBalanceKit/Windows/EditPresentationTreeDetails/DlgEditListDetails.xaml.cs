// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKitBusiness;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Structures.XbrlElementValue;

namespace eBalanceKit.Windows.EditPresentationTreeDetails {
    /// <summary>
    /// Interaktionslogik für DlgEditListDetails.xaml
    /// </summary>
    public partial class DlgEditListDetails : Window {
        public DlgEditListDetails(IPresentationTreeNode root) {
            InitializeComponent();
            listView.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            listView.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            listView.dataPanel.Children.Add(new CtlEditListTreeView(root));
            DataContextChanged += DlgEditListDetails_DataContextChanged;
        }

        void DlgEditListDetails_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var model = DataContext as PresentationTreeDetailModel;
            if (model == null) return;
            var value = model.Value as XbrlElementValue_Tuple;
            if (value == null) return;
            listView.DisplayMemberPath = value.DisplayMemberPath;
        }

        private void SaveValues() {
            txtComment.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            SaveValues();
            DialogResult = true;          
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            switch (e.Key) {
                case Key.Escape:
                    e.Handled = true;
                    DialogResult = false;
                    break;
                case Key.F2:
                    //e.Handled = true;
                    //SaveValues();
                    //DialogResult = true;
                    break;
            }
        }
    }
}