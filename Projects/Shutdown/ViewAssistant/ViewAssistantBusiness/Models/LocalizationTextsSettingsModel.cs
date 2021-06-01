using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewAssistantBusiness.Enums;
using Utils;
using System.Windows.Input;
using ViewAssistant.Controls;
using Base.Localisation;

namespace ViewAssistantBusiness.Models
{
    public delegate void LocalisationTextsSettingsEventHandler(object sender, LocalisationTextsSettingsEventArgs e);

    public class LocalizationTextsSettingsModel : NotifyPropertyChangedBase
    {
        public event LocalisationTextsSettingsEventHandler OnSettingsAccepted;

        private List<string> countryCodes;
        public List<string>  CountryCodes
        {
            get { return countryCodes; }
            set
            {
                countryCodes = value;
                OnPropertyChanged("CountryCodes");
            }
        }

        private string countryCode;
        public string CountryCode
        {
            get { return countryCode; }
            set
            {
                countryCode = value;
                OnPropertyChanged("CountryCode");
            }
        }

        private bool onlyForTheEmptyOnes;
        public bool OnlyForTheEmptyOnes
        {
            get { return onlyForTheEmptyOnes; }
            set
            {
                onlyForTheEmptyOnes = value;
                OnPropertyChanged("OnlyForTheEmptyOnes");
            }
        }

        private bool uppercaseFirstLetters;
        public bool UppercaseFirstLetters
        {
            get { return uppercaseFirstLetters; }
            set
            {
                uppercaseFirstLetters = value;
                OnPropertyChanged("UppercaseFirstLetters");
            }
        }

        public List<LocalisationKeep> Keeps
        {
            get;
            private set;
        }

        private LocalisationKeep keep;
        public LocalisationKeep Keep
        {
            get { return keep; }
            set
            {
                keep = value;
                OnPropertyChanged("Keep");
            }
        }

        private ObservableCollectionAsync<FromToTextDictionaryItem> replaceFromTo;
        public ObservableCollectionAsync<FromToTextDictionaryItem> ReplaceFromTo
        {
            get { return replaceFromTo; }
            set
            {
                replaceFromTo = value;
                OnPropertyChanged("ReplaceFromTo");
            }
            
        }

        private bool onTableNames;
        public bool OnTableNames
        {
            get { return onTableNames; }
            set
            {
                onTableNames = value;
                OnPropertyChanged("OnTableNames");
            }

        }

        private bool onColumnNames;
        public bool OnColumnNames
        {
            get { return onColumnNames; }
            set
            {
                onColumnNames = value;
                OnPropertyChanged("OnColumnNames");
            }

        }

        public LocalizationTextsSettingsModel(MainModel mainModel)
        {
            //Set defaults.
            CountryCodes = new List<string>() { ResourcesCommon.All };
            CountryCodes.AddRange(mainModel.Languages.Select(lang => lang.CountryCode));
            CountryCode = CountryCodes.FirstOrDefault();
            OnlyForTheEmptyOnes = true;
            UppercaseFirstLetters = true;
            Keeps = new List<LocalisationKeep>() { LocalisationKeep.Everything, LocalisationKeep.OnlyAlphabeticalCharacters, LocalisationKeep.OnlyAlphanumericCharacters };
            Keep = LocalisationKeep.Everything;
            OnTableNames = true;
            OnColumnNames = true;
            ReplaceFromTo = new ObservableCollectionAsync<FromToTextDictionaryItem>() { new FromToTextDictionaryItem("_", " ") };
        }

        public ICommand AcceptCommand { get { return new RelayCommand(Accept); } }

        private void Accept(object sender)
        {
            OnSettingsAccepted(this, new LocalisationTextsSettingsEventArgs()
                                         {
                                             CountryCode = this.CountryCode,
                                             Keep = this.Keep,
                                             OnColumnNames = this.OnColumnNames,
                                             OnlyForTheEmptyOnes = this.OnlyForTheEmptyOnes,
                                             OnTableNames = this.OnTableNames,
                                             ReplaceFromTo = this.ReplaceFromTo,
                                             UppercaseFirstLetters = this.UppercaseFirstLetters
                                         });
        }

        public ICommand DeleteCommand { get { return new RelayCommand(Delete); } }

        private void Delete(object sender)
        {
            if (sender != null && sender is FromToTextDictionaryItem)
            {
                ReplaceFromTo.Remove(sender as FromToTextDictionaryItem);
            }
        }

        public ICommand AddCommand { get { return new RelayCommand(Add); } }

        private void Add(object sender)
        {
            ReplaceFromTo.Add(new FromToTextDictionaryItem(String.Empty, String.Empty));
        }
    }

    public class LocalisationTextsSettingsEventArgs : EventArgs
    {
        public string CountryCode {get; set;}

        public bool OnlyForTheEmptyOnes { get; set; }

        public bool UppercaseFirstLetters { get; set; }

        public LocalisationKeep Keep { get; set; }

        public ObservableCollectionAsync<FromToTextDictionaryItem> ReplaceFromTo { get; set; }

        public bool OnTableNames { get; set; }

        public bool OnColumnNames { get; set; }
    }

    /// <summary>
    /// Helper class
    /// </summary>
    public class FromToTextDictionaryItem : NotifyPropertyChangedBase
    {
        private string fromText;

        public string FromText
        {
            get { return fromText; }
            set
            {
                fromText = value;
                OnPropertyChanged("FromText");
            }
        }

        private string toText;

        public string ToText
        {
            get { return toText; }
            set
            {
                toText = value;
                OnPropertyChanged("ToText");
            }
        }

        public FromToTextDictionaryItem(string fromText, string toText)
        {
            this.FromText = fromText;
            this.ToText = toText;
        }
    }
}
