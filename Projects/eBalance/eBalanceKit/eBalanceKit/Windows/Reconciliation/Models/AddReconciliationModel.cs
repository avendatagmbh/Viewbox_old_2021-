// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-07-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using Utils;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.Reconciliation.Models {
       
    public class AddReconciliationModel : NotifyPropertyChangedBase{
        public AddReconciliationModel(Window owner, Document document) {
            Owner = owner;
            Document = document;
            Errors = new ObservableCollectionAsync<string>();
        }

        #region Properties
        public Window Owner { get; set; }
        public Document Document { get; set; }

        public bool IsTaxBalanceSheetValueReconciliationMode { get { return Document.ReconciliationMode == ReconciliationMode.TaxBalanceSheetValues; } }
        public bool IsCustomReconciliation { get; set; }
        //public string Name { get; set; }

        #region Name
        private string _name;

        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    Validate();
                    OnPropertyChanged("Name");
                }
            }
        }
        #endregion Name
        public string Comment { get; set; }
        //public TransferKinds TransferKind { get; set; }

        #region TransferKind
        private TransferKinds _transferKind;

        public TransferKinds TransferKind {
            get { return _transferKind; }
            set {
                if (_transferKind != value) {
                    _transferKind = value;
                    OnPropertyChanged("TransferKind");
                }
            }
        }
        #endregion TransferKind
        public IReconciliation NewReconciliation { get; private set; }
        public ObservableCollectionAsync<string> Errors { get; set; }
        #endregion Properties

        public bool Validate() {
            Errors.Clear();
            if (string.IsNullOrEmpty(Name)) {
                Errors.Add("Sie haben noch keinen Namen für die Überleitung eingegeben");
                return false;
            }
            return true;
        }

        public void AddReconciliation() {
            IReconciliation reconciliation;

            if (IsTaxBalanceSheetValueReconciliationMode) {
                reconciliation = Document.ReconciliationManager.AddReconciliation(ReconciliationTypes.TaxBalanceValue);
                reconciliation.TransferKind = TransferKinds.ReclassificationWithValueChange;
            } else if (IsCustomReconciliation) {
                reconciliation = Document.ReconciliationManager.AddReconciliation(ReconciliationTypes.Delta);
                reconciliation.TransferKind = TransferKind;

            } else switch (TransferKind) {
                    case TransferKinds.Reclassification:
                        reconciliation = Document.ReconciliationManager.AddReconciliation(ReconciliationTypes.Reclassification);
                        break;

                    case TransferKinds.ValueChange:
                        reconciliation = Document.ReconciliationManager.AddReconciliation(ReconciliationTypes.ValueChange);
                        break;

                    case TransferKinds.ReclassificationWithValueChange:
                        reconciliation = Document.ReconciliationManager.AddReconciliation(ReconciliationTypes.Delta);                        
                    break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

            reconciliation.Name = Name;
            reconciliation.Comment = Comment;
            NewReconciliation = reconciliation;
        }
    }
}