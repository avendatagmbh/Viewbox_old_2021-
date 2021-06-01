using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OttoArchive.OttoArchiveWebServer
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "4.0.0.0")]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[MessageContract(WrapperName = "Init", WrapperNamespace = "http://DefaultNamespace/OttoArchiveWeb/", IsWrapped = true)]
	public class InitRequest
	{
		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 2)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public string ArchivPort;

		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 0)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public string ArchivServerName;

		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 5)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public string DataDbName;

		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 7)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public string Password;

		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 8)]
		[XmlElement("PeriNames", Form = XmlSchemaForm.Unqualified)]
		public string[] PeriNames;

		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 3)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public string SeratioPort;

		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 1)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public string SeratioServerName;

		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 4)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public string SysDBName;

		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 6)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public string User;

		public InitRequest()
		{
		}

		public InitRequest(string ArchivServerName, string SeratioServerName, string ArchivPort, string SeratioPort, string SysDBName, string DataDbName, string User, string Password, string[] PeriNames)
		{
			this.ArchivServerName = ArchivServerName;
			this.SeratioServerName = SeratioServerName;
			this.ArchivPort = ArchivPort;
			this.SeratioPort = SeratioPort;
			this.SysDBName = SysDBName;
			this.DataDbName = DataDbName;
			this.User = User;
			this.Password = Password;
			this.PeriNames = PeriNames;
		}
	}
}
