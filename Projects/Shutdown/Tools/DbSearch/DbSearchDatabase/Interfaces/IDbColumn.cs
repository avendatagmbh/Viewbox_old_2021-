using DbAccess.Structures;
using DbSearchBase.Enums;
using DbSearchDatabase.Structures;

namespace DbSearchDatabase.Interfaces {

    public interface IDbColumn {
        DbColumnInfo DbColumnInfo { get; }
        bool IsVisible { get; set; }
        bool IsSelected { get; set; }
        string Comment { get; set; }
        int DisplayIndex { get; set; }
        string Name { get; set; }
        string OriginalColumnName { get; set; }
        DbConfigSearchParams DbConfigSearchParams { get; }
        bool IsUsedInSearch { get; set; }
        bool IsUserDefined { get; set; }
        string Rules { get; set; }
    }
}