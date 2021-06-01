// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-01-24
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Linq;
using DbAccess;
using Taxonomy.Enums;
using eBalanceKitBusiness.FederalGazette;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using System.Collections.Generic;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Manager;

namespace eBalanceKitBusiness.Structures.ValueTree {
    
    /// <summary>
    /// This class represents a value tree, which represents all facts of an instance of a specific taxonomy.
    /// </summary>
    public class ValueTree {

        private enum ValueMappingClasses { Document, Company, FederalGazette };

        private ValueTree(Document document, Taxonomy.ITaxonomy taxonomy) {
            GC.Collect(2);
            Document = document;
            Taxonomy = taxonomy;
        }

        private ValueTree(Company company, Taxonomy.ITaxonomy taxonomy) {
            GC.Collect(2);
            Company = company;
            Taxonomy = taxonomy;
        }

        //added for the federal gazette
        private ValueTree(ReportFederalGazette reportFederalGazette, Taxonomy.ITaxonomy taxonomy) {
            GC.Collect(2);
            ReportFederalGazette = reportFederalGazette;
            Taxonomy = taxonomy;
        }

        
        #region properties
        
        public Document Document { get; set; }
        public Company Company { get; set; }
        public Taxonomy.ITaxonomy Taxonomy { get; set; }
        public ValueTreeNode Root { get; set; }

        //federal gazette report
        public ReportFederalGazette ReportFederalGazette { get; set; }


        #endregion properties

        //federal gazette tree
        public static ValueTree CreateValueTree(Type valueMappingType, ReportFederalGazette reportFederalGazette, Taxonomy.ITaxonomy taxonomy, ProgressInfo progress) {
            try {
                var vtree = new ValueTree(reportFederalGazette, taxonomy);
                var dbMappingRoot = vtree.LoadValueMappingTree(valueMappingType);
                vtree.Root = new ValueTreeNode(vtree, dbMappingRoot, taxonomy);
                vtree.Root.Init(reportFederalGazette, valueMappingType);
                using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                    try {
                        conn.BeginTransaction();
                        vtree.SaveNewEntries(conn, vtree.Root, valueMappingType);
                        conn.CommitTransaction();
                    } catch (Exception ex) {
                        conn.RollbackTransaction();
                        throw new Exception(ex.Message, ex);
                    }
                }
                return vtree;
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Creates the value tree for the specified document.
        /// 
        /// Individual values could be mapped to another value mapping class using the TableType property of the Element class
        /// </summary>
        /// <param name="valueMappingType">Base mapping class for the taxonomy values.</param>
        /// <param name="document"></param>
        /// <param name="taxonomy"></param>
        /// <returns></returns>
        public static ValueTree CreateValueTree(Type valueMappingType, Document document, Taxonomy.ITaxonomy taxonomy, ProgressInfo progress) {
            try {

                ValueTree vtree = new ValueTree(document, taxonomy);
                IValueMapping dbMappingRoot = vtree.LoadValueMappingTree(valueMappingType);

                vtree.Root = new ValueTreeNode(vtree, dbMappingRoot, taxonomy);
                vtree.Root.Init(document, valueMappingType);

                using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                    try {
                        conn.BeginTransaction();
                        vtree.SaveNewEntries(conn, vtree.Root, valueMappingType);
                        conn.CommitTransaction();
                    } catch (Exception ex) {
                        conn.RollbackTransaction();
                        throw new Exception(ex.Message, ex);
                    }
                }

                return vtree;

            } catch (Exception ex) {
                throw new Exception(ex.Message, ex);
            }
        }

        public static ValueTree CreateValueTree(Type valueMappingType, Company company, Taxonomy.ITaxonomy taxonomy, ProgressInfo progress) {
            try {

                if (company == null) throw new Exception("CreateValueTree: Company value is null");
                if (taxonomy == null) throw new Exception("CreateValueTree: Taxonomy value is null");

                ValueTree vtree = new ValueTree(company, taxonomy);
                IValueMapping dbMappingRoot = vtree.LoadValueMappingTree(valueMappingType);

                vtree.Root = new ValueTreeNode(vtree, dbMappingRoot, taxonomy);
                vtree.Root.Init(company, valueMappingType);
 
                using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                    try {
                        conn.BeginTransaction();
                        vtree.SaveNewEntries(conn, vtree.Root, valueMappingType);
                        conn.CommitTransaction();
                    } catch (Exception ex) {
                        conn.RollbackTransaction();
                        throw new Exception(ex.Message, ex);
                    }
                }

                return vtree;

            } catch (Exception ex) {
                throw new Exception(ex.Message, ex);
            }
        }

        internal IValueMapping LoadValueMappingTree(Type valueMappingType) {
            
            ValueMappingClasses valueMappingClass;

            // get value mapping class (document or company value mapping)
            if (valueMappingType.GetInterface("IValueMappingDocument") != null) valueMappingClass = ValueMappingClasses.Document;
            else if (valueMappingType.GetInterface("IValueMappingCompany") != null) valueMappingClass = ValueMappingClasses.Company;
            else throw new Exception("Value mapping type must be derived from interface IValueMappingDocument or IValueMappingCompany.");

            IValueMapping dbMappingRoot;
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {

                if (Document == null) {
                    dbMappingRoot = ValueMappingBaseCompany.Load(conn, valueMappingType, Company, Taxonomy);
                
                } else {
                    switch (valueMappingClass) {
                        case ValueMappingClasses.Document:
                            dbMappingRoot = ValueMappingBaseDocument.Load(conn, valueMappingType, Document, Taxonomy);
                            break;

                        case ValueMappingClasses.Company:
                            dbMappingRoot = ValueMappingBaseCompany.Load(conn, valueMappingType, Document.Company, Taxonomy);
                            break;

                        default:
                            // should not happen
                            throw new NotImplementedException();
                    }
                }
            }
            return dbMappingRoot;
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save() {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    Save(conn, this.Root);
                    conn.CommitTransaction();
                } catch {
                    conn.RollbackTransaction();
                    throw;
                }
            }
        }

        private void Save(IDatabase conn, ValueTreeNode root) {
            List<XbrlElementValue_Tuple> tuples = new List<XbrlElementValue_Tuple>();

            conn.DbMapping.Save(root.DbValue);
            foreach (var value in root.Values.Values) {
                conn.DbMapping.Save(value.DbValue);
                if (value is XbrlElementValue_List) {
                    var list = value as XbrlElementValue_List;
                    foreach (var item in list.Items) Save(conn, item);
                }
            }
        }

        private void SaveNewEntries(IDatabase conn, ValueTreeNode root, Type valueMappingType) {
            List<XbrlElementValue_Tuple> tuples = new List<XbrlElementValue_Tuple>();

            List<IValueMapping> unsavedDbValues = new List<IValueMapping>();

            Action<ValueTreeNode> collectNewEntries = null;
            collectNewEntries = curRoot => {
                if (curRoot.DbValue != null && (curRoot.DbValue as ValueMappingBase).Id == 0) {
                    if (curRoot.DbValue.GetType() == valueMappingType) {
                        unsavedDbValues.Add(curRoot.DbValue);
                    } else {
                    }
                }

                foreach (var value in curRoot.Values.Values) {

                    if (value.DbValue != null && (value.DbValue as ValueMappingBase).Id == 0) {
                        if (value.DbValue.GetType() == valueMappingType) {
                            unsavedDbValues.Add(value.DbValue);
                        } else {
                        }
                    }

                    if (value is XbrlElementValue_Tuple) {
                        var list = value as XbrlElementValue_Tuple;
                        foreach (var item in list.Items) collectNewEntries(item);
                    }
                }
            };
            
            collectNewEntries(root);
            try {
                conn.DbMapping.Save(valueMappingType, unsavedDbValues);
            } catch (Exception) {

            }
        }

        public void SetSendAccountBalancesFlag(bool state, IPresentationTree ptree) {
            if (Document == null) throw new Exception("SetSendAccountBalancesFlag: Invalid function call (document is null)");
            if (!RightManager.WriteRestDocumentAllowed(Document)) return;

            AppConfig.DisableDbUpdates();
            List<IValueMapping> changedValues = new List<IValueMapping>();
            foreach (var node in ptree.RootEntries) {
                if ((node as IPresentationTreeNode).Value != null) {
                    var value = (node as IPresentationTreeNode).Value;
                    value.SendAccountBalances = state;
                    changedValues.Add(value.DbValue);
                }
                SetSendAccountBalancesFlag(state, node, changedValues);
            }
            AppConfig.EnableDbUpdates();

            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    conn.DbMapping.Save(typeof(ValuesGAAP), changedValues);
                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ex.Message, ex);
                }
            }
        }

        private void SetSendAccountBalancesFlag(bool state, Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root, List<IValueMapping> changedValues) {
            foreach (var node in root.Children) {
                if (node is Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode)
                    if ((node as IPresentationTreeNode).Value != null) {
                        var value = (node as IPresentationTreeNode).Value;
                        value.SendAccountBalances = state;
                        changedValues.Add(value.DbValue);
                    }

                if (node is Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode)
                    SetSendAccountBalancesFlag(state, node as Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode, changedValues);
            }
        }

        public void SetSendAccountBalancesFlagIfWished(IPresentationTree ptree) {
            if (Document == null) throw new Exception("SetSendAccountBalancesFlagIfWished: Invalid function call (document is null)");
            if (!RightManager.WriteRestDocumentAllowed(Document)) return;

            // reset all flags
            SetSendAccountBalancesFlag(false, ptree);

            AppConfig.DisableDbUpdates();
            List<IValueMapping> changedValues = new List<IValueMapping>();
            foreach (var node in ptree.RootEntries) {
                if ((node as IPresentationTreeNode).Value != null) {
                    var value = (node as IPresentationTreeNode).Value;
                    if (value.Element.MandatoryType == MandatoryType.AccountBalance) {
                        changedValues.Add(value.DbValue);
                        value.SendAccountBalances = true;
                        SetSendAccountBalancesFlag(true, ptree);
                    } else {
                        SetSendAccountBalancesFlagIfWished(node, changedValues);
                    }
                } else {
                    SetSendAccountBalancesFlagIfWished(node, changedValues);
                }
            }
            AppConfig.EnableDbUpdates();

            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    conn.DbMapping.Save(typeof(ValuesGAAP), changedValues);
                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ex.Message, ex);
                }
            }
        }

        private void SetSendAccountBalancesFlagIfWished(Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode root, List<IValueMapping> changedValues) {
            foreach (var node in root.Children) {
                if (!(node is IPresentationTreeNode)) continue;
                if ((node as IPresentationTreeNode).Value != null) {
                    var value = (node as IPresentationTreeNode).Value;
                    if (value.Element.MandatoryType == MandatoryType.AccountBalance) {
                        changedValues.Add(value.DbValue);
                        value.SendAccountBalances = true;
                        
                        SetSendAccountBalancesFlag(true, node as IPresentationTreeNode, changedValues);
                    } else {
                        SetSendAccountBalancesFlagIfWished(node as IPresentationTreeNode, changedValues);
                    }
                } else {
                    SetSendAccountBalancesFlagIfWished(node as IPresentationTreeNode, changedValues);
                }
            }
        }

        public void SetSendAccountBalancesFlag(bool state, string startsWith) {
            if (Document == null) throw new Exception("SetSendAccountBalancesFlag: Invalid function call (document is null)");
            if (!RightManager.WriteRestDocumentAllowed(Document)) return;
            // update flag without database updates
            AppConfig.DisableDbUpdates();
            foreach (var value in Root.Values.Values) if (value.Element.Id.StartsWith(startsWith)) value.SendAccountBalances = state;
            AppConfig.EnableDbUpdates();

            // save changes to database
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                string sql =
                    "UPDATE " +
                    conn.DbMapping.GetTableName(typeof(ValuesGAAP)) +
                    " SET " + conn.Enquote("send_account_balances") + " = " + (state ? 1 : 0) +
                    " WHERE " + conn.Enquote("document_id") + " = " + Document.Id +
                    " AND " + conn.FunctionSubstring(conn.Enquote("xbrl_elem_id"), 1, startsWith.Length) + " = " + conn.GetSqlString(startsWith);

                conn.ExecuteNonQuery(sql);
            }
        }

        public void SetSendAccountBalancesFlagIfWished(string startsWith) {
            if (Document == null) throw new Exception("SetSendAccountBalancesFlag: Invalid function call (document is null)");
            if (!RightManager.WriteRestDocumentAllowed(Document)) return;

            SetSendAccountBalancesFlag(false, startsWith);
            List<long> updateIds = new List<long>();
            foreach (var value in Root.Values.Values) {
                if (value.Element.MandatoryType == MandatoryType.AccountBalance && value.Element.Id.StartsWith(startsWith)) {
                    ((XbrlElementValueBase)value).SendAccountBalancesRecursiveGetIds(updateIds, true);
                } 
            }

            SetSendAccountBalancesFromList(updateIds, true);
        }

        public static void SetSendAccountBalancesFromList(List<long> updateIds, bool value) {

            // occurs when updating mapping template tree nodes
            if (updateIds.Count == 0) return;
            
            // save changes to database
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                string sql =
                    "UPDATE " +
                    conn.DbMapping.GetTableName(typeof(ValuesGAAP)) +
                    " SET " + conn.Enquote("send_account_balances") + " = " + (value ? 1:0) +
                    " WHERE " + conn.Enquote("id") + " in (" + string.Join(",", updateIds) + ")";

                conn.ExecuteNonQuery(sql);
            }
        }

        public IValueTreeEntry GetValue(string elementName) {
            IValueTreeEntry result = null;
            Action<ValueTreeNode> searchValue = null;
            searchValue = (root) => {
                if (root.Values.ContainsKey(elementName)) {
                    // search node value in root node
                    result = root.Values[elementName];

                } else {
                    // search node value in tuple/list nodes
                    foreach (
                        var item in
                            root.Values.Values.OfType<XbrlElementValue_Tuple>().SelectMany(tuple => tuple.Items)) {
                        searchValue(item);
                    }
                }
            };

            searchValue(Root);
            return result;
        }

        public IEnumerable<IValueTreeEntry> GetValues(string elementName) {
            var result = new List<IValueTreeEntry>();
            
            Action<ValueTreeNode> searchValue = null;
            searchValue = (root) => {
                if (root.Values.ContainsKey(elementName)) {
                    // search node value in root node
                    result.Add(root.Values[elementName]);

                } else {
                    // search node value in tuple/list nodes
                    foreach (
                        var item in
                            root.Values.Values.OfType<XbrlElementValue_Tuple>().SelectMany(tuple => tuple.Items)) {
                        searchValue(item);
                    }
                }
            };

            searchValue(Root);
            return result;
        }
    }
}
