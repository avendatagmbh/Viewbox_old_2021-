using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using System.Windows.Input;
using ViewAssistant.Controls;
using DbAccess;
using SystemDb;
using System.Globalization;
using DbAccess.Structures;
using System.Text.RegularExpressions;

namespace ViewAssistantBusiness.Models
{
    public delegate void LocalizationTextsConfigurationEventHandler(object sender);

    public class LocalizationTextsConfigurationModel : NotifyPropertyChangedBase
    {
        private IViewboxLocalisable localisableModel;
        public IViewboxLocalisable LocalisableModel
        {
            get { return localisableModel; }

            set
            {
                localisableModel = value;
                OnPropertyChanged("LocalisableModel");
            }
        }

        private ObservableCollectionAsync<LocalisationTextItem> fullDescriptions;
        public ObservableCollectionAsync<LocalisationTextItem> FullDescriptions
        {
            get { return fullDescriptions; }
            set
            {
                fullDescriptions = value;
                OnPropertyChanged("FullDescriptions");
            }
        }

        private ILanguageCollection languages { get; set; }

        private DbConfig viewboxConnection { get; set; }

        public LocalizationTextsConfigurationModel(IViewboxLocalisable model, MainModel mainModel)
        {
            FullDescriptions = new ObservableCollectionAsync<LocalisationTextItem>();
            languages = mainModel.Languages;
            viewboxConnection = mainModel.CurrentProfile.ViewboxConnection;

            foreach (var lang in languages)
            {

                if (model.Info.Descriptions.Select(p => p.Key).Contains(lang.CountryCode))
                {
                    FullDescriptions.Add(new LocalisationTextItem(lang.CountryCode, model.Info.Descriptions[lang]));
                }
                else
                    FullDescriptions.Add(new LocalisationTextItem(lang.CountryCode, String.Empty));
            }

            LocalisableModel = model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        public void AutoCorrectTexts(IDatabase db, LocalisationTextsSettingsEventArgs settings)
        {
            string result = String.Empty;

            //Set default language.
            var defaultLanguage = FullDescriptions.FirstOrDefault(p => p.CountryCode.Equals(settings.CountryCode));

            if (settings.OnlyForTheEmptyOnes && !String.IsNullOrEmpty(defaultLanguage.Text)) //If 'only empty ones' checked but its not empty: return.
                return;

            result = LocalisableModel.Name;

            foreach (var item in settings.ReplaceFromTo.Where(p => !String.IsNullOrEmpty(p.FromText))) //Replace string parts to anothers.
            {
                result = result.Replace(item.FromText, item.ToText);
            }

            switch (settings.Keep) //What user is wants to keep.
            {
                case Enums.LocalisationKeep.OnlyAlphabeticalCharacters:
                    {
                        result = Regex.Replace(result, @"[\d-]", String.Empty);
                        break;
                    }
                case Enums.LocalisationKeep.OnlyAlphanumericCharacters:
                    {
                        result = Regex.Replace(result, "[^.0-9]", String.Empty);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            if (settings.UppercaseFirstLetters) //If uppercase first letters checked
            {
                result = new CultureInfo("en-US", false).TextInfo.ToTitleCase(result);
            }


            result = Regex.Replace(result, @"\s+", " "); //Clear multiple spaces.
            defaultLanguage.Text = result.Trim(); //Load back the new value.

            Save(db);
        }

        public ICommand UseForAllCommand { get { return new RelayCommand(UseForAll); } }

        private void UseForAll(object param)
        {
            foreach (var item in FullDescriptions)
            {
                item.Text = param.ToString();
            }
        }


        public ICommand SaveCommand { get { return new RelayCommand(SaveExecuted); } }

        /// <summary>
        /// Save the localisations.
        /// Always create a new connection.
        /// </summary>
        private void SaveExecuted(object sender)
        {
            using (var conn = viewboxConnection.CreateConnection())
            {
                Save(conn);
            }
        }

        public event LocalizationTextsConfigurationEventHandler SaveFinished;
        private void OnSaveFinished(object sender)
        {
            if (SaveFinished != null)
                SaveFinished(sender);
        }

        /// <summary>
        /// Save the localisations.
        /// Uses the parameter connection.
        /// </summary>
        /// <param name="db">Connection.</param>
        private void Save(IDatabase db)
        {
            try
            {
                foreach (var item in FullDescriptions)
                {
                    ILanguage language = languages.FirstOrDefault(p => p.CountryCode.Equals(item.CountryCode));

                    LocalisableModel.Info.SetDescription(item.Text, language);
                }

                if (LocalisableModel.Info.Type == DataObjectType.Table)
                {
                    db.Delete("table_texts", "ref_id=" + LocalisableModel.Info.Id);

                    foreach (var desc in LocalisableModel.Info.Descriptions.Where(p => !String.IsNullOrEmpty(p.Value) && !String.IsNullOrWhiteSpace(p.Value)))
                    {
                        db.DbMapping.Save(new SystemDb.Internal.TableObjectText
                        {
                            CountryCode = desc.Key,
                            RefId = LocalisableModel.Info.Id,
                            Text = desc.Value
                        });
                    }
                }
                else if (LocalisableModel.Info.Type == DataObjectType.Column)
                {
                    db.Delete("column_texts", "ref_id=" + LocalisableModel.Info.Id);

                    foreach (var desc in LocalisableModel.Info.Descriptions.Where(p => !String.IsNullOrEmpty(p.Value) && !String.IsNullOrWhiteSpace(p.Value)))
                    {
                        db.DbMapping.Save(new SystemDb.Internal.ColumnText
                        {
                            CountryCode = desc.Key,
                            RefId = LocalisableModel.Info.Id,
                            Text = desc.Value
                        });
                    }
                }

                OnSaveFinished(this);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    /// <summary>
    /// Helper class for LocalisationTextsConfigurationModel list items.
    /// </summary>
    public class LocalisationTextItem : NotifyPropertyChangedBase
    {
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

        private string text;

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }

        public LocalisationTextItem(string countryCode, string text)
        {
            this.countryCode = countryCode;
            this.text = text;
        }

    }
}
