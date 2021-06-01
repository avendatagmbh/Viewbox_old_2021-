// -----------------------------------------------------------
// Created by Benjamin Held - 25.08.2011 14:19:58
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System.ComponentModel;

namespace ViewValidatorLogic.Structures.InitialSetup {
    public class ColumnMapping : INotifyPropertyChanged {
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion

        #region Properties
        public Column SourceCol { get; private set; }
        public Column DestinationCol { get; private set; }

        public string Source{
            get { return SourceCol.Name; }
            set { SourceCol.Name = value; }
        }

        public string Destination {
            get { return DestinationCol.Name; }
            set { DestinationCol.Name = value; }
        }

        #region IsVisible
        private bool _isVisible = true;
        public bool IsVisible {
            get { return _isVisible; }
            set {
                if (_isVisible != value) {
                    _isVisible = value;
                    OnPropertyChanged("IsVisible");
                }
            }
        }
        #endregion IsVisible

        #endregion

        #region Contructor
        public ColumnMapping(Column sourceCol, Column destCol){
            SourceCol = sourceCol;
            DestinationCol = destCol;
            SourceCol.ColumnMapping = this;
            DestinationCol.ColumnMapping = this;
        }

        public ColumnMapping(string source, string dest){
            SourceCol = new Column(source, this);
            DestinationCol = new Column(dest, this);
        }
        #endregion

        public Column GetColumn(int sourceOrDestIndex){
            if (sourceOrDestIndex == 0) return SourceCol;
            return DestinationCol;
        }

        public string GetColumnName(int sourceOrDestIndex) {
            if (sourceOrDestIndex == 0) return SourceCol.Name;
            return DestinationCol.Name;
        }
    }
}
