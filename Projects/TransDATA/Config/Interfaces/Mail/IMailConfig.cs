// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-02-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Utils;

namespace Config.Interfaces.Mail {
    public interface IMailConfig {
        IMailConfig Clone();
        MailConnectionType MailConnectionType { get; set; }
        string Host { get; set; }
        string RecipentsXML { get; }
        int Port { get; set; }
        string User { get; set; }
        string Password { get; set; }
        bool UseSsl { get; set; }
        bool SendStatusmail { get; set; }
        DateTime StatusmailSendTime { get; set; }
        ObservableCollectionAsync<string> Recipents { get; set; }

        void Save();

        void RemoceRecipient(string recipient);
        void AddRecipient(string recipient);
    }

    public enum MailConnectionType {
        SMTP
    }
}