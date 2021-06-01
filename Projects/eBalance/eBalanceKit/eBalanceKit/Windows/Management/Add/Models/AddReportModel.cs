// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Windows;
using Utils;
using eBalanceKit.Models.Document;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.Add.Models {
    public class AddReportModel : NotifyPropertyChangedBase {
        public AddReportModel(Window owner) { Owner = owner; }

        #region ContentModel
        private EditDocumentModel _contentModel;

        public EditDocumentModel ContentModel {
            get { return _contentModel; }
            set {
                if (_contentModel == value) return;
                _contentModel = value;
                OnPropertyChanged("ContentModel");
            }
        }
        #endregion // ContentModel
        
        public Document Document { get; private set; }
        public Window Owner { get; private set; }

        public void Cancel() { DocumentManager.Instance.DeleteDocument(Document); }
        
        public void InitContentModel(ProgressInfo pi) {
            try {
                Document = DocumentManager.Instance.AddDocument();
                Document.ReportRights = new ReportRights(Document);
                Document.LoadDetails(pi);
                ContentModel = new EditDocumentModel(Document, Owner);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, ResourcesCommon.ErrorsCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}