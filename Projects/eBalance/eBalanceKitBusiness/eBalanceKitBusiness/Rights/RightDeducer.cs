// -----------------------------------------------------------
// Created by Benjamin Held - 19.07.2011 10:29:56
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping.Rights;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Rights {
    public class RightDeducer {
        #region Constructor
        public RightDeducer(User user) {
            Init(user);
        }

        private void Init(User user) {
            CompanyToRight = new Dictionary<int, Right>();
            DocumentToRight = new Dictionary<int, Right>();
            FinancialYearToRight = new Dictionary<int, Right>();
            DocumentSpecialRight = new Dictionary<Document, Dictionary<DbRight.ContentTypes, Right>>();
            CompanyReadAllowed = new Dictionary<int, bool>();
            RootRight = null;
            User = user;

            CompanyRights = new Dictionary<int,List<Right>>();
            FyearRights = new Dictionary<int,List<Right>>();
            DocumentRights = new Dictionary<int,List<Right>>();
            DocumentSpecialRights = new Dictionary<int, Dictionary<DbRight.ContentTypes, List<Right>>>();

            User.AssignedCompanies.CollectionChanged -= AssignedCompaniesOnCollectionChanged;
            User.AssignedCompanies.CollectionChanged += AssignedCompaniesOnCollectionChanged;

            Roles = new List<Role>(user.AssignedRoles);
            Roles.Add(RoleManager.GetUserRole(user));
            
            foreach (var role in Roles) {
                role.LoadDetails();

                //Set root right
                Right root = role.GetRootRight();
                if (root != null) {
                    if (RootRight == null) {
                        RootRight = new Right(new DbRight());
                        RootRight.SetNonInheritedRights(root);
                    } else
                        RootRight.MergeRight(root);
                }
            }

            foreach (Company company in CompanyManager.Instance.Companies) {
                //List<Right> companyRights = new List<Right>();
                CompanyRights[company.Id] = new List<Right>();
                foreach (var role in Roles) {
                    Right right = role.GetRightIfPresent(company);
                    if (right != null) {
                        // CompanyManager.IsCompanyAllowed maz have not initialized yet
                        //if (user.IsCompanyAdmin && CompanyManager.IsCompanyAllowed(company))
                        if (user.IsCompanyAdmin && user.IsCompanyAssigned(company)) {
                            right.SetAdminRights();
                        }
                        //companyRights.Add(right);
                        CompanyRights[company.Id].Add(right);
                        if (right.IsRightExplicitlySet(DbRight.RightTypes.Read) && !right.IsReadAllowed)
                            CompanyReadAllowed[company.Id] = false;
                    }
                }
                //CompanyToRight[company.Id] = GetMergedRight(companyRights);
                CompanyToRight[company.Id] = GetMergedRight(CompanyRights[company.Id]);

                foreach (var fYear in company.VisibleFinancialYears) {
                    //List<Right> fyearRights = new List<Right>();
                    FyearRights[fYear.Id] = new List<Right>();
                    foreach (var role in Roles) {
                        Right right = role.GetRightIfPresent(fYear);
                        //if (right != null) fyearRights.Add(right);
                        if (right != null) FyearRights[fYear.Id].Add(right);
                    }
                    //FinancialYearToRight[fYear.Id] = GetMergedRight(fyearRights);
                    FinancialYearToRight[fYear.Id] = GetMergedRight(FyearRights[fYear.Id]);
                }

                if (CompanyToRight[company.Id].IsReadAllowed && !CompanyReadAllowed.ContainsKey(company.Id))
                    CompanyReadAllowed[company.Id] = true;
            }

            foreach (Document document in DocumentManager.Instance.Documents) {
                //List<Right> documentRights = new List<Right>();
                DocumentRights[document.Id] = new List<Right>();
                foreach (var role in Roles) {
                    Right right = role.GetRightIfPresent(document);
                    //if (right != null) documentRights.Add(right);
                    if (right != null) DocumentRights[document.Id].Add(right);
                }
                //DocumentToRight[document.Id] = GetMergedRight(documentRights);
                DocumentToRight[document.Id] = GetMergedRight(DocumentRights[document.Id]);

                //Fill special rights for each document
                DocumentSpecialRight[document] = new Dictionary<DbRight.ContentTypes, Right>();
                DocumentSpecialRights[document.Id] = new Dictionary<DbRight.ContentTypes, List<Right>>();
                foreach (DbRight.ContentTypes contentType in Enum.GetValues(typeof (DbRight.ContentTypes))) {
                    if (DbRight.IsSpecialRight(contentType)) {
                        List<Right> specialRights = new List<Right>();
                        foreach (var role in Roles) {
                            Right right = role.GetRightIfPresent(contentType, document);
                            if (right != null) specialRights.Add(right);
                        }
                        DocumentSpecialRight[document][contentType] = GetMergedRight(specialRights);
                        DocumentSpecialRights[document.Id][contentType] = specialRights;
                    }
                }
                if (DocumentToRight[document.Id].IsReadAllowed && !CompanyReadAllowed.ContainsKey(document.Company.Id))
                    CompanyReadAllowed[document.Company.Id] = true;
            }
            foreach (Company company in CompanyManager.Instance.Companies)
                if (!CompanyReadAllowed.ContainsKey(company.Id))
                    CompanyReadAllowed[company.Id] = GetRight(company).IsReadAllowed;
        }
        #endregion
        
        #region Properties
        private User User { get; set; }
        private List<Role> Roles { get; set; }
        Dictionary<int, Right> CompanyToRight { get; set; }
        Dictionary<int, bool> CompanyReadAllowed { get; set; }
        Dictionary<int, Right> FinancialYearToRight { get; set; }
        Dictionary<int, Right> DocumentToRight { get; set; }
        Dictionary<Document, Dictionary<DbRight.ContentTypes, Right>> DocumentSpecialRight { get; set; }
        
        /// <summary>
        /// Stores the company rights of this user per company 
        /// </summary>
        private Dictionary<int,List<Right>> CompanyRights = new Dictionary<int,List<Right>>();
        /// <summary>
        /// Stores the financial year rights of this user per financial year 
        /// </summary>
        private Dictionary<int,List<Right>> FyearRights = new Dictionary<int,List<Right>>();
        /// <summary>
        /// Stores the document rights of this user per document 
        /// </summary>
        private Dictionary<int,List<Right>> DocumentRights = new Dictionary<int,List<Right>>();
        /// <summary>
        /// Stores the special rights of this user per document per content type 
        /// </summary>
        private Dictionary<int, Dictionary<DbRight.ContentTypes, List<Right>>> DocumentSpecialRights = new Dictionary<int, Dictionary<DbRight.ContentTypes, List<Right>>>();

        private Right RootRight { get; set; }

        private bool IsCompanyAdmin
        {
            get { return (User.IsCompanyAdmin && User.AssignedCompanies.Any(c => CompanyReadAllowed[c.Id])); }
        }

        public bool HasAnyGrantRight {
            get {
                if (User.IsAdmin) return true;
                if (IsCompanyAdmin) return true;
                bool hasAnyGrantRight = false;
                foreach (var right in CompanyToRight.Values) hasAnyGrantRight |= right.IsGrantAllowed;
                foreach (var right in FinancialYearToRight.Values) hasAnyGrantRight |= right.IsGrantAllowed;
                foreach (var right in DocumentToRight.Values) hasAnyGrantRight |= right.IsGrantAllowed;
                foreach (var dict in DocumentSpecialRight) 
                    foreach (var right in dict.Value.Values) 
                        hasAnyGrantRight |= right.IsGrantAllowed;
                hasAnyGrantRight |= RootRight.IsGrantAllowed;
                return hasAnyGrantRight;
            }
        }
        #endregion

        #region methods

        #region Company admin

        private void AssignedCompaniesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            Init(this.User);
        }

        private bool IsAdminOfCompany(Company company) {
            return (User.IsCompanyAdmin && User.IsCompanyAssigned(company));
        }

        #endregion Company admin

        #region GetMergedRight
        private  Right GetMergedRight(List<Right> rights) {
            var right = new Right(new DbRight());
            for (int i = 0; i < (int)DbRight.RightTypes.Count; ++i)
                right.DbRight.SetRight(i, GetMergedRight(i, rights));
            return right;
        }

        private bool GetMergedRight(int i, List<Right> rights) {

            if(rights.Count == 0) return false;

            bool? result = null;
            foreach (var r in rights) {
                if (r.IsRightExplicitlySet(i)) {
                    if (result.HasValue) result &= r.DbRight.GetRight(i).Value;
                    else result = r.DbRight.GetRight(i).Value;
                }

            }

            if (!result.HasValue) {
                List<Right> subRights = (from r in rights where r.ParentRight != null select r.ParentRight).ToList();
                result = GetMergedRight(i, subRights);
            }

            return result.Value;
        }
        #endregion

        #region get rights

        public Right GetRight(Company company) {

            // admin has full grant rights
            if (User.IsAdmin)
            {
                Right right = new Right(new DbRight());
                if (CompanyToRight.ContainsKey(company.Id))
                    right.SetRights(CompanyToRight[company.Id]);
                right.SetAdminRights();
                return right;
            }
            if (User.IsCompanyAdmin)
            {
                Right right = new Right(new DbRight());
                if (CompanyToRight.ContainsKey(company.Id))
                    if (IsAdminOfCompany(company))
                    {
                        right.SetRights(CompanyToRight[company.Id]);
                        right.SetAdminRights();
                        return right;
                    }
            }

            if (CompanyToRight.ContainsKey(company.Id))
                return CompanyToRight[company.Id];
            return new Right(new DbRight());
        }

        public Right GetFinancialYearRight(int id) {

            // admin has full grant rights
            if (User.IsAdmin) {
                Right right = new Right(new DbRight());
                if (FinancialYearToRight.ContainsKey(id)) right.SetRights(FinancialYearToRight[id]);
                right.SetAdminRights();
                return right;
            }

            if (FinancialYearToRight.ContainsKey(id)) return FinancialYearToRight[id];
            return new Right(new DbRight());
        }

        public bool HasFinancialYearWriteRight(FinancialYear fyear) {
            return (FinancialYearToRight.ContainsKey(fyear.Id) && FinancialYearToRight[fyear.Id].IsWriteAllowed) ||
                   User.IsAdmin || !FinancialYearToRight.ContainsKey(fyear.Id);
        }

        public Right GetRight(FinancialYear fYear) {
            return GetFinancialYearRight(fYear.Id);
        }

        public Right GetRight(Document document) {
            // admin has full grant rights
            if (User.IsAdmin || User.IsCompanyAdmin)
            {
                if (CompanyManager.Instance.IsCompanyAllowed(document.Company)) {
                    Right right = new Right(new DbRight());
                    if (DocumentToRight.ContainsKey(document.Id))
                        right.SetRights(DocumentToRight[document.Id]);
                    right.SetAdminRights();
                    return right;
                }
            }

            // return owner right, if current user is the document owner
            if (document != null && document.Owner != null && User.Id == document.Owner.Id) {
                if (CompanyManager.Instance.IsCompanyAllowed(document.Company)) {
                    Right right = new Right(new DbRight());
                    if (DocumentToRight.ContainsKey(document.Id))
                        right.SetRights(DocumentToRight[document.Id]);
                    right.SetOwnerRights();
                    return right;
                }
            }

            if (document != null && DocumentToRight.ContainsKey(document.Id)) {
                if (CompanyManager.Instance.IsCompanyAllowed(document.Company))
                    return DocumentToRight[document.Id];
            }
            return new Right(new DbRight());
        }

        public Right GetSpecialRight(Document document, DbRight.ContentTypes specialRight) {
            // admin has full grant rights
            // company admin has full grant rights if the company is the administered company of company admin
            if (User.IsAdmin || User.IsCompanyAdmin) {
                Right right = new Right(new DbRight());
                if (DocumentSpecialRight.ContainsKey(document) && DocumentSpecialRight[document].ContainsKey(specialRight))
                    right.SetRights(DocumentSpecialRight[document][specialRight]);
                right.SetAdminRights();
                return right;
            }

            // return owner right, if current user is the document owner
            if (User.Id == document.Owner.Id) {
                Right right = new Right(new DbRight());
                if (DocumentToRight.ContainsKey(document.Id))
                    right.SetRights(DocumentToRight[document.Id]);
                right.SetOwnerRights();
                return right;
            }

            if (DocumentSpecialRight.ContainsKey(document) && DocumentSpecialRight[document].ContainsKey(specialRight))
                return DocumentSpecialRight[document][specialRight];
            return new Right(new DbRight());
        }
        
        public Right GetRootRight() {
            // admin has full grant rights
            if (User.IsAdmin) {
                Right right = new Right(new DbRight());
                right.SetRights(RootRight);
                right.SetAdminRights();
                return right;
            }
            if (User.IsCompanyAdmin) {
                Right right = new Right(new DbRight());
                right.SetRights(RootRight);
                //right.SetAdminRights();
                return right;
            }
            return RootRight;
        }

        #endregion get rights
               
        //Checks if the company should be visible, which is the case if it is not explicitly denied and one or more sub documents are readable or the company is readable
        public bool CompanyVisible(Company company) {
            return CompanyVisibleForUser(company, UserManager.Instance.CurrentUser);
        }

        //Checks if the company should be visible, which is the case if it is not explicitly denied and one or more sub documents are readable or the company is readable
        public bool CompanyVisibleForUser(Company company, User user) {
            if (company.Owner.Id == user.Id) return true;
            //if (UserManager.Instance.CurrentUser.IsAdmin) return true;
            if (user.IsAdmin) return true;
            if (!CompanyReadAllowed.ContainsKey(company.Id)) return false;
            return CompanyReadAllowed[company.Id];
        }

        public bool CompanyEditable(Company company) {
            if (UserManager.Instance.CurrentUser == null) return false;
            if (company.Owner.Id == UserManager.Instance.CurrentUser.Id) return true;
            if (UserManager.Instance.CurrentUser.IsAdmin) return true;
            if (!CompanyToRight.ContainsKey(company.Id)) return false;
            return CompanyToRight[company.Id].IsWriteAllowed;
        }

        public void CompanyAdded(Company company) {
            if (CompanyToRight.ContainsKey(company.Id))
                CompanyToRight[company.Id].SetFullRights();
            CompanyToRight[company.Id] = new Right(new DbRight());
            CompanyToRight[company.Id].SetFullRights();
            CompanyReadAllowed[company.Id] = true;
        }

        public void DocumentAdded(Document document) {
            if (DocumentToRight.ContainsKey(document.Id))
                DocumentToRight[document.Id].SetFullRights();
            DocumentToRight[document.Id] = new Right(new DbRight());
            DocumentToRight[document.Id].SetFullRights();
        }

        internal EffectiveUserRightTreeNode GetEffectiveUserRightTree() {

            Right rootRight = GetRootRight();
            EffectiveRightDescriptor descriptor = GetEffectiveRightDescriptorForRoot(rootRight);
            EffectiveUserRightTreeNode result = new EffectiveUserRightTreeNode(rootRight, ResourcesCommon.RightTreeNodeAllCaption, descriptor);            
            Dictionary<FinancialYear, List<Document>> documentsByFinancialYearDict = new Dictionary<FinancialYear, List<Document>>();

            foreach (Document document in DocumentManager.Instance.Documents) {
                if (!documentsByFinancialYearDict.ContainsKey(document.FinancialYear)) documentsByFinancialYearDict[document.FinancialYear] = new List<Document>();
                documentsByFinancialYearDict[document.FinancialYear].Add(document);
            }

            foreach (var company in (UserManager.Instance.CurrentUser.IsAdmin ? CompanyManager.Instance.Companies : CompanyManager.Instance.AllowedCompanies)) {
                EffectiveRightDescriptor descriptorCompany = GetEffectiveRightDescriptor(company);
                var companyNode = new EffectiveUserRightTreeNode(GetRight(company), company.DisplayString, descriptorCompany);
                result.Children.Add(companyNode);
                foreach (var fyear in company.VisibleFinancialYears) {
                    EffectiveRightDescriptor descriptorFYear = GetEffectiveRightDescriptor(fyear);
                    var fyearNode = new EffectiveUserRightTreeNode(GetRight(fyear), fyear.FYear.ToString(), descriptorFYear);
                    companyNode.Children.Add(fyearNode);
                    if (documentsByFinancialYearDict.ContainsKey(fyear)) {
                        foreach (var document in documentsByFinancialYearDict[fyear]) {
                            EffectiveRightDescriptor descriptorDocument = GetEffectiveRightDescriptor(document);
                            var docNode = new EffectiveUserRightTreeNode(GetRight(document), document.Name, descriptorDocument);
                            fyearNode.Children.Add(docNode);
                            foreach (DbRight.ContentTypes contentType in Enum.GetValues(typeof(DbRight.ContentTypes))) {
                                if (DbRight.IsSpecialRight(contentType)) {
                                    string header;
                                    switch (contentType) {
                                        case DbRight.ContentTypes.DocumentSpecialRight_Rest:
                                            header = ResourcesCommon.RightTreeNodeOtherReportsCaption;
                                            break;
                                        case DbRight.ContentTypes.DocumentSpecialRight_TransferValueCalculation:
                                            header = ResourcesCommon.RightTreeNodeReconciliationCaption;
                                            break;
                                        default:
                                            header = "test";
                                            break;
                                    }
                                    Right specialRight = GetSpecialRight(document, contentType);
                                    EffectiveRightDescriptor descriptorSpecial = GetEffectiveRightDescriptor(specialRight, document, contentType);
                                    var specialRightNode = new EffectiveUserRightTreeNode(specialRight, header, descriptorSpecial);
                                    docNode.Children.Add(specialRightNode);
                                }
                            }
                        }
                    }
                }
            }

            foreach (Document document in DocumentManager.Instance.Documents) {
                Right documentRight = GetRight(document);
                documentRight.ParentRight = GetRight(document.FinancialYear);
                foreach (DbRight.ContentTypes contentType in Enum.GetValues(typeof(DbRight.ContentTypes))) {
                    if (DbRight.IsSpecialRight(contentType))
                        GetSpecialRight(document, contentType).ParentRight = documentRight;
                }
            }

            return result;
        }

        #region [ Right descriptor ]

        #region [ Special right ]

        /// <summary>
        /// Creates an EffectiveRightDescriptor for special right
        /// </summary>
        /// <param name="specialRight"></param>
        /// <param name="document"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        private EffectiveRightDescriptor GetEffectiveRightDescriptor(Right specialRight, Document document, DbRight.ContentTypes contentType) {
            EffectiveRightDescriptor descriptor = new EffectiveRightDescriptor(this.User, specialRight);

            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Read, specialRight, document, contentType);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Write, specialRight, document, contentType);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Send, specialRight, document, contentType);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Export, specialRight, document, contentType);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Grant, specialRight, document, contentType);

            return descriptor;
        }

        private void SetEffectiveRightDescriptorForType(ref EffectiveRightDescriptor descriptor, DbRight.RightTypes type, Right specialRight, Document document, DbRight.ContentTypes contentType, bool inherited = false) {
            List<Right> denied = new List<Right>();
            List<Right> granted = new List<Right>();

            // check Document rights first
            // collect denies
            denied.AddRange(GetSpecialRightForType(document, contentType, type, false));
            // collect allows if no deny exists
            if (denied.Count == 0) {
                granted.AddRange(GetSpecialRightForType(document, contentType, type, true));
            }
            // if rights are inherited form this object (if grant or deny found and inherited == true)
            if (inherited && (granted.Count > 0 || denied.Count > 0))
                SetInheritedFromForType(ref descriptor, specialRight, type);
            if (granted.Count > 0) {
                descriptor.SetRightType(type, true);
                SetSourceRoleForType(ref descriptor, Roles.Single(r => r.DbRole.Id == granted[0].DbRight.RoleId), type);
            } else if (denied.Count > 0) {
                descriptor.SetRightType(type, false);
                SetSourceRoleForType(ref descriptor, Roles.Single(r => r.DbRole.Id == denied[0].DbRight.RoleId), type);
            } else {
                SetEffectiveRightDescriptorForType(ref descriptor, type, document, true);
            }
        }

        #endregion [ Special right ]

        #region [ Document ]

        /// <summary>
        /// Creates EffectiveRightDescriptor for document
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private EffectiveRightDescriptor GetEffectiveRightDescriptor(Document document) {
            EffectiveRightDescriptor descriptor = new EffectiveRightDescriptor(this.User, document);

            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Read, document);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Write, document);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Send, document);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Export, document);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Grant, document);

            return descriptor;
        }

        private void SetEffectiveRightDescriptorForType(ref EffectiveRightDescriptor descriptor, DbRight.RightTypes type, Document document, bool inherited = false) {
            List<Right> denied = new List<Right>();
            List<Right> granted = new List<Right>();

            // check Document rights first
            // collect denies
            denied.AddRange(GetDocumentRightForType(document, type, false));
            // collect allows if no deny exists
            if (denied.Count == 0) {
                granted.AddRange(GetDocumentRightForType(document, type, true));
            }
            // if rights are inherited form this object (if grant or deny found and inherited == true)
            if (inherited && (granted.Count > 0 || denied.Count > 0))
                SetInheritedFromForType(ref descriptor, document, type);
            if (granted.Count > 0) {
                descriptor.SetRightType(type, true);
                SetSourceRoleForType(ref descriptor, Roles.Single(r => r.DbRole.Id == granted[0].DbRight.RoleId), type);
            } else if (denied.Count > 0) {
                descriptor.SetRightType(type, false);
                SetSourceRoleForType(ref descriptor, Roles.Single(r => r.DbRole.Id == denied[0].DbRight.RoleId), type);
            } else {
                SetEffectiveRightDescriptorForType(ref descriptor, type, document.FinancialYear, true);
            }
        }

        #endregion [ Document ]

        #region [ Financial year ]

        /// <summary>
        /// Creates EffectiveRightDescriptor for financial year
        /// </summary>
        /// <param name="fYear"></param>
        /// <returns></returns>
        private EffectiveRightDescriptor GetEffectiveRightDescriptor(FinancialYear fYear) {
            EffectiveRightDescriptor descriptor = new EffectiveRightDescriptor(this.User, fYear);
            
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Read, fYear);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Write, fYear);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Send, fYear);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Export, fYear);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Grant, fYear);

            return descriptor;
        }

        private void SetEffectiveRightDescriptorForType(ref EffectiveRightDescriptor descriptor, DbRight.RightTypes type, FinancialYear fYear, bool inherited = false) {
            List<Right> denied = new List<Right>();
            List<Right> granted = new List<Right>();

            // check FinancialYear rights first
            // collect denies
            denied.AddRange(GetFinancialYearRightForType(fYear, type, false));
            // collect allows if no deny exists
            if (denied.Count == 0) {
                granted.AddRange(GetFinancialYearRightForType(fYear, type, true));
            }
            // if rights are inherited form this object (if grant or deny found and inherited == true)
            if (inherited && (granted.Count > 0 || denied.Count > 0))
                SetInheritedFromForType(ref descriptor, fYear, type);
            if (granted.Count > 0) {
                descriptor.SetRightType(type, true);
                SetSourceRoleForType(ref descriptor, Roles.Single(r => r.DbRole.Id == granted[0].DbRight.RoleId), type);
            } else if (denied.Count > 0) {
                descriptor.SetRightType(type, false);
                SetSourceRoleForType(ref descriptor, Roles.Single(r => r.DbRole.Id == denied[0].DbRight.RoleId), type);
            } else {
                SetEffectiveRightDescriptorForType(ref descriptor, type, fYear.Company, true);
            }
        }

        #endregion [ Financial year ]

        #region [ Company ]

        /// <summary>
        /// Creates EffectiveRightDescriptor for company
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        private EffectiveRightDescriptor GetEffectiveRightDescriptor(Company company) {
            EffectiveRightDescriptor descriptor = new EffectiveRightDescriptor(this.User, company);

            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Read, company);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Write, company);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Send, company);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Export, company);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Grant, company);

            return descriptor;
        }

        private void SetEffectiveRightDescriptorForType(ref EffectiveRightDescriptor descriptor, DbRight.RightTypes type, Company company, bool inherited = false) {
            List<Right> denied = new List<Right>();
            List<Right> granted = new List<Right>();

            // check Company rights first
            // collect denies
            denied.AddRange(GetCompanyRightForType(company, type, false));
            // collect allows if no deny exists
            if (denied.Count == 0) {
                granted.AddRange(GetCompanyRightForType(company, type, true));
            }
            // if rights are inherited form this object (if grant or deny found and inherited == true)
            if (inherited && (granted.Count > 0 || denied.Count > 0))
                SetInheritedFromForType(ref descriptor, company, type);
            if (granted.Count > 0 ) {
                descriptor.SetRightType(type, true);
                SetSourceRoleForType(ref descriptor, Roles.Single(r => r.DbRole.Id == granted[0].DbRight.RoleId), type);
            } else if (denied.Count > 0) {
                descriptor.SetRightType(type, false);
                SetSourceRoleForType(ref descriptor, Roles.Single(r => r.DbRole.Id == denied[0].DbRight.RoleId), type);
            } else {
                SetEffectiveRightDescriptorForType(ref descriptor, type, GetRootRight(), true);
            }
        }

        #endregion [ Company ]

        #region [ Root ]

        /// <summary>
        /// Creates EffectiveRightDescriptor for root node
        /// </summary>
        /// <param name="rootRight"></param>
        /// <returns></returns>
        private EffectiveRightDescriptor GetEffectiveRightDescriptorForRoot(Right rootRight) {
            EffectiveRightDescriptor descriptor = new EffectiveRightDescriptor(this.User, rootRight);

            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Read, rootRight);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Write, rootRight);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Send, rootRight);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Export, rootRight);
            SetEffectiveRightDescriptorForType(ref descriptor, DbRight.RightTypes.Grant, rootRight);

            return descriptor;
        }

        private void SetEffectiveRightDescriptorForType(ref EffectiveRightDescriptor descriptor, DbRight.RightTypes type, Right rootRight, bool inherited = false) {

            switch (type) {
                case DbRight.RightTypes.Read:
                    if (rootRight.IsRightExplicitlySet(type)) descriptor.IsReadAllowed = rootRight.IsReadAllowed;
                    break;
                case DbRight.RightTypes.Write:
                    if (rootRight.IsRightExplicitlySet(type)) descriptor.IsWriteAllowed = rootRight.IsWriteAllowed;
                    break;
                case DbRight.RightTypes.Grant:
                    if (rootRight.IsRightExplicitlySet(type)) descriptor.IsGrantAllowed = rootRight.IsGrantAllowed;
                    break;
                case DbRight.RightTypes.Export:
                    if (rootRight.IsRightExplicitlySet(type)) descriptor.IsExportAllowed = rootRight.IsExportAllowed;
                    break;
                case DbRight.RightTypes.Send:
                    if (rootRight.IsRightExplicitlySet(type)) descriptor.IsSendAllowed = rootRight.IsSendAllowed;
                    break;
                case DbRight.RightTypes.Count:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
            // if rights are inherited form this object (if grant or deny found and inherited == true)
            if (inherited) SetInheritedFromForType(ref descriptor, rootRight, type);
        }

        #endregion [ Root ]

        /// <summary>
        /// Sets the correcponding source property on the EffectiveRightDescriptor
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="role"></param>
        /// <param name="type"></param>
        private void SetSourceRoleForType(ref EffectiveRightDescriptor descriptor, Role role, DbRight.RightTypes type) {
            switch (type) {
                case DbRight.RightTypes.Read:
                    descriptor.ReadSource = role;
                    break;
                case DbRight.RightTypes.Write:
                    descriptor.WriteSource = role;
                    break;
                case DbRight.RightTypes.Grant:
                    descriptor.GrantSource = role;
                    break;
                case DbRight.RightTypes.Export:
                    descriptor.ExportSource = role;
                    break;
                case DbRight.RightTypes.Send:
                    descriptor.SendSource = role;
                    break;
                case DbRight.RightTypes.Count:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Sets the correcponding inherited from property on the EffectiveRightDescriptor
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="parent"></param>
        /// <param name="type"></param>
        private void SetInheritedFromForType(ref EffectiveRightDescriptor descriptor, object parent, DbRight.RightTypes type) {
            switch (type) {
                case DbRight.RightTypes.Read:
                    descriptor.ReadInheritedFrom = parent;
                    break;
                case DbRight.RightTypes.Write:
                    descriptor.WriteInheritedFrom = parent;
                    break;
                case DbRight.RightTypes.Grant:
                    descriptor.GrantInheritedFrom = parent;
                    break;
                case DbRight.RightTypes.Export:
                    descriptor.ExportInheritedFrom = parent;
                    break;
                case DbRight.RightTypes.Send:
                    descriptor.SendInheritedFrom = parent;
                    break;
                case DbRight.RightTypes.Count:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        private IEnumerable<Right> GetSpecialRightForType(Document document, DbRight.ContentTypes contentType, DbRight.RightTypes type, bool value) {
            switch (type) {
                case DbRight.RightTypes.Read:
                    return DocumentSpecialRights[document.Id][contentType].Where(r => r.DbRight.Read == value);
                case DbRight.RightTypes.Write:
                    return DocumentSpecialRights[document.Id][contentType].Where(r => r.DbRight.Write == value);
                case DbRight.RightTypes.Grant:
                    return DocumentSpecialRights[document.Id][contentType].Where(r => r.DbRight.Grant == value);
                case DbRight.RightTypes.Export:
                    return DocumentSpecialRights[document.Id][contentType].Where(r => r.DbRight.Export == value);
                case DbRight.RightTypes.Send:
                    return DocumentSpecialRights[document.Id][contentType].Where(r => r.DbRight.Send == value);
                case DbRight.RightTypes.Count:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        private IEnumerable<Right> GetDocumentRightForType(Document document, DbRight.RightTypes type, bool value) {
            switch (type) {
                case DbRight.RightTypes.Read:
                    return DocumentRights[document.Id].Where(r => r.DbRight.ContentType == DbRight.ContentTypes.Document && r.DbRight.Read == value);
                case DbRight.RightTypes.Write:
                    return DocumentRights[document.Id].Where(r => r.DbRight.ContentType == DbRight.ContentTypes.Document && r.DbRight.Write == value);
                case DbRight.RightTypes.Grant:
                    return DocumentRights[document.Id].Where(r => r.DbRight.ContentType == DbRight.ContentTypes.Document && r.DbRight.Grant == value);
                case DbRight.RightTypes.Export:
                    return DocumentRights[document.Id].Where(r => r.DbRight.ContentType == DbRight.ContentTypes.Document && r.DbRight.Export == value);
                case DbRight.RightTypes.Send:
                    return DocumentRights[document.Id].Where(r => r.DbRight.ContentType == DbRight.ContentTypes.Document && r.DbRight.Send == value);
                case DbRight.RightTypes.Count:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        private IEnumerable<Right> GetFinancialYearRightForType(FinancialYear fYear, DbRight.RightTypes type, bool value) {
            switch (type) {
                case DbRight.RightTypes.Read:
                    return FyearRights[fYear.Id].Where(r => r.DbRight.ContentType == DbRight.ContentTypes.FinancialYear && r.DbRight.Read == value);
                case DbRight.RightTypes.Write:
                    return FyearRights[fYear.Id].Where(r => r.DbRight.ContentType == DbRight.ContentTypes.FinancialYear && r.DbRight.Write == value);
                case DbRight.RightTypes.Grant:
                    return FyearRights[fYear.Id].Where(r => r.DbRight.ContentType == DbRight.ContentTypes.FinancialYear && r.DbRight.Grant == value);
                case DbRight.RightTypes.Export:
                    return FyearRights[fYear.Id].Where(r => r.DbRight.ContentType == DbRight.ContentTypes.FinancialYear && r.DbRight.Export == value);
                case DbRight.RightTypes.Send:
                    return FyearRights[fYear.Id].Where(r => r.DbRight.ContentType == DbRight.ContentTypes.FinancialYear && r.DbRight.Send == value);
                case DbRight.RightTypes.Count:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        private IEnumerable<Right> GetCompanyRightForType(Company company, DbRight.RightTypes type, bool value) {
            switch (type) {
                case DbRight.RightTypes.Read:
                    return CompanyRights[company.Id].Where(r => r.DbRight.ContentType == DbRight.ContentTypes.Company && r.DbRight.Read == value);
                case DbRight.RightTypes.Write:
                    return CompanyRights[company.Id].Where(r => r.DbRight.ContentType == DbRight.ContentTypes.Company && r.DbRight.Write == value);
                case DbRight.RightTypes.Grant:
                    return CompanyRights[company.Id].Where(r => r.DbRight.ContentType == DbRight.ContentTypes.Company && r.DbRight.Grant == value);
                case DbRight.RightTypes.Export:
                    return CompanyRights[company.Id].Where(r => r.DbRight.ContentType == DbRight.ContentTypes.Company && r.DbRight.Export == value);
                case DbRight.RightTypes.Send:
                    return CompanyRights[company.Id].Where(r => r.DbRight.ContentType == DbRight.ContentTypes.Company && r.DbRight.Send == value);
                case DbRight.RightTypes.Count:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion [ Right descriptor ]

        #endregion methods
    }

    /// <summary>
    /// EffectiveRightDescriptor is a helper object for generating tooltip on the effective rights dialog for the purpose of making clear how the current setting came from
    /// </summary>
    public class EffectiveRightDescriptor
    {
        public EffectiveRightDescriptor(User user, object target) {
            this.User = user;
            this.Target = target;
        }

        public object Target { get; internal set; }
        public User User { get; internal set; }

        public bool? IsReadAllowed { get; set; }
        public bool? IsWriteAllowed { get; set; }
        public bool? IsGrantAllowed { get; set; }
        public bool? IsExportAllowed { get; set; }
        public bool? IsSendAllowed { get; set; }
        public bool? IsCountAllowed { get; set; }

        public bool IsReadInherited { get { return ReadInheritedFrom == null ? false : true; } }
        public bool IsWriteInherited { get { return WriteInheritedFrom == null ? false : true; } }
        public bool IsGrantInherited { get { return GrantInheritedFrom == null ? false : true; } }
        public bool IsExportInherited { get { return ExportInheritedFrom == null ? false : true; } }
        public bool IsSendInherited { get { return SendInheritedFrom == null ? false : true; } }
        public bool IsCountInherited { get { return CountInheritedFrom == null ? false : true; } }

        public Role ReadSource { get; set; }
        public Role WriteSource { get; set; }
        public Role GrantSource { get; set; }
        public Role ExportSource { get; set; }
        public Role SendSource { get; set; }
        public Role CountSource { get; set; }

        public object ReadInheritedFrom { get; set; }
        public object WriteInheritedFrom { get; set; }
        public object GrantInheritedFrom { get; set; }
        public object ExportInheritedFrom { get; set; }
        public object SendInheritedFrom { get; set; }
        public object CountInheritedFrom { get; set; }

        public void SetRightType(DbRight.RightTypes type, bool value) {
            switch (type) {
                case DbRight.RightTypes.Read:
                    IsReadAllowed = value;
                    break;
                case DbRight.RightTypes.Write:
                    IsWriteAllowed = value;
                    break;
                case DbRight.RightTypes.Grant:
                    IsGrantAllowed = value;
                    break;
                case DbRight.RightTypes.Export:
                    IsExportAllowed = value;
                    break;
                case DbRight.RightTypes.Send:
                    IsSendAllowed = value;
                    break;
                case DbRight.RightTypes.Count:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        public string GetToolTipForType(DbRight.RightTypes type) {
            switch (type) {
                case DbRight.RightTypes.Read:
                    return GetToolTipString(IsReadAllowed, ReadSource, ReadInheritedFrom, type);
                case DbRight.RightTypes.Write:
                    return GetToolTipString(IsWriteAllowed, WriteSource, WriteInheritedFrom, type);
                case DbRight.RightTypes.Grant:
                    return GetToolTipString(IsGrantAllowed, GrantSource, GrantInheritedFrom, type);
                case DbRight.RightTypes.Export:
                    return GetToolTipString(IsExportAllowed, ExportSource, ExportInheritedFrom, type);
                case DbRight.RightTypes.Send:
                    return GetToolTipString(IsSendAllowed, SendSource, SendInheritedFrom, type);
                case DbRight.RightTypes.Count:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        internal static string GetRightContentTypeVerbal(Right right) {
            switch (right.DbRight.ContentType) {
                case DbRight.ContentTypes.All:
                    return ResourcesCommon.RightTreeNodeAllCaption;
                case DbRight.ContentTypes.System:
                    return ResourcesMain.System;
                case DbRight.ContentTypes.Company:
                    return ResourcesMain.Company;
                case DbRight.ContentTypes.FinancialYear:
                    return ResourcesMain.FinancialYear;
                case DbRight.ContentTypes.Document:
                    return ResourcesMain.Report;
                case DbRight.ContentTypes.DocumentSpecialRight_Rest:
                    return ResourcesCommon.RightTreeNodeOtherReportsCaption;
                case DbRight.ContentTypes.DocumentSpecialRight_TransferValueCalculation:
                    return ResourcesCommon.RightTreeNodeReconciliationCaption;
            }
            return string.Empty;
        }

        private string GetToolTipString(bool? allowed, Role source, object parent, DbRight.RightTypes type) {
            
            string parentName = null;
            if (parent != null) {
                
                if (parent is Right) parentName = GetRightContentTypeVerbal(parent as Right);
                else if (parent is Company) parentName = (parent as Company).Name;
                else if (parent is FinancialYear) parentName = (parent as FinancialYear).FYear.ToString(LocalisationUtils.GermanCulture);
                else if (parent is Document) parentName = (parent as Document).Name;
                else parentName = parent.ToString();
            }

            //return "DEBUG: " + 
            //            (allowed.HasValue ? (allowed.Value ? "allowed" : "denied") : "not set") + 
            //            " by " + givenBy + 
            //            (parentName == null ? "" : " - INHERITED from: " + parentName +" -" ) + 
            //            " (" + type.ToString() + ")";

            string toolTip = string.Empty;

            // if owner of company or document
            //if (type == DbRight.RightTypes.Read || type == DbRight.RightTypes.Write) {
            if (!User.IsAdmin) {
                if (Target is Company && (Target as Company).Owner.Id == User.Id) {
                    toolTip = string.Format(ResourcesCommon.GrantOwnerCompany, User.UserName);
                    return toolTip;
                } else if (Target is Document && (Target as Document).Owner.Id == User.Id) {
                    toolTip = string.Format(ResourcesCommon.GrantOwnerDocument, User.UserName);
                    return toolTip;
                }
            }
            //}

            if (parentName != null &&
                !(User.IsAdmin && (Target is Company || Target is Document))
                ) {
                // if inherited
                toolTip += string.Format(ResourcesCommon.InheritedFrom, parentName);
            } else if (!allowed.HasValue) {
                // if not set
                toolTip += ResourcesCommon.NotSet;
            } else {
                // if allowed or denied
                //if (source == null) givenBy = ResourcesCommon.Self;
                //else if (source.DbRole.UserId == User.Id) givenBy = ResourcesCommon.Self;
                //else givenBy = source.DbRole.Name;
                //toolTip = allowed.Value ? string.Format(ResourcesCommon.Granted, givenBy) : string.Format(ResourcesCommon.Denied, givenBy);
                string givenBy = null;
                if (source == null || source.DbRole.UserId == User.Id) {
                    // if by self
                    givenBy = User.UserName;
                    //toolTip = allowed.Value
                    //              ? string.Format(ResourcesCommon.GrantedUser, givenBy)
                    //              : string.Format(ResourcesCommon.DeniedUser, givenBy);

                    if (allowed.Value)
                        if (!User.IsAdmin || type == DbRight.RightTypes.Send || type == DbRight.RightTypes.Export)
                            toolTip = string.Format(ResourcesCommon.GrantedUser, givenBy);
                        else
                            toolTip = ResourcesCommon.AllowedDueToAdminRights;
                    else
                        toolTip = string.Format(ResourcesCommon.DeniedUser, givenBy);
                } else {
                    // if by role
                    givenBy = source.DbRole.Name;
                    toolTip = allowed.Value
                                  ? string.Format(ResourcesCommon.Granted, givenBy)
                                  : string.Format(ResourcesCommon.Denied, givenBy);
                }
            }

            return toolTip;
        }

        public string ToolTipString {
            get {
                const string separator = ": ";
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}{1}", ResourcesCommon.Read, separator);
                sb.AppendLine(GetToolTipString(IsReadAllowed, ReadSource, ReadInheritedFrom, DbRight.RightTypes.Read));
                sb.AppendFormat("{0}{1}", ResourcesCommon.Write, separator);
                sb.AppendLine(GetToolTipString(IsWriteAllowed, WriteSource, WriteInheritedFrom, DbRight.RightTypes.Write));
                sb.AppendFormat("{0}{1}", ResourcesCommon.Grant, separator);
                sb.AppendLine(GetToolTipString(IsGrantAllowed, GrantSource, GrantInheritedFrom, DbRight.RightTypes.Grant));
                sb.AppendFormat("{0}{1}", ResourcesExport.Export, separator);
                sb.AppendLine(GetToolTipString(IsExportAllowed, ExportSource, ExportInheritedFrom, DbRight.RightTypes.Export));
                sb.AppendFormat("{0}{1}", ResourcesCommon.Send, separator);
                sb.Append(GetToolTipString(IsSendAllowed, SendSource, SendInheritedFrom, DbRight.RightTypes.Send));
                return sb.ToString();
            }
        }
    }
}

