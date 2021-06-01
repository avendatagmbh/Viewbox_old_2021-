using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Utils;
using Utils.Commands;
using WpfControlsSample.Structures.ListDemo;

namespace WpfControlsSample.Models {
    public class ListDemoModel : NotifyPropertyChangedBase{
        public ListDemoModel() {
            _persons = new ObservableCollectionAsync<Person>();
            foreach (var person in PersonProvider.GetPersons())
                _persons.Add(person);
            _addNewPersonCommand = new DelegateCommand((obj) => true, AddNewPerson);
            _sortCommand = new DelegateCommand(obj => true, SortHandler);
            _deletePersonCommand = new DelegateCommand(o => _personsView.CurrentItem != null, DeleteSelectedPerson);
        }

        #region Persons 
        private readonly ObservableCollectionAsync<Person> _persons;

        private ICollectionView _personsView;

        public ICollectionView Persons {
            get {
                if (_personsView == null) {
                    var source = new CollectionViewSource {Source = _persons};
                    _personsView = source.View;
                    _personsView.CurrentChanged += (sender, args) => _deletePersonCommand.RaiseCanExecuteChanged();
                }
                return _personsView;
            }
        }

        #endregion Persons

        #region AddNewPerson
        private readonly ICommand _addNewPersonCommand;

        public ICommand AddNewPersonCommand {
            get { return _addNewPersonCommand; }
        }


        private void AddNewPerson(object unused) {
            _persons.Add(new Person("<Name>",0,false));
        }
        #endregion AddNewPerson

        #region DeletePerson
        private DelegateCommand _deletePersonCommand;
        public ICommand DeletePersonCommand {
            get { return _deletePersonCommand; }
        }

        private void DeleteSelectedPerson(object o) {
            if (_personsView.CurrentItem == null) return;
            _persons.Remove(_personsView.CurrentItem as Person);
            _personsView.MoveCurrentTo(null);
        }
        #endregion DeletePerson

        #region Filter

        private int _filterAgeFrom;
        private int _filterAgeTo;
        public int FilterAgeFrom {
            get { return _filterAgeFrom; }
            set { 
                _filterAgeFrom = value;
                UpdateFilter();
                OnPropertyChanged("FilterAgeFrom");
            }
        }

        private void UpdateFilter() {
            if(!FilterActivated) {
                _personsView.Filter = null;
            }else {
                _personsView.Filter = p => ((Person) p).Age >= FilterAgeFrom && ((Person) p).Age <= FilterAgeTo;
            }
        }

        public int FilterAgeTo {
            get { return _filterAgeTo; }
            set {
                _filterAgeTo = value;
                UpdateFilter();
                OnPropertyChanged("FilterAgeTo");
            }
        }

        private bool _filterActivated;
        public bool FilterActivated {
            get { return _filterActivated; }
            set {
                if (_filterActivated != value) {
                    _filterActivated = value;
                    UpdateFilter();
                    OnPropertyChanged("FilterActivated");
                }
            }
        }
        #endregion Filter

        #region Sorting
        private ICommand _sortCommand;

        public ICommand SortCommand {
            get { return _sortCommand; }
        }




        private void SortHandler(object radioButtonObj) {
            RadioButton radioButton = radioButtonObj as RadioButton;
            _personsView.SortDescriptions.Clear();
            _personsView.SortDescriptions.Add(new SortDescription(radioButton.Tag as string, ListSortDirection.Ascending));
        }
        #endregion Sorting
    }
}
