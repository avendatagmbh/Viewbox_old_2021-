using SystemDb;

namespace ViewboxAdmin.ViewModels
{
    public interface IDeleteSystemStrategy : IStatusReporter {
        void DeleteSystemFromMetaDataBase(int optimizationid);
        ISystemDb SystemDb { get; }
    }
}
