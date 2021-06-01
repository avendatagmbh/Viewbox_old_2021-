// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-15
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Utils;
using eBalanceKitBusiness.AuditCorrections.Interfaces;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.AuditCorrections.ViewModels {
    public class AuditCorrectionsModel : NotifyPropertyChangedBase {
        #region DocumentWrapper
        private ObjectWrapper<Document> _documentWrapper;

        public ObjectWrapper<Document> DocumentWrapper {
            get { return _documentWrapper; }
            set {
                if (_documentWrapper != null) {
                    _documentWrapper.PropertyChanged -= DocumentWrapperPropertyChanged;
                    _documentWrapper.Value.AuditCorrectionManager.PropertyChanged -= AuditCorrectionManagerPropertyChanged;
                }
                _documentWrapper = value;
                if (_documentWrapper != null) {
                    _documentWrapper.PropertyChanged += DocumentWrapperPropertyChanged;
                    _documentWrapper.Value.AuditCorrectionManager.PropertyChanged += AuditCorrectionManagerPropertyChanged;
                }
                OnPropertyChanged("");
            }
        }

        void AuditCorrectionManagerPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) { OnPropertyChanged(e.PropertyName); }
        void DocumentWrapperPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) { if (e.PropertyName == "Value") OnPropertyChanged(""); }
        #endregion // DocumentWrapper

        public Document Document { get { return _documentWrapper.Value; } }

        #region ACM (AuditCorrectionManager)
        /// <summary>
        /// AuditCorrectionManager.
        /// </summary>
        private IAuditCorrectionManager ACM {
            get {
                return _documentWrapper == null
                           ? null
                           : _documentWrapper.Value == null ? null : _documentWrapper.Value.AuditCorrectionManager;
            }
        }
        #endregion // ACM

        #region PositionCorrection
        public IEnumerable<IAuditCorrection> PositionCorrections { get { return ACM == null ? null : ACM.PositionCorrections; } }

        #region SelectedPositionCorrection
        private IAuditCorrection _selectedPositionCorrection;

        public IAuditCorrection SelectedPositionCorrection {
            get { return _selectedPositionCorrection; }
            set {
                if (_selectedPositionCorrection == value) return;
                _selectedPositionCorrection = value;
                OnPropertyChanged("SelectedPositionCorrection");
                OnPropertyChanged("DeletePositionsCorrectionValueAllowed");
            }
        }
        #endregion // SelectedPositionCorrection

        public bool AddPositionsCorrectionValueAllowed { get { return PositionCorrections != null; } }
        public bool DeletePositionsCorrectionValueAllowed { get { return SelectedPositionCorrection != null; } }

        public IAuditCorrection AddPositionCorrection() { return ACM == null ? null : ACM.AddPositionCorrection(); }
        public void DeleteSelectedPositionCorrection() {
            if (ACM != null && SelectedPositionCorrection != null)
                ACM.DeletePositionCorrection(SelectedPositionCorrection);

            SelectedPositionCorrection = null;
        }

        #endregion // PositionCorrection

        #region ReconciliationCorrection
        public IEnumerable<IAuditCorrection> ReconciliationCorrections { get { return ACM == null ? null : ACM.ReconciliationCorrections; } }
        public IAuditCorrection AddReconciliationCorrection() { return ACM == null ? null : ACM.AddReconciliationCorrection(); }
        #endregion // ReconciliationCorrection

        #region PreviousYearReconciliationCorrection
        public IEnumerable<IAuditCorrection> PreviousYearReconciliationCorrections { get { return ACM == null ? null : ACM.PreviousYearReconciliationCorrections; } }
        public IAuditCorrection AddPreviousYearReconciliationCorrection() { return ACM == null ? null : ACM.AddPreviousYearReconciliationCorrection(); }
        #endregion // PreviousYearReconciliationCorrection
    }
}