using System.ComponentModel;
using DbAccess.Enums;

namespace DbAccess
{
	public class ConnectionStringParam
	{
		public string Name { get; set; }

		public object Value { get; set; }

		public string Caption { get; set; }

		public ConnectionStringParamType Type { get; set; }

		public int Ordinal { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(e.PropertyName));
			}
		}
	}
}
