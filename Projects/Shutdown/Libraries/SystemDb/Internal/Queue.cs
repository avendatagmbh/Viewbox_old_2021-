using System;
using System.Collections.Generic;
using DbAccess;

namespace SystemDb.Internal
{
	public class Queue
	{
		private readonly Queue<Tuple<object, bool>> _queue = new Queue<Tuple<object, bool>>();

		internal void AddSave(object o)
		{
			lock (this)
			{
				_queue.Enqueue(new Tuple<object, bool>(o, item2: true));
			}
		}

		internal void AddDelete(object o)
		{
			lock (this)
			{
				_queue.Enqueue(new Tuple<object, bool>(o, item2: false));
			}
		}

		internal void PerformChanges(DatabaseBase db)
		{
			Queue<Tuple<object, bool>> queue = new Queue<Tuple<object, bool>>();
			lock (this)
			{
				while (_queue.Count > 0)
				{
					queue.Enqueue(_queue.Dequeue());
				}
			}
			while (queue.Count > 0)
			{
				Tuple<object, bool> tuple = queue.Dequeue();
				if (tuple.Item2)
				{
					db.DbMapping.Save(tuple.Item1);
				}
				else
				{
					db.DbMapping.Delete(tuple.Item1);
				}
			}
		}
	}
}
