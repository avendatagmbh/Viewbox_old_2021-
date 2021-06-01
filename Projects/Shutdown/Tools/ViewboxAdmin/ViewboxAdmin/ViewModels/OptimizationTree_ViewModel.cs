using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using SystemDb;
using ViewboxAdmin.Command;
using ViewboxAdmin.CustomEventArgs;
using ViewboxAdmin.Factories;
using ViewboxAdmin.Structures;
using ViewboxAdmin_ViewModel;

namespace ViewboxAdmin.ViewModels
{
    public class OptimizationTree_ViewModel: NotifyBase
    {
        public OptimizationTree_ViewModel(ISystemDb systemDb, IOptimizationVM_Factory optVMFactory) {
            this.SystemDb = systemDb;
            this.OptimizationTreeCreator = optVMFactory;
            FirstGeneration = new ObservableCollection<IOptimization_ViewModel>();
            LoadOptimizations();
        }


        public event EventHandler<ErrorEventArgs> ExceptionOccured;
        private void OnException(Exception e) {
            if (ExceptionOccured != null) {
                ExceptionOccured(this,new ErrorEventArgs(e));
            }
        }

        public event EventHandler<MessageBoxActions> DeleteOptimizationRequest;
        private void OnDeleteOptimizationRequest(Action onYes, Action onNo) {
            if (DeleteOptimizationRequest != null)
            {
                DeleteOptimizationRequest(this, new MessageBoxActions(onYes, onNo));
            }
        }

        private void LoadOptimizations() {
            FirstGeneration.Clear();
            foreach (var opt in SystemDb.Optimizations)
            {
                if (opt.Group.Type == OptimizationType.System)
                {
                    IOptimization_ViewModel optVM = OptimizationTreeCreator.CreateRootElement(opt);
                    optVM.DeleteOptimization += DeleteOptimization;
                    FirstGeneration.Add(optVM);
                }
            }
        }

        private void DeleteOptimization(object sender, OptimizationEventArgs optimizationEventArgs) {
            OnDeleteOptimizationRequest(() => DeleteOptimizationTree(optimizationEventArgs.Optimization), () => { ; });
        }

        private void DeleteOptimizationTree(IOptimization opt) {
            try {
                SystemDb.RemoveOptimizationFromAllTables(opt);
            }
            catch(Exception e) {
                OnException(e);
            }

            LoadOptimizations();

        }


        public IOptimizationVM_Factory OptimizationTreeCreator { get; set; }
        public ISystemDb SystemDb { get; private set; }

        public ObservableCollection<IOptimization_ViewModel> FirstGeneration { get; set; }

        
        

       
    }
}
