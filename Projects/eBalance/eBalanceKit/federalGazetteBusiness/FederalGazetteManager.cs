////#define WriteCommunicationIntoFiles
//// --------------------------------------------------------------------------------
//// author: Sebastian Vetter
//// since: 2012-10-22
//// copyright 2012 AvenDATA GmbH
//// --------------------------------------------------------------------------------

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using eBalanceKitBusiness.Manager;
//using eBalanceKitBusiness.Structures.DbMapping;
//using federalGazetteBusiness.External;
//using federalGazetteBusiness.Structures;
//using federalGazetteBusiness.Structures.Manager;

//namespace federalGazetteBusiness
//{
//    public class FederalGazetteManager
//    {
//        public FederalGazetteManager() {
            
//        }

//        //public FederalGazetteManager(Document document) :base(document) {

//        //}

//        //public FederalGazetteManager(Connection loginData) :base(eBalanceKitBusiness.Manager.DocumentManager.Instance.CurrentDocument) { LoginData = loginData; }


//        /// <summary>
//        /// Returns a dictionary with CompanyId as Key and CompanyName as Value.
//        /// </summary>
//        /// <param name="companyName">If the company name is given it tries to find the companyId for only this company.</param>
//        /// <param name="returnAllOnNoMatch">Defines if to return all entries if no exact match could be found in the BAnz.</param>
//        /// <returns><list type="bullet">
//        /// <item>default: List of all Companies / Clients ("Einsenderkunden").</item>
//        /// <item><see cref="companyName"/> set, <see cref="returnAllOnNoMatch"/>=false: List of all Companies that are in the BAnz and have the given <see cref="companyName"/>.</item>
//        /// <item><see cref="companyName"/> set, <see cref="returnAllOnNoMatch"/>=true: List of all Companies, the leading entries are containing the <see cref="companyName"/>.</item>
//        /// </list> 
//        /// Throws an exception if something wents wrong (<see cref="ErrorAnalyse.AnalyseError"/>)
//        /// </returns>
//        public Dictionary<string, string> GetSenderClients(string companyName = null, bool returnAllOnNoMatch = false) {
//            var result = new Dictionary<string, string>();
//            var company = companyName ?? string.Empty;
//            var login = GetLogin();
//            var com = GetBAnzClient();
//            var queryResult = com.listClients(login, string.Empty, company, string.Empty);

//            ErrorAnalyse.AnalyseError(queryResult);
//            if (queryResult.result != 0)
//                return result;

//            if (queryResult.clientList != null) {

//                foreach (var client in queryResult.clientList) {
//                    result.Add(client.clientnumber, client.company_name);
//                }
//            } else {
//                if (string.IsNullOrEmpty(company)) {
//                    throw new NoClientsDefinedException();
//                }
//                queryResult = com.listClients(GetLogin(), string.Empty, string.Empty, string.Empty);
//                result.Clear();
//                var preferedResult = new Dictionary<string, string>();
//                var tmpResult = new Dictionary<string, string>();
//                foreach (var client in queryResult.clientList) {
//                    if (companyName != null && client.company_name.Contains(companyName)) {
//                        preferedResult.Add(client.clientnumber, client.company_name);
//                        continue;
//                    }
//                    tmpResult.Add(client.clientnumber, client.company_name);
//                }
//                if (preferedResult.Any()) {
//                    result = preferedResult;
//                    if (returnAllOnNoMatch) {
//                        foreach (var tmpEntry in tmpResult) {
//                            result.Add(tmpEntry.Key, tmpEntry.Value);
//                        }
//                    }
//                } else {
//                    result = tmpResult;
//                }
//            }

//            return result;
//        }
        

//        public string GenerateXbrl() {
//            return eBalanceKitBusiness.Export.XbrlExporter.GenerateXbrlForFederalGazette(_document);
//        }


//        /// <summary>
//        /// ert:
//        /// Determines whether [is file locked] [the specified path].
//        /// </summary>
//        /// <param name="pPath">The file path.</param>
//        /// <returns>
//        ///   <c>true</c> if [is file locked] [the specified path]; otherwise, <c>false</c>.
//        /// </returns>
//        private static bool IsFileLocked(string pPath) {
//            try
//            {
//                using (Stream stream = new FileStream(pPath, FileMode.Open))
//                    return false;
//            }
//            catch {
//                return true;
//            }
//        }




//        /// <summary>
//        /// Generates a unique ticket ID based on the DateTime.Now.Ticks, DocumentManager.Instance.CurrentDocument.Id and DocumentManager.Instance.CurrentDocument.CompanyId
//        /// </summary>
//        /// <returns></returns>
//        public string GenerateTicketId() {
//            return "BAnz-Ticket_ebk_" + DateTime.Now.Ticks + "_" + DocumentManager.Instance.CurrentDocument.Id + "_" + DocumentManager.Instance.CurrentDocument.CompanyId;
//        }
//        #endregion

//        /// <summary>
//        /// Generates the <see cref="ebanzPublicationOrderData"/> based on the given <see cref="xbrlContent"/> and the selected values in the <see cref="Parameters"/>.
//        /// </summary>
//        /// <param name="xbrlContent">The xbrl content that has to be transmitted.</param>
//        /// <returns></returns>
//        private ebanzPublicationOrderData GetPublicationOrderData(string xbrlContent) {

//            var order = new ebanzPublicationOrderData();
//            order.order_type = int.Parse(Parameters.OrderTypes.Value.ToString()); // 1;
//            order.publication_area = int.Parse(Parameters.PublicationArea.Value.ToString()); // 22;
//            order.publication_category = int.Parse(Parameters.PublicationCategory.Value.ToString()); // 14;
//            order.publication_type = int.Parse(Parameters.PublicationType.Value.ToString()); // 135;
//            order.publication_sub_type = int.Parse(Parameters.PublicationSubType.Value.ToString()); // 3;
//            order.language = Parameters.PublicationLanguage.Value.ToString();
//            order.company_type = int.Parse(Parameters.CompanyType.Value.ToString());
//            order.company_size = Parameters.CompanySize.Value.ToString();
//            order.balance_sheet_standard = int.Parse(Parameters.BalanceSheetStandard.Value.ToString());
//            //order.balance_sheet_standard_concern
//            order.has_natural_person = Parameters.HasNaturalPerson.Value == null
//                                           ? (bool?)null
//                                           : bool.Parse(Parameters.HasNaturalPerson.Value.ToString());
//            order.has_natural_personSpecified = order.has_natural_person.HasValue && order.has_natural_person.Value;
//            var file1 = new ebanzPublicationFile();
//            file1.file = new base64Binary();
//            var enc = new UTF8Encoding();

//            file1.file.Value = enc.GetBytes(xbrlContent);
//            file1.file.contentType = "Xbrl (not specified)";
//            file1.annual_content_type = int.Parse(Parameters.AnnualContentType.Value.ToString()); // 3;
//            file1.file_name = DateTime.Now.ToShortDateString() + "_" + DocumentManager.Instance.CurrentDocument.Company.Name + "_" + FederalGazetteOrderManager.DefaultFileName;

//            order.files = new[] { file1 };

//            order.acquisition_period_start = DateTime.Parse(Parameters.AcquisitationPeriodStart.Value.ToString());
//            order.acquisition_period_end = DateTime.Parse(Parameters.AcquisitationPeriodEnd.Value.ToString());
//            return order;
//        }

//        /// <summary>
//        /// Gets a preview of the report that will be send to the BANz by using the <see cref="BAnzServicePortType.ebanzCheckOrder"/>.
//        /// </summary>
//        /// <param name="xbrlContent">The real content to transfer (XBRL).</param>
//        /// <param name="user">The username to connect to the BAnz webservice.</param>
//        /// <param name="password">The password for the <see cref="user"/></param>
//        /// <param name="clientNumber">The clientNumber for the company the report should be sent.</param>
//        /// <returns>A stream with the HTML content. Throws an exception if something wents wrong (<see cref="ErrorAnalyse.AnalyseError"/>).</returns>
//        public Stream GetPreview(string xbrlContent, string user, string password, string clientNumber) {
//            var c = GetBAnzClient();
//            var login = GetLogin(user, password);

//            var order = GetPublicationOrderData(xbrlContent);


//            var rep = new OrderReplyInfo();
//            rep.html_direct_reply = true;

//            var newOrder = c.ebanzCheckOrder(login, order, clientNumber, rep);

//            ErrorAnalyse.AnalyseError(newOrder);

//            if (newOrder.resultOrderData != null) {

//                MemoryStream memory = new MemoryStream(newOrder.resultOrderData.html_content.Value);

//                // if you want to give back the HTML content as String
//                //var result = enc.GetString(memory.ToArray());
//                //return result;

//#if WriteCommunicationIntoFiles
//    // if you want to write the HTML into a file
//                var html = System.IO.Path.Combine(
//                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),
//                    "response.html");
//                FileStream fs = new FileStream(html, FileMode.Create);
//                memory.WriteTo(fs);
//                fs.Close();

//                // if you want to open the HTML file
//                System.Diagnostics.Process.Start(html);
//#endif
//                return memory;
//            }
//            else {
//                //result = file + System.Environment.NewLine + System.Environment.NewLine + result;
//                //Result = result;
//                //ert: Debug.Fail("newOrder.resultOrderData == null");
//                if (newOrder.validationErrorList != null
//                    && newOrder.validationErrorList.Any()) {
//                    string lastError = newOrder.validationErrorList[newOrder.validationErrorList.Count() - 1].description;

//                    MemoryStream stream = new MemoryStream();
//                    StreamWriter writer = new StreamWriter(stream);
//                    writer.Write(lastError);
//                    writer.Flush();
//                    stream.Position = 0;
//                    return stream;
//                }

//                return null;
//            }
//        }

//        public object SendOrder(string xbrlContent, string user, string password, string clientNumber) {
//            var ticketID = GenerateTicketId() + "_" + xbrlContent.GetHashCode();
//            External.ebanzNewOrderResponseType newOrder = null;
//            Order orderEntry = null;
//            try {
//                var c = GetBAnzClient();
//                var login = GetLogin(user, password);

//                var order = GetPublicationOrderData(xbrlContent);

//                var rep = new OrderReplyInfo();
//                rep.html_direct_reply = true;


//                newOrder = c.ebanzNewOrder(login, order, clientNumber, ticketID, rep);

//                ErrorAnalyse.AnalyseError(newOrder);

//                if (newOrder.resultOrderData != null) {


//                    //var htmlContent = System.Text.Encoding.Default.GetString(memory.ToArray());
//                    //var orderNumber = newOrder.resultOrderData.ordernumber;

//                    orderEntry = new Order(ticketID, xbrlContent, newOrder.resultOrderData, LastCommunicationData);

//                    // if you want to give back the HTML content as String
//                    //var result = enc.GetString(memory.ToArray());
//                    //return result;

//#if WriteCommunicationIntoFiles
//                    MemoryStream memory = new MemoryStream(newOrder.resultOrderData.html_content.Value);

//    // if you want to write the HTML into a file
//                var html = System.IO.Path.Combine(
//                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),
//                    "response.html");
//                FileStream fs = new FileStream(html, FileMode.Create);
//                memory.WriteTo(fs);
//                fs.Close();

//                // if you want to open the HTML file
//                System.Diagnostics.Process.Start(html);
//#endif
//                    return orderEntry;
//                }
//                else {
//                    //something went wrong
//                    return false;
//                }
//            }
//            catch (Exception e) {
//                if (orderEntry == null) {
//                    orderEntry = new Order(ticketID, xbrlContent, newOrder != null ? newOrder.resultOrderData : null, LastCommunicationData);
//                }

//            }
//            return orderEntry;
//        }
//    }

//}