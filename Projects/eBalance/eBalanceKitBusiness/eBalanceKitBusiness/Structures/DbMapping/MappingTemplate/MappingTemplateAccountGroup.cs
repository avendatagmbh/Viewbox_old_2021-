// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-08-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using DbAccess;
using eBalanceKitBusiness.MappingTemplate;
using Utils;

namespace eBalanceKitBusiness.Structures.DbMapping.MappingTemplate
{
    [DbTable("template_account_groups", ForceInnoDb = true)]
    public class MappingTemplateAccountGroup : NotifyPropertyChangedBase
    {
        protected void OnPropertyChanged(string propertyName)
        {
            ClassArgumentHelper.Instance.OnPropertyChangedWithStringValidater(propertyName, this);
            base.OnPropertyChanged(propertyName);
        }


        public MappingTemplateAccountGroup() {
            AccountGroupInfo = new AccountGroupInfo();
        }

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }
        #endregion

        // DEVNOTE: Because of that column the cache of the account grouping can't be saved. If we want to save the input values of the not
        // applied account grouping we have to remove this column.
        #region Head
        [DbColumn("head_id", IsInverseMapping = true)]
        public MappingTemplateHead Head { get; set; }
        #endregion

        #region AccountGroupParentRaw
        [DbColumn("account_group_parent", Length = 1056)]
        public string AccountGroupParentRaw { get; set; }
        #endregion

        #region AccountGroupChildrenRaw
        [DbColumn("account_group_children", Length = int.MaxValue)]
        public string AccountGroupChildrenRaw { get; set; }
        #endregion

        public void LoadAccountGroup() {
            if (!_isLoaded) {
                _isLoaded = true;
                AccountGroupInfo = new AccountGroupInfo(AccountGroupParentRaw, AccountGroupChildrenRaw);
            }
        }

        private bool _isLoaded;

        public AccountGroupInfo AccountGroupInfo { get; set; }

        public void Save() { 
            AccountGroupParentRaw = AccountGroupInfo.GetParentRaw();
            AccountGroupChildrenRaw = AccountGroupInfo.GetChildrenRaw();
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.DbMapping.Save(this);
            }
        }
    }
}
