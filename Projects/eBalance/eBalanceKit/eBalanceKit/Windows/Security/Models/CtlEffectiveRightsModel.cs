using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Security.Models {
    
    class CtlEffectiveRightsModel : INotifyPropertyChanged {

        public CtlEffectiveRightsModel(User user) {
            _rootNodes = new ObservableCollection<EffectiveUserRightTreeNode> { RightManager.GetEffectiveUserRightTree(user) };
            _rootNodes[0].IsSelected = true;
            _rootNodes[0].IsExpanded = true;

            User = user;
            Title = ResourcesCommon.EffectiveRightsCaption + " " + user.DisplayString;

            CtlRoleRightsByTypeModels = new Dictionary<string, CtlRoleRightsByTypeModel>();
            CtlRoleRightsByTypeModels["Read"] = new CtlRoleRightsByTypeModel(RootNodes, eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Read);
            CtlRoleRightsByTypeModels["Write"] = new CtlRoleRightsByTypeModel(RootNodes, eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Write);
            CtlRoleRightsByTypeModels["Grant"] = new CtlRoleRightsByTypeModel(RootNodes, eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Grant);
            CtlRoleRightsByTypeModels["Send"] = new CtlRoleRightsByTypeModel(RootNodes, eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Send);
            CtlRoleRightsByTypeModels["Export"] = new CtlRoleRightsByTypeModel(RootNodes, eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Export);
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion

        #region RootNodes
        ObservableCollection<EffectiveUserRightTreeNode> _rootNodes;
        public ObservableCollection<EffectiveUserRightTreeNode> RootNodes { get { return _rootNodes; } }
        #endregion RootNodes        

        #region SelectedItem
        private object _selectedItem;
        public object SelectedItem {
            get { return _selectedItem; }
            set { 
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }
        #endregion SelectedItem

        #region Title
        private string _title;

        public string Title {
            get { return _title; }
            set {
                _title = value;
                OnPropertyChanged("Title");
            }
        }
        #endregion Title

        public User User { get; set; }

        public Dictionary<string, CtlRoleRightsByTypeModel> CtlRoleRightsByTypeModels { get; private set; }
    }
}
