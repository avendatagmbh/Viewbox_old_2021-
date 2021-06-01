using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using DbAccess;
using Taxonomy.Enums;
using Utils;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitBusiness.Structures.DbMapping;
using Taxonomy.PresentationTree;

namespace eBalanceKitBusiness.Structures.ValueTree {

    /// <summary>
    /// This class represents a node of an xbrl value tree. All elements which could only occur only once are represented at the root level, whereas
    /// lists are represented as children of XbrlElementValue_List values. Each of these childs is another ValueTreeNode object, which represents an
    /// list entry.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-01-24</since>
    public class ValueTreeNode : NotifyPropertyChangedBase, IComparable {

        #region constructors

        static ValueTreeNode() {
            SortFieldIds = new Dictionary<string, string>();
            DisplayStringIds = new Dictionary<string, string>();

            SortFieldIds["de-gcd_genInfo.doc.author"] = "de-gcd_genInfo.doc.author.name";
            DisplayStringIds["de-gcd_genInfo.doc.author"] = "de-gcd_genInfo.doc.author.name";

            SortFieldIds["de-gcd_genInfo.company.id.shareholder"] = "de-gcd_genInfo.company.id.shareholder.name";
            DisplayStringIds["de-gcd_genInfo.company.id.shareholder"] = "de-gcd_genInfo.company.id.shareholder.name";

            SortFieldIds["de-gcd_genInfo.company.id.contactAddress"] = "de-gcd_genInfo.company.id.contactAddress.person";
            DisplayStringIds["de-gcd_genInfo.company.id.contactAddress"] = "de-gcd_genInfo.company.id.contactAddress.person";

            SortFieldIds["de-gcd_genInfo.company.id.contactAddress"] = "de-gcd_genInfo.company.id.contactAddress.person";
            DisplayStringIds["de-gcd_genInfo.company.id.contactAddress"] = "de-gcd_genInfo.company.id.contactAddress.person";
        }

        public ValueTreeNode(ValueTree vtree, IValueMapping dbValue, Taxonomy.ITaxonomy taxonomy) {
            ValueTree = vtree;
            DbValue = dbValue;
            Elements = taxonomy.Elements;
            Taxonomy = taxonomy;

            DisplayString = "<keine Bezeichnung>";
        }

        #endregion

        #region properties

        #region IsSelected
        public bool IsSelected {
            get { return _isSelected; }
            set {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        private bool _isSelected;
        #endregion

        #region DisplayString
        public static Dictionary<string, string> DisplayStringIds { get; set; }
        private string _displayString;

        public string DisplayString {
            get { return _displayString; }
            set { _displayString = value; }
        }
        #endregion

        #region Values
        private readonly Dictionary<string, IValueTreeEntry> _values = new Dictionary<string, IValueTreeEntry>();
        public Dictionary<string, IValueTreeEntry> Values { get { return _values; } }
        #endregion

        public IValueMapping DbValue { get; set; }
        public ValueTree ValueTree { get; set; }
        public static Dictionary<string, string> SortFieldIds { get; set; }
        public Dictionary<string, Taxonomy.IElement> Elements { get; set; }
        public Taxonomy.ITaxonomy Taxonomy { get; set; }

        #endregion properties

        #region eventHandler

        #region newEntry_PropertyChanged
        void newEntry_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            XbrlElementValueBase xbrlValue = sender as XbrlElementValueBase;
            if (xbrlValue == null) return;

            if (xbrlValue.Value == null || string.IsNullOrEmpty(xbrlValue.Value.ToString())) {
                DisplayString = "<keine Bezeichnung>";
                OnPropertyChanged("DisplayString");
            } else if (DbValue.Element != null &&
                DisplayStringIds.ContainsKey(DbValue.Element.Id) &&
                DisplayStringIds[DbValue.Element.Id] == xbrlValue.Element.Id) {

                DisplayString = xbrlValue.Value.ToString();
                OnPropertyChanged("DisplayString");
            }
        }
        #endregion newEntry_PropertyChanged

        #endregion eventHandler

        #region methods

        #region Init
        internal void Init(ITaxonomyIdManagerProvider idObject, Type valueType) {

            using (var conn = AppConfig.ConnectionManager.GetConnection()) { Init(conn, idObject, valueType); }

            // copy values from company subtree into document value tree
            if (idObject is Document && Taxonomy.Filename.StartsWith("de-gcd")) {
                foreach (var value in (idObject as Document).Company.ValueTree.Root.Values) {
                    Values.Add(value.Key, value.Value);
                }
            }

            foreach (Taxonomy.IElement elem in Taxonomy.Elements.Values) AddSummationTargets(elem);
            ForceRecomputation();
        }
        #endregion Init

        #region Init
        internal void Init(IDatabase conn, ITaxonomyIdManagerProvider idObject, Type valueType, Taxonomy.IElement root = null) {

            // create non tuple entries
            var tupleValues = new List<IValueTreeEntry>();

            // get enumeration of all elements, which should be processed
            IEnumerable<Taxonomy.IElement> elements;
            if (root == null) elements = Taxonomy.Elements.Values;
            else elements = root.Children;

            try {
                // avoid nested transactions
                bool useTransaction = !conn.HasTransaction();

                try {
                    if (useTransaction) conn.BeginTransaction();

                    foreach (var elem in elements) {
                        // if (elem.Parents.Count == 0 || root != null)
                        if (elem.Parents.Count == 0 || /*root != null */ elem.Parents.Contains(root)) // elem is no tuple (or no subtuple of root if root <> null)
                            BuildValueTree(conn, idObject, valueType, elem, tupleValues, root != null);
                    }
                    if (useTransaction) conn.CommitTransaction();

                } catch (Exception ex) {
                    if (useTransaction) conn.RollbackTransaction();
                    throw new Exception(ex.Message, ex);
                }

                // create tuple entries
                foreach (var value in tupleValues) {
                    XbrlElementValue_Tuple t = value as XbrlElementValue_Tuple;
                    if (t.Items.Count == 0) {
                        if (t.DbValue == null) continue;
                        // save parent to create id if not yet done                            
                        if ((t.DbValue as ValueMappingBase).Id == 0) conn.DbMapping.Save(t.DbValue);

                        t.AddValue(conn);
                    }
                }

                //foreach (var elem in elements) AddSummationTargets(elem);

            } catch (Exception ex) {
                throw new Exception(ex.Message, ex);
            }

        }
        #endregion

        #region ForceRecomputation
        public void ForceRecomputation() { ForceRecomputation(this); }

        private static void ForceRecomputation(ValueTreeNode root) {
            foreach (var value in root.Values.Values) {
                if (value is XbrlElementValue_Tuple) {
                    foreach (var item in (value as XbrlElementValue_Tuple).Items) {
                        ForceRecomputation(item);
                    }
                } else if (value.Element.ValueType != XbrlElementValueTypes.Monetary) continue;


                XbrlElementValueBase bvalue = (XbrlElementValueBase)value;
                bvalue.UpdateComputedValue();
            }
        }
        #endregion ForceRecomputation

        #region BuildValueTree
        private void BuildValueTree(
            IDatabase conn,
            ITaxonomyIdManagerProvider idObject,
            Type valueType,
            Taxonomy.IElement element,
            List<IValueTreeEntry> tupleValues,
            bool isTupleInitilisation) {

            // abstract members does not have any value
            if (element.ValueType == XbrlElementValueTypes.Abstract) return;

            if (idObject is Document && element.TableType != null) return;
            if (idObject is Company && !isTupleInitilisation && element.TableType == null) return;

            Type currentValueType = (isTupleInitilisation ? valueType : (element.TableType ?? valueType));

            // get value mapping object for current element
            IValueMapping newDbValue = null;
            newDbValue = DbValue.GetChild(element.Id);

            if (newDbValue == null && element.IsPersistant && !element.IsHypercubeItem) {
                newDbValue = (IValueMapping)Activator.CreateInstance(currentValueType);
                
                Debug.Assert((idObject is Document) || (idObject is Company), "Invalid idObject type: " + idObject.GetType().Name);
                
                newDbValue.ElementId = idObject.TaxonomyIdManager.GetId(element.Id);

                var newDbValue1 = newDbValue as ValueMappingBase;
                newDbValue1.SetGroupIdentifier(idObject);
                (DbValue as ValueMappingBase).AddChildren(newDbValue1);

                if (element.ValueType == XbrlElementValueTypes.Monetary) {
                    if (!element.HasComputationSources && !element.HasComputationTargets) {
                        bool foundComputedParent = false;
                        foreach (var ptreeNode in element.PresentationTreeNodes) {
                            foreach (var parent in ptreeNode.Parents) {
                                if ((parent as PresentationTreeNode).IsComputed) {
                                    foundComputedParent = true;
                                    break;
                                }
                            }
                        }
                        if (!foundComputedParent) newDbValue.AutoComputationEnabled = true;
                    }
                }

                // save new value
                conn.DbMapping.Save(newDbValue);
            }

            try {
                IValueTreeEntry newValue = null;

                // create new value
                if (element.IsList) {
                    newValue = new XbrlElementValue_List(conn, idObject, Taxonomy, element, this, newDbValue, currentValueType);
                    
                } else {
                    switch (element.ValueType) {
                        case XbrlElementValueTypes.HyperCubeContainerItem:
                            newValue = new XbrlElementValue_Hypercube(element, this);
                            break;

                        case XbrlElementValueTypes.MultipleChoice:
                            newValue = new XbrlElementValue_MultipleChoice(Taxonomy, element, this, newDbValue, currentValueType);
                            break;

                        case XbrlElementValueTypes.SingleChoice:
                            newValue = new XbrlElementValue_SingleChoice(element, this, newDbValue, currentValueType);
                            break;

                        case XbrlElementValueTypes.Tuple:
                            newValue = new XbrlElementValue_Tuple(conn, idObject, Taxonomy, element, this, newDbValue, currentValueType);
                            tupleValues.Add(newValue);
                            break;

                        case XbrlElementValueTypes.Boolean:
                            newValue = new XbrlElementValue_Boolean(element, this, newDbValue);
                            break;

                        case XbrlElementValueTypes.Date:
                            newValue = new XbrlElementValue_Date(element, this, newDbValue);
                            break;

                        case XbrlElementValueTypes.Int:
                            newValue = new XbrlElementValue_Int(element, this, newDbValue);
                            break;

                        case XbrlElementValueTypes.Monetary:
                            newValue = new XbrlElementValue_Monetary(element, this, newDbValue);
                            break;

                        case XbrlElementValueTypes.Numeric:
                            newValue = new XbrlElementValue_Numeric(element, this, newDbValue);
                            break;

                        case XbrlElementValueTypes.String:
                            newValue = new XbrlElementValue_String(element, this, newDbValue);
                            break;

                        default:
                            // should not happen
                            throw new NotImplementedException();
                    }
                }

                Values[element.Id] = newValue;

                if (DbValue.Element != null &&
                    DisplayStringIds.ContainsKey(DbValue.Element.Id) &&
                    DisplayStringIds[DbValue.Element.Id] == element.Id) {

                    newValue.PropertyChanged += newEntry_PropertyChanged;
                    if (newDbValue.Value != null) DisplayString = newDbValue.Value;

                }

            } catch (Exception ex) {
                throw new Exception(ex.Message, ex);
            }

        }
        #endregion BuildValueTree

        #region AddSummationTargets
        private void AddSummationTargets(Taxonomy.IElement root) {
            if (Values.ContainsKey(root.Id)) {
                IValueTreeEntry srcValue = Values[root.Id];
                foreach (var summationTarget in root.SummationTargets) {
                    if (Values.ContainsKey(summationTarget.Element.Id)) {

                        IValueTreeEntry destValue = Values[summationTarget.Element.Id];
                        srcValue.AddSummationTarget(destValue, (decimal)summationTarget.Weight);
                        destValue.AddSummationSource(srcValue);
                    }
                }
            }
        }
        #endregion AddSummationTargets

        #region CompareTo
        public int CompareTo(object obj) {
            ValueTreeNode node = obj as ValueTreeNode;
            if (node == null) return 1;

            //if (SortFieldIds.ContainsKey(this.DbValue.XbrlElementId)) {
            //    string key = SortFieldIds[this.DbValue.XbrlElementId];
            //    if (this.Values.ContainsKey(key) && node.Values.ContainsKey(key)) {
            //        object v1 = this.Values[key].Value;
            //        object v2 = node.Values[key].Value;

            //        if (v1 == null && v2 != null) return -1;
            //        else if (v1 != null && v2 == null) return 1;
            //        else if (v1 == null && v2 == null) return 0;
            //        else return v1.ToString().CompareTo(v2.ToString());
            //    }
            return 1;
            //} else {
            //    return 1;
            //}
        }
        #endregion CompareTo

        #endregion methods
    }
}
