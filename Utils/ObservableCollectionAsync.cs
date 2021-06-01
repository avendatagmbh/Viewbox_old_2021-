using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace Utils
{
	public class ObservableCollectionAsync<T> : ObservableCollection<T>
	{
		public override event NotifyCollectionChangedEventHandler CollectionChanged;

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			using (BlockReentrancy())
			{
				NotifyCollectionChangedEventHandler eventHandler = CollectionChanged;
				if (eventHandler == null)
				{
					return;
				}
				Delegate[] invocationList = eventHandler.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					NotifyCollectionChangedEventHandler handler = (NotifyCollectionChangedEventHandler)invocationList[i];
					DispatcherObject dispatcherObject = handler.Target as DispatcherObject;
					if (dispatcherObject != null && !dispatcherObject.CheckAccess())
					{
						dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, e);
					}
					else
					{
						handler(this, e);
					}
				}
			}
		}

		public void AddRange(IEnumerable<T> dataToAdd)
		{
			CheckReentrancy();
			foreach (T data in dataToAdd)
			{
				base.Items.Add(data);
			}
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
	}
}
