using System.Collections.ObjectModel;
using System.ComponentModel;
using SystemDb;
using ViewboxAdmin_ViewModel;

namespace ViewboxAdmin.ViewModels
{
    public class EditText_ViewModel :NotifyBase
    {
        

        public EditText_ViewModel(ISystemDb systemDb, IItemLoaderFactory itemLoaderFactory) {
            this.SystemDb = systemDb;
            this.ItemLoaderAbstractFactory = itemLoaderFactory;
            Items = new ObservableCollection<IItemWrapperStructure>();
            Languages = new ObservableCollection<ILanguage>();
            this.TablesWithLocalizedText = TablesWithLocalizedTextEnum.Optimizations;
            InitLanguages();
            SelectedLanguage = systemDb.DefaultLanguage;
        }

        public IItemLoaderFactory ItemLoaderAbstractFactory { get; private set;}

        public IItemLoader ItemloaderState { get; private set; }

        public ISystemDb SystemDb { get; private set; }

        private ILanguage _selectedlanguage = null;
        public ILanguage SelectedLanguage { 
            get {
            return _selectedlanguage;
        }
            set {
                _selectedlanguage = value;
                OnPropertyChanged("SelectedLanguage");//reload tables
                ReLoadTables();
            } }

        public ObservableCollection<IItemWrapperStructure> Items { get; set; }

        public ObservableCollection<ILanguage> Languages { get; set; }


        private TablesWithLocalizedTextEnum _tablesWithLocalizedText;
        public TablesWithLocalizedTextEnum TablesWithLocalizedText {
            get { return _tablesWithLocalizedText; } 
            set { _tablesWithLocalizedText = value;
            OnPropertyChanged("TablesWithLocalizedText");
                //change state here...
                ItemloaderState = ItemLoaderAbstractFactory.Create(value,this.SystemDb);
                ReLoadTables();
            }
        }

        private void InitLanguages() {
            foreach (var language in this.SystemDb.Languages)
            {
                Languages.Add(language);
            }
        }

        private void ReLoadTables() {
            Items.Clear();
            ItemloaderState.InitItems(Items, SelectedLanguage);
        }
    }



    

    
}
