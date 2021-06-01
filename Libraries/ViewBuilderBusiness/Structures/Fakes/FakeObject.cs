using Utils;

namespace ViewBuilderBusiness.Structures.Fakes
{
    public enum State
    {
        Remained,
        Added,
        Deleted
    }

    public class FakeObject : NotifyPropertyChangedBase
    {
        #region Constructor

        public FakeObject()
        {
            State = State.Added;
        }

        public FakeObject(string name)
        {
            OriginalName = name;
            Name = name;
        }

        #endregion Constructor

        #region Properties

        public string OriginalName { get; set; }

        #region Name

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    SetState();
                    OnPropertyChanged("Name");
                }
            }
        }

        #endregion Name

        #region State

        private State _state = State.Added;

        public State State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        #endregion State

        #endregion Properties

        #region Methods

        private void SetState()
        {
            if (State == State.Remained) State = State.Added;
        }

        #endregion Methods
    }
}