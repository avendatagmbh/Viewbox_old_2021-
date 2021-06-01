using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;

namespace ViewboxAdmin.ViewModels.LocalizedTextWrappers
{
    class OptimizationGroupTextWrapperStructure : WrapperStructureBase, IItemWrapperStructure
    {
        public OptimizationGroupTextWrapperStructure(IOptimizationGroup optgroup, ILanguage language, ISystemDb systemdb):base(language,systemdb) { this.OptimizationGroup = optgroup; }

        public IOptimizationGroup OptimizationGroup { get; private set; }

        public string Text {
            get { return OptimizationGroup.Names[Language]; }
            set {
                if (OptimizationGroup.Names[Language] != value) {
                    OptimizationGroup.Names[Language] = value;
                    SystemDb.UpdateOptimizationGroupText(OptimizationGroup,Language);
                    OnPropertyChanged("Text");
                }
            }
        }

        public string Name {
            get { return string.Empty; }
        }

        public int Id {
            get { return OptimizationGroup.Id; }
        }
    }
}
