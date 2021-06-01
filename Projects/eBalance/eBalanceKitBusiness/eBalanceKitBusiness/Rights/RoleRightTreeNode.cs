using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping.Rights;

namespace eBalanceKitBusiness.Rights {

    /// <summary>
    /// Structure for tree representation of role rights.
    /// </summary>
    /// <author>Mirko Dibbert / Benjamin Held</author>
    /// <since>2011-08-24</since>
    public abstract class RoleRightTreeNode : RightTreeNodeBase {
        
        #region Constructor
        public RoleRightTreeNode(Role role, RoleRightTreeNode parent) {
            this.Role = role;
            this.Parent = parent;
        }
        #endregion

        #region event handler
        void _right_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "IsGrantAllowed":
                    OnPropertyChanged("InheritResultGrant");
                    OnPropertyChanged("GrantChecked");
                    break;
                case "IsExportAllowed":
                    OnPropertyChanged("InheritResultExport");
                    OnPropertyChanged("ExportChecked");
                    break;
                case "IsWriteAllowed":
                    OnPropertyChanged("InheritResultWrite");
                    OnPropertyChanged("WriteChecked");
                    break;
                case "IsReadAllowed":
                    OnPropertyChanged("InheritResultRead");
                    OnPropertyChanged("ReadChecked");
                    break;
                case "IsSendAllowed":
                    OnPropertyChanged("InheritResultSend");
                    OnPropertyChanged("SendChecked");
                    break;
            }
        }
        #endregion
        
        #region properties
        //--------------------------------------------------------------------------------        

        #region Parent
        private RoleRightTreeNode _parent;
        public RoleRightTreeNode Parent {
            get { return _parent; }
            set { _parent = value; }
        }
        #endregion

        #region Right
        protected override Right Right {
            get { return base.Right; }
            set {
                if (base.Right != value) {
                    base.Right = value;
                    base.Right.PropertyChanged += new PropertyChangedEventHandler(_right_PropertyChanged);
                }
            }
        }
        #endregion
        
        protected Role Role { get; set; }

        #region IsEditAllowed
        public override bool IsEditAllowed {
            get {
                if (Parent == null) return IsRightAllowed((int)DbRight.RightTypes.Grant);
                return Parent.IsRightAllowed((int)DbRight.RightTypes.Grant);
            }
        }
        #endregion

        #region HasGrantChildren
        public bool HasGrantChildren {
            get {
                //for (int i = 0; i < (int)DbRight.RightTypes.Count; ++i) {
                //    if (IsRightAllowed(i)) return true;
                //}
                if (IsRightAllowed((int)DbRight.RightTypes.Grant)) return true;
                foreach (var child in Children) {
                    if ((child as RoleRightTreeNode).HasGrantChildren) return true;
                }
                return false;
            }
        }
        #endregion

        #region RightAllowed
        public abstract bool IsRightAllowed(int i);
        #endregion
        
        //--------------------------------------------------------------------------------
        #endregion properties



    }
}
