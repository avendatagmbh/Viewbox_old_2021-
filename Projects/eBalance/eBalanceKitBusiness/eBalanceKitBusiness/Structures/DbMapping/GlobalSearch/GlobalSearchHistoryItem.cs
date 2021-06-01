using DbAccess;

namespace eBalanceKitBusiness.Structures.DbMapping.GlobalSearch {

    [DbTable("global_search_history", Description = "Stores the history of searches per user", ForceInnoDb = true)]
    public class GlobalSearchHistoryItem {
        public GlobalSearchHistoryItem() {
            RawHistoryList = string.Empty;
        }

        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }

        [DbColumn("user_id", AllowDbNull = false)]
        public int UserId { get; set; }

        [DbColumn("history_list", AllowDbNull = false, Length = 65536)]
        public string RawHistoryList { get; set; }

    }
}
