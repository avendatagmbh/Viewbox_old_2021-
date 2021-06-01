using System;
using System.ComponentModel;

namespace Utils
{
	public class NotifyPropertyChangedBase : INotifyPropertyChanged, INotifyPropertyChanging
	{
		private PropertyChangedEventHandler _propertyChanged;

		private PropertyChangingEventHandler _propertyChanging;

		public event PropertyChangedEventHandler PropertyChanged
		{
			add
			{
				_propertyChanged = (PropertyChangedEventHandler)Delegate.Combine(_propertyChanged, value);
			}
			remove
			{
				_propertyChanged = (PropertyChangedEventHandler)Delegate.Remove(_propertyChanged, value);
			}
		}

		public event PropertyChangingEventHandler PropertyChanging
		{
			add
			{
				_propertyChanging = (PropertyChangingEventHandler)Delegate.Combine(_propertyChanging, value);
			}
			remove
			{
				_propertyChanging = (PropertyChangingEventHandler)Delegate.Remove(_propertyChanging, value);
			}
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (_propertyChanged != null)
			{
				_propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		protected virtual void OnPropertyChanging(string propertyName)
		{
			if (_propertyChanging != null)
			{
				_propertyChanging(this, new PropertyChangingEventArgs(propertyName));
			}
		}

		public void ClearAllEventHandler()
		{
			_propertyChanged = null;
			_propertyChanging = null;
		}
	}
}
