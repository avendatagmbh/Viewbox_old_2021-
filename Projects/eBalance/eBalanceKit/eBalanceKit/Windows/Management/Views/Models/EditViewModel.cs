using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils.Commands;
using eBalanceKit.Models;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.Management.Views.Models {
    public class EditViewModel {
        #region Constructor
        public EditViewModel(Document document) {
            CancelCommand = new DelegateCommand(o => true, Cancel);
            OkCommand = new DelegateCommand(o => true, OkClick);

            ShowSelectedLegalForm = document.Settings.ShowSelectedLegalForm;
            ShowOnlyMandatoryPostions = document.Settings.ShowOnlyMandatoryPostions;
            ShowSelectedTaxonomy = document.Settings.ShowSelectedTaxonomy;
            ShowTypeOperatingResult = document.Settings.ShowTypeOperatingResult;

            _document = document;
        }

        #endregion Constructor

        #region Properties

        private Document _document;

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand OkCommand { get; set; }

        public bool ShowSelectedLegalForm { get; set; }
        public bool ShowSelectedTaxonomy { get; set; }
        public bool ShowTypeOperatingResult { get; set; }
        public bool ShowOnlyMandatoryPostions { get; set; }
        #endregion Properties

        #region Methods
        private void Cancel(object windowAsObject) {
            ((Window) windowAsObject).Close();
        }

        private void OkClick(object windowAsObject) {
            _document.Settings.ShowSelectedLegalForm = ShowSelectedLegalForm;
            _document.Settings.ShowOnlyMandatoryPostions = ShowOnlyMandatoryPostions;
            _document.Settings.ShowSelectedTaxonomy = ShowSelectedTaxonomy;
            _document.Settings.ShowTypeOperatingResult = ShowTypeOperatingResult;
            _document.Settings.SaveConfiguration();

            foreach (var value in _document.GaapPresentationTrees.Values) {
                value.Filter.SetFilterAfterOptionChanged(ShowOnlyMandatoryPostions);
            }

            ((MainWindowModel) GlobalResources.MainWindow.DataContext).CheckNavigationVisibility();

            ((Window)windowAsObject).Close();
        }
        #endregion Methods
    }
}
