// -----------------------------------------------------------
// Created by Benjamin Held - 14.07.2011 13:27:01
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.ComponentModel;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;

namespace eBalanceKitBusiness.Structures.DbMapping.Rights {
    
    
    #region DbRole
    //--------------------------------------------------------------------------------
    [DbTable("roles", ForceInnoDb = true)]
    public partial class DbRole {
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events

        #region Properties

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id {get;set;}
        #endregion

        #region Name
        public string _name;

        [DbColumn("name", Length = 256, AllowDbNull = true)]
        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged("Name");
                    //Save();
                }
            }
        }
        #endregion

        #region Comment
        public string _comment;

        [DbColumn("comment", Length = 1024, AllowDbNull = true)]
        public string Comment {
            get { return _comment; }
            set {
                if (_comment != value) {
                    _comment = value;
                    OnPropertyChanged("Comment");
                    //Save();
                }
            }
        }
        #endregion

        #region UserId
        //If this role is a user role then a value different from NULL or 0 must be present to reference the user it belongs to
        [DbColumn("user_id", AllowDbNull = true)]
        public int? UserId { get; set; }
        #endregion

        #region DoDbUpdate
        public bool DoDbUpdate {
            get { return _doDbUpdate; }
            set { _doDbUpdate = value; }
        }
        private bool _doDbUpdate = true;
        #endregion

        #endregion

        #region Methods
        public void Save() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                if (DoDbUpdate) conn.DbMapping.Save(this);
            }
        }
        #endregion
    }
    //--------------------------------------------------------------------------------
    #endregion



    #region DbRights
    //--------------------------------------------------------------------------------
    [DbTable("rights", ForceInnoDb = true)]
    public partial class DbRight {
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events

        public DbRight(int rightFlagAsInt) { Rights = rightFlagAsInt; }
        public DbRight() {}
        #region Properties

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }
        #endregion

        #region RoleId
        [DbColumn("role_id", AllowDbNull = false)]
        [DbIndex("rights_content")]
        public int RoleId { get; set; }
        #endregion

        #region ContentEnum
        public enum ContentTypes {
            All,
            System,
            Company,
            FinancialYear,
            Document,
            DocumentSpecialRight_TransferValueCalculation,
            DocumentSpecialRight_Rest,
        }

        [DbColumn("content_type", AllowDbNull = false)]
        [DbIndex("rights_content")]
        public ContentTypes ContentType { get; set; }
        #endregion

        #region ReferenceId
        [DbColumn("ref_id", AllowDbNull = false)]
        [DbIndex("rights_content")]
        public int ReferenceId { get; set; }
        #endregion

        #region Rights
        enum RightFlag {
            ReadAllow = 1,
            ReadDeny = 2,

            WriteAllow = 4,
            WriteDeny = 8,

            GrantAllow = 16,
            GrantDeny = 32,

            ExportAllow = 64,
            ExportDeny = 128,

            SendAllow = 256,
            SendDeny = 512,
        }

        [DbColumn("rights", AllowDbNull = false)]
        private int Rights { get; set; }
        #endregion
        #endregion

        #region ConvenienceProperties

        public static bool IsSpecialRight(ContentTypes contentType) {
            return 
                contentType == ContentTypes.DocumentSpecialRight_Rest || 
                contentType == ContentTypes.DocumentSpecialRight_TransferValueCalculation;
        }

        public bool IsSpecialRight() { return IsSpecialRight(ContentType); }
        
        #region Grant
        public bool? Grant{
            get{
                if ((Rights & (int)RightFlag.GrantAllow) != 0) return true;
                if ((Rights & (int)RightFlag.GrantDeny) != 0) return false;
                return null;
            }
            set {
                if (Grant != value) {
                    //Delete Grant flag
                    Rights &= ~(int)RightFlag.GrantAllow & ~(int)RightFlag.GrantDeny;
                    if (value.HasValue) {
                        if (value.Value) Rights |= (int)RightFlag.GrantAllow;
                        else Rights |= (int)RightFlag.GrantDeny;
                    } 
                    OnPropertyChanged("Grant");
                }
            }
        }
        #endregion

        ////add this code to class ThreeDPoint as defined previously
        ////
        //public static bool operator ==(DbRight a, DbRight b) {
        //    // If both are null, or both are same instance, return true.
        //    if (ReferenceEquals(a, b)) {
        //        return true;
        //    }

        //    // If one is null, but not both, return false.
        //    if (((object)a == null) || ((object)b == null)) {
        //        return false;
        //    }

        //    // Return true if the fields match:
        //    return a.Rights == b.Rights;
        //}

        //public static bool operator !=(DbRight a, DbRight b) {
        //    return !(a == b);
        //}

        #region Export
        public bool? Export {
            get {
                if ((Rights & (int)RightFlag.ExportAllow) != 0) return true;
                if ((Rights & (int)RightFlag.ExportDeny) != 0) return false;
                return null;
            }
            set {
                if (Export != value) {
                    //Delete Export flag
                    Rights &= ~(int)RightFlag.ExportAllow & ~(int)RightFlag.ExportDeny;
                    if (value.HasValue) {
                        if (value.Value) Rights |= (int)RightFlag.ExportAllow;
                        else Rights |= (int)RightFlag.ExportDeny;
                    }
                    OnPropertyChanged("Export");
                }
            }
        }
        #endregion

        #region Send
        public bool? Send {
            get {
                if ((Rights & (int)RightFlag.SendAllow) != 0) return true;
                if ((Rights & (int)RightFlag.SendDeny) != 0) return false;
                return null;
            }
            set {
                if (Send != value) {
                    //Delete Send flag
                    Rights &= ~(int)RightFlag.SendAllow & ~(int)RightFlag.SendDeny;
                    if (value.HasValue) {
                        if (value.Value) Rights |= (int)RightFlag.SendAllow;
                        else Rights |= (int)RightFlag.SendDeny;
                    }
                    OnPropertyChanged("Send");
                }
            }
        }
        #endregion

        #region Write
        public bool? Write {
            get {
                if ((Rights & (int)RightFlag.WriteAllow) != 0) return true;
                if ((Rights & (int)RightFlag.WriteDeny) != 0) return false;
                return null;
            }
            set {
                if (Write != value) {
                    //Delete Write flag
                    Rights &= ~(int)RightFlag.WriteAllow & ~(int)RightFlag.WriteDeny;
                    if (value.HasValue) {
                        if (value.Value) Rights |= (int)RightFlag.WriteAllow;
                        else Rights |= (int)RightFlag.WriteDeny;
                    }
                    OnPropertyChanged("Write");
                }
            }
        }
        #endregion

        #region Read
        public bool? Read {
            get {
                if ((Rights & (int)RightFlag.ReadAllow) != 0) return true;
                if ((Rights & (int)RightFlag.ReadDeny) != 0) return false;
                return null;
            }
            set {
                if (Read != value) {
                    //Delete Read flag
                    Rights &= ~(int)RightFlag.ReadAllow & ~(int)RightFlag.ReadDeny;
                    if (value.HasValue) {
                        if (value.Value) Rights |= (int)RightFlag.ReadAllow;
                        else Rights |= (int)RightFlag.ReadDeny;
                    }
                    OnPropertyChanged("Read");
                }
            }
        }
        #endregion

        #region methods

        #region RightType

        public enum RightTypes {
            Read=0,
            Write,
            Grant,
            Export,
            Send,
            Count
        }

        private static IEnumerable<RightTypes> _rightTypeList; 
        public static IEnumerable<RightTypes> RightTypeList
        {
            get {
                if (_rightTypeList == null)
                    _rightTypeList = Enum.GetNames(typeof(RightTypes)).Select(enumElement => (RightTypes)Enum.Parse(typeof(RightTypes), enumElement));
                return _rightTypeList;
            }
        }

        #endregion

        #region GetRight
        public bool? GetRight(RightTypes type) { return GetRight((int)type); }
        public void SetRight(RightTypes type, bool? value) { SetRight((int)type, value); }

        public void SetRight(int index, bool? value) {
            if ((RightTypes)index == RightTypes.Read) Read = value;
            else if ((RightTypes)index == RightTypes.Write) Write = value;
            else if ((RightTypes)index == RightTypes.Grant) Grant = value;
            else if ((RightTypes)index == RightTypes.Export) Export = value;
            else if ((RightTypes)index == RightTypes.Send) Send = value;
        }


        public bool? GetRight(int index) {
            if ((RightTypes)index == RightTypes.Read) return Read;
            else if ((RightTypes)index == RightTypes.Write) return Write;
            else if ((RightTypes)index == RightTypes.Grant) return Grant;
            else if ((RightTypes)index == RightTypes.Export) return Export;
            else if ((RightTypes)index == RightTypes.Send) return Send;
            return null;
        }
        #endregion GetRight

        #region GetRightForLogging
        /// <summary>
        /// Get the private property Rights to log what kind of right was changed / deleted.
        /// </summary>
        /// <returns>The private property Rights that is stored on the database.</returns>
        public int GetRightForLogging() { return Rights; }
        #endregion

        #endregion methods

        #endregion
    }
    //--------------------------------------------------------------------------------
    #endregion



    #region DbUserRole
    //--------------------------------------------------------------------------------
    [DbTable("user_roles", ForceInnoDb = true)]
    public partial class DbUserRole {

        #region Properties

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }
        #endregion

        #region UserId
        [DbColumn("user_id", AllowDbNull = false)]
        [DbIndex("idx_user_roles_1")]
        public int UserId { get; set; }
        #endregion

        public User User {
            get { return UserId > 0 ? UserManager.Instance.GetUser(UserId) : null; }
            set { UserId = value.Id; }
        }

        #region RoleId
        [DbColumn("role_id", AllowDbNull = false)]
        [DbIndex("idx_user_roles_1")]        
        public int RoleId { get; set; }
        #endregion

        public Role Role {
            get { return RoleId > 0 ? RoleManager.GetRole(RoleId) : null; }
            set { RoleId = value.DbRole.Id; }
        }

        #endregion
    }
    //--------------------------------------------------------------------------------
    #endregion


}
