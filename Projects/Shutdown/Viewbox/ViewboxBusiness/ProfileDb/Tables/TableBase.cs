using System.ComponentModel;
using DbAccess;
using DbAccess.Attributes;

namespace ViewboxBusiness.ProfileDb.Tables
{
	public class TableBase : INotifyPropertyChanged
	{
		[DbColumn("id", AutoIncrement = true, AllowDbNull = false)]
		[DbPrimaryKey]
		public int Id { get; set; }

		public ProjectDb ProjectDb { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string property)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		public void SaveOrUpdate()
		{
			using DatabaseBase conn = ProjectDb.ConnectionManager.GetConnection();
			SaveOrUpdate(conn);
		}

		public virtual void SaveOrUpdate(DatabaseBase conn)
		{
			conn.DbMapping.Save(this);
		}

		public void Delete()
		{
			if (!ProjectDb.ConnectionManager.IsDisposed)
			{
				using DatabaseBase conn = ProjectDb.ConnectionManager.GetConnection();
				Delete(conn);
			}
		}

		public virtual void Delete(DatabaseBase conn)
		{
			conn.DbMapping.Delete(this);
		}
	}
}
