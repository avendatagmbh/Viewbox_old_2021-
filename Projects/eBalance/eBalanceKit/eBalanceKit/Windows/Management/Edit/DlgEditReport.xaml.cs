using System;
using System.Windows.Input;
using System.Windows.Threading;
using eBalanceKit.Models.Document;
using eBalanceKitBase.Structures;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.Edit {
    public partial class DlgEditReport {
        public DlgEditReport(Document document) {
            InitializeComponent();
            Document = document;
            ctlReportOverview.Owner = this;
        }

        protected Document Document { get; set; }

        private void WindowKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                if (DocumentManager.Instance.CurrentDocument == null || Document.Id != DocumentManager.Instance.CurrentDocument.Id) {
                    Document.ClearDetails();
                }
                DialogResult = false;
            }
        }

        private void BtnOkClick(object sender, System.Windows.RoutedEventArgs e) {
            if (DocumentManager.Instance.CurrentDocument == null || Document.Id != DocumentManager.Instance.CurrentDocument.Id) {
                Document.ClearDetails();
            }
            DialogResult = true;
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            new DlgProgress(this) { ProgressInfo = { Caption = ResourcesCommon.ProgressReportLoading, IsIndeterminate = true } }.
                ExecuteModal(() => {
                    if (DocumentManager.Instance.CurrentDocument == null || Document.Id != DocumentManager.Instance.CurrentDocument.Id) {
                        Document.ReportRights = new ReportRights(Document);
                        var progress = new ProgressInfo();
                        Document.LoadDetails(progress);
                    }

                    if (!Dispatcher.CheckAccess()) {
                            Dispatcher.Invoke(
                                DispatcherPriority.Background,
                                new Action(
                                    delegate { DataContext = new EditDocumentModel(Document, this); }));
                        } else {
                            DataContext = new EditDocumentModel(Document, this);
                        }
                });
        }
    }
}