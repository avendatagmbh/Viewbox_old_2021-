// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-02-14
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Business.Interfaces;
using Utils;

namespace Business.Structures {
    public class ImportDbStructureTableProgressWrapper : NotifyPropertyChangedBase, IImportDbStructureTableProgressWrapper {
        private IImportDbStructureTableProgress _progress;

        public IImportDbStructureTableProgress Progress {
            get { return _progress; }
            set {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }
    }
}