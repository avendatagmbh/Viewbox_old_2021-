using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SystemDb;
using ViewboxAdmin_ViewModel;

namespace ViewboxAdmin.ViewModels.Collections
{
    public class LanguageTextModel : NotifyBase
    {

        

        

       

        #region Language
        private ILanguage _language;

        public ILanguage Language {
            get { return _language; }
            set {
                if (_language != value) {
                    _language = value;
                    OnPropertyChanged("Language");
                }
            }
        }

        #endregion Language

        #region Text
        private string _text;

        public string Text {
            get { return _text; }
            set {
                if (_text != value) {
                    _text = value;
                    OnPropertyChanged("Text");
                    
                    //call systemdb method here
                }
            }
        }
        #endregion Text
        
        

        public LanguageTextModel(ILocalizedTextCollection loctextcoll, ILanguage language) {
            this.Language = language;
            this.Text = loctextcoll[language];
            
        }

        public LanguageTextModel(ILanguage language) {
            this.Language = language;
            this.Text = String.Empty;
            
        }
        



    }
}
