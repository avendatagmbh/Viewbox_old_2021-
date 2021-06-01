using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbSearchBase.Interfaces;

namespace DbSearchLogic.SearchCore.Keys
{
    public interface IDisplayDbKey {
        int Id { get; }
        string Label { get; }
        string DisplayString { get; }
        bool IsInitialized { get; }
        void Initialize();
    }

    public interface IDisplayPrimaryKey : IDisplayDbKey {
        string TableName { get; }
        List<IColumn> Columns { get; }
    }

    public interface IDisplayForeignKey : IDisplayDbKey {
        string ForeignKeyTableName { get; }
        string PrimaryKeyTableName { get; }
        List<IColumn> ForeignKeyColumns { get; }
        List<IColumn> PrimaryKeyColumns { get; }
    }
}
