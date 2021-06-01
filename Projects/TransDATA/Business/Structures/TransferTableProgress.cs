// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Business.Interfaces;
using DbAccess.Interaction;
using Utils;

namespace Business.Structures
{
    public class TransferTableProgress : NotifyPropertyChangedBase, ITransferTableProgress
    {
        public string TableName { get; internal set; }

        public string Filter { get; internal set; }

        public DateTime StartTime { get; internal set; }

        public string StartTimeString
        {
            get { return StartTime.ToString("F"); }
        }

        public long DatasetsTotal { get; internal set; }

        #region FilesTotal
        private long _filesTotal;
        public long FilesTotal {
            get
            {
                return _filesTotal;
            }
            internal set
            {
                if (_filesTotal != value)
                {
                    _filesTotal = value;
                    OnPropertyChanged("DatasetProgressString");
                }
            }
        }
        #endregion FilesTotal

        #region FilesProcessed
        private long _filesProcessed;
        public long FilesProcessed
        {
            get { return _filesProcessed; }
            internal set
            {
                if (_filesProcessed != value)
                {
                    _filesProcessed = value;
                    OnPropertyChanged("DatasetProgressString");
                }
            }
        }
        #endregion FilesProcessed


        #region DataSetsProcessed
        private long _dataSetsProcessed;
        public long DataSetsProcessed
        {
            get { return _dataSetsProcessed; }
            set
            {
                _dataSetsProcessed = value;
                OnPropertyChanged("DatasetProgressString");
            }
        }
        #endregion DataSetsProcessed

        public string DatasetProgressString
        {
            get
            {
                string retStr="";

                var percent = ((double)_dataSetsProcessed / (double)DatasetsTotal) * 100;
                if (DatasetsTotal == 0) percent = 0;

                retStr+= _dataSetsProcessed + " / " + DatasetsTotal + " (" + (int)percent + " %)";

                if (_filesTotal > 1)
                {
                    int filesPercent = (int)(((double)_filesProcessed / (double)_filesTotal) * 100);
                    retStr += "         "  + Base.Localisation.ResourcesCommon.CSV + ": " + _filesProcessed + " / " + _filesTotal + " (" + filesPercent + " %)";
                }

                return retStr;
            }
        }
    }
}