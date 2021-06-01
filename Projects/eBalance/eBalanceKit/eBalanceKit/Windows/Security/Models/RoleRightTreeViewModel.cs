using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Security.Models {
    
    public class RoleRightTreeViewModel : INotifyPropertyChanged {
        
        #region Constructor
        public RoleRightTreeViewModel(Role role) {
            this.Role = role;
            RoleRightTreeNode rootNode = new RoleRightTreeNodeRoot(role) { IsSelected = true, IsExpanded = true };
            List<RoleRightTreeNode> roots = new List<RoleRightTreeNode>();
            _rootNodes = new ReadOnlyCollection<RoleRightTreeNode>(new RoleRightTreeNode[] { rootNode });

            foreach (var r in RoleManager.UserRoles) {
                if (r.Value.Equals(role)) {
                    Title = ResourcesCommon.RightsForUser + " " + UserManager.Instance.GetUser(r.Key).DisplayString;
                    break;
                }
            }

            if (string.IsNullOrEmpty(Title)) Title = ResourcesCommon.RightsForRole + " " + role.Name;

            CtlRoleRightsByTypeModels = new Dictionary<string, CtlRoleRightsByTypeModel>();
            CtlRoleRightsByTypeModels["Read"] = new CtlRoleRightsByTypeModel(RootNodes, eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Read);
            CtlRoleRightsByTypeModels["Write"] = new CtlRoleRightsByTypeModel(RootNodes, eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Write);
            CtlRoleRightsByTypeModels["Grant"] = new CtlRoleRightsByTypeModel(RootNodes, eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Grant);
            CtlRoleRightsByTypeModels["Send"] = new CtlRoleRightsByTypeModel(RootNodes, eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Send);
            CtlRoleRightsByTypeModels["Export"] = new CtlRoleRightsByTypeModel(RootNodes, eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Export);
        }
        #endregion


        public Dictionary<string, CtlRoleRightsByTypeModel> CtlRoleRightsByTypeModels { get; private set; }

        
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events

        
        #region properties
        //--------------------------------------------------------------------------------

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
        
        public Role Role { get; set; }

        private ReadOnlyCollection<RoleRightTreeNode> _rootNodes;
        public ReadOnlyCollection<RoleRightTreeNode> RootNodes {
            get { return _rootNodes; }
            set { _rootNodes = value; }
        }

        #region SelectedItem
        object _selectedItem;
        public object SelectedItem {
            get { return _selectedItem; }
            set { 
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }
        #endregion SelectedItem

        //--------------------------------------------------------------------------------
        #endregion properties
    }
}
