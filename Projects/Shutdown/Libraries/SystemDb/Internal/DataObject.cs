using System.Collections.Generic;
using System.ComponentModel;
using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	public abstract class DataObject : IDataObject, INotifyPropertyChanged
	{
		private int _id;

		private string _name;

		private bool _userDefined;

		private int _ordinal;

		private LocalizedTextCollection _descriptions = new LocalizedTextCollection();

		private LocalizedTextCollection _objectTypes = new LocalizedTextCollection();

		private List<User> _users = new List<User>();

		private Dictionary<string, object> _properties = new Dictionary<string, object>();

		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id
		{
			get
			{
				return _id;
			}
			set
			{
				if (_id != value)
				{
					_id = value;
					NotifyPropertyChanged("Id");
				}
			}
		}

		[DbColumn("name", Length = 256)]
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
					NotifyNameChanged(value);
					NotifyPropertyChanged("Name");
				}
			}
		}

		public DataObjectType Type { get; private set; }

		[DbColumn("user_defined")]
		public bool UserDefined
		{
			get
			{
				return _userDefined;
			}
			set
			{
				if (_userDefined != value)
				{
					_userDefined = value;
					NotifyPropertyChanged("UserDefined");
				}
			}
		}

		[DbColumn("ordinal")]
		public int Ordinal
		{
			get
			{
				return _ordinal;
			}
			set
			{
				if (_ordinal != value)
				{
					int ordinal = _ordinal;
					_ordinal = value;
					NotifyOrdinalChanged(ordinal);
					NotifyPropertyChanged("Ordinal");
				}
			}
		}

		public ILocalizedTextCollection Descriptions => _descriptions;

		public ILocalizedTextCollection ObjectTypes => _objectTypes;

		public List<User> Users => _users;

		public Dictionary<string, object> Properties => _properties;

		public event PropertyChangedEventHandler PropertyChanged;

		public event NameChangedHandler NameChanged;

		public event OrdinalChangedHandler OrdinalChanged;

		public event ObjectRemovedHandler ObjectRemoved;

		protected DataObject(DataObjectType type)
		{
			Type = type;
		}

		protected void NotifyPropertyChanged(string property)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		private void NotifyNameChanged(string old_name)
		{
			if (this.NameChanged != null)
			{
				this.NameChanged(this, old_name);
			}
		}

		private void NotifyOrdinalChanged(int old_ordinal)
		{
			if (this.OrdinalChanged != null)
			{
				this.OrdinalChanged(this, old_ordinal);
			}
		}

		private void NotifyObjectRemoved()
		{
			if (this.ObjectRemoved != null)
			{
				this.ObjectRemoved(this);
			}
		}

		public virtual void Clone(ref TableObject t)
		{
			t.Id = Id;
			t.Name = Name;
			t.UserDefined = UserDefined;
			t.Ordinal = Ordinal;
			t.SetUsers(Users);
			t._descriptions = Descriptions.Clone() as LocalizedTextCollection;
			t._objectTypes = ObjectTypes.Clone() as LocalizedTextCollection;
			t._properties = Properties;
			t.Type = (TableType)Type;
		}

		protected virtual void Clone(ref Column c)
		{
			c.Id = Id;
			c.Name = Name;
			c.UserDefined = UserDefined;
			c.Ordinal = Ordinal;
			c.SetUsers(Users);
			c._descriptions = Descriptions.Clone() as LocalizedTextCollection;
			c._objectTypes = ObjectTypes.Clone() as LocalizedTextCollection;
			c._properties = Properties;
			c.Type = Type;
		}

		protected virtual void Clone(ref HistoryParameterValue par)
		{
			par.Id = Id;
			par.Name = Name;
			par.UserDefined = UserDefined;
			par.Ordinal = Ordinal;
			par.SetUsers(Users);
			par._descriptions = Descriptions.Clone() as LocalizedTextCollection;
			par._objectTypes = ObjectTypes.Clone() as LocalizedTextCollection;
			par._properties = Properties;
			par.Type = Type;
		}

		public void SetDescription(string description, ILanguage language)
		{
			_descriptions.Add(language, description);
		}

		public void SetObjectType(string objectType, ILanguage language)
		{
			_objectTypes.Add(language, objectType);
		}

		private void SetUsers(List<User> users)
		{
			_users = users;
		}

		public void Remove()
		{
			NotifyObjectRemoved();
		}

		public void SetOrdinalWithoutNotify(int ordinal)
		{
			_ordinal = ordinal;
			NotifyPropertyChanged("Ordinal");
		}
	}
}
