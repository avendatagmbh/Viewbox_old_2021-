using Utils;

namespace WpfControlsSample.Structures.ListDemo {
    class Person :NotifyPropertyChangedBase {
        public Person(string name, int age, bool isMarried) {
            Name = name;
            Age = age;
            IsMarried = isMarried;
        }

        #region Name
        private string _name;
        public string Name {
            get { return _name; }
            set {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        #endregion Name

        #region Age
        private int _age;

        public int Age {
            get { return _age; }
            set {
                _age = value;
                OnPropertyChanged("Age");
                OnPropertyChanged("AgeString");
            }
        }
        public string AgeString { get { return Age + " years"; } }
        #endregion Age

        #region IsMarried
        private bool _isMarried;

        public bool IsMarried {
            get { return _isMarried; }
            set {
                _isMarried = value;
                OnPropertyChanged("IsMarried");
                OnPropertyChanged("IsMarriedString");
            }
        }
        public string IsMarriedString { get { return "married: " + (IsMarried ? "yes" : "no"); } }
        #endregion IsMarried
    }
}
