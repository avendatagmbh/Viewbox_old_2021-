using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Structures.DbMapping.Rights;
using System.ComponentModel;

namespace eBalanceKitBusiness.Rights {
    
    /// <summary>
    /// This class represents a specific right.
    /// </summary>
    /// <author>Benjamin Held</author>
    /// <since>2011-07-18</since>
    public class Right {
    
        #region constructor
        //--------------------------------------------------------------------------------
        
        internal Right(DbRight dbRight) {
            this.DbRight = dbRight;
            if (this.DbRight != null) {
                this.DbRight.PropertyChanged += new PropertyChangedEventHandler(DbRight_PropertyChanged);
            }
        }

        void DbRight_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "Grant":
                    OnPropertyChanged("IsGrantAllowed");
                    break;
                case "Read":
                    OnPropertyChanged("IsReadAllowed");
                    break;
                case "Write":
                    OnPropertyChanged("IsWriteAllowed");
                    break;
                case "Send":
                    OnPropertyChanged("IsSendAllowed");
                    break;
                case "Export":
                    OnPropertyChanged("IsExportAllowed");
                    break;
            }

        }
        //--------------------------------------------------------------------------------
        #endregion constructor


        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events


        #region properties
        //--------------------------------------------------------------------------------

        #region DbRight
        public DbRight DbRight { get; set; }
        #endregion

        #region ParentRight
        private Right _parentRight;
        public Right ParentRight {
            get{return _parentRight;}
            set {
                if (_parentRight != value) {
                    _parentRight = value;
                    if (_parentRight != null) {
                        _parentRight.PropertyChanged += new PropertyChangedEventHandler(_parentRight_PropertyChanged);
                    }
                }
            }
        }

        void _parentRight_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnPropertyChanged(e.PropertyName);
        }
        #endregion

        #region Grant
        public bool? Grant {
            get { return DbRight.Grant; }
            set { DbRight.Grant = value; }
        }
        #endregion

        #region Export
        public bool? Export {
            get { return DbRight.Export; }
            set {
                DbRight.Export = value;
            }
        }
        #endregion

        #region Send
        public bool? Send {
            get { return DbRight.Send; }
            set { DbRight.Send = value; }
        }
        #endregion

        #region Write
        public bool? Write {
            get { return DbRight.Write; }
            set { DbRight.Write = value; }
        }
        #endregion

        #region Read
        public bool? Read {
            get { return DbRight.Read; }
            set { DbRight.Read = value; }
        }
        #endregion

        #region IsGrantAllowed
        public bool IsGrantAllowed {
            get {
                return IsRightAllowed(DbRight.RightTypes.Grant);
            }
        }
        #endregion

        #region IsReadAllowed
        public bool IsReadAllowed {
            get {
                return IsRightAllowed(DbRight.RightTypes.Read) || IsRightAllowed(DbRight.RightTypes.Write);
            }
        }
        #endregion

        #region IsWriteAllowed
        public bool IsWriteAllowed {
            get {
                return IsRightAllowed(DbRight.RightTypes.Write);
            }
        }
        #endregion

        #region IsExportAllowed
        public bool IsExportAllowed {
            get {
                return IsRightAllowed(DbRight.RightTypes.Export);
            }
        }
        #endregion

        #region IsSendAllowed
        public bool IsSendAllowed {
            get {
                return IsRightAllowed(DbRight.RightTypes.Send);
            }
        }
        #endregion

        #region RightAllowed
        public bool IsRightAllowed(DbRight.RightTypes type) {
            return IsRightAllowed((int)type);
        }

        public bool IsRightAllowed(int index) {
            if (DbRight.GetRight(index).HasValue) return DbRight.GetRight(index).Value;
            return ParentRight != null && ParentRight.IsRightAllowed(index);
        }

        public bool? GetInheritedRight(int index) {
            if (DbRight.GetRight(index).HasValue) return DbRight.GetRight(index).Value;
            return ParentRight != null ? ParentRight.GetInheritedRight(index) : null;
        }
        #endregion RightAllowed

        public bool IsAnyRightAllowed() {
            foreach (DbRight.RightTypes rightType in DbRight.RightTypeList) {
                if (IsRightAllowed(rightType))
                    return true;
            }
            return false;
        }

        #region
        public bool IsRightExplicitlySet(DbRight.RightTypes type) {
            return IsRightExplicitlySet((int)type);
        }

        public bool IsRightExplicitlySet(int index) {
            return DbRight.GetRight(index).HasValue;
            //if (DbRight.GetRight(index).HasValue) return true;
            //if (ParentRight != null) return ParentRight.IsRightExplicitlySet(index);
            //return false;
        }

        //public bool IsAnyRightExplicitlySet() {
        //    for (int i = 0; i < (int)DbRight.RightTypes.Count; ++i)
        //        if (IsRightExplicitlySet(i)) return true;
        //    return false;
        //}
        #endregion

        //--------------------------------------------------------------------------------
        #endregion properties


        #region methods
        //--------------------------------------------------------------------------------
        #region MergeRights
        public void MergeRight(Right other){
            for (int i = 0; i < (int)DbRight.RightTypes.Count; ++i) {
                var isExplicitlySet = IsRightExplicitlySet(i);
                var isExplicitlySetOther = other.IsRightExplicitlySet(i);
                var right = DbRight.GetRight(i);
                var rightOther = other.DbRight.GetRight(i);

                if (isExplicitlySet && !isExplicitlySetOther) continue; // nothing to do
                if (!isExplicitlySet && isExplicitlySetOther) DbRight.SetRight(i, rightOther); // use explicite right from other
                if (isExplicitlySet && isExplicitlySetOther) {
                    DbRight.SetRight(i, right.Value & rightOther.Value);
                }                
            }
            //for (int i = 0; i < (int)DbRight.RightTypes.Count; ++i) {
            //    bool isExplicitlySet = this.IsRightExplicitlySet(i);
            //    bool isExplicitlySetOther = other.IsRightExplicitlySet(i);
            //    if(isExplicitlySet && !isExplicitlySetOther)
            //        DbRight.SetRight(i, this.GetInheritedRight(i));
            //    else if(!isExplicitlySet && isExplicitlySetOther)
            //        DbRight.SetRight(i, other.GetInheritedRight(i));
            //    else if (!isExplicitlySet && !isExplicitlySetOther)
            //        DbRight.SetRight(i, null);
            //    else
            //        DbRight.SetRight(i, DbRight.GetRight(i) & other.GetInheritedRight(i));
            //}
        }

        public void SetFullRights() {
            for (int i = 0; i < (int)DbRight.RightTypes.Count; ++i) {
                if (i == (int)DbRight.RightTypes.Grant) continue;
                DbRight.SetRight(i, true);
            }
        }

        public void SetAdminRights() {
            DbRight.Grant = true;
            DbRight.Read = true;
            DbRight.Write = true;
        }

        public void SetOwnerRights() {
            DbRight.Read = true;
            DbRight.Write = true;
            DbRight.Send = true;
            DbRight.Export = true;
        }

        public void SetRights(Right other) {
            for(int i = 0; i <  (int)DbRight.RightTypes.Count; ++i)
                DbRight.SetRight(i, other.GetInheritedRight(i));
        }

        public void SetNonInheritedRights(Right other) {
            for(int i = 0; i <  (int)DbRight.RightTypes.Count; ++i)
                DbRight.SetRight(i, other.DbRight.GetRight(i));
        }
        #endregion

        //--------------------------------------------------------------------------------
        #endregion methods
    }
}