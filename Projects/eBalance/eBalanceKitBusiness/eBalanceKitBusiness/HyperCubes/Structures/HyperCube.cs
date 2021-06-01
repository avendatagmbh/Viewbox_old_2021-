// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Taxonomy.Interfaces.PresentationTree;
using Utils;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.HyperCubes.DbMapping;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Interfaces.ViewModels;
using eBalanceKitBusiness.HyperCubes.ViewModels;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.HyperCubes.Structures {
    internal class HyperCube : IHyperCube {
        #region constructor
        internal HyperCube(Document document, IPresentationTreeNode root) {
            Document = document;
            Root = root;
            Dimensions = new HyperCubeDimensionSet(this);

            InitHyperCubeProperty();
        }
        #endregion constructor
        
        #region properties

        #region internal
        internal int CubeElementId { get { return TaxonomyIdManager.GetId(Root.Element.Id); } }
        internal TaxonomyIdManager TaxonomyIdManager { get { return Document.TaxonomyIdManager; } }
        internal DbEntityHyperCube DbEntityHyperCube { get; set; }
        #endregion internal

        #region public

        #region Comment
        public string Comment {
            set {
                if (!Document.ReportRights.WriteAllowed)
                    throw new AccessDeniedException(Localisation.ExceptionMessages.InsufficentWriteRights);
                
                if (DbEntityHyperCube.Comment == value) return;
                DbEntityHyperCube.Comment = string.IsNullOrEmpty(value) ? null : StringUtils.Left(value, 4096);

                Save();
            }
            
            get {
                if (!Document.ReportRights.ReadAllowed)
                    throw new AccessDeniedException(Localisation.ExceptionMessages.InsufficentReadRights);
                
                return DbEntityHyperCube.Comment;
            }
        }
        #endregion

        public int TaxonomyId { get { return ((TaxonomyInfo) Document.MainTaxonomyInfo).Id; } }
        public Document Document { get; private set; }
        public IPresentationTreeNode Root { get; private set; }
        public IHyperCubeDimensionSet Dimensions { get; private set; }

        #region Items
        private IHyperCubeItemCollection _items;
        public IHyperCubeItemCollection Items { get { return _items ?? (_items = new HyperCubeItemCollection(this)); } }
        #endregion Items

        #endregion public

        #endregion properties

        #region methods

        #region private

        public static bool ExistsCube(Document document, IPresentationTreeNode root) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                // load hypercube object from database or create a new one
                var existingHyperCubes = conn.DbMapping.Load<DbEntityHyperCube>(
                    conn.Enquote("document_id") + " = " + document.Id + " AND " +
                    conn.Enquote("taxonomy_id") + " = " + ((TaxonomyInfo)document.MainTaxonomyInfo).Id + " AND " + 
                    conn.Enquote("cube_element_id") + " = " + document.TaxonomyIdManager.GetId(root.Element.Id));

                return existingHyperCubes.Any();
            }
        }

        #region InitHyperCubeProperty
        private void InitHyperCubeProperty() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                // load hypercube object from database or create a new one
                var existingHyperCubes = conn.DbMapping.Load<DbEntityHyperCube>(
                    conn.Enquote("document_id") + " = " + Document.Id + " AND " +
                    conn.Enquote("taxonomy_id") + "=" + TaxonomyId + " AND " +
                    conn.Enquote("cube_element_id") + " = " + Document.TaxonomyIdManager.GetId(Root.Element.Id));

                switch (existingHyperCubes.Count) {
                    case 0:
                        DbEntityHyperCube = new DbEntityHyperCube {
                            TaxonomytId = TaxonomyId,
                            Document = Document,
                            CubeElementId = Document.TaxonomyIdManager.GetId(Root.Element.Id)
                        };

                        conn.DbMapping.Save(DbEntityHyperCube);
                        break;

                    case 1:
                        DbEntityHyperCube = existingHyperCubes[0];
                        DbEntityHyperCube.Document = Document;
                        DbEntityHyperCube.CubeElementId = Document.TaxonomyIdManager.GetId(Root.Element.Id);
                        break;

                    default:
                        throw new Exception("Database error: found multiple hyper cubes with the same id.");
                }
            }
        }
        #endregion InitHyperCubeProperty

        #endregion private

        #region public

        public void Save() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Save(DbEntityHyperCube);
                } catch (Exception ex) {
                    throw new Exception(ex.Message, ex);
                }
            }
        }

        #region Delete
        /// <summary>
        /// Deletes the persistant representation of this entity.
        /// </summary>
        public void Delete() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.ExecuteNonQuery(
                    "DELETE FROM " +
                    conn.Enquote(conn.DbMapping.GetTableName(typeof (DbEntityHyperCubeItem))) + " WHERE " +
                    conn.Enquote("hypercube_id") + " = " + DbEntityHyperCube.Id);

                conn.ExecuteNonQuery(
                    "DELETE FROM " +
                    conn.Enquote(conn.DbMapping.GetTableName(typeof (DbEntityHyperCube))) + " WHERE " +
                    conn.Enquote("id") + " = " + DbEntityHyperCube.Id);
            }
        }
        #endregion Delete

        #region Clear
        /// <summary>
        /// Sets all Items.Items.Value to null.
        /// </summary>
        public void Clear()
        {
            foreach (IHyperCubeItem item in Items.Items)
            {
                item.Value = null;
            }
        } 
        #endregion

        #region GetDimensionkey
        public long GetDimensionkey(long row, long column) {
            // Use the manager to get the DimensionKey
            var manager = new Manager.HyperCubeDimensionKeyManager(this);
            return manager.GetDimensionKey(new[] {row, column});
        }
        #endregion

        #region GetTable
        public IHyperCubeTable GetTable(
            IHyperCubeDimension dim1,
            IHyperCubeDimension dim2,
            IEnumerable<IHyperCubeDimensionValue> fixedDimensionCoordinates = null) { return new HyperCubeTable(this, dim1, dim2, fixedDimensionCoordinates); }
        #endregion GetTable

        #region Get3DCube
        public IHyperCube3DCube Get3DCube(
            IHyperCubeDimension dim1,
            IHyperCubeDimension dim2,
            IHyperCubeDimension dim3) { return new HyperCube3DCube(this, dim1, dim2, dim3); }
        #endregion GetTable

        #region GetScenarioContexts
        public IEnumerable<IScenarioContext> GetScenarioContexts() {
            var contexts = new HashSet<IScenarioContext>();
            foreach (IHyperCubeItem item in Items.Items) {
                if (item.HasValue && !contexts.Contains(item.Context))
                    contexts.Add(item.Context);
            }

            return contexts;
        }
        #endregion GetScenarioContexts

        #region PreloadItems
        public void PreloadItems() { var items = Items; }
        #endregion

        #endregion public

        #endregion methods

    }
}