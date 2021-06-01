namespace AvdCommon.Rules.ExecuteRules
{
    public abstract class ExecuteRule : Rule
    {
        #region Methods

        public virtual string Execute(string value)
        {
            return value;
        }

        #endregion Methods

        #region Properties

        protected bool _isSpecialRule;

        public bool IsSpecialRule
        {
            get { return _isSpecialRule; }
        }

        public virtual string SpecialValue
        {
            get { return ""; }
        }

        #endregion Properties
    }
}