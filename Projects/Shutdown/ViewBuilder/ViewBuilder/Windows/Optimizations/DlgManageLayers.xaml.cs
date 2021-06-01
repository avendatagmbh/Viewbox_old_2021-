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
using ViewBuilder.Models;

namespace ViewBuilder.Windows.Optimizations {
    /// <summary>
    /// Interaktionslogik für DlgManageLayers.xaml
    /// </summary>
    public partial class DlgManageLayers : Window {
        public DlgManageLayers() {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(DlgManageLayers_DataContextChanged);
            
        }

        void DlgManageLayers_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(Model != null)
                Model.SetupColumns(dgEditGroupTexts);
        }

        ManageLayersModel Model { get { return DataContext as ManageLayersModel; } }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            Model.Apply();
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void dgEditGroupTexts_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if ((e.Key >= Key.A && e.Key <= Key.Z) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || (e.Key >= Key.D0 && e.Key <= Key.D9))
                dgEditGroupTexts.BeginEdit();
        }

        private void dgEditGroupTexts_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e) {
            ContentPresenter cp = (ContentPresenter)e.EditingElement;

            TextBox destinationTextBox = VisualTreeHelper.GetChild(cp, 0) as TextBox;

            if (destinationTextBox != null) {
                destinationTextBox.Focus();
                destinationTextBox.SelectAll();
            }
        }
    }
}
