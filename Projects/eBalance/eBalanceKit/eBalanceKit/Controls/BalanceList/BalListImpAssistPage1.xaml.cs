/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2012-01-18      initial implementation
 *************************************************************************************************************/

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
using eBalanceKit.Models.Assistants;
using System.IO;
using eBalanceKitBusiness.Structures.DbMapping.Templates;

namespace eBalanceKit.Controls.BalanceList {
    
    /// <summary>
    /// Interaktionslogik für BalListImpAssistPage1.xaml
    /// </summary>
    //public partial class BalListImpAssistPage1 : UserControl {
    public partial class BalListImpAssistPage1 : BalListImpAssistPageBase {

        /// <summary>
        /// Initializes a new instance of the <see cref="BalListImpAssistPage1"/> class.
        /// </summary>
        public BalListImpAssistPage1() {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>
        BalListImportAssistantModel Model {
            get { return this.DataContext as BalListImportAssistantModel; }
        }

        /// <summary>
        /// Handles the Click event of the btnSelectCsvFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectCsvFile_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileOk += new System.ComponentModel.CancelEventHandler(dlg_FileOk);
            dlg.Filter = "csv files (*.csv)|*.csv|txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.Multiselect = false;
            dlg.ShowDialog();
            Validate();
        }

        /// <summary>
        /// Handles the FileOk event of the dlg control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        void dlg_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            if (this.Model != null) {
                this.Model.Importer.Config.CsvFileName = ((Microsoft.Win32.OpenFileDialog)sender).FileName;
                /*
                if (Model.Importer.LastError.Contains(Model.Importer.Config.CsvFileName))
                {
                    Model.Importer.Config.CsvFileName = null;
                    e.Cancel = true;
                }
                 */
            }
        }

        public override bool Validate() {
            
            txtNameWarning1.Visibility = System.Windows.Visibility.Collapsed;
            txtNameWarning2.Visibility = System.Windows.Visibility.Collapsed;
            txtNameWarning3.Visibility = System.Windows.Visibility.Collapsed;

            // check if a file has been entered
            if (string.IsNullOrEmpty(this.Model.Importer.Config.CsvFileName)) {
                txtNameWarning1.Visibility = System.Windows.Visibility.Visible;
                return false;
            }

            // check if the file exists
            if (!File.Exists(this.Model.Importer.Config.CsvFileName)) {
                txtNameWarning2.Visibility = System.Windows.Visibility.Visible;
                return false;
            }

            // check file size
            FileInfo fi = new FileInfo(this.Model.Importer.Config.CsvFileName);
            if (fi.Length > 100 * 1024 * 1024) {
                txtNameWarning3.Visibility = System.Windows.Visibility.Visible;
                return false;
            }

            if (!string.IsNullOrEmpty(Model.Importer.Config.Warning)) {
                return false;
            }

            return true;
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e) {

        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e) {
            foreach (var actualItem in AccountsProfileManager.Items) {
                actualItem.IsSelected = false;
                if (e.AddedItems[0].Equals(actualItem)) {
                    actualItem.IsSelected = true;
                }
                AccountsProfileManager.SelectedElement = (AccountsInformationProfile)e.AddedItems[0];
            }
        }

        //private string _lastInsertFileName;
        //private eBalanceKitBusiness.Import.BalanceListImportConfig _lastConfig;
        //private string TestListFilePath;

        //private void checkUseTestlist_Checked(object sender, RoutedEventArgs e) {
        //    _lastInsertFileName = Model.Importer.Config.CsvFileName;
        //    _lastConfig = Model.Importer.Config;

        //    if (!System.IO.File.Exists(TestListFilePath)) {
        //        TestListFilePath = Model.GenerateTestBalanceList();
        //    }

        //    Model.Importer.Config.CsvFileName = TestListFilePath;

        //    var assistant = new Models.ExampleCSVModel();
        //    if (assistant == null || !File.Exists(TestListFilePath)) {
        //        return;
        //    }

        //    Model.Importer.Config.Comment = assistant.exampleCsv.CtlExampleCsv.ImportConfig.Comment;
        //    Model.Importer.Config.Encoding = assistant.exampleCsv.CtlExampleCsv.ImportConfig.Encoding;
        //    Model.Importer.Config.FirstLineIsHeadline = assistant.exampleCsv.CtlExampleCsv.ImportConfig.FirstLineIsHeadline;
        //    Model.Importer.Config.ImportType = assistant.exampleCsv.CtlExampleCsv.ImportConfig.ImportType;
        //    Model.Importer.Config.Index = assistant.exampleCsv.CtlExampleCsv.ImportConfig.Index;
        //    Model.Importer.Config.Seperator = assistant.exampleCsv.CtlExampleCsv.ImportConfig.Seperator;
        //    Model.Importer.Config.TaxonomyColumnExists = assistant.exampleCsv.CtlExampleCsv.ImportConfig.TaxonomyColumnExists;
        //    Model.Importer.Config.TextDelimiter = assistant.exampleCsv.CtlExampleCsv.ImportConfig.TextDelimiter;
        //}

        //private void checkUseTestlist_Unchecked(object sender, RoutedEventArgs e) { 
        //    Model.Importer.Config.CsvFileName = _lastInsertFileName;

        //    Model.Importer.Config.Comment = _lastConfig.Comment;
        //    Model.Importer.Config.Encoding = _lastConfig.Encoding;
        //    Model.Importer.Config.FirstLineIsHeadline = _lastConfig.FirstLineIsHeadline;
        //    Model.Importer.Config.ImportType = _lastConfig.ImportType;
        //    Model.Importer.Config.Index = _lastConfig.Index;
        //    Model.Importer.Config.Seperator = _lastConfig.Seperator;
        //    Model.Importer.Config.TaxonomyColumnExists = _lastConfig.TaxonomyColumnExists;
        //    Model.Importer.Config.TextDelimiter = _lastConfig.TextDelimiter;
        //}


    }
}
