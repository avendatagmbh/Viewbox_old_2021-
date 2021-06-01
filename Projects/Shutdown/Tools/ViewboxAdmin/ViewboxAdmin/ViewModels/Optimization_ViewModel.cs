using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using SystemDb;
using ViewboxAdmin.Command;
using ViewboxAdmin.CustomEventArgs;
using ViewboxAdmin.Structures;
using ViewboxAdmin_ViewModel;

namespace ViewboxAdmin.ViewModels
{
    public class Optimization_ViewModel: NotifyBase, IOptimization_ViewModel {


        public Optimization_ViewModel(IOptimization optimization) :this(optimization,null) {
        }

        public Optimization_ViewModel(IOptimization optimization, IOptimization_ViewModel parentoptimization) {
            this.Optimization = optimization;
            this.Parent = parentoptimization;
            this.Name = optimization.Value;
            this.Children = new ObservableCollection<IOptimization_ViewModel>();

            foreach (var opt in optimization.Children) {
                IOptimization_ViewModel optVM = new Optimization_ViewModel(opt, this);
                optVM.DeleteOptimization += (o, e) => OnDeleteOptimization(e.Optimization);
                Children.Add(optVM);
            }
            DeleteOptimizationCommand = new RelayCommand((o)=>OnDeleteOptimization(Optimization));
        }


        public ICommand DeleteOptimizationCommand { get; set; }

        public event EventHandler<OptimizationEventArgs> DeleteOptimization; 
        
        public void OnDeleteOptimization(IOptimization opt) {
            if (DeleteOptimization != null)
                DeleteOptimization(this, new OptimizationEventArgs(opt));
        }

        #region Name
        private string _name;

        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        #endregion Name

        #region Children
        private ObservableCollection<IOptimization_ViewModel> _children;
        public ObservableCollection<IOptimization_ViewModel> Children {
            get { return _children; }
            set {
                if (_children != value) {
                    _children = value;
                    OnPropertyChanged("Children");
                }
            }
        }
        #endregion Children

        #region Parent
        private IOptimization_ViewModel _parent;

        public IOptimization_ViewModel Parent {
            get { return _parent; }
            set {
                if (_parent != value) {
                    _parent = value;
                    OnPropertyChanged("Parent");
                }
            }
        }
        #endregion Parent

        #region Optimization
        private IOptimization _optimization;

        public IOptimization Optimization {
            get { return _optimization; }
            set {
                if (_optimization != value) {
                    _optimization = value;
                    OnPropertyChanged("Optimization");
                }
            }
        }
        #endregion Optimization

        #region IsExpanded
        private bool _isexpanded;

        public bool IsExpanded {
            get { return _isexpanded; }
            set {
                if (_isexpanded != value) {
                    _isexpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }
        #endregion IsExpanded

        #region IsSelected
        private bool _isselected;

        public bool IsSelected {
            get { return _isselected; }
            set {
                if (_isselected != value) {
                    _isselected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        #endregion IsSelected

    }
}
