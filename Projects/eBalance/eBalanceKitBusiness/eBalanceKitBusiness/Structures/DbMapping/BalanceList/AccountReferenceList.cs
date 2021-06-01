// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-05-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Structures.DbMapping.BalanceList {
    public class AccountReferenceList : IAccountReferenceList {
        
        internal DbEntityAccountReferenceList DbEntity { get; set; }

        /// <summary>
        /// Public constructor for ReferenceList
        /// </summary>
        /// <param name="document">Assigned document.</param>
        /// <param name="user">Assigned user.</param>
        public AccountReferenceList(Document document, User user) {
            if (document == null)
                throw new ArgumentException(ResourcesReconciliation.ParameterMissing, "document");
            if (user == null)
                throw new ArgumentException(ResourcesReconciliation.ParameterMissing, "user");
            
            DbEntity = new DbEntityAccountReferenceList(document, user);
            Save();

            Document = document;
            User = user;
            this._items = new Dictionary<long, IAccountReferenceListItem>();
        }

        /// <summary>
        /// Constructor for existing reference list.
        /// </summary>
        /// <param name="dbEntityAccountReferenceList">Assigned reference list.</param>
        internal AccountReferenceList(DbEntityAccountReferenceList dbEntityAccountReferenceList) {
            Document = dbEntityAccountReferenceList.Document;
            User = dbEntityAccountReferenceList.User;
            this._items = new Dictionary<long, IAccountReferenceListItem>();
            foreach (DbEntityAccountReferenceListItem referenceListItem in dbEntityAccountReferenceList.Items) {
                this._items.Add(AccountReferenceListItem.GetHash(referenceListItem.AccountType, referenceListItem.AccountId), new AccountReferenceListItem(referenceListItem));
            }
            DbEntity = dbEntityAccountReferenceList;
        }

        #region [ IReferenceList ]

        /// <inheritdoc />
        public Document Document { get; private set; }
        /// <inheritdoc />
        public User User { get; private set; }

        private Dictionary<long, IAccountReferenceListItem> _items;
        /// <inheritdoc />
        public IEnumerable<IAccountReferenceListItem> Items {
            get { return this._items.Values; }
        }

        /// <inheritdoc />
        public void AddItemToReferenceList(IAccountReferenceListItem item) {
            if (!IsAccountContainedInReferenceList(item)) {
                this._items[item.Hash] = item;
                DbEntityAccountReferenceListItem dbItem = new DbEntityAccountReferenceListItem(DbEntity, item.AccountType, item.AccountId); 
                DbEntity.Items.Add(dbItem);
                SaveItem(dbItem);
            }
        }

        /// <inheritdoc />
        public void RemoveItemFromReferenceList(IAccountReferenceListItem item) {
            if (IsAccountContainedInReferenceList(item)) {
                this._items.Remove(item.Hash);
                DbEntityAccountReferenceListItem dbItem = DbEntity.Items.FirstOrDefault(e => e.AccountId == item.AccountId && e.AccountType == item.AccountType && e.DbEntityAccountReferenceList.Id == DbEntity.Id);
                if (dbItem != null) {
                    DbEntity.Items.Remove(dbItem);
                    DeleteItem(dbItem);
                }
            }
        }

        /// <inheritdoc />
        public bool IsAccountContainedInReferenceList(AccountTypeEnum accountType, long accounttId) { return this._items.ContainsKey(AccountReferenceListItem.GetHash(accountType, accounttId)); }
        /// <inheritdoc />
        public bool IsAccountContainedInReferenceList(IAccountReferenceListItem item) { return this._items.ContainsKey(AccountReferenceListItem.GetHash(item)); }

        #endregion [ IReferenceList ]

        #region [ Database related methods ]
        internal void Save() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Save(DbEntity);
                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.SaveException + ex.Message, ex);
                }
            }
        }
        internal void SaveItem(DbEntityAccountReferenceListItem entity) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Save(entity);
                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.SaveException + ex.Message, ex);
                }
            }
        }
        internal void DeleteItem(DbEntityAccountReferenceListItem entity) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Delete(entity);
                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.SaveException + ex.Message, ex);
                }
            }
        }
        #endregion [ Database related methods ]
    }
}
