// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-04-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Input;
using Utils;
using Utils.Commands;
using eBalanceKitBusiness.Options;
using eBalanceKitBusiness.Structures;

namespace eBalanceKit.Models {
    public class GlobalOptionsModel {

        internal GlobalOptionsModel() {
            _saveCommand = new DelegateCommand((obj) => true, SaveOptions);
            _closeCommand = new DelegateCommand((obj) => true, Cancel);

            // We do it this way because we just want to save the configuration if the SaveCommand is executed
            HideChosenWarnings = Options.HideChosenWarnings;
            HideAllWarnings = Options.HideAllWarnings;
            ShowSelectedLegalForm = Options.ShowSelectedLegalForm;
            ShowSelectedTaxonomy = Options.ShowSelectedTaxonomy;
            ShowTypeOperatingResult = Options.ShowTypeOperatingResult;
            ShowOnlyMandatoryPostions = Options.ShowOnlyMandatoryPostions;
            AuditModeEnabled = Options.AuditModeEnabled;
        }

        internal GlobalOptionsModel(MainWindowModel mainWindowModel) : this() { this._mainWindowModel = mainWindowModel; }

        private readonly MainWindowModel _mainWindowModel;
        private IOptions Options { get { return GlobalUserOptions.UserOptions; } }

        public bool HideChosenWarnings { get; set; }
        public bool HideAllWarnings { get; set; }
        public bool ShowSelectedLegalForm { get; set; }
        public bool ShowSelectedTaxonomy { get; set; }
        public bool ShowTypeOperatingResult { get; set; }
        public bool ShowOnlyMandatoryPostions { get; set; }
        public bool AuditModeEnabled { get; set; }
        
        #region Save
        private readonly ICommand _saveCommand;

        public ICommand SaveCommand {
            get { return _saveCommand; }
        }

        /// <summary>
        /// Save the current configuration, if necessary validate again and refresh everything that is connected with the options.
        /// </summary>
        /// <param name="window">The options window.</param>
        private void SaveOptions(object window) {

            Options.HideChosenWarnings = HideChosenWarnings;
            Options.HideAllWarnings = HideAllWarnings;
            Options.ShowSelectedLegalForm = ShowSelectedLegalForm;
            Options.ShowSelectedTaxonomy = ShowSelectedTaxonomy;
            Options.ShowTypeOperatingResult = ShowTypeOperatingResult;
            Options.ShowOnlyMandatoryPostions = ShowOnlyMandatoryPostions;
            Options.AuditModeEnabled = AuditModeEnabled;

            if (_mainWindowModel.CurrentDocument != null) {
                foreach (var value in _mainWindowModel.CurrentDocument.GaapPresentationTrees.Values) {
                    value.Filter.ShowOnlyMandatoryPostions = ShowOnlyMandatoryPostions;
                }
            }

            if (Options.SaveConfiguration() && _mainWindowModel.CurrentDocument != null) {
                if (_mainWindowModel.ValidationExecuted) {
                    _mainWindowModel.NavigationTree.RemoveValidationInfos();
                    string refParam = string.Empty;
                    _mainWindowModel.CurrentDocument.Validate(ref refParam);
                    _mainWindowModel.NavigationTree.Validate();
                }
                RefreshAllOptionRelatedStuff();
            }
            
            Close(window);
        }

        /// <summary>
        /// Refresh all things (NavigationTree, TaxonomyTree ...)
        /// </summary>
        private void RefreshAllOptionRelatedStuff() {
            _mainWindowModel.CheckNavigationVisibility();
            _mainWindowModel.NavigationTree.UpdateVisibilityValidationWarning(_mainWindowModel.CurrentDocument);
        }
        #endregion Save

        #region Cancel
        private readonly ICommand _closeCommand;

        public ICommand CloseCommand {
            get { return _closeCommand; }
        }

        private void Cancel(object window) {
            //Options.Reset();
            Close(window);
        }

        private void Close(object window) {
            try {

                if (window == null) {
                    var mainWindow =
                        Utils.UIHelpers.TryFindParent<Windows.MainWindow>(
                            (DependencyObject)_mainWindowModel.NavigationTree.NavigationTreeReport);
                    window = Utils.UIHelpers.FindVisualChild<Windows.DlgGlobalOptions>(mainWindow);
                }
                ((System.Windows.Window)window).Close();
            }
            catch (System.Exception) {

            }
        }

        #endregion Cancel
        

    }
}