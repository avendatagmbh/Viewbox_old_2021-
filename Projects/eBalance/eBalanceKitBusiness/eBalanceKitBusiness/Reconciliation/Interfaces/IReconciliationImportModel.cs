using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils.Commands;
using eBalanceKitBase.Interfaces;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Reconciliation.Import;
using eBalanceKitBusiness.Reconciliation.ViewModels;

namespace eBalanceKitBusiness.Reconciliation.Interfaces {
    internal interface IReconciliationImportModel : IAssistedImport {
        ReconciliationsModel ReconciliationsModel { get; }
        ValueImporter Importer { get; }
        bool? HasErrors { get; }
        void SetCreateExampleFileCommand(DelegateCommand command);
    }
}
