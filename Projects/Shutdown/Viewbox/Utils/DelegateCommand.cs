using System;
using System.Windows.Input;

namespace Utils
{
	public class DelegateCommand : ICommand
	{
		private readonly Func<object, bool> _canExecuteHandler;

		private readonly Action<object> _executeHandler;

		private bool _canExecuteCache;

		private bool _isRaising;

		public event EventHandler CanExecuteChanged;

		public DelegateCommand(Func<object, bool> canExecuteHandler, Action<object> executeHandler)
		{
			_executeHandler = executeHandler;
			_canExecuteHandler = canExecuteHandler;
		}

		public bool CanExecute(object parameter)
		{
			bool temp = _canExecuteHandler(parameter);
			if (_canExecuteCache != temp)
			{
				_canExecuteCache = temp;
				if (this.CanExecuteChanged != null && !_isRaising)
				{
					this.CanExecuteChanged(this, new EventArgs());
				}
			}
			return _canExecuteCache;
		}

		public void Execute(object parameter)
		{
			_executeHandler(parameter);
		}

		public void RaiseCanExecuteChanged()
		{
			_isRaising = true;
			if (this.CanExecuteChanged != null)
			{
				this.CanExecuteChanged(this, EventArgs.Empty);
			}
			_isRaising = false;
		}
	}
}
