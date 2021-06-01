using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using SystemDb;
using System.Collections.ObjectModel;
using Utils;
using ViewboxAdmin.Command;
using ViewboxAdmin.Structures;


namespace ViewboxAdmin.ViewModels
{
    public class SystemDeleteViewModel : ViewModelBaseWithDebug
    {


        public SystemDeleteViewModel(ISystemDb systemDbModel, IDeleteSystemStrategy deletesystemstrategy) :base(deletesystemstrategy) {
            //Init members
            this._systemDbModel = systemDbModel;
            this._deletesystemstrategy = deletesystemstrategy;
            this._optimizations = new ObservableCollectionAsync<IOptimization>();
            //Init commands
            ClearDebugWindowCommand = new RelayCommand(o => this.DebugText = string.Empty);
            DeleteSystemCommand = new RelayCommand(o => this.OnDeleteItem(DeleteSystem,() => { ; }), a => !this.IsWorking);
            CancelCommand = new RelayCommand(o => { ; }, a => this.IsWorking);
            LoadSystems();
            
        }
        public event EventHandler<MessageBoxActions> DeleteSystemRequest;
        private void OnDeleteItem(Action onYes, Action onNo) {
            if (DeleteSystemRequest != null)
            {
                DeleteSystemRequest(this, new MessageBoxActions(onYes, onNo));
            }
        }

        private IDeleteSystemStrategy _deletesystemstrategy = null;
        public IDeleteSystemStrategy DeleteSystemStrategy { get { return _deletesystemstrategy; } set { _deletesystemstrategy = value; } }

        private ObservableCollectionAsync<IOptimization> _optimizations = null;
        public ObservableCollectionAsync<IOptimization> Optimizations { get { return _optimizations; } set { _optimizations = value; } }

        private readonly ISystemDb _systemDbModel;
        public ISystemDb SystemDBModel { get { return _systemDbModel; } }

        public ICommand DeleteSystemCommand { get; set; }
        public ICommand ClearDebugWindowCommand { get; set; }
        public ICommand CancelCommand { get; set; }


        public void LoadSystems() {
            Optimizations.Clear();
            foreach (var opt in _systemDbModel.Optimizations)
            {
                if (opt.Group.Type==OptimizationType.System)
                Optimizations.Add(opt);
            }
            try {
                Selected = Optimizations[0];
            }
            catch(Exception e) {
                Selected = null;
            }
            AddDebugInfo("Systems have been loaded successfully");
        }

        private int _selectedsystemId;
        public int SelectedsystemId { get { return _selectedsystemId; } set { _selectedsystemId = value; } }


        private IOptimization _selected;
        public IOptimization Selected {
            get { return _selected; }
            set {
                _selected = value;
                OnPropertyChanged("Selected");
            }
        }


        private void DeleteSystem() {
            if (Selected == null) return;
            if (!IsWorking)
            {
                Task.Factory.StartNew(() => DeleteSystemStrategy.DeleteSystemFromMetaDataBase(Selected.Id));
            }
        }

        protected override void OnDeleteProcessCompleted(object sender, EventArgs e) {
            base.OnDeleteProcessCompleted(sender, e);
            LoadSystems();
        }
    }
}
