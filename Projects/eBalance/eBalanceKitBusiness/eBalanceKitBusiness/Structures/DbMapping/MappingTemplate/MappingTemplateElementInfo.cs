// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-05-30
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using eBalanceKitBusiness.Interfaces.PresentationTree;

namespace eBalanceKitBusiness.Structures.DbMapping.MappingTemplate {
    /// <summary>
    /// This class represents additional infos for assignment templates.
    /// </summary>
    [DbTable("mapping_template_element_info", ForceInnoDb = true)]
    internal class MappingTemplateElementInfo {

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public long Id { get; set; }
        #endregion

        #region Template
        [DbColumn("template_id", IsInverseMapping = true)]
        public MappingTemplateHead Template { get; set; }
        #endregion

        #region DebitElementId
        [DbColumn("element_id", Length = 2048)]
        public string ElementId { get; set; }
        #endregion

        #region AutoComputeEnabled
        [DbColumn("auto_compute_enabled", IsInverseMapping = true)]
        public bool AutoComputeEnabled { get; set; }
        #endregion

        #region SupressWarningMessages
        [DbColumn("supress_warning_messages", AllowDbNull = false)]
        public bool SupressWarningMessages { get; set; }
        #endregion

        #region SendAccountBalances
        [DbColumn("send_account_balances", AllowDbNull = false)]
        public bool SendAccountBalances { get; set; }
        #endregion

        #region PresentationTreeNode
        /// <summary>
        /// Assigned presentation tree node.
        /// </summary>
        internal IPresentationTreeNode PresentationTreeNode { get; set; }
        #endregion
    }
}