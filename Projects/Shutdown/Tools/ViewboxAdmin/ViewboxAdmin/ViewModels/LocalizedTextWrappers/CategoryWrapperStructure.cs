using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SystemDb;

namespace ViewboxAdmin.ViewModels
{
    
    public class CategoryWrapperStructure : WrapperStructureBase, IItemWrapperStructure {

        public CategoryWrapperStructure(ICategory category, ILanguage language,ISystemDb systemDb) : base(language,systemDb) {
            this._category = category;
        }

        private ICategory _category;
        
        public string Text { 
            get {
                string text = _category.Names[Language];
                return text;
            } 
            set {
                if (_category.Names[Language] != value)
                    _category.Names[Language] = value;
                //add db method here
                    this.SystemDb.UpdateCategoriesText(_category,Language);
                    OnPropertyChanged("Text"); }
        }

        public string Name { get { return string.Empty; } }
        public int Id { get { return _category.Id; } }
        
       
    }
}
