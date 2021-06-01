using SystemDb;

namespace ViewboxAdmin.ViewModels {
    public interface IItemLoaderFactory {
        IItemLoader Create(TablesWithLocalizedTextEnum table, ISystemDb sysdb);
    }
}