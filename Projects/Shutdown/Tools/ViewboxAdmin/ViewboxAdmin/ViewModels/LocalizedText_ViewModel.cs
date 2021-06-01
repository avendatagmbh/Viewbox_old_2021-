using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ViewboxAdmin.ViewModels.Collections;
using ViewboxAdmin_ViewModel;

namespace ViewboxAdmin.ViewModels
{
    public class LocalizedText_ViewModel : NotifyBase
    {
        public LocalizedText_ViewModel(ObservableCollection<LanguageTextModel> LanguageList,CollectionEdit_ViewModel parentVM) {
            this.Texts = LanguageList;
            this.ParentVM = parentVM;
        }

        public CollectionEdit_ViewModel ParentVM { get; set; }
        public ObservableCollection<LanguageTextModel> Texts { get; set; }

        #region SelectedItem
        private LanguageTextModel _selectedItem;

        void ParameterValueNameChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Text") {
                ParentVM.Edited(SelectedItem);
            }
        }

        public LanguageTextModel SelectedItem {
            get { return _selectedItem; }
            set {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    if (value != null)
                    {
                        value.PropertyChanged -= ParameterValueNameChanged;
                        value.PropertyChanged += ParameterValueNameChanged;
                    }
                    OnPropertyChanged("SelectedItem");

                }
            }
        }
        #endregion SelectedItem
    }
}
