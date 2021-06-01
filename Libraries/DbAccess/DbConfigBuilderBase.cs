using MySql.Data.MySqlClient;

namespace DbAccess
{
	internal class DbConfigBuilderBase
	{
		private readonly MySqlConnectionStringBuilder _csb = new MySqlConnectionStringBuilder
		{
			AllowZeroDateTime = true,
			AllowUserVariables = true
		};

		public string Hostname
		{
			get
			{
				return _csb.Server;
			}
			set
			{
				_csb.Server = value;
			}
		}

		public string Username
		{
			get
			{
				return _csb.UserID;
			}
			set
			{
				_csb.UserID = value;
			}
		}

		public string Password
		{
			get
			{
				return _csb.Password;
			}
			set
			{
				_csb.Password = value;
			}
		}

		public string DbName
		{
			get
			{
				return _csb.Database;
			}
			set
			{
				_csb.Database = value;
			}
		}

		public int Port
		{
			get
			{
				return (int)_csb.Port;
			}
			set
			{
				_csb.Port = (uint)value;
			}
		}

		public string CharacterSet
		{
			get
			{
				return _csb.CharacterSet;
			}
			set
			{
				_csb.CharacterSet = value;
			}
		}

		public string ConnectionString
		{
			get
			{
				return _csb.ConnectionString;
			}
			set
			{
				_csb.ConnectionString = value;
			}
		}
	}
}
