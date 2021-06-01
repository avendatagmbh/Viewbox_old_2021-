using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("passwords", ForceInnoDb = true)]
	public class Password : IPassword, ICloneable
	{
		private string _passwordText;

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("password", Length = 64)]
		public string PasswordHash { get; set; }

		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("creation_date")]
		public DateTime CreationDate { get; set; }

		public string PasswordText
		{
			get
			{
				if (_passwordText == null)
				{
					throw new InvalidOperationException("Currently only hash is known");
				}
				return _passwordText;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				PasswordHash = GetPasswordHash(value, Id.ToString(CultureInfo.InvariantCulture));
				_passwordText = value;
			}
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public bool IsValidPassword(string password)
		{
			if (PasswordHash == GetPasswordHash(password, Id.ToString(CultureInfo.InvariantCulture)))
			{
				_passwordText = password;
				return true;
			}
			return false;
		}

		private string GetPasswordHash(string password, string salt)
		{
			return BitConverter.ToString(new SHA1CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(password + salt))).Replace("-", "");
		}
	}
}
