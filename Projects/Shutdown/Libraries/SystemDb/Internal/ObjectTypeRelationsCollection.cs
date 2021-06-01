using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public class ObjectTypeRelationsCollection : Dictionary<int, Tuple<int, int>>, IObjectTypeRelationsCollection, IDictionary<int, Tuple<int, int>>, ICollection<KeyValuePair<int, Tuple<int, int>>>, IEnumerable<KeyValuePair<int, Tuple<int, int>>>, IEnumerable
	{
		private Dictionary<int, Tuple<int, int>> _dic = new Dictionary<int, Tuple<int, int>>();

		public bool IsEmpty => _dic.Count == 0;

		public void Add(ObjectTypeRelations otr)
		{
			if (!_dic.ContainsKey(otr.Id))
			{
				_dic.Add(otr.Id, Tuple.Create(otr.Ref_Id, otr.Object_Id));
			}
		}

		public List<int> ValueByObjectId(int objectId)
		{
			List<int> retList = new List<int>();
			foreach (Tuple<int, int> item in _dic.Values)
			{
				if (item.Item2 == objectId)
				{
					retList.Add(item.Item1);
				}
			}
			return retList;
		}
	}
}
