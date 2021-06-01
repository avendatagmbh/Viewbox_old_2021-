using SystemDb;
using ViewboxAdmin.ViewModels;

namespace ViewboxAdmin.Factories {
    public interface IOptimizationVM_Factory {
        IOptimization_ViewModel Create(IOptimization opt,IOptimization_ViewModel parentVM);
        IOptimization_ViewModel CreateRootElement(IOptimization opt);
    }
}