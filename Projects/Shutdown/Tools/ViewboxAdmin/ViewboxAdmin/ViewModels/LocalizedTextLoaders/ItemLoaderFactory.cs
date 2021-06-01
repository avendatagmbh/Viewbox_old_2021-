using System;
using SystemDb;
using ViewboxAdmin.ViewModels.LocalizedTextLoaders;

namespace ViewboxAdmin.ViewModels
{
    public class ItemLoaderFactory : IItemLoaderFactory {

        public  IItemLoader Create(TablesWithLocalizedTextEnum table, ISystemDb sysdb) {
            switch (table)
            {
                case (TablesWithLocalizedTextEnum.Tableobjects):
                    return new TableLoader(sysdb);
                case (TablesWithLocalizedTextEnum.Optimizations):
                    return new OptimizationLoader(sysdb);
                case (TablesWithLocalizedTextEnum.Category):
                    return new CategoryLoader(sysdb);
                case (TablesWithLocalizedTextEnum.Parameter):
                    return new ParameterLoader(sysdb);
                case (TablesWithLocalizedTextEnum.Collections):
                    return new CollectionLoader(sysdb);
                case (TablesWithLocalizedTextEnum.OptimizationGroupTexts):
                    return new OptimizationGroupLoader(sysdb);
                case (TablesWithLocalizedTextEnum.Scheme):
                    return new SchemeLoader(sysdb);
                case (TablesWithLocalizedTextEnum.Property):
                    return new PropertyLoader(sysdb);
                case (TablesWithLocalizedTextEnum.Column):
                    return new ColumnLoader(sysdb);
                default:
                    throw new Exception("This type is still not implemented");
            }
        }
    }
}
