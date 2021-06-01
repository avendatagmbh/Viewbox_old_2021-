using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using SystemDb;
using ViewboxAdmin.Command;
using ViewboxAdmin.CustomEventArgs;
using ViewboxAdmin.Structures;
using ViewboxAdmin.ViewModels.Collections;
using ViewboxAdmin_ViewModel;

namespace ViewboxAdmin.ViewModels
{
    public class CollectionEdit_ViewModel : NotifyBase, ICollectionEdit_ViewModel {
        public CollectionEdit_ViewModel( ObservableCollection<ICollectionModel> collectionModels, ObservableCollection<LanguageTextModel> availableLanguages, IParameters_ViewModel parentViewModel) {
            //inject dependencies
            this.LocalizedTexts = availableLanguages;
            this.Collections = collectionModels;
            this.ParentViewModel = parentViewModel;
            //command for delete a parameter value
            DeleteCollectionRequest = new RelayCommand(o=>AskUserIfReallyWantToDelete(),o=>IsAnyItemSelected());
            //command for create a new parameter value
            NewCollectionRequest = new RelayCommand(o=>OnUserNewCollectionRequest());
        }

        #region Properties
        public ObservableCollection<LanguageTextModel> LocalizedTexts { get; private set; }
        public ObservableCollection<ICollectionModel> Collections { get; private set; }
        public IParameters_ViewModel ParentViewModel { get; private set; }
        public ICommand DeleteCollectionRequest { get; set; }
        public ICommand NewCollectionRequest { get; set; }
        #region SelecedItem
        private ICollectionModel _selectedItem;

        public ICollectionModel SelectedItem {
            get { return _selectedItem; }
            set {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    if (value != null && value.Texts !=null)
                    {
                        value.PropertyChanged -= ParameterValueNameChanged;
                        value.PropertyChanged += ParameterValueNameChanged;
                        value.Texts.CollectionChanged -= Texts_CollectionChanged;
                        value.Texts.CollectionChanged += Texts_CollectionChanged;
                    }
                    OnPropertyChanged("colletionModel");
                }
            }
        }

        private void Texts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            ParentViewModel.Edited(SelectedItem);
        }

        private void ParameterValueNameChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Value") {
                ParentViewModel.Edited(SelectedItem);
            }
        }

        #endregion SelecedItem
        #endregion Properties

        #region Events
        public EventHandler<MessageBoxActions> UserApproveRequest;
        private void OnUserApproveRequest(Action onYes, Action onNo) {
            if(UserApproveRequest!=null) {
                UserApproveRequest(this, new MessageBoxActions(onYes,onNo));
            }
        }

        public EventHandler<DataContextChangeEventArg<CreateParameterValue_ViewModel>> UserNewCollectionRequest;
        private void OnUserNewCollectionRequest() {
            if(UserNewCollectionRequest!=null) {
                UserNewCollectionRequest(this,new DataContextChangeEventArg<CreateParameterValue_ViewModel>(GetNewParameterViewModel()));
            }
        }
        #endregion Events

        #region Public methods

        public void Edited(LanguageTextModel SelectedItem) {
            ParentViewModel.Edited(this.SelectedItem);
        }

        public void CreateNewProfileValueCollection(ICollectionModel o) {
            Collections.Add(o);
            ParentViewModel.CreateNewCollection(o);
        }

        #endregion Public methods

        #region Private methods
        private void AskUserIfReallyWantToDelete() { OnUserApproveRequest(DeleteCollection, () => { }); }
        private bool IsAnyItemSelected() { return SelectedItem != null; }
        private ObservableCollection<LanguageTextModel> CreateAnEmptyLanguageTextCollection() {
            var o = new ObservableCollection<LanguageTextModel>();
            foreach (var languageTextModel in LocalizedTexts)
            {
                //creating languagetextmodels with empty strings...
                o.Add(new LanguageTextModel(languageTextModel.Language));
            }
            return o;
        }
        private void DeleteCollection() {
            ParentViewModel.Remove(SelectedItem);
            Collections.Remove(SelectedItem);

        }
        private CreateParameterValue_ViewModel GetNewParameterViewModel() {
            var emptytexts = CreateAnEmptyLanguageTextCollection();
            return new CreateParameterValue_ViewModel(new CollectionModel(emptytexts), this);
        }
        #endregion Private methods
    }
}
