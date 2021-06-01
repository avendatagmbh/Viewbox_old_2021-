using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public class PasswordCollection : List<Password>, IPasswordCollection, IEnumerable<IPassword>, IEnumerable
	{
		public PasswordCollection()
		{
		}

		public PasswordCollection(IEnumerable<Password> passwords)
			: base(passwords)
		{
		}

		public void Insert(int index, IPassword pwd)
		{
			base.Insert(index, pwd as Password);
		}

		public new void Remove(Password oldestPwd)
		{
			base.Remove(oldestPwd);
		}

		public new IEnumerator<IPassword> GetEnumerator()
		{
			return base.GetEnumerator();
		}
	}
}
