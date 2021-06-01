// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.ComponentModel;
using Utils.Commands;
using eBalanceKitBusiness.Interfaces;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Model {
    public interface IAskingModel : INotifyPropertyChanged {
        
        DelegateCommand CmdYes { get; set; }
        DelegateCommand CmdNo { get; set; }
        ObjectTypes ObjectType { get; set; }
        object Result { get; set; }
        string Question { get; }
    }
}