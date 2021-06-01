using System;
using System.Collections.Generic;
using System.IO;
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
using EricWrapper;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Structures;

namespace EricTest {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new Model(this);
        }

        Model Model { get { return DataContext as Model; } }

        private void button1_Click(object sender, RoutedEventArgs e) {
            Model.Validate();
        }

        /// <summary>
        /// Handles the Click event of the btnSelectCsvFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectFile_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileOk += new System.ComponentModel.CancelEventHandler(dlg_FileOk);
            dlg.Multiselect = false;
            dlg.ShowDialog();
        }

        /// <summary>
        /// Handles the FileOk event of the dlg control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        void dlg_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            Model.XbrlFile = ((Microsoft.Win32.OpenFileDialog)sender).FileName;
        }

        private void btSave_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".xml";
            if (dlg.ShowDialog() == true) {
                File.WriteAllText(dlg.FileName, Model.XbrlContent);
            } 
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.F5) {
                button1_Click(sender, e);
            }
        }
    }
}
