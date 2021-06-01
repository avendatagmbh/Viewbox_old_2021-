using System;
using System.Collections.Generic;
using System.Linq;
using SystemDb;

namespace Viewbox
{
	internal class SessionMarker
	{
		private class Marker
		{
			public bool Marked { get; set; }

			public DateTime Timestamp { get; set; }
		}

		public class UserSessions
		{
			private readonly Dictionary<string, Marker> _dic = new Dictionary<string, Marker>();

			public bool this[string key]
			{
				get
				{
					CleanUp();
					if (_dic.ContainsKey(key))
					{
						_dic[key].Timestamp = DateTime.Now;
						return _dic[key].Marked;
					}
					return false;
				}
				set
				{
					_dic[key] = new Marker
					{
						Marked = value,
						Timestamp = DateTime.Now
					};
					CleanUp();
				}
			}

			public void RefreshSession()
			{
				if (_dic.ContainsKey(ViewboxSession.Key))
				{
					_dic[ViewboxSession.Key].Timestamp = DateTime.Now;
					return;
				}
				_dic[ViewboxSession.Key] = new Marker
				{
					Marked = false,
					Timestamp = DateTime.Now
				};
			}

			public void MarkAll()
			{
				foreach (string key in _dic.Keys)
				{
					_dic[key].Marked = true;
				}
			}

			private void CleanUp()
			{
				DateTime limit = DateTime.Now - ViewboxApplication.TemporaryObjectsLifetime;
				List<string> keys = new List<string>(_dic.Where(delegate(KeyValuePair<string, Marker> mkv)
				{
					KeyValuePair<string, Marker> keyValuePair2 = mkv;
					return keyValuePair2.Value.Timestamp < limit;
				}).Select(delegate(KeyValuePair<string, Marker> mkv)
				{
					KeyValuePair<string, Marker> keyValuePair = mkv;
					return keyValuePair.Key;
				}));
				foreach (string key in keys)
				{
					_dic.Remove(key);
				}
			}
		}

		private readonly Dictionary<IUser, UserSessions> _dic = new Dictionary<IUser, UserSessions>();

		public UserSessions this[IUser user]
		{
			get
			{
				if (!_dic.ContainsKey(user))
				{
					_dic[user] = new UserSessions();
				}
				return _dic[user];
			}
		}

		public bool Current
		{
			get
			{
				return this[ViewboxSession.User][ViewboxSession.Key];
			}
			set
			{
				this[ViewboxSession.User][ViewboxSession.Key] = value;
			}
		}
	}
}
