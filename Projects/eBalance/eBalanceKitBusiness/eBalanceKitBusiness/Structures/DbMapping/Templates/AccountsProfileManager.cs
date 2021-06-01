// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-04-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;

namespace eBalanceKitBusiness.Structures.DbMapping.Templates {

    /// <summary>
    /// class to manage all AccountInformationProfile items. Used in importing csv files.
    /// </summary>
    public static class AccountsProfileManager {

        private static readonly ObservableCollectionAsync<AccountsInformationProfile> _items =
            new ObservableCollectionAsync<AccountsInformationProfile>();

        public static ObservableCollectionAsync<AccountsInformationProfile> Items { get { return _items; } }

        public static readonly AccountsInformationProfile DefaultElement = new AccountsInformationProfile{
                Name = eBalanceKitResources.Localisation.ResourcesCommon.NoProfile,
                IsSelected = true
            };

        public static AccountsInformationProfile SelectedElement = DefaultElement;

        static AccountsProfileManager() {

            if (AppConfig.ConnectionManager == null) return;

            Items.Add(DefaultElement);

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.DbMapping.Load<AccountsInformationProfile>().ForEach(item => {
                    item.ReadTemplateXml();
                    item.IsSelected = false;
                    _items.Add(item);
                });
            }

        }
    }
}