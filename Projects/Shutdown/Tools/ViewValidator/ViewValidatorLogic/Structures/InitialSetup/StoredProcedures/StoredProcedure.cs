using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DbAccess;
using DbAccess.Structures;

namespace ViewValidatorLogic.Structures.InitialSetup.StoredProcedures {
    public class StoredProcedure : INotifyPropertyChanged {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        #region Properties

        #region Name
        private string _name;
        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        #endregion

        public long Id { get; set; }
        public List<ProcedureArgument> Arguments { get; private set; }
        //public ProcedureArgument this[int index] { get { return Arguments[index]; } }
        #endregion Properties

        #region Constructor
        public StoredProcedure() {
            Arguments = new List<ProcedureArgument>();
            Name = string.Empty;
        }

        public StoredProcedure(long id, string name) {
            Arguments = new List<ProcedureArgument>();
            Id = id;
            Name = name;
        }
        #endregion

        #region Methods
        public void AddArgument(ProcedureArgument arg) {
            Arguments.Add(arg);
            arg.PropertyChanged += new PropertyChangedEventHandler(arg_PropertyChanged);
            OnPropertyChanged("GeneratedCallStatement");
        }

        void arg_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == "Value")
                OnPropertyChanged("GeneratedCallStatement");
        }

        public string GeneratedCallStatement {
            get {
                //STR_TO_DATE('31.12.2011','%d.%m.%Y')
                StringBuilder builder = new StringBuilder("CALL ");
                builder.Append(Name);
                builder.Append("(");
                var sortedArguments = from arg in Arguments orderby arg.Ordinal select arg;

                foreach (var arg in sortedArguments) {
                    builder.Append(arg.ValueForCall);
                    builder.Append(",");
                }
                builder.Remove(builder.Length - 1, 1);
                builder.Append(")");
                return builder.ToString();
            }
        }


        public void Call(DbConfig dbConfig) {
            using (IDatabase conn = ConnectionManager.CreateConnection(dbConfig)) {
                conn.Open();
                conn.ExecuteNonQuery(GeneratedCallStatement);

            }
        }
        #endregion

        public void Clear() {
            Arguments.Clear();
            Name = string.Empty;
            Id = 0;
        }
    }
}
