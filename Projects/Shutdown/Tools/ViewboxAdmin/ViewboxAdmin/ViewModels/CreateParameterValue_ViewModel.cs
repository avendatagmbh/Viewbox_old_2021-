using System;
using System.Windows.Input;
using ViewboxAdmin.Command;
using ViewboxAdmin.ViewModels.Collections;
using ViewboxAdmin_ViewModel;

namespace ViewboxAdmin.ViewModels
{
    public class CreateParameterValue_ViewModel: NotifyBase
    {

        public CreateParameterValue_ViewModel(ICollectionModel collectionModel,ICollectionEdit_ViewModel parentViewModel) {
            this.CollectionModel = collectionModel;
            this.ParentViewModel = parentViewModel;
            this.AddNewCollection = new RelayCommand((o)=>CreateNewParameterValue());
        }

        public ICollectionEdit_ViewModel ParentViewModel { get; private set; }

        public ICommand AddNewCollection { get; set; }

        public event EventHandler<EventArgs> ParameterValuaCreationFinished;
        private void OnParameterValueCreationFinished() {
            if (ParameterValuaCreationFinished!=null) {
                ParameterValuaCreationFinished(this, EventArgs.Empty);
            }
        }

        private void CreateNewParameterValue() {
            ParentViewModel.CreateNewProfileValueCollection(CollectionModel);
            OnParameterValueCreationFinished();
        }

        #region CollectionModel
        private ICollectionModel _collectionmodel;

        public ICollectionModel CollectionModel {
            get { return _collectionmodel; }
            set {
                if (_collectionmodel != value) {
                    _collectionmodel = value;
                    OnPropertyChanged("CollectionModel");
                }
            }
        }
        #endregion CollectionModel
    }
}
