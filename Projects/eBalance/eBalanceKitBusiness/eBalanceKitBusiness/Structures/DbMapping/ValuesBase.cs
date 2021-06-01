/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2011-02-01      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using XbrlProcessor.Taxonomy;
using XbrlProcessor.Taxonomy.XbrlElementValue;
using eBalanceKitBusiness.Structures.Interfaces;

namespace eBalanceKitBusiness.Structures.DbMapping {

    public class ValuesBase<T> : IValueMapping where T : IValueMapping {

        /// <summary>
        /// Initializes a new instance of the <see cref="ValuesBase"/> class.
        /// </summary>
        public ValuesBase() {
            this.IsManualValue = false;
        }

        #region properties

        [DbColumn("id", AllowDbNull = false, AutoIncrement = true)]
        [DbPrimaryKey]
        public int Id { get; set; }

        [DbColumn("document_id", AllowDbNull = true, IsInverseMapping = true)]
        public Document Document { get; set; }

        [DbColumn("parent_id", AllowDbNull = true, IsInverseMapping = true)]
        public T Parent { get; set; }

        [DbColumn("xbrl_elem_id", AllowDbNull = true, Length = 128)]
        public string XbrlElementId { get; set; }

        [DbColumn("value", AllowDbNull = true, Length = 4096)]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the value for entry "other" in comboboxes.
        /// </summary>
        /// <value>The value.</value>
        [DbColumn("cb_value_other", AllowDbNull = true, Length = 4096)]
        public string ValueOther { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance represents a manual value.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance represents a manual value; otherwise, <c>false</c>.
        /// </value>
        public bool _isManualValue;

        [DbColumn("is_manual_value", AllowDbNull = false)]
        public bool IsManualValue {
            get { return _isManualValue; }
            set { _isManualValue = value; }
        }

        [DbCollection("Parent")]
        public List<T> Children {
            get {
                if (_children == null) _children = new List<T>();
                return _children;
            }
            set { _children = value; }
        }
        private List<T> _children;
        
        [DbColumn("flag1", AllowDbNull = false)]
        public bool Flag1 { get; set; }

        [DbColumn("flag2", AllowDbNull = false)]
        public bool Flag2 { get; set; }

        [DbColumn("flag3", AllowDbNull = false)]
        public bool Flag3 { get; set; }

        [DbColumn("flag4", AllowDbNull = false)]
        public bool Flag4 { get; set; }

        public List<IValueMapping> ChildrenBase {
            get {
                List<IValueMapping> result = new List<IValueMapping>();
                foreach (var obj in this.Children) {
                    result.Add(obj);
                }
                return result; 
            }
        }

        public IValueMapping ParentBase {
            get { return this.Parent as IValueMapping; }
            set { this.Parent = (T)value; }
        }

        /// <summary>
        /// Gets the taxonomy.
        /// </summary>
        /// <value>The taxonomy.</value>
        public static Taxonomy Taxonomy {
            get { return AppConfig.GAAP_Taxonomy; }
        }

        #endregion properties

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Adds the children.
        /// </summary>
        /// <param name="child">The child.</param>
        public void AddChildren(IValueMapping child) {
            if (child is T) {
                this.Children.Add((T)child);
            }
        }

        public void Remove() {
            this.Parent.ChildrenBase.Remove(this);
        }

        #endregion methods
    }
}
