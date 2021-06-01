using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SystemDb.Internal
{
	internal class UserRoleCollection : Dictionary<Tuple<IUser, IRole>, IUserRoleMapping>, IUserRoleCollection
	{
		public IUserRoleMapping this[IUser user, IRole role]
		{
			get
			{
				Tuple<IUser, IRole> key = Tuple.Create(user, role);
				if (!ContainsKey(key))
				{
					return null;
				}
				return base[key];
			}
		}

		public IEnumerable<IUser> this[IRole role] => new HashSet<IUser>(from t in base.Keys
			where t.Item2 == role
			select t.Item1);

		public IList<IRole> this[IUser user] => new List<IRole>(this.Where(delegate(KeyValuePair<Tuple<IUser, IRole>, IUserRoleMapping> kv)
		{
			KeyValuePair<Tuple<IUser, IRole>, IUserRoleMapping> keyValuePair3 = kv;
			return keyValuePair3.Key.Item1 == user;
		}).OrderBy(delegate(KeyValuePair<Tuple<IUser, IRole>, IUserRoleMapping> kv)
		{
			KeyValuePair<Tuple<IUser, IRole>, IUserRoleMapping> keyValuePair2 = kv;
			return keyValuePair2.Value.Ordinal;
		}).Select(delegate(KeyValuePair<Tuple<IUser, IRole>, IUserRoleMapping> kv)
		{
			KeyValuePair<Tuple<IUser, IRole>, IUserRoleMapping> keyValuePair = kv;
			return keyValuePair.Key.Item2;
		}));

		public void Add(IUser user, IRole role, IUserRoleMapping mapping)
		{
			base[Tuple.Create(user, role)] = mapping;
		}

		//[SpecialName]
		//IEqualityComparer<Tuple<IUser, IRole>> IUserRoleCollection.get_Comparer()
		//{
		//	return base.Comparer;
		//}

		//[SpecialName]
		//KeyCollection IUserRoleCollection.get_Keys()
		//{
		//	return base.Keys;
		//}

		//[SpecialName]
		//ValueCollection IUserRoleCollection.get_Values()
		//{
		//	return base.Values;
		//}

		bool IUserRoleCollection.ContainsValue(IUserRoleMapping value)
		{
			return ContainsValue(value);
		}

		Enumerator IUserRoleCollection.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
