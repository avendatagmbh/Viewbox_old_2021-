// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-02-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using Config.Interfaces.Mail;
using DbAccess;
using Utils;

namespace Config.DbStructure {
    [DbTable("Mail_config")]
    public class MailConfig : NotifyPropertyChangedBase, IMailConfig {

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }
        #endregion

        #region MailConnectionType
        private MailConnectionType _mailConnectionType;

        [DbColumn("MailConnectionType")]
        public MailConnectionType MailConnectionType {
            get { return _mailConnectionType; }
            set {
                _mailConnectionType = value;
                OnPropertyChanged("MailConnectionType");
            }
        }
        #endregion

        #region Host
        private string _host;

        [DbColumn("Host")]
        public string Host {
            get { return _host; }
            set {
                _host = value;
                OnPropertyChanged("Host");
            }
        }
        #endregion

        #region Port
        private int _port;

        [DbColumn("Port")]
        public int Port {
            get { return _port; }
            set {
                _port = value;
                OnPropertyChanged("Port");
            }
        }
        #endregion

        #region User
        private string _user;

        [DbColumn("User")]
        public string User {
            get { return _user; }
            set {
                _user = value;
                OnPropertyChanged("User");
            }
        }
        #endregion

        #region RecipentsXML
        [DbColumn("Recipents")]
        public string RecipentsXML {
            get { return GetRecipentsXML(); }
            set { SetRecipentsFromXML(value);
                OnPropertyChanged("Recipents");
            }
        }
        #endregion RecipentsXML

        #region Recipents
        private ObservableCollectionAsync<string> _recipents;

        public ObservableCollectionAsync<string> Recipents {
            get { return _recipents ?? (_recipents = new ObservableCollectionAsync<string>()); }
            set {
                if (_recipents != value) {
                    _recipents = value;
                    OnPropertyChanged("Recipents");
                }
            }
        }
        #endregion Recipents

        #region Password
        private string _password;

        [DbColumn("Password")]
        public string Password {
            get { return _password; }
            set {
                _password = value;
                OnPropertyChanged("Password");
            }
        }
        #endregion

        #region UseSsl
        private bool _useSsl;

        [DbColumn("UseSsl")]
        public bool UseSsl {
            get { return _useSsl; }
            set {
                _useSsl = value;
                OnPropertyChanged("UseSsl");
            }
        }
        #endregion

        #region SendStatusmail
        private bool _sendStatusmail;

        [DbColumn("SendStatusmail")]
        public bool SendStatusmail {
            get { return _sendStatusmail; }
            set {
                _sendStatusmail = value;
                OnPropertyChanged("SendStatusmail");
            }
        }
        #endregion

        #region StatusmailSendTime
        private DateTime _statusmailSendTime;

        [DbColumn("StatusmailSendTime")]
        public DateTime StatusmailSendTime {
            get { return _statusmailSendTime; }
            set {
                _statusmailSendTime = value;
                OnPropertyChanged("StatusmailSendTimeHour");
                OnPropertyChanged("StatusmailSendTimeMinute");
            }
        }
        #endregion

        #region StatusmailSendTimeHour
        public int StatusmailSendTimeHour {
            get { return StatusmailSendTime.Hour; }
            set {
                while (_statusmailSendTime.Hour < value) {
                    StatusmailSendTime = _statusmailSendTime.AddHours(1);
                }
                while (_statusmailSendTime.Hour > value) {
                    StatusmailSendTime = _statusmailSendTime.AddHours(-1);
                }
            }
        }
        #endregion

        #region StatusmailSendTimeMinute
        public int StatusmailSendTimeMinute {
            get { return StatusmailSendTime.Minute; }
            set {
                while (_statusmailSendTime.Minute < value) {
                    StatusmailSendTime = _statusmailSendTime.AddMinutes(1);
                }
                while (_statusmailSendTime.Minute > value) {
                    StatusmailSendTime = _statusmailSendTime.AddHours(-1);
                }
            }
        }
        #endregion

        #region DoDbUpdate
        internal bool DoDbUpdate { get; set; }
        #endregion

        #region Save
        public void Save() { ConfigDb.Save(this); }
        #endregion Save

        public IMailConfig Clone() {
            MailConfig mailConfig = new MailConfig();
            mailConfig.Id = Id;
            mailConfig.MailConnectionType = MailConnectionType;
            mailConfig.Host = Host;
            mailConfig.Port = Port;
            mailConfig.User = User;
            mailConfig.Password = Password;
            mailConfig.UseSsl = UseSsl;
            mailConfig.SendStatusmail = SendStatusmail;
            mailConfig.StatusmailSendTime = StatusmailSendTime;
            mailConfig.StatusmailSendTimeHour = StatusmailSendTimeHour;
            mailConfig.StatusmailSendTimeMinute = StatusmailSendTimeMinute;
            foreach(var recipent in Recipents) mailConfig.Recipents.Add(recipent);

            return mailConfig;
        }

        public string GetRecipentsXML() {
            var result = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine };
            var writer = XmlWriter.Create(result, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("Recipents");
            foreach (var recipent in Recipents) {
                writer.WriteElementString("Recipent", recipent);
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();

            return result.ToString();
        }

        private void SetRecipentsFromXML(string value) {
            var doc = new XmlDocument();
            doc.LoadXml(value);
            var root = doc.DocumentElement;
            foreach (XmlNode child in root.ChildNodes) {
                switch (child.Name) {
                    case "Recipent":
                        Recipents.Add(child.InnerText);
                        break;
                }
            }
        }

        public void RemoceRecipient(string recipient) {
            Recipents.Remove(recipient);
            OnPropertyChanged("Recipents");
        }

        public void AddRecipient(string recipient) {
            Recipents.Add(recipient);
            OnPropertyChanged("Recipents");
        }
    }
}