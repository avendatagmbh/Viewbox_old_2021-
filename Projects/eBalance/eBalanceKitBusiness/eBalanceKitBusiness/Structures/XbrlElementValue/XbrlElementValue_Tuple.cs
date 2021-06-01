// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-02-20
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using DbAccess;
using Taxonomy;
using Utils;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;
using eBalanceKitBusiness.Structures.ValueTree;

namespace eBalanceKitBusiness.Structures.XbrlElementValue {
    /// <summary>
    /// Presentation tree value class for list elements.
    /// </summary>
    public class XbrlElementValue_Tuple : XbrlElementValueBase, IValueTreeEntry {
        #region constructor
        internal XbrlElementValue_Tuple(IDatabase conn, ITaxonomyIdManagerProvider idObject, ITaxonomy taxonomy, IElement element,
                                        ValueTreeNode parent, IValueMapping dbValue, Type valueType)
            : base(element, parent, dbValue) {
            IdObject = idObject;
            Taxonomy = taxonomy;
            ValueType = valueType;
            if (DbValue != null) Init(conn);

            Debug.Assert(Element.Children.Any());
            // Should allways have a child but eg. in first taxonomy there was a problem with "de-gaap-ci_nt.segmGeographical.OtherImportant"
            if (Element.Children.Any()) {
                DisplayMemberPath = "Values[" + Element.Children.First().Id + "].DisplayString";
            }
        }
        #endregion

        #region properties

        #region Items
        protected ObservableCollectionAsync<ValueTreeNode> _items = new ObservableCollectionAsync<ValueTreeNode>();

        public ObservableCollectionAsync<ValueTreeNode> Items {
            get { return _items; }
            set {
                if (_items != value) {
                    _items = value;
                    OnPropertyChanged("ListValue");
                    OnPropertyChanged("DisplayString");
                    OnPropertyChanged("Value");
                }
            }
        }
        #endregion

        private ITaxonomyIdManagerProvider IdObject { get; set; }
        protected Type ValueType { get; set; }
        public ITaxonomy Taxonomy { get; set; }

        public bool DeleteItemAllowed {
            get {
                return (ReportRights != null && ReportRights.WriteRestAllowed) ||
                       (ReportRights == null && IsCompanyEditAllowed);
            }
        }

        public bool AddItemAllowed {
            get {
                return (ReportRights != null && ReportRights.WriteRestAllowed) ||
                       (ReportRights == null && IsCompanyEditAllowed);
            }
        }

        #region ChildValidation
        public bool ChildHasValidationError {
            get {
                foreach (ValueTreeNode item in Items) foreach (var value in item.Values) if (value.Value.ValidationError) return true;
                return false;
            }
        }

        public bool ChildHasValidationWarning {
            get {
                foreach (ValueTreeNode item in Items) foreach (var value in item.Values) if (value.Value.ValidationWarning) return true;
                return false;
            }
        }
        #endregion

        public string DisplayString { get { return string.Empty; } }
        public override bool IsNumeric { get { return false; } }
        public override bool HasValue { get { return Items.Count > 0; } }
        public string ItemsCountDisplayString { get { return Items.Count + " Listeneinträge"; } }
        public string DisplayMemberPath { get; internal set; }

        #endregion properties

        #region methods

        #region Init
        private void Init(IDatabase conn) {
            var tmp = new List<ValueTreeNode>();
            foreach (IValueMapping value in (DbValue as ValueMappingBase).Children) {
                var node = new ValueTreeNode(Parent.ValueTree, value, Taxonomy);
                node.Init(conn, IdObject, ValueType, Element);
                AddSummationTargets(node, Element);
                tmp.Add(node);
            }

            tmp.Sort();
            foreach (ValueTreeNode node in tmp) {
                Items.Add(node);
                foreach (IValueTreeEntry value in node.Values.Values) {
                    value.PropertyChanged += value_PropertyChanged;
                    ((XbrlElementValueBase) value).ForceValueComputationForParents();
                }
                InitStates(node);
            }
        }

        private void value_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "ValidationError") OnPropertyChanged("ChildHasValidationError");
            if (e.PropertyName == "ValidationWarning") OnPropertyChanged("ChildHasValidationWarning");
        }
        #endregion Init

        #region AddSummationTargets
        protected void AddSummationTargets(ValueTreeNode node, IElement root) {
            foreach (IElement child in root.Children) {
                foreach (IElement summationSource in child.SummationSources) {
                    foreach (Taxonomy.SummationItem summationTarget in summationSource.SummationTargets) {
                        try {
                            IValueTreeEntry srcValue = null;
                            IValueTreeEntry destValue = null;

                            if (node.Values.ContainsKey(summationSource.Id)) {
                                srcValue = node.Values[summationSource.Id];
                            } else if (Parent.Values.ContainsKey(summationSource.Id)) {
                                srcValue = Parent.Values[summationSource.Id];
                            }

                            if (node.Values.ContainsKey(summationTarget.Element.Id)) {
                                destValue = node.Values[summationTarget.Element.Id];
                            } else if (Parent.Values.ContainsKey(summationTarget.Element.Id)) {
                                destValue = Parent.Values[summationTarget.Element.Id];
                            } else if (child.Id == summationTarget.Element.Id) {
                                //destValue = node.Value;
                            }


                            if (srcValue != null && destValue != null && child.Id == destValue.Element.Id) {
                                srcValue.AddSummationTarget(destValue, (decimal) summationTarget.Weight);
                                destValue.AddSummationSource(srcValue);
                            } else {
                            }
                        } catch (Exception ex) {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }

                AddSummationTargets(node, child);
            }
        }
        #endregion AddSummationTargets

        #region DeleteSummationTargets
        protected void DeleteSummationTargets(ValueTreeNode node, IElement root) {
            foreach (IValueTreeEntry value in node.Values.Values) {
                foreach (SummationSource summationSource in ((XbrlElementValueBase) value).SummationSources) {
                    var removeList = new List<SummationItem>();

                    foreach (
                        SummationItem summationTarget in ((XbrlElementValueBase) summationSource.Value).SummationTargets
                        ) {
                        if (summationTarget.Value == value) {
                            removeList.Add(summationTarget);
                        }
                    }

                    foreach (SummationItem si in removeList) {
                        ((XbrlElementValueBase) summationSource.Value).SummationTargets.Remove(si);
                        ((XbrlElementValueBase) summationSource.Value).UpdateComputedValue();
                    }
                }
            }

            foreach (IElement child in root.Children) {
                foreach (IElement summationSource in child.SummationSources) {
                }
            }
        }
        #endregion DeleteSummationTargets

        #region Clear
        public void Clear() {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.BeginTransaction();

                try {
                    conn.ExecuteNonQuery(
                        "DELETE FROM " + conn.Enquote("values_gaap") + " WHERE " +
                        conn.Enquote("document_id") + "=" + (IdObject as Document).Id +
                        " AND " +
                        conn.FunctionSubstring(conn.Enquote("xbrl_elem_id"), 0,
                                               "detailedInformation.accountBalances".Length) +
                        "='detailedInformation.accountBalances'" +
                        " AND " + conn.Enquote("id") + "<>" + (DbValue as ValueMappingBase).Id);

                    foreach (ValueTreeNode item in Items) {
                        (item.DbValue as ValueMappingBase).RemoveFromParent();
                        DeleteSummationTargets(item, Element);
                    }
                    Items.Clear();

                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ex.Message, ex);
                }
            }
        }
        #endregion Clear

        #region DeleteValue
        public void DeleteValue(ValueTreeNode node) {
            //Not allowed if parent is not accessible
            if (!DeleteItemAllowed) return;

            if (node.DbValue != null) {
                // delete value from value tree and from database
                (node.DbValue as ValueMappingBase).RemoveFromParent();

                if (DoDbUpdate) {
                    using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                        try {
                            conn.BeginTransaction();
                            foreach (IValueTreeEntry value in node.Values.Values) {
                                if (value is XbrlElementValue_List) {
                                    var tValue = value as XbrlElementValue_List;
                                    var items = new List<ValueTreeNode>(tValue.Items);
                                    foreach (ValueTreeNode item in items) 
                                        tValue.DeleteValue(item, conn);
                                }
                                conn.DbMapping.Delete(value.DbValue);
                            }
                            conn.DbMapping.Delete(node.DbValue);
                            if (this is XbrlElementValue_List)
                                LogManager.Instance.UpdateValue(
                                    new LogManager.AddValueInfo((object)Parent.ValueTree.Document ?? Parent.ValueTree.Company) {
                                        TaxonomyString = node.DbValue.Element.Id,
                                        Change = "DeleteFromList",
                                        NewValue = node.DbValue,
                                        OldValue = null
                                    });
                            conn.CommitTransaction();
                        } catch (Exception ex) {
                            conn.RollbackTransaction();
                            throw new Exception(ex.Message, ex);
                        }
                    }
                }
            }

            // delete value from treeview
            DeleteSummationTargets(node, Element);
            Items.Remove(node);
        }
        #endregion DeleteValue

        #region AddValue
        public virtual ValueTreeNode AddValue() {
            //Not allowed if parent is not accessible
            if (!AddItemAllowed) return null;

            ValueTreeNode node = null;
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    node = AddValue(conn);
                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ex.Message, ex);
                }
            }
            
            return node;
        }

        public ValueTreeNode AddValue(IDatabase conn) {
            IValueMapping newDbValue = null;
            if (DbValue != null) {
                newDbValue = (IValueMapping) Activator.CreateInstance(ValueType);
                newDbValue.ElementId = (Parent.ValueTree.Document != null
                                            ? Parent.ValueTree.Document.TaxonomyIdManager.GetId(Element.Id)
                                            : Parent.ValueTree.Company.TaxonomyIdManager.GetId(Element.Id));
                newDbValue.Element = Element;
                var newDbValue1 = newDbValue as ValueMappingBase;
                newDbValue1.SetGroupIdentifier(IdObject);

                (DbValue as ValueMappingBase).AddChildren(newDbValue1);

                if ( /*DoDbUpdate &&*/ conn != null) {
                    try {
                        conn.DbMapping.Save(newDbValue);
                        if (this is XbrlElementValue_List)
                            LogManager.Instance.UpdateValue(new LogManager.AddValueInfo((object)Parent.ValueTree.Document ?? Parent.ValueTree.Company) {
                                TaxonomyString = Element.Id,
                                Change = "AddToList",
                                NewValue = newDbValue,
                                OldValue = null
                            });
                    } catch (Exception ex) {
                        throw new Exception(ex.Message, ex);
                    }
                }
            }

            var node = new ValueTreeNode(Parent.ValueTree, newDbValue, Taxonomy);
            node.Init(conn, IdObject, ValueType, Element);

            //conn.DbMapping.Save(newDbValue);
            AddSummationTargets(node, Element);

            Items.Add(node);
            InitStates(node);

            //Sort();

            return node;
        }
        #endregion AddValue

        #region Sort
        public void Sort() {
            var tmp = new List<ValueTreeNode>(Items);
            Items.Clear();
            tmp.Sort();
            foreach (ValueTreeNode node in tmp) Items.Add(node);
        }
        #endregion

        #region GetValue/SetValue
        protected override void SetValue(object value) { }
        protected override object GetValue() { return null; }
        #endregion

        #endregion methods        
    }
}