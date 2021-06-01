using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using ViewboxAdmin.ViewModels;

namespace ViewboxAdmin.Factories
{
    public class OptimizationVM_Factory : IOptimizationVM_Factory {
        public IOptimization_ViewModel Create(IOptimization opt, IOptimization_ViewModel parentVM) {

            IOptimization_ViewModel optVM = new Optimization_ViewModel(opt);
            //optVM.DeleteOptimization += parentVM.OnDeleteOptimization(optVM.Optimization);
            return optVM;
        }

        public IOptimization_ViewModel CreateRootElement(IOptimization opt) {
            IOptimization_ViewModel optVM = new Optimization_ViewModel(opt);
            return optVM;
        }
    }
}
