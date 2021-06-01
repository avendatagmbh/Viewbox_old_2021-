using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness;

namespace eBalanceKit.Windows.Security.Models {

    public class RightObjectTreeNode : INotifyPropertyChanged, IRightToolTipTreeNode {

        public RightObjectTreeNode(
            eBalanceKitBusiness.IRightTreeNode node, 
            eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes rightType) {
            
            AssignedNode = node;
            RightType = rightType;
            AssignedNode.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(AssignedNode_PropertyChanged);

            foreach (var child in node.Children) Children.Add(new RightObjectTreeNode(child, rightType));
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion

        void AssignedNode_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "IsExpanded":
                    OnPropertyChanged("IsExpanded");
                    break;

                default:
                    OnPropertyChanged("IsChecked");
                    OnPropertyChanged("IsInherited");
                    break;
            }
        }

        public string HeaderString { get { return AssignedNode.HeaderString; } }
        public string ToolTip {
            get {
                EffectiveUserRightTreeNode effectiveNode = AssignedNode as EffectiveUserRightTreeNode;
                if (effectiveNode != null) {
                    switch (RightType) {
                        case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Read:
                        case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Write:
                        case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Grant:
                        case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Export:
                        case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Send:
                            return effectiveNode.GetToolTipForType(RightType);
                        case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Count:
                            throw new NotImplementedException();
                        default:
                            throw new NotImplementedException();
                    };
                }
                else
                    return AssignedNode.ToolTip;
            }
            set { AssignedNode.ToolTip = value; }
        }

        public eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes RightType { get; private set; }

        private eBalanceKitBusiness.IRightTreeNode AssignedNode { get; set; }

        public bool IsExpanded {
            get { return AssignedNode.IsExpanded; }
            set {
                AssignedNode.IsExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        #region Children
        ObservableCollection<RightObjectTreeNode> _children = new ObservableCollection<RightObjectTreeNode>();
        public ObservableCollection<RightObjectTreeNode> Children { get { return _children; } }
        #endregion Children

        public bool? IsChecked {
            get {                
                switch (RightType) {
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Read:
                        return AssignedNode.ReadChecked;
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Write:
                        return AssignedNode.WriteChecked;
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Grant:
                        return AssignedNode.GrantChecked;
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Export:
                        return AssignedNode.ExportChecked;
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Send:
                        return AssignedNode.SendChecked;
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Count:
                        throw new NotImplementedException();
                    default:
                        throw new NotImplementedException();
                }
            }
            set {
                switch (RightType) {
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Read:
                        AssignedNode.ReadChecked = value;
                        break;
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Write:
                        AssignedNode.WriteChecked = value;
                        break;
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Grant:
                        AssignedNode.GrantChecked = value;
                        break;
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Export:
                        AssignedNode.ExportChecked = value;
                        break;
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Send:
                        AssignedNode.SendChecked = value;
                        break;
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Count:
                        throw new NotImplementedException();
                    default:
                        throw new NotImplementedException();
                }

                OnPropertyChanged("IsChecked");
            }
        }

        public bool IsInherited {
            get {
                switch (RightType) {
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Read:
                        return AssignedNode.InheritResultRead;
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Write:
                        return AssignedNode.InheritResultWrite;
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Grant:
                        return AssignedNode.InheritResultGrant;
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Export:
                        return AssignedNode.InheritResultExport;
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Send:
                        return AssignedNode.InheritResultSend;
                    case eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes.Count:
                        throw new NotImplementedException();
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public bool IsEditAllowed { get { return AssignedNode.IsEditAllowed; } }
        public bool IsSpecialRight { get { return AssignedNode.IsSpecialRight; } }        
    }

    public class CtlRoleRightsByTypeModel {

        public CtlRoleRightsByTypeModel(IEnumerable<RoleRightTreeNode> rootNodes, eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes rightType) {
            RightType = rightType;
            foreach (var rootNode in rootNodes) RootNodes.Add(new RightObjectTreeNode(rootNode, rightType));
        }

        public CtlRoleRightsByTypeModel(IEnumerable<eBalanceKitBusiness.Rights.EffectiveUserRightTreeNode> rootNodes, eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes rightType) {
            RightType = rightType;
            foreach (var rootNode in rootNodes) RootNodes.Add(new RightObjectTreeNode(rootNode, rightType));
        }

        public eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes RightType { get; set; }

        #region RootNodes
        ObservableCollection<RightObjectTreeNode> _rootNodes = new ObservableCollection<RightObjectTreeNode>();
        public ObservableCollection<RightObjectTreeNode> RootNodes { get { return _rootNodes; } }
        #endregion RootNodes        
    }
}
