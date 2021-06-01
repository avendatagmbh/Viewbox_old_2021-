using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using SystemDb;
using ViewboxAdmin.Command;
using ViewboxAdmin.CustomEventArgs;
using ViewboxAdmin.ViewModels.Collections;
using ViewboxAdmin_ViewModel;

namespace ViewboxAdmin.ViewModels
{
    public class Parameters_ViewModel : NotifyBase, IParameters_ViewModel
    {
        #region Constructor
        public Parameters_ViewModel(ObservableCollection<IParameterModel> parameters, ObservableCollection<LanguageTextModel> languages, ICollectionsUnitOfWork unitOfWork ) {
            this.Parameters = parameters;
            this.Languages = languages;
            this.UnitOfWork = unitOfWork;
            CommitCommand = new RelayCommand((o)=>Commit());
        }
        #endregion Constructor

        #region Properties
        public ICollectionsUnitOfWork UnitOfWork { get; private set; }

        public ICommand CommitCommand { get; set; }

        public ObservableCollection<IParameterModel> Parameters { get; set; }

        public ObservableCollection<LanguageTextModel> Languages { get; private set; }

        #region SelectedParameter
        private IParameterModel _selectedparameter;

        public IParameterModel SelectedParameter {
            get { return _selectedparameter; }
            set {
                if (_selectedparameter != value) {
                    _selectedparameter = value;
                    OnPropertyChanged("SelectedParameter");
                    OnDataContextChanged();
                }
            }
        }
        #endregion SelectedParameter
        #endregion Properties

        #region Public Methods

        public void Remove(ICollectionModel colletionModel) {
            UnitOfWork.MarkAsDeleted(colletionModel);
        }

        public void Edited(ICollectionModel collectionModel) {
            UnitOfWork.MarkAsDirty(collectionModel);
        }

        public void CreateNewCollection(ICollectionModel collectionModel) {
            UnitOfWork.MarkAsNew(Tuple.Create(collectionModel, SelectedParameter));
        }

        private void Commit() {
            UnitOfWork.Commit();
        }

        #endregion Public Methods

        #region Events
        public event EventHandler<DataContextChangeEventArg<CollectionEdit_ViewModel>> DataContextChange;
        private void OnDataContextChanged() {
            if (DataContextChange != null)
            {
                DataContextChange(this, new DataContextChangeEventArg<CollectionEdit_ViewModel>(GetCollectionEditViewModel()));
            }
        }
        #endregion Events

        #region Private methods
        private CollectionEdit_ViewModel GetCollectionEditViewModel() {
            return new CollectionEdit_ViewModel(SelectedParameter.ParameterValues, Languages, this);
        }
        #endregion Private methods
    }
}
