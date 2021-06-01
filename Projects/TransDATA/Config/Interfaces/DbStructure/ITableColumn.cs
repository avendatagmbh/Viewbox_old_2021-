// (C)opyright 2011 AvenDATA GmbH
// ----------------------------------------
// author: Mirko Dibbert
// since : 2011-08-27
using Config.DbStructure;

namespace Config.Interfaces.DbStructure {
    /// <summary>
    /// Interface for a column structure.
    /// </summary>
    public interface ITableColumn : IColumn{
        IProfile Profile { get; }
        ITable Table { get; }
        void Save();
    }
}