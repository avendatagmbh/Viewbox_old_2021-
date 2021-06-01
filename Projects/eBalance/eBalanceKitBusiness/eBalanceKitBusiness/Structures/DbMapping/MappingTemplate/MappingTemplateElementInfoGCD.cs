using DbAccess;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using Utils;

namespace eBalanceKitBusiness.Structures.DbMapping.MappingTemplate
{
    /// <summary>
    /// This class represents additional infos for assignment templates.
    /// </summary>
    [DbTable("template_element_info_gcd", ForceInnoDb = true)]
    internal class MappingTemplateElementInfoGCD : NotifyPropertyChangedBase
    {
        protected void OnPropertyChanged(string propertyName)
        {
            ClassArgumentHelper.Instance.OnPropertyChangedWithStringValidater(propertyName, this);
            base.OnPropertyChanged(propertyName);
        }


        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public long Id { get; set; }
        #endregion

        #region Template
        [DbColumn("template_id", IsInverseMapping = true)]
        public MappingTemplateHeadGCD Template { get; set; }
        #endregion

        #region DebitElementId
        [DbColumn("element_id", Length = 2048)]
        public string ElementId { get; set; }
        #endregion

        #region ElementValue
        [DbColumn("element_value", Length = int.MaxValue / 4)]
        public string ElementValue { get; set; }
        #endregion

        #region PresentationTreeNode
        /// <summary>
        /// Assigned presentation tree node.
        /// </summary>
        internal IPresentationTreeNode PresentationTreeNode { get; set; }
        #endregion
    }
}
