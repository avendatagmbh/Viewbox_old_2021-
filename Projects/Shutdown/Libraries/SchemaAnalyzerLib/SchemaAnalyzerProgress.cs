// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-01-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;

namespace SchemaAnalyzerLib {
    public class SchemaAnalyzerProgress : NotifyPropertyChangedBase {

        internal SchemaAnalyzerProgress() { }

        #region Caption
        private string _caption;

        public string Caption {
            get { return _caption; }
            internal set {
                _caption = value;
                OnPropertyChanged("Caption");
            }
        }
        #endregion

        #region Step
        private int _step;

        public int Step {
            get { return _step; }
            internal set {
                _step = value;
                OnPropertyChanged("Step");
            }
        }
        #endregion

        #region ProgressMin
        private int _progressMin;

        public int ProgressMin {
            get { return _progressMin; }
            internal set {
                _progressMin = value;
                OnPropertyChanged("ProgressMin");
            }
        }
        #endregion

        #region ProgressCurrent
        private int _progressCurrent;

        public int ProgressCurrent {
            get { return _progressCurrent; }
            internal set {
                _progressCurrent = value;
                OnPropertyChanged("ProgressCurrent");
            }
        }
        #endregion

        #region ProgressMax
        private int _progressMax;

        public int ProgressMax {
            get { return _progressMax; }
            internal set {
                _progressMax = value;
                OnPropertyChanged("ProgressMax");
            }
        }
        #endregion
    }
}