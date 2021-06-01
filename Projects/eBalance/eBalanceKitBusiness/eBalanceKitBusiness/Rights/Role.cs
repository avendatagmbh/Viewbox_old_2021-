// -----------------------------------------------------------
// Created by Benjamin Held - 18.07.2011 13:52:34
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Structures.DbMapping.Rights;
using DbAccess;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Manager;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace eBalanceKitBusiness.Rights {

    public class Role : INotifyPropertyChanged {

        #region Constructor
        public Role(DbRole dbRole) {
            this.DbRole = dbRole;
            this.Rights = new List<Right>();
            //LoadDetails();
        }
        #endregion

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        #region Properties
        public DbRole DbRole { get; set; }

        #region Name
        public string Name { 
            get { return DbRole.Name; }
            set { 
                DbRole.Name = value;
                OnPropertyChanged("Name");
                OnPropertyChanged("DisplayString");
            }
        }
        #endregion

        #region Comment
        public string Comment {
            get { return DbRole.Comment; }
            set { 
                DbRole.Comment = value;
                OnPropertyChanged("Comment");
                OnPropertyChanged("DisplayString");
            }
        }
        #endregion

        #region IsSelected
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set { 
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion

        Role UserRole { get; set; }
        
        #region AssignedUsers
        private ObservableCollection<User> _assignedUsers = new ObservableCollection<User>();
        public ObservableCollection<User> AssignedUsers { get { return _assignedUsers; } }
        #endregion AssignedUsers

        public string DisplayString { get { return string.IsNullOrEmpty(Comment) ? Name : Name + " (" + Comment + ")"; } }

        public List<Right> Rights { get; set; }
        #endregion

        #region Methods
        public void LoadDetails() {
            if (DbRole == null) return;
            Rights.Clear();

            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                List<DbRight> dbRights = conn.DbMapping.Load<DbRight>(conn.Enquote("role_id") +" = " + DbRole.Id);
                foreach (var right in dbRights) Rights.Add(new Right(right));
            }
            
            //Set parents
            Right rootRight = GetRootRight();
            foreach (Company company in CompanyManager.Instance.Companies) {
                Right companyRight = GetRight(company);
                companyRight.ParentRight = rootRight;
                foreach (FinancialYear fYear in company.VisibleFinancialYears) {
                    GetRight(fYear).ParentRight = companyRight;
                }
            }
            foreach (Document document in DocumentManager.Instance.Documents) {
                Right documentRight = GetRight(document);
                documentRight.ParentRight = GetRight(document.FinancialYear);
                foreach (DbRight.ContentTypes contentType in Enum.GetValues(typeof(DbRight.ContentTypes))) {
                    if (DbRight.IsSpecialRight(contentType))
                        GetRight(contentType, document).ParentRight = documentRight;
                }
            }
        }

        private void AddRight(Right right) { Rights.Add(right); }

        public void Save() {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                foreach (var right in Rights) {
                    if (right.Grant != null || right.Export != null || right.Read != null || right.Write != null || right.Send != null) {
                        var isNew = right.DbRight.Id == 0;
                        if (!isNew) {
                            Logs.LogManager.Instance.UpdateRight(right);
                        }
                        conn.DbMapping.Save(right.DbRight);
                        if (isNew) {
                            Logs.LogManager.Instance.NewRight(right);
                        }
                    }
                    else {
                        Logs.LogManager.Instance.DeleteRight(right, right.DbRight.ContentType);
                        conn.DbMapping.Delete(right.DbRight);
                    }
                }
            }
        }

        #region GetRights

        public Right GetRootRight() {
            foreach (var right in Rights)
                if (right.DbRight.ContentType == DbRight.ContentTypes.All)
                    return right;
            Right newRight = new Right(new DbRight() { RoleId = DbRole.Id, ContentType = DbRight.ContentTypes.All });
            AddRight(newRight);
            return newRight;
        }

        public Right GetRootRightIfPresent() {
            foreach (var right in Rights)
                if (right.DbRight.ContentType == DbRight.ContentTypes.All)
                    return right;
            return null;
        }

        public Right GetRight(eBalanceKitBusiness.Structures.DbMapping.System system) {
            foreach (var right in Rights)
                if (right.DbRight.ContentType == DbRight.ContentTypes.System && right.DbRight.ReferenceId == system.Id)
                    return right;
            Right newRight = new Right(new DbRight() { RoleId = DbRole.Id, ContentType = DbRight.ContentTypes.System, ReferenceId = system.Id });
            AddRight(newRight);
            return newRight;
        }

        public Right GetRight(Company company) {
            foreach (var right in Rights)
                if (right.DbRight.ContentType == DbRight.ContentTypes.Company && right.DbRight.ReferenceId == company.Id)
                    return right;
            Right newRight = new Right(new DbRight() { RoleId = DbRole.Id, ContentType = DbRight.ContentTypes.Company, ReferenceId = company.Id });
            AddRight(newRight);
            return newRight;
        }

        public Right GetRightIfPresent(Company company) {
            foreach (var right in Rights)
                if (right.DbRight.ContentType == DbRight.ContentTypes.Company && right.DbRight.ReferenceId == company.Id)
                    return right;
            return null;
        }

        public Right GetRight(FinancialYear fYear) {
            foreach (var right in Rights)
                if (right.DbRight.ContentType == DbRight.ContentTypes.FinancialYear && right.DbRight.ReferenceId == fYear.Id)
                    return right;
            Right newRight = new Right(new DbRight() { RoleId = DbRole.Id, ContentType = DbRight.ContentTypes.FinancialYear, ReferenceId = fYear.Id });
            AddRight(newRight);
            return newRight;
        }

        public Right GetRightIfPresent(FinancialYear fYear) {
            foreach (var right in Rights)
                if (right.DbRight.ContentType == DbRight.ContentTypes.FinancialYear && right.DbRight.ReferenceId == fYear.Id)
                    return right;
            return null;
        }

        public Right GetRight(Document document) {
            foreach (var right in Rights)
                if (right.DbRight.ContentType == DbRight.ContentTypes.Document && right.DbRight.ReferenceId == document.Id)
                    return right;
            Right newRight = new Right(new DbRight() { RoleId = DbRole.Id, ContentType = DbRight.ContentTypes.Document, ReferenceId = document.Id });
            AddRight(newRight);
            return newRight;
        }

        public Right GetRightIfPresent(Document document) {
            foreach (var right in Rights)
                if (right.DbRight.ContentType == DbRight.ContentTypes.Document && right.DbRight.ReferenceId == document.Id)
                    return right;
            return null;
        }
        public Right GetRight(DbRight.ContentTypes specialRight, Document parentDocument) {
            foreach (var right in Rights)
                if (right.DbRight.ContentType == specialRight && right.DbRight.ReferenceId == parentDocument.Id)
                    return right;
            Right newRight = new Right(new DbRight() { RoleId = DbRole.Id, ContentType = specialRight, ReferenceId = parentDocument.Id });
            AddRight(newRight);
            return newRight;
        }
        public Right GetRightIfPresent(DbRight.ContentTypes specialRight, Document parentDocument) {
            foreach (var right in Rights)
                if (right.DbRight.ContentType == specialRight && right.DbRight.ReferenceId == parentDocument.Id)
                    return right;
            return null;
        }
        #endregion

        #region IsAllowed
        //public bool IsAllowed(RightDeducer rightDeducer) {
        //    foreach (Right right in Rights) {
        //        if (right.IsAnyRightExplicitlySet()) {
        //            //Parent right has to be grantable
        //            switch (right.DbRight.ContentType) {
        //                case DbRight.ContentTypes.All:
        //                    if (!rightDeducer.GetRootRight().IsGrantAllowed) return false;
        //                    break;
        //                case DbRight.ContentTypes.System:
        //                    break;
        //                case DbRight.ContentTypes.Company:
        //                    if (!rightDeducer.GetRootRight().IsGrantAllowed) return false;
        //                    break;
        //                case DbRight.ContentTypes.FinancialYear:
        //                    if (!rightDeducer.GetRight(CompanyManager.GetCompany(right.ParentRight.DbRight.ReferenceId)).IsGrantAllowed) return false;
        //                    break;
        //                case DbRight.ContentTypes.Document:
        //                    if (!rightDeducer.GetFinancialYearRight(right.ParentRight.DbRight.ReferenceId).IsGrantAllowed) return false;
        //                    break;
        //                case DbRight.ContentTypes.DocumentSpecialRight_TransferValueCalculation:
        //                case DbRight.ContentTypes.DocumentSpecialRight_Rest:
        //                    if (!rightDeducer.GetRight(DocumentManager.GetDocument(right.ParentRight.DbRight.ReferenceId)).IsGrantAllowed) return false;
        //                    break;
        //                default:
        //                    throw new Exception("Unbekannter ContentType in IsAllowed");
        //            }
        //        }
        //    }

        //    return true;
        //}
        #endregion

        #endregion

    }
}
