// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.ComponentModel;
using Utils.Commands;
using eBalanceKit.Windows.Management.ManagementAssistant;
using eBalanceKit.Windows.Management.ManagementAssistant.Model;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Models.FederalGazette {
    public class FederalGazetteCopyValuesQueryModel : Utils.NotifyPropertyChangedBase, IAskingModel {
        

        #region CmdYes
        private DelegateCommand _cmdYes;

        public DelegateCommand CmdYes {
            get { return _cmdYes; }
            set {
                if (_cmdYes != value) {
                    _cmdYes = value;
                    OnPropertyChanged("CmdYes");
                }
            }
        }
        #endregion // CmdYes

        #region CmdNo
        private DelegateCommand _cmdNo;

        public DelegateCommand CmdNo {
            get { return _cmdNo; }
            set {
                if (_cmdNo != value) {
                    _cmdNo = value;
                    OnPropertyChanged("CmdNo");
                }
            }
        }
        #endregion // CmdNo


        public ObjectTypes ObjectType { get { return ObjectTypes.Other; } set { } }

        #region Result
        private object _result;

        public object Result {
            get { return _result; }
            set {
                if (_result != value) {
                    _result = value;
                    OnPropertyChanged("Result");
                }
            }
        }
        #endregion // Result

        public string Question { get { return ResourcesFederalGazette.OvertakeDataQuestion; } }
    }
}