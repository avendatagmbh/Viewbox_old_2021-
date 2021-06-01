using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using AV.Log;
using Base.Localisation;
using Business;
using Business.Interfaces;
using Config.Interfaces.DbStructure;
using Utils;
using log4net;

namespace TransDATA.Models {
    public class TransferModel : NotifyPropertyChangedBase{
        internal ILog _log = LogHelper.GetLogger();

        #region Constructor
        public TransferModel(IProfile profile, Window owner) {
            _log.ContextLog( LogLevelEnum.Debug,"TransferModel Initiated. Profile.Id: {0}", profile!=null?profile.Id:0);
            Profile = profile;
            _owner = owner;
        }
        #endregion Constructor

        #region Properties
        private IProfile Profile { get; set; }
        private Window _owner;
        //public IDataTransferAgent TransferAgent { get; set; }

        #region TransferAgent
        private IDataTransferAgent _transferAgent;

        public IDataTransferAgent TransferAgent {
            get { return _transferAgent; }
            set {
                if (_transferAgent != value) {
                    _transferAgent = value;
                    OnPropertyChanged("TransferAgent");
                }
            }
        }
        #endregion TransferAgent

        #endregion Properties

        #region Methods
        public void Cancel() {
            _log.ContextLog( LogLevelEnum.Info,"TransferModel.Cancel");
            if (MessageBox.Show(ResourcesCommon.RequestCancelExport, ResourcesCommon.RequestCancelExportCaption,
                                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
                MessageBoxResult.No)
                return;
            TransferAgent.Cancel();
            _owner.Close();
        }

        public bool StartExport() {
            _log.ContextLog( LogLevelEnum.Info,"Validate Begin");

            string error;
            bool hasExportTable = Profile.Tables.Any(table => table.DoExport);
            if (!hasExportTable) {
                error = ResourcesCommon.TransferModelNoTablesSelected;
                _log.ContextLog( LogLevelEnum.Error, "Validation Error: {0}", error);
                ValidationError(error);
                return false;
            }

            if (!Profile.InputConfig.Config.Validate(out error)) {
                _log.ContextLog( LogLevelEnum.Error, "Profile.InputConfig.Config.Validate Error: {0}", error);
                ValidationError(error);
                return false;
            }
            if (!Profile.OutputConfig.Config.Validate(out error)) {
                _log.ContextLog( LogLevelEnum.Error, "Profile.OutputConfig.Config.Validate Error: {0}", error);
                ValidationError(error);
                return false;
            }

            TransferAgent = AppController.GetDataTransferAgent(Profile);
            TransferAgent.Finished += TransferFinished;
            _log.ContextLog( LogLevelEnum.Info,"Validate OK");
            TransferAgent.Start();
            return true;
        }

        private void ValidationError(string text) {
            _log.ContextLog( LogLevelEnum.Error, "TransferModel.ValidationError {0}", text);
            MessageBox.Show(_owner.Owner, text, "", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void TransferFinished(object sender, EventArgs e)
        {
            if (_owner.Dispatcher.CheckAccess()) {
                _log.ContextLog( LogLevelEnum.Info,"TransferModel.TransferFinished");

                MessageBox.Show(_owner, ResourcesCommon.TransferFinished, ResourcesCommon.TransferFinishedCaption,
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                _owner.Close();

            } else {
                _owner.Dispatcher.Invoke(new EventHandler(TransferFinished), new[] { sender, e });
            }
        }
        #endregion Methods
    }
}
