// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-15
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.ComponentModel;
using Taxonomy;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.AuditCorrections {
    public interface IAuditCorrectionTransaction : IsSelectable, INotifyPropertyChanged {
        IAuditCorrection Parent { get; }
        Document Document { get; }

        decimal? Value { get; set; }
        string Label { get; }
        IElement Element { get; }
        
        /// <summary>
        /// Removes this transaction from the parent AuditCorrection object.
        /// </summary>
        void Remove();

        void ShowInTreeview();
    }
}