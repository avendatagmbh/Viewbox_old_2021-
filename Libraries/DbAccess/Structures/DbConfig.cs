using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace DbAccess.Structures
{
	[DataContract]
	public class DbConfig : INotifyPropertyChanged, ICloneable
	{
		private DbConfigBuilderBase _builder;

		internal DbTemplate DbTemplate { get; set; }

		[DataMember(Order = 1)]
		public string Hostname
		{
			get
			{
				return _builder.Hostname;
			}
			set
			{
				if (_builder.Hostname != value)
				{
					_builder.Hostname = value;
					OnPropertyChanged("Hostname");
				}
			}
		}

		[DataMember(Order = 1)]
		public string Username
		{
			get
			{
				return _builder.Username;
			}
			set
			{
				if (_builder.Username != value)
				{
					_builder.Username = value;
					OnPropertyChanged("Username");
				}
			}
		}

		[DataMember(Order = 1)]
		public string Password
		{
			get
			{
				return _builder.Password;
			}
			set
			{
				if (_builder.Password != value)
				{
					_builder.Password = value;
					OnPropertyChanged("Password");
				}
			}
		}

		[DataMember(Order = 1)]
		public string DbName
		{
			get
			{
				return _builder.DbName;
			}
			set
			{
				if (_builder.DbName != value)
				{
					_builder.DbName = value;
					OnPropertyChanged("DbName");
				}
			}
		}

		[DataMember(Order = 1)]
		public int Port
		{
			get
			{
				return _builder.Port;
			}
			set
			{
				if (_builder.Port != value)
				{
					_builder.Port = value;
					OnPropertyChanged("Port");
				}
			}
		}

		[DataMember(Order = 1)]
		public string CharacterSet
		{
			get
			{
				return _builder.CharacterSet;
			}
			set
			{
				if (_builder.CharacterSet != value)
				{
					_builder.CharacterSet = value;
					OnPropertyChanged("CharacterSet");
				}
			}
		}

		public string ConnectionString
		{
			get
			{
				return _builder.ConnectionString;
			}
			set
			{
				if (_builder.ConnectionString != value)
				{
					_builder.ConnectionString = value;
					OnPropertyChanged("ConnectionString");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public DbConfig(DbTemplate dbTemplate = null)
		{
			DbTemplate = dbTemplate;
			_builder = new DbConfigBuilderBase();
		}

		private void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public object Clone()
		{
			return new DbConfig
			{
				Hostname = Hostname,
				DbName = DbName,
				Username = Username,
				Password = Password,
				Port = Port,
				CharacterSet = CharacterSet
			};
		}

		private DatabaseBase CreateConnection()
		{
			DatabaseBase databaseBase = ConnectionManager.CreateConnection(this);
			databaseBase.Open();
			return databaseBase;
		}

		public void TestConnection()
		{
			try
			{
				using DatabaseBase conn = CreateConnection();
				conn.Close();
			}
			catch (Exception ex)
			{
				throw new Exception("Problem with connection", ex);
			}
		}

		public bool CreateDbIfNotExists()
		{
			try
			{
				using DatabaseBase conn = CreateConnection();
				conn.Open();
				conn.CreateDatabaseIfNotExists(DbName);
				conn.Close();
			}
			catch
			{
				return false;
			}
			return true;
		}
	}
}
