using System;
using DbAccess;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;

namespace eBalanceKitBusiness.Structures.XbrlElementValue {

    /// <summary>
    /// Presentation tree value class for list elements.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2010-11-04</since>
    public class XbrlElementValue_List : XbrlElementValue_Tuple, IValueTreeEntry {

        #region constructor
        internal XbrlElementValue_List(IDatabase conn, ITaxonomyIdManagerProvider idObject, Taxonomy.ITaxonomy taxonomy, Taxonomy.IElement element, ValueTreeNode parent, IValueMapping dbValue, Type valueType)
            : base(conn, idObject, taxonomy, element, parent, dbValue, valueType) { }
        #endregion

        #region fields

        /// <summary>
        /// See property SelectedItem.
        /// </summary>
        ValueTreeNode _selectedItem;

        #endregion fields

        #region properties

        public ValueTreeNode SelectedItem {
            get { return _selectedItem; }
            set {
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        #endregion properties

        #region methods

        public override ValueTreeNode AddValue() {
            var node = base.AddValue();
            SelectedItem = node;
            return node;
        }

        /// <summary>
        /// Deletes the selected value.
        /// </summary>
        public void DeleteValue(ValueTreeNode node, IDatabase conn) {

            // delete value from value tree and from database
            (node.DbValue as ValueMappingBase).RemoveFromParent();

            if (XbrlElementValueBase.DoDbUpdate) {
                conn.DbMapping.Delete(node.DbValue);
            }

            // delete value from treeview
            DeleteSummationTargets(node, this.Element);
            this.Items.Remove(node);
        }

        /// <summary>
        /// Deletes the selected value.
        /// </summary>
        public void DeleteSelectedValue() {
            
            if (this.SelectedItem != null) {
                DeleteValue(this.SelectedItem);
                this.SelectedItem = null;
            }
        }

        #endregion methods
    }
}
