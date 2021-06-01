// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2011-12-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.FederalGazette.Model;
using eFederalGazette;
using ReferencedCompany = eBalanceKitBusiness.FederalGazette.Model.ReferencedCompany;

namespace eBalanceKitBusiness.FederalGazette {

    public class TransferFederalGazetteData {
        public TransferFederalGazetteData(FederalGazetteMainModel model) {
            Model = model;
            _loginData = new LoginData {username = Model.Username, password = Model.Password};
            _orderReply = new OrderReplyInfo
            {html_direct_reply = Model.ShouldReceiveEmail, email = Model.EmailRecevingFederalGazetteAsHtml};
        }

        #region locals
        private LoginData _loginData;
        private FederalGazetteMainModel Model { get; set; }
        private OrderReplyInfo _orderReply;
        private FederalGazetteErrorAnalysis _err = new FederalGazetteErrorAnalysis();
        #endregion

        #region CheckOrder
        public void CheckOrder() {
            using (var federalGazetteService = new BAnzServicePortTypeClient()) {
                try {
                    //todo valid client number "2400000008"
                    var response = federalGazetteService.ebanzCheckOrder(_loginData, GetPublicationData(),
                                                                         "2400000008" /*Model.ClientNumber*/,
                                                                         _orderReply);
                    _err.AnalyseError(response);
                    if (response.result == 0 && Model.ShouldSaveHtml) {
                        Model.OrderNumber = response.resultOrderData.ordernumber;
                        SaveRecievedHtml(response.resultOrderData.html_content);
                    }

                } catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
        }
        #endregion

        #region NewOrder
        public void NewOrder() {
            using (var federalGazetteService = new BAnzServicePortTypeClient()) {
                try {

                    var response = federalGazetteService.ebanzNewOrder(_loginData, GetPublicationData(),
                                                                       Model.ClientNumber, Model.TicketId, _orderReply);
                    _err.AnalyseError(response);
                    if (response.result == 0 && Model.ShouldSaveHtml) {
                        Model.OrderNumber = response.resultOrderData.ordernumber;
                        SaveRecievedHtml(response.resultOrderData.html_content);
                    }
                } catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
        }
        #endregion

        #region ChangeOrder
        public void ChangeOrder() {
            using (var federalGazetteService = new BAnzServicePortTypeClient()) {
                try {
                    var response = federalGazetteService.ebanzChangeOrder(_loginData, GetPublicationData(),
                                                                          Model.ClientNumber, Model.TicketId,
                                                                          _orderReply);
                    _err.AnalyseError(response);
                    if (response.result == 0 && Model.ShouldSaveHtml) {
                        Model.OrderNumber = response.resultOrderData.ordernumber;
                        SaveRecievedHtml(response.resultOrderData.html_content);
                    }
                } catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
        }
        #endregion

        #region CancelOrder
        public bool CancelOrder() {
            using (var federalGazetteService = new BAnzServicePortTypeClient()) {
                try {

                    var response = federalGazetteService.ebanzCancelOrder(_loginData, Model.OrderNumber);
                    _err.AnalyseError(response);
                    if (response.result == 0) return true;
                } catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
            return false;
        }
        #endregion

        #region GetOrderList
        public List<FederalGazetteOrder> GetOrderList() {
            using (var federalGazetteService = new BAnzServicePortTypeClient()) {
                try {
                    var filter = new OrderFilter();
                    //set the filter props otherwise it will return all orders

                    var response = federalGazetteService.ebanzListOrders(_loginData, filter);
                    _err.AnalyseError(response);
                    if (response.result == 0) {
                        var orderList = new List<FederalGazetteOrder>();

                        foreach (var orderEntry in response.orderList) {
                            var entry = new FederalGazetteOrder {
                                ClientNumber = orderEntry.clientnumber,
                                CompanyName = orderEntry.company_name,
                                CreateDate = orderEntry.creation_date,
                                OrderNumber = orderEntry.ordernumber,
                                OrderStatus = orderEntry.status.ToString(),
                                Sign = orderEntry.sign
                            };
                            var dateList = new List<DateEntry>();
                            if (orderEntry.publication_date_list != null) {
                                dateList.AddRange(
                                    orderEntry.publication_date_list.Select(
                                        dateListEntry =>
                                        new DateEntry {Date = dateListEntry.date, DateType = dateListEntry.date_type}));
                                entry.DateList = dateList;
                            }
                            orderList.Add(entry);
                        }
                        return orderList;
                    }
                } catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
            return null;
        }
        #endregion

        #region GetOrderDetail
        public FederalGazetteMainModel GetOrderDetail() {
            var orderDetail = new FederalGazetteMainModel(null);

            using (var federalGazetteService = new BAnzServicePortTypeClient()) {
                try {
                    var response = federalGazetteService.ebanzGetOrder(_loginData, Model.OrderNumber);
                    _err.AnalyseError(response);
                    if (response.result == 0) {
                        orderDetail = GetOrderProperties(response.ebanzPublicationOrder);
                    }
                } catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
            return orderDetail;
        }
        #endregion

        #region GetOrderStatus
        public void GetOrderStatus() {
            using (var federalGazetteService = new BAnzServicePortTypeClient()) {
                try {
                    var response = federalGazetteService.ebanzGetOrderStatus(_loginData, Model.OrderNumber);
                    _err.AnalyseError(response);
                    if (response.result == 0) Model.OrderStatus = response.status.ToString();

                } catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
        }
        #endregion

        #region GetOrderReceipt
        public void GetOrderReceipt(string filePath) {
            using (var federalGazetteService = new BAnzServicePortTypeClient()) {
                try {
                    var response = federalGazetteService.ebanzGetReceipt(_loginData, Model.OrderNumber);
                    _err.AnalyseError(response);
                    if (response.result == 0) {
                        using (var writer = new StreamWriter(filePath + "\\" + response.receiptFile.filename)) {
                            //todo convert the file.
                            //response.receiptFile.file
                            writer.WriteLine(response.receiptFile.file);
                        }
                    }
                } catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
        }
        #endregion

        #region GetOrderProperties
        private FederalGazetteMainModel GetOrderProperties(ebanzPublicationOrderData data) {
            var orderData = new FederalGazetteMainModel(null);
            orderData.AcquisitionPeriodEnd = data.acquisition_period_end;
            orderData.AcquisitionPeriodStart = data.acquisition_period_start;

            //normally setting the ID in FederalGazetteModelElementInfo is enough. that's what we get.
            orderData.SelectedBalanceSheetStandard =
                new FederalGazetteModelElementInfo(data.balance_sheet_standard.ToString(), null);
            orderData.BB264 = data.bb_264;
            orderData.BH264 = data.bh_264;
            orderData.BlockingDate = data.blocking_date;
            orderData.SelectedCompanySize = new FederalGazetteModelElementInfo(data.company_size, null);
            orderData.SelectedCompanyType = new FederalGazetteModelElementInfo(data.company_type.ToString(), null);
            orderData.HasNaturalperson = data.has_natural_person;
            orderData.SelectedLanguage = new FederalGazetteModelElementInfo(data.language, null);
            orderData.OldRegisterData = data.oldregisterdata;
            orderData.SelectedOrderType = new FederalGazetteModelElementInfo(data.order_type.ToString(), null);
            orderData.SubmitterType = data.submitter_type != null && data.submitter_type.HasValue
                                          ? data.submitter_type.Value.ToString()
                                          : null;
            orderData.OriginalPublicationDate = data.orig_pub_date;
            orderData.OriginalPublicationOrderNumber = data.orig_pub_ordernumber;
            orderData.SelectedPublicatioanArea = new FederalGazetteModelElementInfo(data.publication_area.ToString(),
                                                                                    null);
            orderData.SelectedPublicationCategory =
                new FederalGazetteModelElementInfo(data.publication_category.ToString(), null);
            orderData.SelectedPublicationSubType =
                new FederalGazetteModelElementInfo(
                    data.publication_sub_type != null && data.publication_sub_type.HasValue
                        ? data.publication_sub_type.Value.ToString()
                        : null, null);
            orderData.SelectedPublicationType = new FederalGazetteModelElementInfo(data.publication_type.ToString(),
                                                                                   null);
            orderData.VU264 = data.vu_264;
            orderData.YourReference = data.your_reference;
            orderData.HasNaturalPersonSpecified = data.has_natural_personSpecified;
            var complist = new List<ReferencedCompany>();
            if (data.referencedCompanyList != null) {
                foreach (var comp in data.referencedCompanyList) {
                    var tmp = new ReferencedCompany();
                    tmp.CompanyId = comp.company_id;
                    tmp.CompanyLocation = comp.company_location;
                    tmp.CompanyName = comp.company_name;
                    tmp.Companytype = comp.company_type;
                    tmp.Country = comp.country;
                    tmp.LegalFormForeign = comp.legalform_other;
                    complist.Add(tmp);
                }
            }

            orderData.ReferencedCompanyList = complist;

            var billAdd = new BillingData();
            if (data.bill_address != null) {
                billAdd.CompanyName = data.bill_address.company;
                billAdd.Country = data.bill_address.country;
                billAdd.Devision = data.bill_address.devision;
                billAdd.Email = data.bill_address.email;
                billAdd.Fax = data.bill_address.fax.country_area_code + data.bill_address.fax.area_code +
                              data.bill_address.fax.number;
                billAdd.FirstName = data.bill_address.firstname;
                billAdd.LastName = data.bill_address.lastname;
                billAdd.Mobile = data.bill_address.mobile.country_area_code +
                                 data.bill_address.mobile.area_code + data.bill_address.mobile.number;
                billAdd.PostCode = data.bill_address.postbox;
                if (data.bill_address.salutation.HasValue)
                    billAdd.Salutation = data.bill_address.salutation.ToString().ToLower().Equals("herr")
                                             ? salutation_type.Herr.ToString()
                                             : salutation_type.Frau.ToString();
                billAdd.Street = data.bill_address.street;
                billAdd.Telephone = data.bill_address.telephone.country_area_code +
                                    data.bill_address.telephone.area_code + data.bill_address.telephone.number;
                billAdd.Title = data.bill_address.title;
                billAdd.Town = data.bill_address.town;
                billAdd.ZipCode = data.bill_address.zipcode;
            }
            orderData.BillAddress = billAdd;
            var billReceiver = new BillingData();
            if (data.bill_receiver != null) {
                billReceiver.CompanyName = data.bill_receiver.company;
                billReceiver.Country = data.bill_receiver.country;
                billReceiver.Devision = data.bill_receiver.devision;
                billReceiver.Email = data.bill_receiver.email;
                billReceiver.Fax = data.bill_receiver.fax.country_area_code + data.bill_address.fax.area_code +
                                   data.bill_receiver.fax.number;
                billReceiver.FirstName = data.bill_receiver.firstname;
                billReceiver.LastName = data.bill_receiver.lastname;
                billReceiver.Mobile = data.bill_receiver.mobile.country_area_code +
                                      data.bill_receiver.mobile.area_code + data.bill_receiver.mobile.number;
                billReceiver.PostCode = data.bill_receiver.postbox;
                if (data.bill_receiver.salutation.HasValue)
                    billReceiver.Salutation = data.bill_receiver.salutation.ToString().ToLower().Equals("herr")
                                                  ? salutation_type.Herr.ToString()
                                                  : salutation_type.Frau.ToString();
                billReceiver.Street = data.bill_receiver.street;
                billReceiver.Telephone = data.bill_receiver.telephone.country_area_code +
                                         data.bill_receiver.telephone.area_code +
                                         data.bill_receiver.telephone.number;
                billReceiver.Title = data.bill_receiver.title;
                billReceiver.Town = data.bill_receiver.town;
                billReceiver.ZipCode = data.bill_receiver.zipcode;
            }
            orderData.BillReceiver = billReceiver;

            var files = new List<FederalGazetteFiles>();

            foreach (var file in data.files) {
                var tmp = new FederalGazetteFiles();
                tmp.FileAnnualContentType = file.annual_content_type;
                //tmp.FileAnnualContentTypeFree = file.annual_content_type_free;
                //todo what the heck?! base64 file format so basically byte[] and contentType which is unknown
                //tmp.FileDataAsBase64 = file.file;
                tmp.FileName = file.file_name;
                files.Add(tmp);
            }
            Model.Files = files;


            return orderData;
        }
        #endregion

        #region  GetXbrlContentAsFile
        private void GetXbrlContentAsFile() {
            var federalGazetteManager = new FederalGazetteGetXbrl(Model);
            try {
                var files = new List<FederalGazetteFiles>();

                if (Model.ExportBalanceSheet) {
                    var file = new FederalGazetteFiles {
                        FileAnnualContentType = 3,
                        FileContent = federalGazetteManager.GetBalanceSheet(),
                        FileName = "balance_sheet.xml"
                    };

                    files.Add(file);
                }
                if (Model.ExportIncomeStatement) {
                    var file = new FederalGazetteFiles {
                        FileAnnualContentType = 4,
                        FileName = "income_statement.xml",
                        FileContent = federalGazetteManager.GetIncomeStatement()
                    };

                    files.Add(file);
                }
                if (Model.ExportNotes) {
                    var file = new FederalGazetteFiles {
                        FileAnnualContentType = 5,
                        FileName = "notes.xml",
                        FileContent = federalGazetteManager.GetNotes()
                    };

                    files.Add(file);
                }

                if (Model.ExportFixedAssets) {
                    var file = new FederalGazetteFiles {
                        FileAnnualContentType = 12,
                        //is "anlagespiegel" the same as "eingenkapital"? if yes then all good
                        FileName = "fixed_assets.xml",
                        FileContent = federalGazetteManager.GetFixedAssets()
                    };

                    files.Add(file);
                }

                if (Model.ExportManagementReport) {
                    var file = new FederalGazetteFiles {
                        FileAnnualContentType = 2,
                        FileName = "management_report.xml",
                        FileContent = federalGazetteManager.GetManagementReport()
                    };
                    files.Add(file);
                }

                if (Model.ExportNetProfit) {
                    var file = new FederalGazetteFiles {
                        FileAnnualContentType = 9,
                        //that should be checked if "beschluss über die ergebnisverwendung" is contained in "ergebnisverwendung" taxonomy!
                        FileName = "income_use.xml",
                        FileContent = federalGazetteManager.GetNetProfit()
                    };
                    files.Add(file);
                }

                Model.Files = files;
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region SaveRecievedHtml
        private void SaveRecievedHtml(base64Binary content) {
            try {
                var encoder = new ASCIIEncoding();
                var decoder = encoder.GetDecoder();

                var charCount = decoder.GetCharCount(content.Value, 0, content.Value.Length);
                var decodedChar = new char[charCount];
                decoder.GetChars(content.Value, 0, content.Value.Length, decodedChar, 0);

                using (var writeFile = new StreamWriter(Model.SaveHtmlFilePath)) {
                    writeFile.WriteLine(decodedChar);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region GetPublicationData
        private ebanzPublicationOrderData GetPublicationData() {
            var publicationData = new ebanzPublicationOrderData();
            try {
                publicationData.order_type = int.Parse(Model.SelectedOrderType.Id);
                publicationData.publication_area = int.Parse(Model.SelectedPublicatioanArea.Id);
                publicationData.publication_category = int.Parse(Model.SelectedPublicationCategory.Id);
                publicationData.publication_type = int.Parse(Model.SelectedPublicationType.Id);
                publicationData.publication_sub_type = int.Parse(Model.SelectedPublicationSubType.Id);
                publicationData.language = Model.SelectedLanguage.Id;
                publicationData.company_type = int.Parse(Model.SelectedCompanyType.Id);
                if (!string.IsNullOrEmpty(Model.SubmitterType))
                    publicationData.submitter_type = Model.SubmitterType.ToLower().Equals("parent")
                                                         ? submittertype_type.parent
                                                         : submittertype_type.subsidiary;

                publicationData.balance_sheet_standard = int.Parse(Model.SelectedBalanceSheetStandard.Id);
                publicationData.company_size = Model.SelectedCompanySize.Id;
                publicationData.blocking_date = Model.BlockingDate;
                publicationData.your_reference = Model.YourReference;

                GetXbrlContentAsFile();
                //publicationData.files = new ebanzPublicationFile[6];

                //if(Model.Files.Count<1) throw new Exception("Die Dateien fehlen.");

                //var fileList = new ebanzPublicationFile[Model.Files.Count];

                //for (var i = 0; i < Model.Files.Count; i++) {
                //    var file = new ebanzPublicationFile();
                //    file.annual_content_type = Model.Files[i].FileAnnualContentType;
                //    file.file_name = Model.Files[i].FileName;
                //    var tmp = new base64Binary();
                
                //    tmp.Value = Encoding.ASCII.GetBytes(Model.Files[i].FileContent.ToString());
                //    //tmp.contentType = "test";

                //    //todo contentType unclear and not documented. what to do?
                //    //tmp.contentType =????????????
                //    file.file = tmp;
                //    fileList[i] = file;
                //}
                //publicationData.files = fileList;

                if (Model.ReferencedCompanyList != null) {
                    var companyList = new eFederalGazette.ReferencedCompany[Model.ReferencedCompanyList.Count];

                    for (var i = 0; i < Model.ReferencedCompanyList.Count; i++) {
                        var companyRefernced = new eFederalGazette.ReferencedCompany {
                            company_id = Model.ReferencedCompanyList[i].CompanyId,
                            company_location = Model.ReferencedCompanyList[i].CompanyLocation,
                            company_name = Model.ReferencedCompanyList[i].CompanyName,
                            company_type = Model.ReferencedCompanyList[i].Companytype,
                            country = Model.ReferencedCompanyList[i].Country
                        };
                        companyList[i] = companyRefernced;
                    }

                    publicationData.referencedCompanyList = companyList;
                }

                publicationData.oldregisterdata = Model.OldRegisterData;
                publicationData.acquisition_period_start = Model.AcquisitionPeriodStart;
                publicationData.acquisition_period_end = Model.AcquisitionPeriodEnd;
                publicationData.bb_264 = Model.BB264;
                publicationData.vu_264 = Model.VU264;
                publicationData.bh_264 = Model.BH264;
                publicationData.orig_pub_date = Model.OriginalPublicationDate;
                publicationData.orig_pub_ordernumber = Model.OriginalPublicationOrderNumber;
                publicationData.has_natural_person = Model.HasNaturalperson;
                return publicationData;
            } catch (Exception) {
                throw new Exception("Ein Pflichtfeld ist nicht gesetzt.");
            }
        }
        #endregion

        #region
        private StringBuilder EncodeToBase64(StringBuilder data) {
            var encodeAsByte = Encoding.ASCII.GetBytes(data.ToString());
            var convertedData = new StringBuilder();
            convertedData.Append(Convert.ToBase64String(encodeAsByte));
            return convertedData;
        }
        #endregion
    }
}