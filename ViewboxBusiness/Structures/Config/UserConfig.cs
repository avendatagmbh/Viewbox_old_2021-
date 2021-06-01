using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using DbAccess;

namespace ViewboxBusiness.Structures.Config
{
	public class UserConfig : INotifyPropertyChanged
	{
		internal static string ElementName = "users";

		internal static string FieldnameName = "name";

		internal static string FieldnameFullName = "fullName";

		internal static string FieldnamePasswordHash = "passwordHash";

		private string _email;

		private string _fullName;

		private string _name;

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (_name != value)
				{
					_name = value;
					OnPropertyChanged("Name");
					OnPropertyChanged("DisplayString");
				}
			}
		}

		public string FullName
		{
			get
			{
				return _fullName;
			}
			set
			{
				if (_fullName != value)
				{
					_fullName = value;
					OnPropertyChanged("FullName");
					OnPropertyChanged("DisplayString");
				}
			}
		}

		public string EMail
		{
			get
			{
				return _email;
			}
			set
			{
				if (_email != value)
				{
					_email = value;
					OnPropertyChanged("EMail");
					OnPropertyChanged("DisplayString");
				}
			}
		}

		public string PasswordHash { get; set; }

		public string DisplayString
		{
			get
			{
				string ds = FullName;
				if (ds.Length == 0)
				{
					return Name;
				}
				return ds + " (" + Name + ")";
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public UserConfig()
		{
			Name = string.Empty;
			FullName = string.Empty;
			PasswordHash = string.Empty;
		}

		public UserConfig(XmlNode configNode)
		{
			Read(configNode);
		}

		private void OnPropertyChanged(string property)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		internal static void CreateTable(DatabaseBase db)
		{
			db.ExecuteNonQuery("CRATE TABLE IF NOT EXISTS " + db.Enquote(ElementName) + "(" + db.Enquote(FieldnameName) + " VARCHAR(20)," + db.Enquote(FieldnameFullName) + " VARCHAR(64)," + db.Enquote(FieldnamePasswordHash) + " VARCHAR(512),) ENGINE = MyISAM");
		}

		internal void Save(DatabaseBase db)
		{
		}

		internal void Read(DatabaseBase db)
		{
		}

		internal void Delete(DatabaseBase db)
		{
			db.ExecuteNonQuery("DELETE FROM " + db.Enquote(ElementName) + " WHERE " + db.Enquote(FieldnameName) + "='" + Name + "'");
		}

		internal void Save(XmlTextWriter writer, string nodeName)
		{
			writer.WriteStartElement(nodeName);
			writer.WriteAttributeString("name", Name);
			writer.WriteAttributeString("fullName", FullName);
			writer.WriteAttributeString("email", EMail);
			writer.WriteAttributeString("password", PasswordHash);
			writer.WriteEndElement();
		}

		internal void Read(XmlNode configNode)
		{
			if (configNode.Attributes == null)
			{
				return;
			}
			foreach (XmlAttribute attr in configNode.Attributes)
			{
				switch (attr.Name)
				{
				case "name":
					Name = attr.Value;
					break;
				case "fullName":
					FullName = attr.Value;
					break;
				case "email":
					EMail = attr.Value;
					break;
				case "password":
					PasswordHash = attr.Value;
					break;
				}
			}
		}

		public bool CheckPassword(string password)
		{
			string passwordHash = ComputePasswordHash(password, EMail);
			return PasswordHash.Equals(passwordHash);
		}

		public void SetPassword(string password)
		{
			PasswordHash = ComputePasswordHash(password, EMail);
		}

		private string ComputePasswordHash(string password, string salt)
		{
			return ByteArrayToString(new SHA256CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(password + salt)));
		}

		private static string ByteArrayToString(byte[] arrInput)
		{
			StringBuilder sOutput = new StringBuilder(arrInput.Length);
			for (int i = 0; i < arrInput.Length - 1; i++)
			{
				sOutput.Append(arrInput[i].ToString("X2"));
			}
			return sOutput.ToString();
		}
	}
}
