using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace eBalanceKitBusiness.Rights {

    /// <summary>
    /// Base class for RoleRightTreeNode and EffectiveRightTreeNode.
    /// </summary>
    public abstract class RightTreeNodeBase : IRightTreeNode, INotifyPropertyChanged {

        internal RightTreeNodeBase() {
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion

        #region Children
        private ObservableCollection<IRightTreeNode> _children = new ObservableCollection<IRightTreeNode>();
        public ObservableCollection<IRightTreeNode> Children {
            get { return _children; }
            set { _children = value; }
        }
        #endregion Children

        protected virtual Right Right { get; set; }
        public abstract string HeaderString { get; }
        public virtual string ToolTip { get; set; }

        #region IsSelected
        private bool _isSelected;
        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (value != _isSelected) {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        #endregion

        #region IsExpanded
        private bool _isExpanded = false;
        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (value != _isExpanded) {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }
        #endregion
        
        #region ReadChecked
        public bool? ReadChecked {
            get { return Right.Read; }
            set { Right.Read = value; }
        }
        #endregion

        #region WriteChecked
        public bool? WriteChecked {
            get { return Right.Write; }
            set { Right.Write = value; }
        }
        #endregion

        #region SendChecked
        public bool? SendChecked {
            get { return Right.Send; }
            set { Right.Send = value; }
        }
        #endregion

        #region ExportChecked
        public bool? ExportChecked {
            get { return Right.Export; }
            set { Right.Export = value; }
        }
        #endregion

        #region GrantChecked
        public bool? GrantChecked {
            get { return Right.Grant; }
            set { Right.Grant = value; }
        }
        #endregion

        #region InheritResultGrant
        public bool InheritResultGrant {
            get {
                if (GrantChecked.HasValue) return false;
                return Right.IsGrantAllowed;
            }
        }
        #endregion

        #region InheritResultWrite
        public bool InheritResultWrite {
            get {
                if (WriteChecked.HasValue) return false;
                return Right.IsWriteAllowed;
            }
        }
        #endregion

        #region InheritResultRead
        public bool InheritResultRead {
            get {
                if (ReadChecked.HasValue) return false;
                return Right.IsReadAllowed;
            }
        }
        #endregion

        #region InheritResultExport
        public bool InheritResultExport {
            get {
                if (ExportChecked.HasValue) return false;
                return Right.IsExportAllowed;
            }
        }
        #endregion

        #region InheritResultSend
        public bool InheritResultSend {
            get {
                if (SendChecked.HasValue) return false;
                return Right.IsSendAllowed;
            }
        }
        #endregion

        public abstract bool IsEditAllowed { get; }

        public bool IsSpecialRight { get { return Right.DbRight.IsSpecialRight(); } }
    }
}
