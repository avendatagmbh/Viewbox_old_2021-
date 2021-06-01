using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class ArchiveSettingsCollection : Dictionary<int, IArchiveSetting>, IArchiveSettingsCollection, IEnumerable<IArchiveSetting>, IEnumerable
	{
		public new IArchiveSetting this[int key]
		{
			get
			{
				if (!ContainsKey(key))
				{
					return null;
				}
				return base[key];
			}
		}

		public new IEnumerator<IArchiveSetting> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}

		public void Add(ArchiveSetting setting)
		{
			base[setting.Id] = setting;
		}
	}
}
