// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-04-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;
using System.Linq;

namespace eBalanceKitBusiness.Reconciliation {
    public class ReferenceList : IReferenceList {

        internal DbEntityReferenceList DbEntity { get; set; }

        /// <summary>
        /// Public constructor for ReferenceList
        /// </summary>
        /// <param name="document">Assigned document.</param>
        /// <param name="user">Assigned user.</param>
        public ReferenceList(Document document, User user) {
            if (document == null)
                throw new ArgumentException(ResourcesReconciliation.ParameterMissing, "document");
            if (user == null)
                throw new ArgumentException(ResourcesReconciliation.ParameterMissing, "user");
            
            DbEntity = new DbEntityReferenceList(document, user);
            Save();

            Document = document;
            User = user;
            this._items = new Dictionary<int, IReferenceListItem>();
        }

        /// <summary>
        /// Constructor for existing reference list.
        /// </summary>
        /// <param name="dbEntityReferenceList">Assigned reference list.</param>
        internal ReferenceList(DbEntityReferenceList dbEntityReferenceList) {
            Document = dbEntityReferenceList.Document;
            User = dbEntityReferenceList.User;
            this._items = new Dictionary<int, IReferenceListItem>();
            foreach (DbEntityReferenceListItem referenceListItem in dbEntityReferenceList.Items) {
                this._items.Add(referenceListItem.ElementId, new ReferenceListItem(referenceListItem));
            }
            DbEntity = dbEntityReferenceList;
        }

        #region [ IReferenceList ]

        /// <inheritdoc />
        public Document Document { get; private set; }
        /// <inheritdoc />
        public User User { get; private set; }

        private Dictionary<int, IReferenceListItem> _items;
        /// <inheritdoc />
        public IEnumerable<IReferenceListItem> Items {
            get { return this._items.Values; }
        }

        /// <inheritdoc />
        public void AddItemToReferenceList(IReferenceListItem item) {
            if (!IsElementContainedInReferenceList(item.ElementId)) {
                this._items[item.ElementId] = item;
                DbEntityReferenceListItem dbItem = new DbEntityReferenceListItem(DbEntity, item.ElementId); 
                DbEntity.Items.Add(dbItem);
                SaveItem(dbItem);
            }

        }
        /// <inheritdoc />
        public void RemoveItemFromReferenceList(IReferenceListItem item) {
            if (IsElementContainedInReferenceList(item.ElementId)) {
                this._items.Remove(item.ElementId);
                DbEntityReferenceListItem dbItem = DbEntity.Items.FirstOrDefault(e => e.ElementId == item.ElementId && e.DbEntityReferenceList.Id == DbEntity.Id);
                if (dbItem != null) {
                    DbEntity.Items.Remove(dbItem);
                    DeleteItem(dbItem);
                }
            }
        }
        /// <inheritdoc />
        public bool IsElementContainedInReferenceList(int elementId) { return this._items.ContainsKey(elementId); }

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
        internal void SaveItem(DbEntityReferenceListItem entity) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Save(entity);
                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.SaveException + ex.Message, ex);
                }
            }
        }
        internal void DeleteItem(DbEntityReferenceListItem entity) {
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
