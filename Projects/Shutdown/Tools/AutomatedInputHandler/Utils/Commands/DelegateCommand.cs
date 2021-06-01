using System;
using System.Windows.Input;

namespace Utils.Commands
{
    public class DelegateCommand : ICommand
    {
        private Func<object, bool> _canExecuteHandler;
        private Action<object> _executeHandler;
        private bool _canExecuteCache;

        public DelegateCommand(Func<object, bool> canExecuteHandler, Action<object> executeHandler)
        {
            this._executeHandler = executeHandler;
            this._canExecuteHandler = canExecuteHandler;
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            bool temp = _canExecuteHandler(parameter);

            if (_canExecuteCache != temp)
            {
                _canExecuteCache = temp;
                if (CanExecuteChanged != null && _isRaising == false)
                {
                    CanExecuteChanged(this, new EventArgs());
                }
            }

            return _canExecuteCache;
        }
        bool _isRaising = false;
        public void RaiseCanExecuteChanged()
        {
            _isRaising = true;
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
            _isRaising = false;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _executeHandler(parameter);
        }

        #endregion

    }
}
