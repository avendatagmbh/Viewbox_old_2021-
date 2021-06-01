// --------------------------------------------------------------------------------
// author: Gábor Bauer
// since: 2012-05-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using eBalanceKitBusiness.Import.ImportCompanyDetails;
using eBalanceKitBusiness.Reconciliation.Import;
using eBalanceKitBusiness.Reconciliation.ViewModels;

namespace eBalanceKitBusiness.Reconciliation.Interfaces {
    public interface IValueImporter {
        ReconciliationsModel ReconciliationsModel { get; }
        void ImportFile(string fileName);
        void CreateReconciliations();
        ObservableCollectionAsync<WrongTaxonomyIdError> TaxonomyIdErrors { get; }
        ObservableCollectionAsync<InvalidValueError> TaxonomyValueErrors { get; }
        ObservableCollectionAsync<ImportRowError> ImportRowErrors { get; }
        bool HasErrors { get; }
        bool HasImportRowErrors { get; }
        bool HasTaxonomyIdErrors { get; }
        bool HasTaxonomyValueErrors { get; }
        string ImportStatusMessage { get; }
    }
}
