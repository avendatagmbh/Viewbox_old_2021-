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
using TransDATA.Windows;
using log4net;

namespace TransDATA.Models {
    internal class ImportDbStructureModel {
        internal ILog _log = LogHelper.GetLogger();

        #region Constructor
        internal ImportDbStructureModel(Window owner) { Owner = owner; }
        #endregion Constructor

        #region Properties
        public IImporterDbStructure Importer { get; set; }
        private Window Owner {get; set; }
        #endregion Properties

        #region Methods
        public void StartImportDbStructure(IProfile profile) {
            _log.ContextLog( LogLevelEnum.Debug,"");

            Importer = AppController.GetImporterDbStructure(profile);
            //DataContext = Importer.ImportProgress;
            Importer.Finished += ImportDbStructureFinished;
            Importer.Start();
        }
        private void ImportDbStructureFinished(object sender, System.EventArgs e) {
            if (Owner.Dispatcher.CheckAccess()) {
                _log.ContextLog( LogLevelEnum.Debug,"");

                if (Importer.ImportProgress.ErrorMessages.Count != 0) {
                    string errorMessage = ResourcesCommon.ImportStepFinished + Environment.NewLine +
                                          "Folgende Fehler sind aufgetreten:" + Environment.NewLine +
                                          string.Join(Environment.NewLine, Importer.ImportProgress.ErrorMessages);

                    _log.ContextLog( LogLevelEnum.Error, "{0}", errorMessage);

                    MessageBox.Show(Owner.Owner,
                                    errorMessage,
                                    ResourcesCommon.ImportStepFinishedCaption,
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                MessageBox.Show(Owner.Owner, ResourcesCommon.ImportStepFinished, ResourcesCommon.ImportStepFinishedCaption,
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                Owner.Close();

            } else {
                Owner.Dispatcher.Invoke(new EventHandler(ImportDbStructureFinished), new[] { sender, e });
            }
        }

        public void Cancel() {
            _log.ContextLog( LogLevelEnum.Debug,"");

            if (MessageBox.Show(ResourcesCommon.RequestCancelExport, ResourcesCommon.RequestCancelExportCaption,
                                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
                MessageBoxResult.No)
                return;

            Importer.Cancel();
            Owner.Close();
        }
        #endregion Methods
    }
}
