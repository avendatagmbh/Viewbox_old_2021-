// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-05-14
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using DbAccess;
using Taxonomy;
using eBalanceKitBusiness.Interfaces;

namespace eBalanceKitBusiness.Structures.DbMapping.ValueMappings {
    /// <summary>
    /// This class is the base class for all value mapping classes. Each instance
    /// of a this class represents an individual fact of the assigned taxonomy.
    /// </summary>
    public abstract class ValueMappingBase {

        #region properties

        #region persistent

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        internal long Id { get; set; }
        #endregion

        #region ElementId
        [DbColumn("elem_id", AllowDbNull = true)]
        public int ElementId { get; set; }
        #endregion

        #region value
        private string _value;

        [DbColumn("value", AllowDbNull = true, Length = 4096)]
        public string Value {
            get { return _value; }
            set {
                if (_value == value) return;
                if (value != null && value.Length > 4096) value = value.Substring(0, 4096);
                _value = value;
            }            
        }
        #endregion

        #region ValueOther
        private string _valueOther;

        /// <summary>
        /// Gets or sets the value for entry "other" in comboboxes.
        /// </summary>
        /// <value>The value.</value>
        [DbColumn("cb_value_other", AllowDbNull = true, Length = 4096)]
        public string ValueOther {
            get { return _valueOther; }
            set {
                if (_valueOther == value) return;
                if (value != null && value.Length > 4096) value = value.Substring(0, 4096);
                _valueOther = value;
            }
        }
        #endregion

        #region SupressWarningMessages
        [DbColumn("supress_warning_messages", AllowDbNull = false)]
        public bool SupressWarningMessages { get; set; }
        #endregion

        #region AutoComputationEnabled
        [DbColumn("auto_computation_enabled", AllowDbNull = false)]
        public bool AutoComputationEnabled { get; set; }
        #endregion

        #region SendAccountBalances
        public bool _sendAccountBalances;

        [DbColumn("send_account_balances", Length = 1, AllowDbNull = true)]
        public bool SendAccountBalances {
            get { return _sendAccountBalances; }
            set { _sendAccountBalances = value; }
        }
        #endregion

        #region Comment
        private string _comment;

        [DbColumn("comment", AllowDbNull = true, Length = 4096)]
        public string Comment {
            get { return _comment; }
            set {
                if (_comment == value) return;
                if (value == null) _comment = null;
                else {
                    string tmp = value.Trim();
                    if (tmp.Length == 0) _comment = null;
                    else {
                        if (value.Length > 4096) value = value.Substring(0, 4096);
                        _comment = value;
                    }
                }
            }
        }
        #endregion

        #endregion persistent

        #region non persistant

        #region Children
        private readonly List<ValueMappingBase> _children = new List<ValueMappingBase>();

        private readonly Dictionary<string, List<ValueMappingBase>> _childrenDict =
            new Dictionary<string, List<ValueMappingBase>>();

        public IEnumerable<ValueMappingBase> Children {
            get { return _children; }
        }
        #endregion

        #region Element
        /// <summary>
        /// The assigned xbrl element.
        /// </summary>
        public IElement Element { get; set; }
        #endregion

        #endregion non persistant

        #endregion properties

        #region methods

        #region AddChildren
        internal void AddChildren(ValueMappingBase child) {
            child.SetParent(this);
            _children.Add(child);
            if (child.Element != null) {
                if (!_childrenDict.ContainsKey(child.Element.Id))
                    _childrenDict[child.Element.Id] = new List<ValueMappingBase>();
                _childrenDict[child.Element.Id].Add(child);
            }
        }
        #endregion

        #region RemoveChildren
        internal void RemoveChildren(ValueMappingBase child) {
            child.SetParent(null);
            _children.Remove(child);
            if (child.Element != null && _childrenDict.ContainsKey(child.Element.Id)) {
                _childrenDict[child.Element.Id].Remove(child);
                if (_childrenDict[child.Element.Id].Count == 0) _childrenDict.Remove(child.Element.Id);
            }
        }
        #endregion

        #region Remove
        internal void RemoveFromParent() {
            var parent = GetParent();
            if (parent != null) parent.RemoveChildren(this);
        }
        #endregion

        #region Load
        /// <summary>
        /// Loads all existing values from the database, that fits the specified filter (e.g. document_id or company_id filter).
        /// </summary>
        /// <param name="conn">An open database connection.</param>
        /// <param name="valueMappingType">Type of the value mapping class.</param>
        /// <param name="filterColumn">The filter column.</param>
        /// <param name="filterValue">The filter value.</param>
        /// <returns></returns>
        protected static IValueMapping Load(IDatabase conn, Type valueMappingType, ITaxonomy taxonomy,
                                            string filterColumn, int filterValue, object idObject) {
            ValueMappingBase root = null;
            var valueDict = new Dictionary<long, ValueMappingBase>();
            var values = new List<Tuple<ValueMappingBase, long>>();

            Debug.Assert(idObject is Document || idObject is Company, "Wrong idObject type.");

            var taxonomyIdManager = (idObject is Document
                                         ? (idObject as Document).TaxonomyIdManager
                                         : (idObject as Company).TaxonomyIdManager);

            // load database values (manually loaded due to performance reasons)
            IDataReader reader = conn.ExecuteReader(
                "SELECT " +
                conn.Enquote("id") + "," +
                conn.Enquote("parent_id") + "," +
                conn.Enquote("elem_id") + "," +
                conn.Enquote("value") + "," +
                conn.Enquote("cb_value_other") + "," +
                conn.Enquote("comment") + "," +
                conn.Enquote("auto_computation_enabled") + "," +
                conn.Enquote("supress_warning_messages") + "," +
                conn.Enquote("send_account_balances") +
                " FROM " + conn.Enquote(conn.DbMapping.GetTableName(valueMappingType)) +
                " WHERE " + conn.Enquote(filterColumn) + "=" + filterValue);

            while (reader.Read()) {
                var value = (ValueMappingBase) Activator.CreateInstance(valueMappingType);
                value.Id = Convert.ToInt32(reader[0]);
                value.ElementId = Convert.ToInt32(reader[2]);
                value.Value = (reader.IsDBNull(3) ? null : reader[3].ToString());
                value.ValueOther = (reader.IsDBNull(4) ? null : reader[4].ToString());
                value.Comment = (reader.IsDBNull(5) ? null : reader[5].ToString());
                value.AutoComputationEnabled = Convert.ToBoolean(reader[6]);
                value.SupressWarningMessages = Convert.ToBoolean(reader[7]);
                value.SendAccountBalances = Convert.ToBoolean(reader[8]);

                // set assigned xbrl element
                try {
                    IElement elem = taxonomyIdManager.GetElement(value.ElementId);
                    value.Element = elem;
                    if (elem == null) {
                    // DEBUG
                    //TODO Lösche Element statt abzubrechen
                    //throw new Exception("Konnte das Element " + value.XbrlElementId +
                    //                    " nicht in der Taxonomie finden.");
                    }
                } catch (Exception) {
                    throw;
                }

                valueDict[value.Id] = value;
                if (reader.IsDBNull(1)) root = value;
                else values.Add(new Tuple<ValueMappingBase, long>(value, Convert.ToInt64(reader[1])));
            }

            reader.Close();

            // build tree structure
            foreach (var val in values) {
                if (!valueDict.ContainsKey(val.Item2)) {
                    Debug.WriteLine("Database error: found orphaned entries in table '" +
                                    conn.DbMapping.GetTableName(valueMappingType) + "' / id = " + val.Item1.ElementId + " / xbrl-name = '.");
                    conn.DbMapping.Delete(val.Item1);
                } else {
                    ValueMappingBase value = val.Item1;
                    ValueMappingBase parentValue = valueDict[val.Item2];
                    parentValue.AddChildren(value);
                }
            }

            // create new root node, if no root does exist
            if (root == null) root = (ValueMappingBase) Activator.CreateInstance(valueMappingType);

            return root as IValueMapping;
        }
        #endregion Load

        #region SetGroupIdentifier
        /// <summary>
        /// Sets the identifier object for the value mapping group (e.g. a specific Document or Company).
        /// </summary>
        /// <param name="idObject">The id object.</param>
        internal void SetGroupIdentifier(object idObject) {
            if (idObject is Document) {
                if (GetType().GetInterface("IValueMappingDocument") != null)
                    (this as IValueMappingDocument).Document = idObject as Document;
                else if (GetType().GetInterface("IValueMappingCompany") != null)
                    (this as IValueMappingCompany).Company = (idObject as Document).Company;
                else throw new Exception("Unsupported interface type.");
            } else if (idObject is Company) {
                if (GetType().GetInterface("IValueMappingCompany") != null)
                    (this as IValueMappingCompany).Company = (idObject as Company);
                else throw new Exception("Unsupported interface type.");
            } else {
                new Exception("Unsupported id-object type.");
            }
        }
        #endregion

        #region GetGroupId
        public int GetGroupId() {
            if (GetType().GetInterface("IValueMappingDocument") != null)
                return (this as IValueMappingDocument).Document.Id;
            else if (GetType().GetInterface("IValueMappingCompany") != null)
                return (this as IValueMappingCompany).Company.Id;
            else throw new Exception("Unsupported interface type.");
        }
        #endregion

        #region GetChild
        public IValueMapping GetChild(string name) {
            if (_childrenDict.ContainsKey(name)) return _childrenDict[name][0] as IValueMapping;
            else return null;
        }
        #endregion

        //--------------------------------------------------------------------------------

        protected abstract ValueMappingBase GetParent();
        protected abstract void SetParent(ValueMappingBase parent);

        //--------------------------------------------------------------------------------
        #endregion methods
    }
}