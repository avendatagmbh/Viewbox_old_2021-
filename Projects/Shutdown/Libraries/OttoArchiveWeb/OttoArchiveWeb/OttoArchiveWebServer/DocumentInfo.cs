using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OttoArchive.OttoArchiveWebServer
{
	[Serializable]
	[GeneratedCode("System.Xml", "4.0.30319.18034")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(Namespace = "http://DefaultNamespace/OttoArchiveWeb/")]
	public class DocumentInfo : INotifyPropertyChanged
	{
		private string[] keysField;

		private string[] valuesField;

		[XmlElement("keys", Form = XmlSchemaForm.Unqualified, Order = 0)]
		public string[] keys
		{
			get
			{
				return keysField;
			}
			set
			{
				keysField = value;
				RaisePropertyChanged("keys");
			}
		}

		[XmlElement("values", Form = XmlSchemaForm.Unqualified, Order = 1)]
		public string[] values
		{
			get
			{
				return valuesField;
			}
			set
			{
				valuesField = value;
				RaisePropertyChanged("values");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged(string propertyName)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
