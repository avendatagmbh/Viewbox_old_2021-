using System;
using System.Collections;
using System.Collections.Generic;

namespace Viewbox.Models
{
	public class NotificationModel : BaseModel, IEnumerable<NotificationModel.INotification>, IEnumerable
	{
		public enum Type
		{
			Info,
			Mail,
			Warning,
			Download,
			DownloadBlueline
		}

		public interface INotification
		{
			Type Type { get; }

			string Title { get; }

			string Description { get; }

			string Link { get; }

			string Key { get; }

			bool IsEmpty { get; }

			DateTime Date { get; }

			bool Active { get; }

			bool Visible { get; }
		}

		private class Notification : INotification
		{
			public Type Type { get; private set; }

			public string Title { get; internal set; }

			public string Description { get; private set; }

			public string Link { get; private set; }

			public string Key { get; private set; }

			public bool IsEmpty { get; private set; }

			public DateTime Date { get; private set; }

			public bool Active { get; private set; }

			public bool Visible { get; private set; }

			public Notification(Type type, string title, string description, string link, string key, DateTime date, bool active = true, bool isEmpty = false, bool visible = true)
			{
				Type = type;
				Title = title;
				Description = description;
				Link = link;
				Key = key;
				IsEmpty = isEmpty;
				Date = date;
				Active = active;
				Visible = visible;
			}
		}

		private readonly List<Notification> _list = new List<Notification>();

		public int Count => _list.Count;

		public int MaxEntriesLatest { get; internal set; }

		public int MaxEntriesHistory { get; internal set; }

		public INotification this[int index] => (index < Count) ? _list[index] : null;

		public IEnumerator<INotification> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void AddNotification(Type type, string title, string description, string link, string key, DateTime date, bool active = true, bool isEmpty = false, bool visible = true)
		{
			_list.Add(new Notification(type, title, description, link, key, date, active, isEmpty, visible));
		}

		public void RemoveNotification(int index)
		{
			_list.RemoveAt(index);
		}
	}
}
