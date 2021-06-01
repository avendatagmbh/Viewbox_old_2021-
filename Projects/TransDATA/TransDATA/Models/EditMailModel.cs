using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Config.Interfaces.Mail;

namespace TransDATA.Models {
    public class EditMailModel {
        #region Constructor
        public EditMailModel(IMailConfig mailConfig) {
            MailConfig = mailConfig;
        }
        #endregion Constructor

        #region Properties
        public IMailConfig MailConfig { get; set; }
        public bool Saved { get; set; }
        #endregion Properties

        #region Methods
        #endregion Methods

        public void Save(Window owner) {
            Saved = true;
            MailConfig.Save();
            owner.Close();
        }

        public void Cancel(Window owner) {
            Saved = false;
            owner.Close();
        }

        internal void AddRecipient(string recipient) { MailConfig.AddRecipient(recipient); }

        internal void RemoveRecipient(string recipient) { MailConfig.RemoceRecipient(recipient); }
    }
}
