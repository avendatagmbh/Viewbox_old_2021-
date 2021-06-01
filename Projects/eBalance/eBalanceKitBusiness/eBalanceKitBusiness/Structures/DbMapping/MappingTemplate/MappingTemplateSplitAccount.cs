// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-08-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using DbAccess;
using Utils;
using eBalanceKitBusiness.MappingTemplate;

namespace eBalanceKitBusiness.Structures.DbMapping.MappingTemplate
{
    [DbTable("template_split_accounts", ForceInnoDb = true)]
    public class MappingTemplateSplitAccount : NotifyPropertyChangedBase 
    {

        protected void OnPropertyChanged(string propertyName)
        {
            ClassArgumentHelper.Instance.OnPropertyChangedWithStringValidater(propertyName, this);
            base.OnPropertyChanged(propertyName);
        }

        public MappingTemplateSplitAccount() {
            SplitAccountGroupInfo = new SplitAccountGroupInfo();
        }

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }
        #endregion

        // DEVNOTE: Because of that column the cache of the account splitting can't be saved. If we want to save the input values of the not
        // applied account splitting we have to remove this column.
        #region Head
        [DbColumn("head_id", IsInverseMapping = true)]
        public MappingTemplateHead Head { get; set; }
        #endregion

        #region SplitAccountParentRaw
        [DbColumn("split_account_parent", Length = 1056)]
        public string SplitAccountParentRaw { get; set; }
        #endregion

        #region SplitAccountChildrenRaw
        [DbColumn("split_account_children", Length = int.MaxValue)]
        public string SplitAccountChildrenRaw { get; set; }
        #endregion
        
        public void LoadAccountGroup() {
            if (!_isLoaded) {
                _isLoaded = true;
                SplitAccountGroupInfo = new SplitAccountGroupInfo(SplitAccountParentRaw, SplitAccountChildrenRaw);
            }
        }

        private bool _isLoaded;

        public SplitAccountGroupInfo SplitAccountGroupInfo { get; set; }

        public void Save() { 
            SplitAccountParentRaw = SplitAccountGroupInfo.GetParentRaw();
            SplitAccountChildrenRaw = SplitAccountGroupInfo.GetChildrenRaw();
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.DbMapping.Save(this);
            }
        }
    }
}
