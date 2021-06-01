using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SystemDb.Internal
{
	public interface IUserRoleCollection
	{
		IUserRoleMapping this[IUser user, IRole role] { get; }

		IEnumerable<IUser> this[IRole role] { get; }

		IList<IRole> this[IUser user] { get; }

		IEqualityComparer<Tuple<IUser, IRole>> Comparer { get; }

		int Count { get; }

		Dictionary<Tuple<IUser, IRole>, IUserRoleMapping>.KeyCollection Keys { get; }

		Dictionary<Tuple<IUser, IRole>, IUserRoleMapping>.ValueCollection Values { get; }

		IUserRoleMapping this[Tuple<IUser, IRole> key] { get; set; }

		void Add(IUser user, IRole role, IUserRoleMapping mapping);

		void Add(Tuple<IUser, IRole> key, IUserRoleMapping value);

		void Clear();

		bool ContainsKey(Tuple<IUser, IRole> key);

		bool ContainsValue(IUserRoleMapping value);

		Dictionary<Tuple<IUser, IRole>, IUserRoleMapping>.Enumerator GetEnumerator();

		void GetObjectData(SerializationInfo info, StreamingContext context);

		void OnDeserialization(object sender);

		bool Remove(Tuple<IUser, IRole> key);

		bool TryGetValue(Tuple<IUser, IRole> key, out IUserRoleMapping value);
	}
}
