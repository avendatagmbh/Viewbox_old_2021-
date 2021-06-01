namespace eBalanceKitBusiness.Algorithm.Interfaces {
    public interface IListEntry {
        string Name { get; set; }
        string Number { get; set; }
        string SortIndex { get; set; }
        bool IsHidden { get; set; }
    }
}