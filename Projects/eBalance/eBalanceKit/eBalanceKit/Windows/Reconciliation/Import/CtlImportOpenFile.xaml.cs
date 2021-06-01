using System.Windows;
using Utils;
using eBalanceKitBase.Interfaces;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.Reconciliation.Import {
    public partial class CtlImportOpenFile {
        public CtlImportOpenFile() { InitializeComponent(); }

        private IAssistedImport Model { get { return DataContext as IAssistedImport; } }
    }
}