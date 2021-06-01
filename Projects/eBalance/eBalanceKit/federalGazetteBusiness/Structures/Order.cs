// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.ServiceModel.Channels;
using DbAccess;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using federalGazetteBusiness.External;
using federalGazetteBusiness.Structures.Manager;

namespace federalGazetteBusiness.Structures {
    [DbTable("federalGazetteOrders", ForceInnoDb = true)]
    public class Order : Utils.NotifyPropertyChangedBase {

        protected void OnPropertyChanged(string propertyName) {
            ClassArgumentHelper.Instance.OnPropertyChangedWithStringValidater(propertyName, this);
            base.OnPropertyChanged(propertyName);
        }

        public Order() {
            Sent = System.DateTime.Now;
        }

        private Order(string ticketId, string xbrl, External.ebanzResultOrderData response) : this() {
            ContentSent = xbrl;
            TicketId = ticketId;
            if (response != null) {
                OrderNo = response.ordernumber;
                var memory = new System.IO.MemoryStream(response.html_content.Value);
                var htmlContent = System.Text.Encoding.Default.GetString(memory.ToArray());
                ResponseHtml = htmlContent;
            }
        }

        public Order(string ticketId, string xbrl, External.ebanzResultOrderData response, Message requestSOAP, Message responseSOAP) : this(ticketId, xbrl, response) {
            DisableSaving = true;
            FullRequest = requestSOAP == null ? null : requestSOAP.ToString();
            FullRsponse = responseSOAP == null ? null : responseSOAP.ToString();
            DisableSaving = false;
            Save();
        }

        public Order(string ticketId, string xbrl, External.ebanzResultOrderData response, FederalGazetteManagerBase.SoapCommunicationData soapCommunicationData) : this(ticketId, xbrl, response) {
            DisableSaving = true;
            if (soapCommunicationData != null) {
                FullRequest = soapCommunicationData.Request.ToString();
                FullRsponse = soapCommunicationData.Response.ToString();
                Sent = soapCommunicationData.RequestTime;   
            }
            DisableSaving = false;
            // we save it in one step because it has better performance than saving every changed value
            Save();
        }

        [DbColumn("id")]
        [DbPrimaryKey]
        public int Id { get; set; }
        
        [DbColumn("document_id", AllowDbNull = false)]
        public int DocumentId { get; set; }

        public Document Document { get { return eBalanceKitBusiness.Manager.DocumentManager.Instance.GetDocument(DocumentId); } }

        [DbColumn("ticket_id", AllowDbNull = true)]
        public string TicketId { get; set; }
        

        #region OrderNo
        private string _orderNo;

        [DbColumn("order_No", AllowDbNull = false)]
        public string OrderNo {
            get { return _orderNo; }
            set {
                if (_orderNo != value) {
                    _orderNo = value;
                    Save();
                }
            }
        }
        #endregion // OrderNo


        #region ContentSent
        private string _contentSent;

        [DbColumn("xbrl", AllowDbNull = false, Length = MaxLength)]
        public string ContentSent {
            get { return _contentSent; }
            set {
                if (_contentSent != value) {
                    _contentSent = value.Substring(0, MaxLength);
                    Save();
                }
            }
        }
        #endregion // ContentSent

        #region FullRequest
        private string _fullRequest;

        [DbColumn("request_soap", AllowDbNull = false, Length = MaxLength)]
        public string FullRequest {
            get { return _fullRequest; }
            set {
                if (_fullRequest != value) {
                    _fullRequest = value.Substring(0, MaxLength);
                    Save();
                }
            }
        }
        #endregion // FullRequest

        #region FullRsponse
        private string _fullRsponse;

        [DbColumn("response_soap", AllowDbNull = true, Length = MaxLength)]
        public string FullRsponse {
            get { return _fullRsponse; }
            set {
                if (_fullRsponse != value) {
                    _fullRsponse = value.Substring(0, MaxLength);
                    Save();
                }
            }
        }
        #endregion // FullRsponse
        
        #region ResponseHtml
        private string _responseHtml;

        [DbColumn("html", AllowDbNull = true, Length = MaxLength)]
        public string ResponseHtml {
            get { return _responseHtml; }
            set {
                if (_responseHtml != value) {
                    _responseHtml = value.Substring(0, MaxLength);
                    Save();
                }
            }
        }
        #endregion // ResponseHtml


        #region Sent
        private DateTime _sent;

        [DbColumn("date_sent", AllowDbNull = false)]
        public DateTime Sent {
            get { return _sent; }
            set {
                if (_sent != value) {
                    _sent = value;
                    Save();
                }
            }
        }
        #endregion // Sent

        #region LastChecked
        private DateTime _lastChecked;

        [DbColumn("date_last_check", AllowDbNull = true)]
        public DateTime LastChecked {
            get { return _lastChecked; }
            set {
                if (_lastChecked != value) {
                    _lastChecked = value;
                    Save();
                }
            }
        }
        #endregion // LastChecked

        /// <summary>
        /// Gets or Sets if the Order should be changed as soon as a value changes
        /// </summary>
        public static bool DisableSaving { get; set; }

        private const int MaxLength = int.MaxValue;

        public void Save() {

            if (DisableSaving) {
                return;
            }

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {

                try {
                    conn.DbMapping.Save(this);
                }
                catch (Exception ex) {

                    throw new Exception(ex.Message, ex);
                }
            }
        }

        //public void GetStatus(string username, string password) {
        //    LastChecked = System.DateTime.Now;
            
        //}

        //public void GetStatus(FederalGazetteManager federalGazetteManager) {
        //    var login = federalGazetteManager.GetLogin();
        //    var client = federalGazetteManager.GetBAnzClient();
        //    client.ebanzGetOrderStatus(login, OrderNo)
        //}

        #region Status
        private orderStatusType? _status;

        public orderStatusType? Status {
            get { return _status; }
            set {
                if (_status != value) {
                    _status = value;
                    OnPropertyChanged("Status");
                }
            }
        }
        #endregion // Status

    }
}