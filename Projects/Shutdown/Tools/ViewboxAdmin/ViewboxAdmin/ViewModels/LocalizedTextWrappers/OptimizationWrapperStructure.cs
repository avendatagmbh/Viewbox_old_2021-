using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SystemDb;

namespace ViewboxAdmin.ViewModels
{

    

    /// <summary>
    /// wrapper class around a tableobject
    /// </summary>
    public class OptimizationWrapperStructure :WrapperStructureBase, IItemWrapperStructure {

        public OptimizationWrapperStructure(IOptimization optimization, ILanguage language,ISystemDb systemDb) : base(language,systemDb) {
            this._optimization = optimization;
        }

        
        private IOptimization _optimization;

        
        public string Text { 
            get {
                string text = _optimization.Descriptions[Language];
                return text;
            } 
            set {
                if (_optimization.Descriptions[Language] != value)
                    _optimization.Descriptions[Language] = value;
                    SystemDb.UpdateOptimizationText(_optimization,Language);
                    OnPropertyChanged("Text"); }
        }

        public string Name { get { return _optimization.Value; } }
        public int Id { get { return _optimization.Id; } }
        
    }
}
