// --------------------------------------------------------------------------------
// author: Benjamin Held / Mirko Dibbert
// since:  2011-07-01
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.ComponentModel;
using DbAccess;

namespace eBalanceKitBusiness.Logs.DbMapping {
    [DbTable("log_send", ForceInnoDb = true)]
    internal class DbSendLog : DbLogBase {
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events

        #region properties

        #region DocumentId
        [DbColumn("document_id", AllowDbNull = false)]
        public long DocumentId { get; set; }
        #endregion

        #region SendData
        public string _sendData;

        [DbColumn("send_data", Length = 8000000, AllowDbNull = false)]
        public string SendData {
            get { return _sendData; }
            set {
                _sendData = value;
                OnPropertyChanged("SendData");
            }
        }
        #endregion

        #region ReportInfo
        public string _reportInfo;

        [DbColumn("report_info", Length = 100000, AllowDbNull = false)]
        public string ReportInfo {
            get { return _reportInfo; }
            set {
                _reportInfo = value;
                OnPropertyChanged("ReportInfo");
            }
        }
        #endregion

        #region SendError
        public enum SendErrorType {
            NoError,
            VerificationError,
            SendError,
            UnknownError
        }

        private SendErrorType _sendError;

        [DbColumn("send_error", AllowDbNull = false)]
        public SendErrorType SendError {
            get { return _sendError; }
            set {
                _sendError = value;
                OnPropertyChanged("SendError");
            }
        }
        #endregion

        #region ResultMessage
        public string _resultMessage;

        [DbColumn("result_message", Length = 30000, AllowDbNull = false)]
        public string ResultMessage {
            get { return _resultMessage; }
            set {
                _resultMessage = value;
                OnPropertyChanged("ResultMessage");
            }
        }
        #endregion

        #endregion properties
    }
}