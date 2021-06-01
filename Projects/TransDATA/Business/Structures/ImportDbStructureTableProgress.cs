// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since:  2011-01-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Interfaces;
using Utils;

namespace Business.Structures {
    internal class ImportDbStructureTableProgress : NotifyPropertyChangedBase, IImportDbStructureTableProgress {

        public string TableName { get; internal set; }

        public string Filter { get; internal set; }

        public long DatasetsTotal { get; internal set; }

        private long _dataSetsProcessed;

        #region DataSetsProcessed
        public long DataSetsProcessed {
            get { return _dataSetsProcessed; }
            internal set {
                _dataSetsProcessed = value;
                OnPropertyChanged("DataSetsProcessed");
            }
        }

        #endregion DataSetsProcessed
    }
}
