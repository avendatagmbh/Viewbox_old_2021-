// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-15
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.AuditCorrections {
    public interface IAuditCorrection : INotifyPropertyChanged {
        string Name { get; set; }
        string Comment { get; set; }
        Document Document { get; }        

        bool IsSelected { get; set; }

        IEnumerable<IAuditCorrectionTransaction> Transactions { get; }

        IAuditCorrectionTransaction AddTransaction(IPresentationTreeNode node);
        
        void RemoveTransaction(IAuditCorrectionTransaction transaction);
    }
}