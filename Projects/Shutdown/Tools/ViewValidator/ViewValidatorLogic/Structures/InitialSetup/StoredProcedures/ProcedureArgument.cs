using System.ComponentModel;

namespace ViewValidatorLogic.Structures.InitialSetup.StoredProcedures {
    public class ProcedureArgument : INotifyPropertyChanged {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        #region Properties
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public long Ordinal { get; set; }

        private string _value;
        public string Value {
            get { return _value; }
            set {
                if (_value != value) {
                    _value = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        public string ValueForCall {
            get {
                if(Value == null) return "";
                switch(Type.ToLower()) {
                    case "date":
                        return "STR_TO_DATE('" + Value.ToString() + "','%d.%m.%Y')";
                    default:
                        return Value;
                }
            }
        }

        #endregion Properties
    }
}
