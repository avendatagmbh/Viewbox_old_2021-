using System.ComponentModel;

namespace AvdCommon.Rules.Gui.Models
{
    public class EditRuleModel : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Properties

        #region Rule

        private Rule _rule;

        public Rule Rule
        {
            get { return _rule; }
            set
            {
                if (_rule != value)
                {
                    _rule = value;
                    RuleWithParameters = (Rule) _rule.Clone();
                    OnPropertyChanged("Rule");
                    OnPropertyChanged("RuleWithParameters");
                }
            }
        }

        #endregion

        #endregion

        public Rule RuleWithParameters { get; set; }
        public string ParameterError { get; set; }

        public string ExecutionError { get; set; }

        #region Methods

        public void ParametersChanged()
        {
            OnPropertyChanged("Parameters");
            OnPropertyChanged("ParameterError");
            OnPropertyChanged("ExecutionError");
        }

        #endregion
    }
}