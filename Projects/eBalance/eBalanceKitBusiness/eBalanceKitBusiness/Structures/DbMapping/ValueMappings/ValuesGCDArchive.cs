using DbAccess;
using eBalanceKitBusiness.Interfaces;

namespace eBalanceKitBusiness.Structures.DbMapping.ValueMappings {
    /// <summary>
    /// Each instance of this class represents an archived individual fact of the GCD taxonomy, whereas
    /// the company values and the report period entries are excluded.
    /// </summary>
    [DbTable("values_gcd_archive", ForceInnoDb = true)]
    public class ValuesGCDArchive : ValueMappingBaseDocument, IValueMappingCompany {
        #region Parent
        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        [DbColumn("parent_id", AllowDbNull = true, IsInverseMapping = true)]
        public ValuesGCDArchive Parent { get; set; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        IValueMapping IValueMapping.Parent { get { return Parent; } }
        #endregion

        #region IValueMappingCompany Members
        /// <summary>
        /// Gets or sets the company.
        /// </summary>
        /// <value>
        /// The company.
        /// </value>
        public Company Company { get { return Document.Company; } set { Document.Company = value; } }
        #endregion

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <returns></returns>
        protected override ValueMappingBase GetParent() { return Parent; }

        /// <summary>
        /// Sets the parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        protected override void SetParent(ValueMappingBase parent) { Parent = (parent as ValuesGCDArchive); }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() { return ((Element != null ? Element.Id : "") + " = " + (Value ?? "")); }
    }
}