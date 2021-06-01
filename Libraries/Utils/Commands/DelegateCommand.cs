using System;
using System.Windows.Input;

namespace Utils.Commands
{
    public class DelegateCommand : ICommand
    {
        private readonly Func<object, bool> _canExecuteHandler;
        private readonly Action<object> _executeHandler;
        private bool _canExecuteCache;
        private bool _isRaising;

        public DelegateCommand(Func<object, bool> canExecuteHandler, Action<object> executeHandler)
        {
            _executeHandler = executeHandler;
            _canExecuteHandler = canExecuteHandler;
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

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _executeHandler(parameter);
        }

        #endregion

        public void RaiseCanExecuteChanged()
        {
            _isRaising = true;
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
            _isRaising = false;
        }
    }
}