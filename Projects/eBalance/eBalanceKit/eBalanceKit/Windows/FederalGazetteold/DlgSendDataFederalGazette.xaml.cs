using System;
using System.Text;
using System.Windows;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Export;
using eBalanceKitBusiness.Send;
using eBalanceKitBusiness.Structures.DbMapping;
using eFederalGazette;

namespace eBalanceKit.Windows.FederalGazette {
       
    /// <summary>
    /// Interaktionslogik für DlgSendDataFederalGazette.xaml
    /// </summary>
    public partial class DlgSendDataFederalGazette : Window {
        public DlgSendDataFederalGazette(Document document) {
            InitializeComponent();
            Model = new FederalGazetteModel(document);
            DataContext = Model;
        }
        private DlgProgress Progress { get; set; }
        private FederalGazetteModel Model { get; set; }

        private void btnExport_Click(object sender, RoutedEventArgs e) {
            try {
                var xbrlExportAnnual = new XbrlExportAnnualStatement();
                var data = xbrlExportAnnual.ExportXmlAnnualStatement(Model.Document, txtFile.Text, true);
                
                var dataEncodedAsByte = Encoding.UTF8.GetBytes(data.ToString());
                var dataEncodedAsBase64 = Convert.ToBase64String(dataEncodedAsByte);
                
                var file = new ebanzPublicationFile();
                var baseBin = new base64Binary {Value = dataEncodedAsByte, contentType = dataEncodedAsBase64};
                
                file.annual_content_type = 3;
                file.file = baseBin;
                file.file_name = "test_file.xml";
                
                Model.Files = file;
                
                var sendFederalGazette = new SendFederalGazette(Model);
                sendFederalGazette.Send();

                MessageBox.Show("Export Abgeschlossen");
                Close();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) { 
            //var client = new FederalGazetteService();
            //var login = new LoginData();
            //login.username = "DemoEinsender";
            //login.password = "tx12pr97";
            //var eclient = new EbanzClient();
            //eclient.company_domicile = "berlin";
            ////eclient.company_id = 1;
            //eclient.company_name = "Test AG";
            //eclient.company_type = "german_not_registered";
            //eclient.legal_form = "AG";
            //var add = new CompanyAddress {
            //    city = "Berlin",
            //    company_name = "Test AG",
            //    country = "de",
            //    postcode = "10587",
            //    state = "BE",
            //    street = "Salzufer 8"
            //};

            //eclient.companyaddress = add;
            //var response =client.createClient(login, eclient, "");
            //MessageBox.Show(response.result.ToString());
        }
    }
}
