// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Linq;
using System.Text;
using Utils;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using federalGazetteBusiness.External;
using federalGazetteBusiness.Structures.Parameters;
using federalGazetteBusiness.Structures.ValueTypes;

namespace federalGazetteBusiness.Structures.Manager {

    public class FederalGazetteOrderManager : FederalGazetteManagerBase {

        public FederalGazetteSendParameter SendParameter { get; set; }

        public FederalGazetteOrderManager(Document document) : base(document) {
            SendParameter = new FederalGazetteSendParameter();
            Orders = new ObservableCollectionAsync<Order>();
            Init();
        }

        public FederalGazetteOrderManager() : this(DocumentManager.Instance.CurrentDocument) {
        }

        public void Init() {

            var props = SendParameter.GetType().GetProperties();
            foreach (var propertyInfo in props) {
                LoadValue(propertyInfo.GetValue(SendParameter, null) as IFederalGazetteElementInfo);
            }

            LoadValue(SendParameter.CompanySize);
            LoadValue(SendParameter.CompanyType);
            SendParameter.AcquisitationPeriodStart.Value = GetValue<DateTime>(SendParameter.AcquisitationPeriodStart);
            SendParameter.AcquisitationPeriodEnd.Value = GetValue<DateTime>(SendParameter.AcquisitationPeriodEnd);
        }

        public ObservableCollectionAsync<Order> Orders { get; set; }

        #region Order

        #region sending

        #region private helper
        private const string DefaultFileName = "BAnz-Einreichung.xbrl";


        /// <summary>
        /// Generates the <see cref="ebanzPublicationOrderData"/> based on the given <see cref="xbrlContent"/> and the selected values in the <see cref="Parameters"/>.
        /// </summary>
        /// <param name="xbrlContent">The xbrl content that has to be transmitted.</param>
        /// <returns></returns>
        private ebanzPublicationOrderData GetPublicationOrderData(string xbrlContent) {

            var order = new ebanzPublicationOrderData();
            order.order_type = Int32.Parse(SendParameter.OrderTypes.Value.ToString()); // 1;
            order.publication_area = Int32.Parse(SendParameter.PublicationArea.Value.ToString()); // 22;
            order.publication_category = Int32.Parse(SendParameter.PublicationCategory.Value.ToString()); // 14;
            order.publication_type = Int32.Parse(SendParameter.PublicationType.Value.ToString()); // 135;
            order.publication_sub_type = Int32.Parse(SendParameter.PublicationSubType.Value.ToString()); // 3;
            order.language = SendParameter.PublicationLanguage.Value.ToString();
            order.company_type = Int32.Parse(SendParameter.CompanyType.Value.ToString());
            order.company_size = SendParameter.CompanySize.Value.ToString();
            order.balance_sheet_standard = Int32.Parse(SendParameter.BalanceSheetStandard.Value.ToString());
            //order.balance_sheet_standard_concern
            order.has_natural_person = SendParameter.HasNaturalPerson.Value == null
                                           ? (bool?)null
                                           : Boolean.Parse(SendParameter.HasNaturalPerson.Value.ToString());
            order.has_natural_personSpecified = order.has_natural_person.HasValue && order.has_natural_person.Value;
            var file1 = new ebanzPublicationFile();
            file1.file = new base64Binary();
            var enc = new UTF8Encoding();

            file1.file.Value = enc.GetBytes(xbrlContent);
            file1.file.contentType = "Xbrl (not specified)";
            file1.annual_content_type = Int32.Parse(SendParameter.AnnualContentType.Value.ToString()); // 3;
            file1.file_name = DateTime.Now.ToShortDateString() + "_" + DocumentManager.Instance.CurrentDocument.Company.Name + "_" + DefaultFileName;

            order.files = new[] { file1 };

            order.acquisition_period_start = DateTime.Parse(SendParameter.AcquisitationPeriodStart.Value.ToString());
            order.acquisition_period_end = DateTime.Parse(SendParameter.AcquisitationPeriodEnd.Value.ToString());
            return order;
        }
        #endregion // private helper

        #region preview

        /// <summary>
        /// Gets a preview of the report that will be send to the BANz by using the <see cref="BAnzServicePortType.ebanzCheckOrder"/>.
        /// </summary>
        /// <param name="xbrlContent">The real content to transfer (XBRL).</param>
        /// <param name="user">The username to connect to the BAnz webservice.</param>
        /// <param name="password">The password for the <see cref="user"/></param>
        /// <param name="clientNumber">The clientNumber for the company the report should be sent.</param>
        /// <returns>A stream with the HTML content. Throws an exception if something wents wrong (<see cref="ErrorAnalyse.AnalyseError"/>).</returns>
        public Stream GetPreview(string xbrlContent, string user, string password, string clientNumber) {
            var c = GetBAnzClient();
            var login = GetLogin(user, password);

            //xbrlContent =
            //    xbrlContent.Replace(
            //        "<xbrli:identifier scheme=\"http://www.rzf-nrw.de/Steuernummer\"></xbrli:identifier>",
            //        "<xbrli:identifier scheme=\"http://www.rzf-nrw.de/Steuernummer\">I-2011</xbrli:identifier>");


            //xbrlContent = File.ReadAllText(@"C:\Users\sev\Documents\Bundesanzeiger\Fragen\request.txt");

            //xbrlContent = File.ReadAllText(@"Q:\Softwareentwicklung\Projects\eBilanz-Kit\bundesanzeiger\07.2012\Taxonomie\sample_03_staffelform.xml");



            var order = GetPublicationOrderData(xbrlContent);


            var rep = new OrderReplyInfo();
            rep.html_direct_reply = true;

            var newOrder = c.ebanzCheckOrder(login, order, clientNumber, rep);

            ErrorAnalyse.AnalyseError(newOrder);

            if (newOrder.resultOrderData != null) {

                MemoryStream memory = new MemoryStream(newOrder.resultOrderData.html_content.Value);

                // if you want to give back the HTML content as String
                //var result = enc.GetString(memory.ToArray());
                //return result;

#if WriteCommunicationIntoFiles
                // if you want to write the HTML into a file
                var html = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),
                    "response.html");
                FileStream fs = new FileStream(html, FileMode.Create);
                memory.WriteTo(fs);
                fs.Close();

                // if you want to open the HTML file
                System.Diagnostics.Process.Start(html);
#endif
                return memory;
            }
            else {
                //result = file + System.Environment.NewLine + System.Environment.NewLine + result;
                //Result = result;
                //ert: Debug.Fail("newOrder.resultOrderData == null");
                if (newOrder.validationErrorList != null
                    && newOrder.validationErrorList.Any()) {
                    string lastError = newOrder.validationErrorList[newOrder.validationErrorList.Count() - 1].description;

                    MemoryStream stream = new MemoryStream();
                    StreamWriter writer = new StreamWriter(stream);
                    writer.Write(lastError);
                    writer.Flush();
                    stream.Position = 0;
                    return stream;
                }

                return null;
            }
        }

        #endregion

        #region sending

        public object SendOrder(string xbrlContent, string user, string password, string clientNumber) {
            var ticketID = GenerateTicketId() + "_" + xbrlContent.GetHashCode();
            ebanzNewOrderResponseType newOrder = null;
            Order orderEntry = null;
            try {
                var c = GetBAnzClient();
                var login = GetLogin(user, password);

                var order = GetPublicationOrderData(xbrlContent);

                var rep = new OrderReplyInfo();
                rep.html_direct_reply = true;


                newOrder = c.ebanzNewOrder(login, order, clientNumber, ticketID, rep);

                ErrorAnalyse.AnalyseError(newOrder);

                if (newOrder.resultOrderData != null) {


                    //var htmlContent = System.Text.Encoding.Default.GetString(memory.ToArray());
                    //var orderNumber = newOrder.resultOrderData.ordernumber;

                    orderEntry = new Order(ticketID, xbrlContent, newOrder.resultOrderData, LastCommunicationData);

                    // if you want to give back the HTML content as String
                    //var result = enc.GetString(memory.ToArray());
                    //return result;

#if WriteCommunicationIntoFiles
                    MemoryStream memory = new MemoryStream(newOrder.resultOrderData.html_content.Value);

    // if you want to write the HTML into a file
                var html = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),
                    "response.html");
                FileStream fs = new FileStream(html, FileMode.Create);
                memory.WriteTo(fs);
                fs.Close();

                // if you want to open the HTML file
                System.Diagnostics.Process.Start(html);
#endif
                    Orders.Add(orderEntry);
                    
                    return orderEntry;
                }
                else {
                    //something went wrong
                    return false;
                }
            }
            catch (Exception e) {
                if (orderEntry == null) {
                    orderEntry = new Order(ticketID, xbrlContent, newOrder != null ? newOrder.resultOrderData : null, LastCommunicationData);
                }

            }
            return orderEntry;
        }
        #endregion

        #endregion // sending

        #region status requestion

        private orderStatusType? GetStatus(Order order) {
            var login = GetLogin();
            var client = GetBAnzClient();
            var status = client.ebanzGetOrderStatus(login, order.OrderNo);
            ErrorAnalyse.AnalyseError(status);
            if (!status.status.HasValue) {
                var ticketStatus = client.getTicketStatus(login, order.TicketId);
                ErrorAnalyse.AnalyseError(ticketStatus);
                if (ticketStatus.data != order.OrderNo) {
                    // something went wrong, the ticket maybe belongs to an other order
                    // we are updating it
                    throw new Exception("ticketStatus is strange " + ticketStatus.data);
                }

            }
            return status.status;
        }
        #endregion

        public void RequestStatus(Order order) {
            var status = GetStatus(order);
            order.Status = status;

        }

        public void LoadOrders() {
            using (var conn = eBalanceKitBusiness.Structures.AppConfig.ConnectionManager.GetConnection()) {
                //conn.DbMapping.CreateTableIfNotExists<Order>();
                var orders = conn.DbMapping.Load<Order>();
                foreach (var order in orders) {
                    Orders.Add(order);
                }
            }
        }

        public string GetOrderContent(Order order) { return order.ResponseHtml; }

        #endregion // Order
    }
}