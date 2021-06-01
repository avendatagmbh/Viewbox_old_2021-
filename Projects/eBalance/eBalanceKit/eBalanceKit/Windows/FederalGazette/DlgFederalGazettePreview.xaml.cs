using System;
using System.Collections.Generic;
using System.IO;
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
using AvdWpfControls;
using Microsoft.Win32;
using eBalanceKit.Structures;
using eBalanceKit.Windows.FederalGazette.Model;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.FederalGazette;
using eBalanceKitBusiness.FederalGazette.Model;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.FederalGazette
{
    /// <summary>
    /// Interaktionslogik für DlgFederalGazettePreview.xaml
    /// </summary>
    public partial class DlgFederalGazettePreview : Window
    {
        public DlgFederalGazettePreview(Document document)
        {

            InitializeComponent();
            _federalGazetteModel= new FederalGazetteMainModel(document);
            Model = new DlgFederalGazettePreviewModel (_federalGazetteModel);
            
            DataContext = Model; 
            balanceList.accountList.GiveFeedback += new GiveFeedbackEventHandler(accountList_GiveFeedback);
        }
        internal DlgFederalGazettePreviewModel Model { get; set; }
        private FederalGazetteMainModel _federalGazetteModel;
        
        void accountList_GiveFeedback(object sender, GiveFeedbackEventArgs e) {
            var data = DragDropPopup.DataContext as AccountListDragDropData;
            if (data == null) return;
            else if (data.AllowDrop) popupBorder.Opacity = 1.0;
            else popupBorder.Opacity = 0.5;
        }

        

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void nav_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            Model.SelectedNavigationEntry = (sender as HierarchicalTabControl).SelectedItem as NavigationTreeEntryFg;
        }

        private void btnValidate_Click(object sender, RoutedEventArgs e)
        {
            try {
                var getLogin = new DlgFederalGazetteLogin();
                var result = getLogin.ShowDialog();
                if (result == true) {
                    _federalGazetteModel.Username = getLogin.UserName;
                    _federalGazetteModel.Password = getLogin.Password;

                    getLogin.Close();
                    var sendFederalGazette = new TransferFederalGazetteData(_federalGazetteModel);
                    sendFederalGazette.CheckOrder();

                }
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void btnExportXbrl_Click(object sender, RoutedEventArgs e) {       
            var dlgFile = new SaveFileDialog();
            dlgFile.Filter = "xml files (*.xml)|*.xml";
            var result = dlgFile.ShowDialog();

            if (result==true) {
                Model.SaveFiles(dlgFile.FileName);
            }

        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            new DlgInfo { Owner = this }.ShowDialog();
        }

        private void btnSendData_Click(object sender, RoutedEventArgs e) {
            var order = new TransferFederalGazetteData(_federalGazetteModel);
            var client = new FederalGazetteClientOperations(_federalGazetteModel);
            var xbrl = new FederalGazetteGetXbrl(_federalGazetteModel);


            //order.GetOrderDetail(); //okay file conversion
            //order.GetOrderList();//okay
            //order.GetOrderReceipt(@"c:\test.txt"); //okay
            //order.GetOrderStatus();
            //order.NewOrder();//okay file conversion
            //order.CheckOrder();//okay file conversion
            //order.CancelOrder();// must be okay. 
            //order.ChangeOrder();//okay file conversion

            
            //client.ChangeClient();//okay
            //client.CreateClient();//okay
            //client.DeleteClient("12312312");//okay
            //client.GetClientData();//okay
            //client.GetClientList();//okay
            //client.GetCompanyId();//not tested
            //client.GetCompanyListQueryId();//not tested
            //client.ReassignCompanyIndexNumberFromRegCentral();//not tested
            //client.UpdateClientDataFromRegCentral();//not tested

            //xbrl.GetFixedAssets();//okay
            //xbrl.GetIncomeStatement();//okay
            //xbrl.GetManagementReport();// okay
            //xbrl.GetNetProfit();//okay
            //xbrl.GetNotes();//okay
            //xbrl.GetAllAccounts();//okay
        }
    }
}
