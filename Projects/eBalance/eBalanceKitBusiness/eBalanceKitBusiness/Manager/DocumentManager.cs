// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-01-13
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DbAccess;
using DbAccess.Structures;
using Taxonomy.Enums;
using Utils;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.AuditCorrections;
using eBalanceKitBusiness.AuditCorrections.DbMapping;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.HyperCubes.DbMapping;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Manager {
    /// <summary>
    /// This class provides several document management functions.
    /// </summary>
    public class DocumentManager : NotifyPropertyChangedBase {

        private readonly ReaderWriterLockSlim _rwLockDocumentById = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _rwLockDocument = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _rwLockAllowed = new ReaderWriterLockSlim();
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);
        private static DocumentManager _instance;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private DocumentManager() {
            SystemManager.Instance.Systems.CollectionChanged += delegate { OnPropertyChanged("NewDocumentAllowed"); };
            _documentById = new Dictionary<int, Document>();
            CompanyManager.Instance.AllowedCompanies.CollectionChanged += delegate { OnPropertyChanged("NewDocumentAllowed"); };
            _documents = new ObservableCollectionAsync<Document>();
            _allowedDocuments = new ObservableCollectionAsync<Document>();
        }

        public static DocumentManager Instance { get { return _instance ?? (_instance = new DocumentManager()); } }

        public static void Reinitialize() {
            _instance = new DocumentManager();
        }

        #region class TreeNode
        private class TreeNode {
            public TreeNode() {
                Chields = new Dictionary<object, TreeNode>();
                Items = new ObservableCollectionAsync<object>();
            }

            private Dictionary<object, TreeNode> Chields { get; set; }
            public ObservableCollectionAsync<object> Items { get; private set; }

            public bool Contains(object obj) { return Chields.ContainsKey(obj); }

            public void Add(object obj) {
                Chields.Add(obj, new TreeNode());
                Items.Add(obj);

                var tmp = new List<object>();
                tmp.AddRange(Items);
                tmp.Sort();
                Items.Clear();
                foreach (object o in tmp) Items.Add(o);
            }

            public void Remove(object obj) {
                Chields.Remove(obj);
                Items.Remove(obj);
            }

            public TreeNode GetChield(object obj) { return Chields[obj]; }
        }
        #endregion class TreeNode

        #region properties
        private Dictionary<int, Document> _documentById { get; set; }

        private ObservableCollectionAsync<Document> _documents;
        public ObservableCollectionAsync<Document> Documents { 
            get {
                _rwLockDocument.TryEnterReadLock(Timeout.Infinite);
                try {
                    return _documents;
                } finally {
                    _rwLockDocument.ExitReadLock();
                }
            }
        }

        private ObservableCollectionAsync<Document> _allowedDocuments;
        public ObservableCollectionAsync<Document> AllowedDocuments {
            get {
                _rwLockAllowed.TryEnterReadLock(Timeout.Infinite);
                try {
                    return _allowedDocuments;
                } finally {
                    _rwLockAllowed.ExitReadLock();
                }
            }
        }

        private TreeNode _usedElementsTree { get; set; }

        #region HasAllowedDocuments
        public bool HasAllowedDocuments { get { return _allowedDocuments != null && _allowedDocuments.Any(); } }
        #endregion // HasAllowedDocuments

        #endregion properties

        #region methods

        #region GetUsedXXX
        public ObservableCollection<object> GetUsedSystems() { return _usedElementsTree.Items; }

        public ObservableCollection<object> GetUsedCompanies(
            Structures.DbMapping.System system) { return _usedElementsTree.GetChield(system).Items; }

        public ObservableCollection<object> GetUsedFinancialYears(
            Structures.DbMapping.System system, Company company) { return _usedElementsTree.GetChield(system).GetChield(company).Items; }

        public ObservableCollection<object> GetUsedDocuments(
            Structures.DbMapping.System system, Company company, FinancialYear fyear) { return _usedElementsTree.GetChield(system).GetChield(company).GetChield(fyear).Items; }
        #endregion GetUsedXXX

        #region Init

        public void RequestCancellation() {
            _tokenSource.Cancel();
        }

        //public void InitRemainingDocuments(Action action) {

        //    EnvironmentState state = UserConfig.EnvironmentStates.FirstOrDefault(e => e.UserId == UserManager.Instance.CurrentUser.Id);
        //    if (state != null) {
        //        tokenSource = new CancellationTokenSource();
        //        IEnumerable<Company> remainingCompanies = CompanyManager.Instance.Companies.Where(c => c.Id != state.SelectedCompanyId);
        //        foreach (Company remainingCompany in remainingCompanies) {
        //            Company company = remainingCompany;
        //            Task task = new Task(() => InitCompany(company, action, tokenSource.Token), tokenSource.Token);
        //            task.Start();
        //        }
        //    }
        //}

        public void InitRemainingDocuments(Action action) {

            _tokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(() => {
                List<Task> tasks = new List<Task>();
                EnvironmentState state = UserConfig.EnvironmentStates.FirstOrDefault(e => e.UserId == UserManager.Instance.CurrentUser.Id);
                if (state != null) {
                    _tokenSource = new CancellationTokenSource();
                    IEnumerable<Company> remainingCompanies = CompanyManager.Instance.Companies.Where(c => c.Id != state.SelectedCompanyId);
                    foreach (Company remainingCompany in remainingCompanies) {
                        Company company = remainingCompany;
                        tasks.Add(Task.Factory.StartNew(() => InitCompany(company, action, _tokenSource.Token), _tokenSource.Token));
                    }
                }

                try {
                    Task.WaitAll(tasks.ToArray());
                    if (!_tokenSource.Token.IsCancellationRequested)
                        UserManager.Instance.ReinitializeRightDeducer();
                    if (!_tokenSource.Token.IsCancellationRequested)
                        RightManager.InitAllowedDetails();
                }
                catch (AggregateException)
                {
                    Debug.WriteLine("Tasks were canceled");
                }
            }, _tokenSource.Token);
        }

        private void InitCompany(Company company, Action action, CancellationToken token) {
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();
            
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                List<Document> tmp = conn.DbMapping.Load<Document>(conn.Enquote("company_id") + "=" + company.Id);

                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();

                foreach (var document in tmp) {
                    if (token.IsCancellationRequested)
                        token.ThrowIfCancellationRequested();

                    LockDocumentById(() => _documentById[document.Id] = document);
                    document.Init(conn);
                }

                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();

                tmp.Sort();

                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();

                foreach (var document in tmp) {
                    if (token.IsCancellationRequested)
                        token.ThrowIfCancellationRequested();

                    Semaphore.Wait();
                    try {
                        AddDocumentPropertyChangedHandler(document);
                        LockDocuments(() => _documents.Add(document));
                        if (UserManager.Instance.RightDeducer.GetRight(document).IsReadAllowed) {
                            AddUsedElement(document);
                            LockAllowedDocuments(() => _allowedDocuments.Add(document));
                        }
                        document.Company.NotifyAssignedReportChanged();
                        document.System.NotifyAssignedReportChanged();
                        CompanyManager.Instance.NotifyAssignedReportChanged();
                        SystemManager.Instance.NotifyAssignedReportChanged();
                    } finally {
                        Semaphore.Release();
                    }
                }
            }

            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            // refresh companies in MainWindowModel
            action();
        }

        public void Init() {
            _documents = new ObservableCollectionAsync<Document>();
            _documentById = new Dictionary<int, Document>();

            _allowedDocuments = new ObservableCollectionAsync<Document>();
            _allowedDocuments.CollectionChanged += (sender, args) => OnPropertyChanged("HasAllowedDocuments");

            //AllowedOwners = UserManager.Instance.ActiveUsers;
            UserManager.Instance.ActiveUsers.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ActiveUsers_CollectionChanged);

            EnvironmentState state = UserConfig.EnvironmentStates.FirstOrDefault(e => e.UserId == UserManager.Instance.CurrentUser.Id);
            
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                List<Document> tmp;
                //if (state != null)
                //    tmp = conn.DbMapping.Load<Document>(conn.Enquote("company_id") + "=" + state.SelectedCompanyId);
                //else
                    tmp = conn.DbMapping.Load<Document>();
                
                foreach (var document in tmp) {
                    _documentById[document.Id] = document;
                    document.Init(conn);
                }

                tmp.Sort();
                foreach (var document in tmp) {
                    AddDocumentPropertyChangedHandler(document);
                    _documents.Add(document);
                    document.Company.NotifyAssignedReportChanged();
                    document.System.NotifyAssignedReportChanged();
                    CompanyManager.Instance.NotifyAssignedReportChanged();
                    SystemManager.Instance.NotifyAssignedReportChanged();
                }
            }
        }

        void ActiveUsers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged("Owner");
        }

        private void AddDocumentPropertyChangedHandler(Document document) {
            document.PropertyChanged += (sender, e) => {
                switch (e.PropertyName) {
                    case "FinancialYear":
                    case "Company":
                        AddUsedElement(sender as Document);
                        OnPropertyChanged("HasPreviousYearReports");

                        break;

                    case "System":
                        AddUsedElement(sender as Document);
                        break;
                    case "Owner":
                        OnPropertyChanged("AllowedOwners");
                        break;
                }
            };
            
            document.PropertyChanging += (sender, e) => {
                switch (e.PropertyName) {
                    case "Company":
                    case "System":
                    case "FinancialYear":
                        var doc = (Document) sender;
                        DeleteUsedElement(doc);                        
                        break;
                }                
            };
        }

        #endregion Init

        #region AddDocument
        public Document AddDocument(Taxonomy.Interfaces.ITaxonomyInfo mainTaxonomyInfo = null /*IElement selectedBusinessClass*/) {
            if (!NewDocumentAllowed) return null;

            var document = new Document();
            if (mainTaxonomyInfo == null) {
                document.Init();
            } else {
                document.Init(mainTaxonomyInfo);
            }
            

            int number = 1;
            while (Exists(document.System, document.Company, document.FinancialYear, document.Name)) {
                document.Name = "Standardreport_" + number++;
            }

            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    try {
                        conn.DbMapping.Save(document);
                        LogManager.Instance.NewDocument(document, false);
                        foreach (IBalanceList balanceList in document.BalanceLists) BalanceListManager.Save(conn, balanceList);
                        conn.CommitTransaction();
                    } catch (Exception ex) {
                        conn.RollbackTransaction();
                        throw new Exception(ex.Message, ex);
                    }

                    // insert sorted into observable collection
                    LockDocuments(() => {
                        var tmp = new List<Document>(_documents) {document};
                        tmp.Sort();
                        _documents.Clear();
                        foreach (var item in tmp) _documents.Add(item);
                    });

                    LockDocumentById(() => _documentById[document.Id] = document);

                    // add entry to used elements, if it does not yet exist
                    AddUsedElement(document);
                    AddDocumentPropertyChangedHandler(document);
                    
                    RightManager.DocumentAdded(document);
                } catch (Exception ex) {
                    throw new Exception(ExceptionMessages.DocumentManagerAdd + ex.Message);
                }
            }

            LockAllowedDocuments(() => _allowedDocuments.Add(document));
            GenerateVirtualBalanceList(document);

            OnPropertyChanged("HasPreviousYearReports");
            OnPropertyChanged("HasAllowedDocuments");

            document.Company.NotifyAssignedReportChanged();
            document.System.NotifyAssignedReportChanged();
            CompanyManager.Instance.NotifyAssignedReportChanged();
            SystemManager.Instance.NotifyAssignedReportChanged();

            return document;
        }
        #endregion AddDocument

        #region GenerateVirtualBalanceList
        private void GenerateVirtualBalanceList(Document document, bool assignVirtualAccounts = true) {
            var vBalList = document.BalanceLists.Where(x => (x is VirtualBalanceList) || x.IsImported == false).ToList();
            if (!vBalList.Any()) {
                VirtualBalanceListAndAccountManager.GenerateAndAssignVirtualBalanceList(document, doAssignments: assignVirtualAccounts);
                //vBalList = document.BalanceLists.Where(x => (x is VirtualBalanceList)).ToList();
            }
        }
        #endregion

        #region Copy Document

        public class DbRelations {

            public DbRelations(string tableName, string keyColumn) {
                TableName = tableName;
                KeyColumn = keyColumn;
            }

            public string TableName;
            public string KeyColumn;
        }


        #region GetMaxInsertId
        private Int64 GetNextPkId(IDatabase conn, string tableName, string idColumn) {
            object obj = conn.ExecuteScalar("SELECT MAX(" + conn.Enquote(idColumn) + ") FROM " + conn.Enquote(tableName));
            if (obj is DBNull) return 0;
            else return Convert.ToInt64(obj) + 1;
        }
        #endregion


        //private int GetId(IDatabase conn, string query) {
        //    object obj = conn.ExecuteScalar(query);
        //    if (obj is DBNull) return 0;
        //    else return Convert.ToInt32(obj);

        //}

        private const string DefaultDocIdColumn = "document_id";
        private const string DefaultBalanceIdColumn = "balance_list_id";
        private const string DefaultAccountIdColumn = "account_id";
        private const string DefaultHyperCubeIdColumn = "hypercube_id";
        private const string DefaultReconciliationIdColumn = "reconciliation_id";
        private const string DefaultSplitAccountGroupIdColumn = "split_group_id";
        private const string DefaultIdColumnName = "id";
        private const string DocumentTableName = "documents";


        private IEnumerable<DbRelations> GetDocumentRelations() {

            HashSet<DbRelations> result = new HashSet<DbRelations> {
                new DbRelations("balance_lists", DefaultDocIdColumn),
                //new DbRelations("values_gaap", DefaultDocIdColumn),
                //new DbRelations("values_gcd", DefaultDocIdColumn),
                //new DbRelations("reconciliation_transactions", defaultDocIdColumn),
                new DbRelations("reconciliations", DefaultDocIdColumn),
                new DbRelations("hypercubes", DefaultDocIdColumn),
                new DbRelations("hypercube_items", DefaultDocIdColumn)
            };

            return result;
        }

        private IEnumerable<DbRelations> GetBalanceRelations() {
            HashSet<DbRelations> result = new HashSet<DbRelations> {
                new DbRelations("accounts", DefaultBalanceIdColumn),
                new DbRelations("virtual_accounts", DefaultBalanceIdColumn)
                //new DbRelations("splitted_accounts", defaultBalanceIdColumn),
                //new DbRelations("split_account_groups", defaultBalanceIdColumn)
            };

            return result;
        }

        private IEnumerable<DbRelations> GetValueTreeRelations() {
            HashSet<DbRelations> result = new HashSet<DbRelations> {
                new DbRelations("values_gaap", DefaultDocIdColumn),
                new DbRelations("values_gcd", DefaultDocIdColumn)
            };

            return result;
        }

        private IEnumerable<DbRelations> GetHyperCubeRelations() {
            HashSet<DbRelations> result = new HashSet<DbRelations> {
                new DbRelations("hypercube_items", DefaultHyperCubeIdColumn)
            };

            return result;
        }
        private IEnumerable<DbRelations> GetReconciliationRelations() {
            HashSet<DbRelations> result = new HashSet<DbRelations> {
                new DbRelations("reconciliation_transactions", DefaultReconciliationIdColumn)
            };

            return result;
        }

        private IEnumerable<DbRelations> GetAccountRelations() {
            HashSet<DbRelations> result = new HashSet<DbRelations> {
                new DbRelations("account_groups", "number"),
                //new DbRelations("splitted_accounts", defaultAccountIdColumn),
                new DbRelations("split_account_groups", DefaultAccountIdColumn)
            };

            return result;
        }

        private IEnumerable<DbRelations> GetSplitAccountGroupsRelations() {
            HashSet<DbRelations> result = new HashSet<DbRelations> {
                new DbRelations("splitted_accounts", DefaultSplitAccountGroupIdColumn)
            };

            return result;
        }

        private bool CopyBalanceListRelations(IDatabase conn, int oldBalanceListId, int newBalanceListId) {
            try {
                IEnumerable<DbRelations> balanceListRelations = GetBalanceRelations();

                foreach (DbRelations relation in balanceListRelations) {

                    var columns = conn.GetColumnNames(relation.TableName);
                    string query = "SELECT " + conn.Enquote(DefaultIdColumnName) + " FROM " + relation.TableName +
                                   " WHERE ";

                    var oldValueReader =
                        conn.ExecuteReader("select " + string.Join(", ", columns) + " from " + relation.TableName +
                                           " where " + conn.Enquote(relation.KeyColumn) + "=" + oldBalanceListId);

                    HashSet<Dictionary<string, object>> oldValues = new HashSet<Dictionary<string, object>>();
                    Dictionary<string, object> oldValueEntry = new Dictionary<string, object>();
                    Int64 pkId = 0;

                    while (oldValueReader.Read()) {
                        oldValueEntry = new Dictionary<string, object>();
                        foreach (var column in columns) {
                            oldValueEntry.Add(column, oldValueReader[column]);
                        }
                        oldValues.Add(oldValueEntry);

                    }
                    oldValueReader.Close();

                    foreach (Dictionary<string, object> values in oldValues) {
                        DbColumnValues insertValues = new DbColumnValues();
                        foreach (var column in columns) {
                            var val = values[column];
                            if (column.Equals(DefaultIdColumnName)) {
                                val = pkId = GetNextPkId(conn, relation.TableName, column);
                            }
                            if (column.Equals(relation.KeyColumn)) {
                                val = newBalanceListId;
                            }
                            insertValues[column] = val;

                            query += conn.Enquote(column) + "=" + conn.GetSqlString(val.ToString()) + ",";
                        }

                        // remove the last comma
                        query.Remove(query.Length - 1);

                        conn.InsertInto(relation.TableName, insertValues);


                        if (relation.TableName.Equals("accounts")) {
                            CopyAccountRelations(conn, int.Parse(values[DefaultIdColumnName].ToString()), (int) pkId,
                                                 oldBalanceListId, newBalanceListId);
                        }
                    }
                }
            } catch (Exception) {
                return false;
            }

            return true;
        }

        private bool CopyHyperCubeRelations(IDatabase conn, int oldHyperCubeId, int newHyperCubeId, int oldDocumentId,
                                            int newDocumentId) {
            try {
                IEnumerable<DbRelations> hyperCubeRelations = GetHyperCubeRelations();

                foreach (DbRelations relation in hyperCubeRelations) {

                    var columns = conn.GetColumnNames(relation.TableName);

                    var oldValueReader =
                        conn.ExecuteReader("select " + string.Join(", ", columns) + " from " + relation.TableName +
                                           " where " + relation.KeyColumn + "=" + oldHyperCubeId +
                                           " and " + DefaultDocIdColumn + "=" + oldDocumentId);

                    HashSet<Dictionary<string, object>> oldValues = new HashSet<Dictionary<string, object>>();
                    Dictionary<string, object> oldValueEntry;

                    while (oldValueReader.Read()) {
                        oldValueEntry = new Dictionary<string, object>();
                        foreach (var column in columns) {
                            oldValueEntry.Add(column, oldValueReader[column]);
                        }
                        oldValues.Add(oldValueEntry);

                    }
                    ;
                    oldValueReader.Close();

                    foreach (Dictionary<string, object> values in oldValues) {
                        DbColumnValues insertValues = new DbColumnValues();
                        foreach (var column in columns) {
                            var val = values[column];
                            if (column.Equals(DefaultIdColumnName)) {
                                val = GetNextPkId(conn, relation.TableName, column);
                            }
                            if (column.Equals(relation.KeyColumn)) {
                                val = newHyperCubeId;
                            }
                            if (column.Equals(DefaultDocIdColumn)) {
                                val = newDocumentId;
                            }
                            insertValues[column] = val;
                        }

                        conn.InsertInto(relation.TableName, insertValues);
                    }
                }
            } catch (Exception) {
                return false;
            }

            return true;
        }
        private bool CopyReconciliationRelations(IDatabase conn, int oldReconciliationId, int newReconciliationId, int oldDocumentId,
                                            int newDocumentId) {
            try {
                IEnumerable<DbRelations> reconciliationRelations = GetReconciliationRelations();

                foreach (DbRelations relation in reconciliationRelations) {

                    var columns = conn.GetColumnNames(relation.TableName);

                    var oldValueReader =
                        conn.ExecuteReader("select " + string.Join(", ", columns) + " from " + relation.TableName +
                                           " where " + relation.KeyColumn + "=" + oldReconciliationId +
                                           " and " + DefaultDocIdColumn + "=" + oldDocumentId);

                    HashSet<Dictionary<string, object>> oldValues = new HashSet<Dictionary<string, object>>();
                    Dictionary<string, object> oldValueEntry;

                    while (oldValueReader.Read()) {
                        oldValueEntry = new Dictionary<string, object>();
                        foreach (var column in columns) {
                            oldValueEntry.Add(column, oldValueReader[column]);
                        }
                        oldValues.Add(oldValueEntry);

                    }

                    oldValueReader.Close();

                    foreach (Dictionary<string, object> values in oldValues) {
                        DbColumnValues insertValues = new DbColumnValues();
                        foreach (var column in columns) {
                            var val = values[column];
                            if (column.Equals(DefaultIdColumnName)) {
                                val = GetNextPkId(conn, relation.TableName, column);
                            }
                            if (column.Equals(relation.KeyColumn)) {
                                val = newReconciliationId;
                            }
                            if (column.Equals(DefaultDocIdColumn)) {
                                val = newDocumentId;
                            }
                            insertValues[column] = val;
                        }

                        conn.InsertInto(relation.TableName, insertValues);
                    }
                }
            } catch (Exception) {
                return false;
            }

            return true;
        }

        private bool CopySplittedAccounts(IDatabase conn, int oldSplitGroupId, int newSplitGroupId, int oldBalanceListId,
                                          int newBalanceListId) {
            IEnumerable<DbRelations> accountRelations = GetSplitAccountGroupsRelations();

            foreach (DbRelations relation in accountRelations) {

                var columns = conn.GetColumnNames(relation.TableName);

                var oldValueReader =
                    conn.ExecuteReader("select " + string.Join(", ", columns) + " from " + relation.TableName +
                                       " where " + relation.KeyColumn + "=" + oldSplitGroupId +
                                       " and " + DefaultBalanceIdColumn + "=" + oldBalanceListId);

                HashSet<Dictionary<string, object>> oldValues = new HashSet<Dictionary<string, object>>();
                Dictionary<string, object> oldValueEntry = new Dictionary<string, object>();

                while (oldValueReader.Read()) {
                    oldValueEntry = new Dictionary<string, object>();
                    foreach (var column in columns) {
                        oldValueEntry.Add(column, oldValueReader[column]);
                    }
                    oldValues.Add(oldValueEntry);

                }
                oldValueReader.Close();

                foreach (Dictionary<string, object> values in oldValues) {
                    DbColumnValues insertValues = new DbColumnValues();
                    foreach (var column in columns) {
                        var val = values[column];
                        if (column.Equals(DefaultIdColumnName)) {
                            val = GetNextPkId(conn, relation.TableName, column);
                        }
                        if (column.Equals(relation.KeyColumn)) {
                            val = newSplitGroupId;
                        }
                        if (column.Equals(DefaultBalanceIdColumn)) {
                            val = newBalanceListId;
                        }
                        insertValues[column] = val;
                    }

                    conn.InsertInto(relation.TableName, insertValues);
                }
            }
            //catch (Exception) {
            //    throw;
            //    //return false;
            //}

            return true;
        }

        private bool CopyAccountRelations(IDatabase conn, int oldAccountId, int newAccountId, int oldBalanceListId,
                                          int newBalanceListId) {
            IEnumerable<DbRelations> accountRelations = GetAccountRelations();

            foreach (DbRelations relation in accountRelations) {

                var columns = conn.GetColumnNames(relation.TableName);

                var oldValueReader =
                    conn.ExecuteReader("select " + string.Join(", ", columns) + " from " + relation.TableName +
                                       " where " + relation.KeyColumn + "=" + oldAccountId +
                                       " and " + DefaultBalanceIdColumn + "=" + oldBalanceListId);

                HashSet<Dictionary<string, object>> oldValues = new HashSet<Dictionary<string, object>>();
                Dictionary<string, object> oldValueEntry = new Dictionary<string, object>();
                Int64 pkId = 0;

                while (oldValueReader.Read()) {
                    foreach (var column in columns) {
                        oldValueEntry.Add(column, oldValueReader[column]);
                    }
                    oldValues.Add(oldValueEntry);

                }
                oldValueReader.Close();

                foreach (Dictionary<string, object> values in oldValues) {
                    DbColumnValues insertValues = new DbColumnValues();
                    foreach (var column in columns) {
                        var val = values[column];
                        if (column.Equals(DefaultIdColumnName)) {
                            val = pkId = GetNextPkId(conn, relation.TableName, column);
                        }
                        if (column.Equals(relation.KeyColumn)) {
                            val = newAccountId;
                        }
                        if (column.Equals(DefaultBalanceIdColumn)) {
                            val = newBalanceListId;
                        }
                        insertValues[column] = val;
                    }

                    conn.InsertInto(relation.TableName, insertValues);

                    if (relation.TableName.Equals("split_account_groups")) {
                        CopySplittedAccounts(conn, int.Parse(values[DefaultIdColumnName].ToString()), (int) pkId,
                                             oldBalanceListId, newBalanceListId);
                    }
                }
            }
            //catch (Exception) {
            //    throw;
            //    return false;
            //}

            return true;
        }

        /// <summary>
        /// Creates an copy of the given document with the name "Copy of ..." (sourceDoc.Name)
        /// </summary>
        /// <param name="sourceDoc"></param>
        /// <returns></returns>
        private Document ExecCopyDocument(Document sourceDoc) {

            //throw new NotImplementedException();

            //RightManager.WriteRestDocumentAllowed(sourceDoc)


            IEnumerable<DbRelations> documentRelations = GetDocumentRelations();

            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    try {

                        var columns = conn.GetColumnNames(DocumentTableName);

                        var oldValueReader =
                            conn.ExecuteReader("SELECT " + string.Join(", ", columns) + " FROM " + DocumentTableName +
                                               " WHERE " + conn.Enquote(DefaultIdColumnName) + "=" + sourceDoc.Id);

                        HashSet<Dictionary<string, object>> oldValues = new HashSet<Dictionary<string, object>>();
                        Dictionary<string, object> oldValueEntry = new Dictionary<string, object>();
                        Int64 pkId = 0;


                        while (oldValueReader.Read()) {
                            foreach (var column in columns) {
                                oldValueEntry.Add(column, oldValueReader[column]);
                            }
                            oldValues.Add(oldValueEntry);

                        }
                        oldValueReader.Close();

                        foreach (Dictionary<string, object> oldValue in oldValues) {


                            DbColumnValues insertValues = new DbColumnValues();

                            foreach (var column in columns) {
                                var newValue = oldValue[column];
                                if (column.Equals(DefaultIdColumnName)) {
                                    newValue = pkId = GetNextPkId(conn, DocumentTableName, column);
                                } else if (column.Equals("owner_id")) {
                                    newValue = UserManager.Instance.CurrentUser.Id;
                                } else if (column.Equals("creation_date")) {
                                    newValue = DateTime.Now;
                                } else if (column.Equals("name")) {
                                    newValue = string.Format(ResourcesMain.CopyOf, oldValue[column]);
                                }

                                insertValues[column] = newValue;

                            }


                            conn.InsertInto(DocumentTableName, insertValues);
                        }

                        int newDocId = (int) pkId;


                        foreach (DbRelations relation in documentRelations) {

                            oldValues = new HashSet<Dictionary<string, object>>();

                            //var columnInfos = conn.GetColumnInfos(documentTable.TableName);

                            columns = conn.GetColumnNames(relation.TableName);

                            oldValueReader =
                                conn.ExecuteReader("SELECT " + string.Join(", ", columns) + " FROM " +
                                                   relation.TableName + " WHERE " + conn.Enquote(DefaultDocIdColumn) +
                                                   "=" + sourceDoc.Id);


                            while (oldValueReader.Read()) {
                                oldValueEntry = new Dictionary<string, object>();

                                foreach (var column in columns) {
                                    oldValueEntry.Add(column, oldValueReader[column]);
                                }
                                oldValues.Add(oldValueEntry);
                            }
                            oldValueReader.Close();

                            foreach (Dictionary<string, object> values in oldValues) {
                                DbColumnValues insertValues = new DbColumnValues();
                                foreach (var column in columns) {
                                    var val = values[column];
                                    if (column.Equals(DefaultIdColumnName)) {
                                        val = pkId = GetNextPkId(conn, relation.TableName, column);
                                    }
                                    if (column.Equals(relation.KeyColumn)) {
                                        val = newDocId;
                                    }
                                    insertValues[column] = val;

                                }


                                conn.InsertInto(relation.TableName, insertValues);

                                if (relation.TableName.Equals("balance_lists")) {
                                    CopyBalanceListRelations(conn, int.Parse(values[DefaultIdColumnName].ToString()),
                                                             (int) pkId);
                                } else if (relation.TableName.Equals("hypercubes")) {
                                    CopyHyperCubeRelations(conn, int.Parse(values[DefaultIdColumnName].ToString()),
                                                           (int) pkId, sourceDoc.Id, newDocId);
                                }
                                else if (relation.TableName.Equals("reconciliations")) {
                                    CopyReconciliationRelations(conn, int.Parse(values[DefaultIdColumnName].ToString()),
                                                           (int) pkId, sourceDoc.Id, newDocId);
                                }
                            }
                        }
                        conn.CommitTransaction();
                        
                        var newDoc =
                            conn.DbMapping.Load<Document>(conn.Enquote(DefaultIdColumnName) + "=" + newDocId).First();
                        newDoc.Init(conn);
                        //newDoc.
                        newDoc.ReportRights = new ReportRights(newDoc);
                        newDoc.LoadDetails(null);

                        //CopyValueTrees(sourceDoc, newDocId, conn);

                        CopyValueTreeValues(sourceDoc, newDoc);

                        newDoc.LoadValueTrees(null);

                        return newDoc;
                    } catch (Exception) {
                        if (conn.HasTransaction()) {
                            conn.RollbackTransaction();
                        }
                        throw;
                    }
                } catch (Exception ex) {
                    //System.Threading.Thread.CurrentThread.Abort();
                    throw new Exception(ExceptionMessages.DocumentManagerAdd + ex.Message, ex);
                }
            }

            //return null;
        }

        private void CopyValueTrees(Document sourceDoc, int newDocId, IDatabase conn) {

            conn.BeginTransaction();
            try {

                var documentRelations = GetValueTreeRelations();

                HashSet<Dictionary<string, object>> oldValues = new HashSet<Dictionary<string, object>>();
                Dictionary<string, object> oldValueEntry = new Dictionary<string, object>();
                HashSet<string> blackList = new HashSet<string>() { "parent_id", DefaultIdColumnName, "document_id" };

                foreach (DbRelations relation in documentRelations) {

                    oldValues = new HashSet<Dictionary<string, object>>();

                    //var columnInfos = conn.GetColumnInfos(documentTable.TableName);

                    var columns = conn.GetColumnNames(relation.TableName);

                    var oldValueReader =
                        conn.ExecuteReader("SELECT " + string.Join(", ", columns) + " FROM " +
                                           relation.TableName + " WHERE " + conn.Enquote(DefaultDocIdColumn) +
                                           "=" + sourceDoc.Id);


                    while (oldValueReader.Read()) {
                        oldValueEntry = new Dictionary<string, object>();

                        foreach (var column in columns) {
                            oldValueEntry.Add(column, oldValueReader[column]);
                        }
                        oldValues.Add(oldValueEntry);
                    }
                    oldValueReader.Close();

                    foreach (Dictionary<string, object> values in oldValues) {
                        DbColumnValues updateValues = new DbColumnValues();
                        object elemId = null;
                        foreach (var column in columns) {
                            if (blackList.Contains(column)) {
                                continue;
                            }
                            var val = values[column];

                            if (column.Equals("elem_id")) {
                                elemId = val;
                                continue;
                            }

                            //if (column.Equals(relation.KeyColumn)) {
                            //    val = newDocId;
                            //}
                            updateValues[column] = val;

                        }

                        conn.Update(relation.TableName, updateValues, conn.Enquote("elem_id") + " = '" + elemId + "' AND " + conn.Enquote("document_id") + "= '" + newDocId + "'");
                    }
                }
                conn.CommitTransaction();
            } catch (Exception) {
                if (conn.HasTransaction()) {
                    conn.RollbackTransaction();
                }
            }
        }

        private void CopyValueTreeValues(Document oldDoc, Document newDoc) {
#if DEBUG
            Type t = typeof (ValueMappingBase);
            var memberArray = t.GetProperties();
            var counter = memberArray.Count(x => x.PropertyType == typeof (bool));
            Debug.Assert(counter == 3,
                                            "number of bool properties in ValueMappingBase has changed, please extend this function here as well (if required)");
#endif
            XbrlElementValueBase.DoDbUpdate = false;

            TransferValues(oldDoc.ValueTreeMain.Root.Values, newDoc.ValueTreeMain.Root.Values);

            foreach (var oldValuePair in oldDoc.ValueTreeGcd.Root.Values) {
                var newValue = newDoc.ValueTreeGcd.Root.Values[oldValuePair.Key];
                var oldValue = oldValuePair.Value;
                TransferIValueTreeEntry(oldValue, newValue);
            }

            XbrlElementValueBase.DoDbUpdate = true;
            newDoc.ValueTreeMain.Save();
            newDoc.ValueTreeGcd.Save();
        }

        public void TransferValues(Dictionary<string, IValueTreeEntry> oldD, Dictionary<string, IValueTreeEntry> newD) {
            foreach (var oldValuePair in oldD) {
                //System.Diagnostics.Debug.WriteLine(i);
                var newValue = newD[oldValuePair.Key];
                var oldValue = oldValuePair.Value;
                TransferIValueTreeEntry(oldValue, newValue);
                //i++;
                int i = 1;
                if (oldValue.Element.ValueType == XbrlElementValueTypes.Tuple) {
                    var oldTupel = (oldValue as XbrlElementValue_Tuple);
                    var newTupel = (newValue as XbrlElementValue_Tuple);
                    if (oldTupel != null && newTupel != null && oldTupel.Items.Any()) {
                        foreach (var tupelItem in oldTupel.Items) {
                            try {
                                var newTupelItem = newTupel.AddValue();
                                TransferValues(tupelItem.Values, newTupelItem.Values);

                                //if (1 == i++) {
                                //    throw new NotImplementedException("Ein unerwarteter Fehler ist aufgetreten.", new ExpiredSecurityTokenException());
                                //}

                                TransferIValueMappingEntry(tupelItem.DbValue, newTupelItem.DbValue);
                            }
                            catch (Exception e) {
                                ExceptionLogging.LogException(e);
                                continue;
                            }
                        }
                    }
                }

                if (oldValue.Element.ValueType == XbrlElementValueTypes.SingleChoice) {
                    var oldEntry = (oldValue as XbrlElementValue_SingleChoice);
                    var newEntry = (newValue as XbrlElementValue_SingleChoice);
                    if (oldEntry != null && newEntry != null) {
                        newEntry.SelectedValue = oldEntry.SelectedValue;
                        //newEntry.DbValue.
                    }
                }
            }
        }

        private bool TransferIValueTreeEntry(IValueTreeEntry oldValue, IValueTreeEntry newValue) {
            try {
                newValue.Value = oldValue.Value;
                newValue.SendAccountBalances = oldValue.SendAccountBalances;
                // use this methode to set the value without auto recursive call for children
                newValue.SetSendAccountBalancesRecursive(oldValue.SendAccountBalancesRecursive);
                //newValue.SendAccountBalancesRecursive = oldValue.SendAccountBalancesRecursive;
                newValue.SupressWarningMessages = oldValue.SupressWarningMessages;
                TransferIValueMappingEntry(oldValue.DbValue, newValue.DbValue);
                return true;
            }
            catch (Exception) {
                return false;
            }

        }

        private bool TransferIValueMappingEntry(IValueMapping oldValue, IValueMapping newValue) {
            try {
                if (oldValue == null ||newValue == null) {
                    return false;
                }
                newValue.Value = oldValue.Value;
                newValue.SendAccountBalances = oldValue.SendAccountBalances;
                // use this methode to set the value without auto recursive call for children
                newValue.ValueOther = oldValue.ValueOther;
                //newValue.SendAccountBalancesRecursive = oldValue.SendAccountBalancesRecursive;
                newValue.SupressWarningMessages = oldValue.SupressWarningMessages;
                newValue.Comment = oldValue.Comment;
                newValue.AutoComputationEnabled = oldValue.AutoComputationEnabled;
                return true;
            }
            catch (Exception) {
                return false;
            }

        }


        /// <summary>
        /// Create a copy of the report
        /// </summary>
        /// <param name="sourceDoc">The report you would like to copy.</param>
        /// <returns>The new document.</returns>
        public Document CopyDocument(Document sourceDoc) {

            Document newDoc = null;

            try {

                //if (!(sourceDoc.ReportRights.FullWriteRights && sourceDoc.ReportRights.ExportAllowed && RightManager.CompanyWriteAllowed(sourceDoc.Company))) {
                if (!(sourceDoc.ReportRights.FullWriteRights && sourceDoc.ReportRights.ExportAllowed)) {
                    throw new ActionNotAllowed();
                }

                newDoc = ExecCopyDocument(sourceDoc);
                if (newDoc == null) {
                    //System.Threading.Thread.CurrentThread.Abort();
                    throw new Exception("report copy process failed");
                }
                try {

                    AddDocumentPropertyChangedHandler(newDoc);
                    LockDocuments(() => _documents.Add(newDoc));
                    LockAllowedDocuments(() => _allowedDocuments.Add(newDoc));
                    AddUsedElement(newDoc);
                    LockDocumentById(() => _documentById.Add(newDoc.Id, newDoc));
                    RightManager.DocumentAdded(newDoc);

                    LogManager.Instance.CopyDocument(sourceDoc, newDoc);
                }
#pragma warning disable 0168
                catch (Exception ex) {
                    //TODO: Log this message and don't show it - the doc was created only the loading process failed
                    //throw;
                    ExceptionLogging.LogException(ex);
                }
#pragma warning restore 0168
            } catch (ExceptionBase ex) {
                MessageBox.Show(ex.Message, ex.Header);
                //throw;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
                //throw;
            }

            OnPropertyChanged("HasPreviousYearReports");

            return newDoc;
        }

        #endregion Copy Document

        #region DeleteDocument
        public void DeleteDocument(Document document) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    // log document deletion
                    LogManager.Instance.DeleteDocument(document);
                    
                    // manual deletion of value table entries due to performance reasons
                    conn.ExecuteNonQuery(
                        "DELETE FROM " +
                        conn.Enquote(conn.DbMapping.GetTableName(typeof (ValuesGAAP))) + " WHERE " +
                        conn.Enquote("document_id") + " = " + document.Id);

                    conn.ExecuteNonQuery(
                        "DELETE FROM " +
                        conn.Enquote(conn.DbMapping.GetTableName(typeof (ValuesGCD))) + " WHERE " +
                        conn.Enquote("document_id") + " = " + document.Id);

                    // manually delete hyper cubes entries
                    conn.ExecuteNonQuery(
                        "DELETE FROM " +
                        conn.Enquote(conn.DbMapping.GetTableName(typeof (DbEntityHyperCubeItem))) + " WHERE " +
                        conn.Enquote("document_id") + " = " + document.Id);

                    conn.ExecuteNonQuery(
                        "DELETE FROM " +
                        conn.Enquote(conn.DbMapping.GetTableName(typeof (DbEntityHyperCube))) + " WHERE " +
                        conn.Enquote("document_id") + " = " + document.Id);

                    document.ClearHyperCubes();

                    // manually delete balance lists
                    foreach (var balanceList in document.BalanceLists) {
                        //conn.ExecuteNonQuery("DELETE FROM accounts WHERE balance_list_id = " + balanceList.Id);
                        //TODO Could be faster (delete document_id)
                        //conn.ExecuteNonQuery("DELETE FROM balance_lists WHERE id = " + balanceList.Id);
                        BalanceListManager.RemoveBalanceListFromDb(balanceList);

                        balanceList.ClearEntries();
                        //document.BalanceLists.Remove(balanceList);
                    }
                    document.BalanceLists.Clear();
                    
                    // delete values for transfer from comercial to tax (Überleitungsrechnung Handelsbilanz -> Steuerbilanz)
                    conn.ExecuteNonQuery(
                        "DELETE FROM " + 
                        conn.Enquote(conn.DbMapping.GetTableName(typeof(DbEntityReconciliation))) + " WHERE " + 
                        conn.Enquote("document_id") + " = " + document.Id);
                    
                    conn.ExecuteNonQuery(
                        "DELETE FROM " + 
                        conn.Enquote(conn.DbMapping.GetTableName(typeof(DbEntityReconciliationTransaction))) + " WHERE " + 
                        conn.Enquote("document_id") + " = " + document.Id);
                    
                    // delete document
                    conn.DbMapping.Delete(document);

                    CurrentDocument = null;

                    GC.Collect();

                    CompanyManager.Instance.NotifyAssignedReportChanged();
                    SystemManager.Instance.NotifyAssignedReportChanged();
                } catch (Exception ex) {
                    throw new Exception(ExceptionMessages.DocumentManagerDelete + ex.Message);
                }
            }

            LockDocuments(() => _documents.Remove(document));
            LockAllowedDocuments(() => _allowedDocuments.Remove(document));
            DeleteUsedElement(document);
            LockDocumentById(() => _documentById.Remove(document.Id));
            RightManager.DocumentDeleted(document);

            document.Company.NotifyAssignedReportChanged();
            document.System.NotifyAssignedReportChanged();
            CompanyManager.Instance.NotifyAssignedReportChanged();
            SystemManager.Instance.NotifyAssignedReportChanged();

            OnPropertyChanged("HasPreviousYearReports");
            OnPropertyChanged("HasAllowedDocuments");
        }
        #endregion DeleteDocument

        #region SaveDocument
        public void SaveDocument(Document document) {
            // DEVNOTE: in case of lazy loading documents the app can be closed while document is initialized so need to check connection is still available
            if (AppConfig.ConnectionManager == null) return;
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    if (conn != null)
                        conn.DbMapping.Save(document);
                } catch (Exception ex) {
                    throw new Exception(ExceptionMessages.DocumentManagerUpdate + ex.Message);
                }
            }
        }
        #endregion SaveDocument

        #region Exists
        /// <summary>
        /// Returns true, if the specified document name exists.
        /// </summary>
        public bool Exists(
            Structures.DbMapping.System system,
            Company company,
            FinancialYear fyear,
            string documentName) {
            return
                Documents.Any(
                    document =>
                    document.System.Id == system.Id && document.Company.Id == company.Id &&
                    document.FinancialYear.Id == fyear.Id && document.Name.ToLower().Equals(documentName.ToLower()));
        }
        #endregion Exists

        #region AddUsedElement
        internal void AddUsedElement(Document document) {

            //if (document.System == null || document.Company == null || document.FinancialYear == null) return;

            // add system
            if (!_usedElementsTree.Contains(document.System)) {
                _usedElementsTree.Add(document.System);
            }

            // add company
            TreeNode tns = _usedElementsTree.GetChield(document.System);
            if (!tns.Contains(document.Company)) tns.Add(document.Company);

            // add financial year
            TreeNode tnc = tns.GetChield(document.Company);

            if (document.FinancialYear == null) {
                document.FinancialYear = document.Company.VisibleFinancialYears[0];
                //throw new Exception(
                //    "Datenbankfehler: Es wird auf kein oder auf ein nicht existierendes Geschäftsjahr verwiesen.");
            }
            if (!tnc.Contains(document.FinancialYear)) tnc.Add(document.FinancialYear);

            // add document
            TreeNode tnf = tnc.GetChield(document.FinancialYear);
            tnf.Add(document);
        }
        #endregion AddUsedElement

        #region DeleteUsedElement
        internal void DeleteUsedElement(Document document) {

            //if (document.System == null || document.Company == null || document.FinancialYear == null) return;

            TreeNode tns = _usedElementsTree.GetChield(document.System);
            TreeNode tnc = tns.GetChield(document.Company);
            TreeNode tnf = tnc.GetChield(document.FinancialYear);

            // remove document from financial year node
            tnf.Remove(document);

            // remove financial year node, if it is not longer used
            if (tnf.Items.Count == 0) tnc.Remove(document.FinancialYear);

            // remove company node, if it is not longer used
            if (tnc.Items.Count == 0) tns.Remove(document.Company);

            // remove system node, if it is not longer used
            if (tns.Items.Count == 0) _usedElementsTree.Remove(document.System);
        }
        #endregion DeleteUsedElement

        public bool NewDocumentAllowed {
            get {

            return SystemManager.Instance.Systems.Count > 0 && CompanyManager.Instance.AllowedCompanies.Count > 0;

            //use if NewDocumentAllowed should be true if the user has write rights for the company
            //return SystemManager.Systems.Count > 0 && RightManager.CompanyWriteAllowed(CompanyManager.CurrentCompany);
            
            //use to check if the user has rights for the CurrentFinancialYear
            //var z = RoleManager.GetUserRole(UserManager.Instance.CurrentUser).Rights.FirstOrDefault(yz => yz.DbRight.ReferenceId == CompanyManager.CurrentFinancialYear.Id);
            //var z2 RightManager.RightDeducer.GetFinancialYearRight(CompanyManager.CurrentFinancialYear.Id).IsWriteAllowed;
            }
        }
        
        #region CurrentDocument
        private Document _currentDocument;

        public Document CurrentDocument {
            get { return _currentDocument; }
            set {
                if (_currentDocument != value) {
                    // Set the IsSelected property
                    if (_currentDocument != null) {
                        _currentDocument.IsSelected = false;
                    }

                    _currentDocument = value;

                    // Set the IsSelected property
                    if (_currentDocument != null) {
                        _currentDocument.IsSelected = true;
                    }

                    OnPropertyChanged("CurrentDocument");
                }
            }
        }

        #endregion CurrentDocument

        #region GetDocument
        public Document GetDocument(int id) {
            _rwLockDocumentById.TryEnterReadLock(Timeout.Infinite);
            try {
                return _documentById.ContainsKey(id) ? _documentById[id] : null;
            }
            finally {
                _rwLockDocumentById.ExitReadLock();
            }
        }
        #endregion GetDocument

        #region InitAllowedDocuments
        internal void InitAllowedDocuments(RightDeducer rightDeducer) {
            _usedElementsTree = new TreeNode();
            _allowedDocuments.Clear();
            foreach (Document document in _documents) {
                if (!rightDeducer.GetRight(document).IsReadAllowed) continue;
                AddUsedElement(document);
                _allowedDocuments.Add(document);
            }
        }
        #endregion

        #region UpgradeTaxonomy
        public DocumentUpgradeResults UpgradeTaxonomy(Document document, ProgressInfo progressInfo) {
            var result = new DocumentUpgradeResults {
                DocumentUpgradeResultGcd =
                    TaxonomyManager.UpgradeDocumentTaxonomyVersion(document.GcdTaxonomyInfo, document.ValueTreeGcd),
                DocumentUpgradeResultGaap =
                    TaxonomyManager.UpgradeDocumentTaxonomyVersion(document.MainTaxonomyInfo, document.ValueTreeMain)
            };
            
            document.LoadValueTrees(progressInfo);

            document.ValueTreeMain.Root.ForceRecomputation();
            XbrlElementValueBase.InitStates(document.ValueTreeMain.Root);

            return result;
        }

        private static void SetCoreTaxonomyIfNotSet(Document document) {
            //Set branch taxonomy to core taxonomy if it is empty
            IValueTreeEntry accountingStandardGeneral;
            if (document.ValueTreeGcd.Root.Values.TryGetValue("de-gcd_genInfo.report.id.specialAccountingStandard",
                                                              out accountingStandardGeneral)) {
                XbrlElementValue_SingleChoice accountingStandard = (XbrlElementValue_SingleChoice) accountingStandardGeneral;
                if (accountingStandard.SelectedValue == null) {
                    var coreTaxonomy =
                        accountingStandard.Elements.FirstOrDefault(
                            (e) => e.Id == "de-gcd_genInfo.report.id.specialAccountingStandard.K");
                    if (coreTaxonomy != null) {
                        var needsEnabling = !XbrlElementValueBase.DoDbUpdate;
                        try {
                            if (needsEnabling)
                                AppConfig.EnableDbUpdates();
                            accountingStandard.SelectedValue = coreTaxonomy;
                        } finally {
                            if (needsEnabling)
                                AppConfig.DisableDbUpdates();
                        }
                    }
                }
            }
        }
        #endregion
        
        public void ProcessUpdateChanges(DocumentUpgradeResults documentUpgradeResults, Document document) {

            // save updated values
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    try {
                        foreach (var updatedValue in documentUpgradeResults.DocumentUpgradeResultGcd.UpdatedValues) {
                            conn.DbMapping.Save(updatedValue);
                        }

                        foreach (var updatedValue in documentUpgradeResults.DocumentUpgradeResultGaap.UpdatedValues) {
                            conn.DbMapping.Save(updatedValue);
                        }
                        conn.CommitTransaction();
                    } catch
                        (Exception ex) {
                        conn.RollbackTransaction();
                        throw new Exception(ex.Message, ex);
                    }
                } catch (Exception ex) {
                    throw new Exception(ExceptionMessages.CompanyManagerUpdate + ex.Message);
                }
            }

            ValueManager.RemoveValues(documentUpgradeResults.DocumentUpgradeResultGcd.MissingValues);
            ValueManager.RemoveValues(documentUpgradeResults.DocumentUpgradeResultGaap.MissingValues);
            
            document.GcdTaxonomyInfo = TaxonomyManager.GetLatestTaxonomyInfo(document.GcdTaxonomyInfo.Type);
            document.MainTaxonomyInfo = TaxonomyManager.GetLatestTaxonomyInfo(document.MainTaxonomyInfo.Type);
            SetCoreTaxonomyIfNotSet(document);

            SaveDocument(document);
        }

        #endregion methods

        #region previous year reports
        /// <summary>
        /// Returns an enumeration of all reports which has been assigned for the previous financial year depending on CurrentDocument.
        /// </summary>
        public IEnumerable<Document> GetPreviousYearReports() { return GetPreviousYearReports(CurrentDocument); }

        /// <summary>
        /// Returns an enumeration of all reports which has been assigned for the previous financial year depending on the specified document.
        /// </summary>
        public IEnumerable<Document> GetPreviousYearReports(Document document) {
            List<Document> result = new List<Document>();
            if (CurrentDocument != null) {
                int refyear = document.FinancialYear.FYear - 1;
                result.AddRange(
                    AllowedDocuments.Where(d =>
                                           d.Company == document.Company &&
                                           d.FinancialYear.FYear == refyear &&
                                           d.MainTaxonomyInfo.Name == document.MainTaxonomyInfo.Name));
            }
            return result;
        }

        public bool HasPreviousYearReports { get { return GetPreviousYearReports().Any(); } }
        #endregion // previous year reports

        #region following year reports

        /// <summary>
        /// Returns an enumeration of all reports which has been assigned for the following financial year depending on CurrentDocument.
        /// </summary>
        public IEnumerable<Document> GetFollowingYearReports() { return GetFollowingYearReports(CurrentDocument); }

        /// <summary>
        /// Returns an enumeration of all reports which has been assigned for the following financial year depending on the specified document.
        /// </summary>
        public IEnumerable<Document> GetFollowingYearReports(Document document) {
            List<Document> result = new List<Document>();
            if (CurrentDocument != null) {
                int refyear = document.FinancialYear.FYear + 1;
                result.AddRange(
                    AllowedDocuments.Where(d =>
                                           d.Company == document.Company &&
                                           d.FinancialYear.FYear == refyear &&
                                           d.MainTaxonomyInfo.Name == document.MainTaxonomyInfo.Name));
            }
            return result;
        }

        public bool HasFollowingYearReports { get { return GetFollowingYearReports().Any(); } }

        #endregion // following year reports

        #region [ Lock helper methods ]

        private void LockDocuments(Action action) {
            _rwLockDocument.TryEnterWriteLock(Timeout.Infinite);
            action();
            _rwLockDocument.ExitWriteLock();
        }

        private void LockDocumentById(Action action) {
            _rwLockDocumentById.TryEnterWriteLock(Timeout.Infinite);
            action();
            _rwLockDocumentById.ExitWriteLock();
        }

        private void LockAllowedDocuments(Action action) {
            _rwLockAllowed.TryEnterWriteLock(Timeout.Infinite);
            action();
            _rwLockAllowed.ExitWriteLock();
        }

        #endregion [ Lock helper methods ]
    }
}