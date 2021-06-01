// (C)opyright 2011 AvenDATA GmbH
// ----------------------------------------
// author: Mirko Dibbert
// since : 2011-08-27
using System.Collections.Generic;

namespace Config.Interfaces.DbStructure {
    /// <summary>
    /// Interface for a table structure.
    /// </summary>
    public interface ITable : ITransferEntity {
        //long Id { get; set; }
        IProfile Profile { get; }        
        string Type { get; set; }
        List<string> FileNames { get; }
        //string Filter { get; set; }
        
        ITableColumn CreateColumn();
        void AddColumn(ITableColumn tableColumn);
    }
}