using System.Collections.Generic;
using System.Collections.ObjectModel;
using DbSearchDatabase.TableRelated;

namespace DbSearchDatabase.Interfaces {
    public interface IDbQueries {
        //ObservableCollection<IDbQuery> Items { get; set; }
        void AddFromValidationDatabase(List<IDbQuery> items, List<ImportTable> tables);
        //void Load(List<IDbQuery> items);
        IEnumerable<IDbQuery> Load();
        void DeleteQuery(IDbQuery dbQuery);
    }
}