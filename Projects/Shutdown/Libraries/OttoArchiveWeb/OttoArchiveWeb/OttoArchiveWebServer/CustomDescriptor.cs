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
	public class CustomDescriptor : INotifyPropertyChanged
	{
		private string[] idsField;

		private string[] namesField;

		[XmlElement("ids", Form = XmlSchemaForm.Unqualified, Order = 0)]
		public string[] ids
		{
			get
			{
				return idsField;
			}
			set
			{
				idsField = value;
				RaisePropertyChanged("ids");
			}
		}

		[XmlElement("names", Form = XmlSchemaForm.Unqualified, Order = 1)]
		public string[] names
		{
			get
			{
				return namesField;
			}
			set
			{
				namesField = value;
				RaisePropertyChanged("names");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged(string propertyName)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
