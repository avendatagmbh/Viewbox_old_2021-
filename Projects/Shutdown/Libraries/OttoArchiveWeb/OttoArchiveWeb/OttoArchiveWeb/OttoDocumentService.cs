using System;
using System.Collections.Generic;
using System.Text;
using AV.Log;
using log4net;
using OttoArchive.OttoArchiveWebServer;
using WebServiceInterfaces;

namespace OttoArchive.OttoArchiveWeb
{
	public class OttoDocumentService : IDocumentService, IDisposable
	{
		//private readonly ILog _logger = LogHelper.GetLogger();

		private OttoArchiveWebClient _client;

		public void Dispose()
		{
			CloseContext();
		}

		public bool Init(IParameter parameter)
		{
			InitParameter initParameter = parameter as InitParameter;
			if (initParameter == null)
			{
				return false;
			}
			_client = new OttoArchiveWebClient("OttoArchiveWebSOAP");
			string errors;
			bool num = _client.Init(initParameter.ArchivServerName, initParameter.SeratioServerName, initParameter.ArchivPort, initParameter.SeratioPort, initParameter.SysDbName, initParameter.DataDbName, initParameter.User, initParameter.Password, initParameter.PeriNames, out errors);
			parameter.Errors = errors;
			if (!num)
			{
				//_logger.Error(errors);
			}
			return num;
		}

		public bool GetDocumentList(IDocumentListParameter parameter)
		{
			parameter.output.DocumentList = null;
			if (_client.GetDocuments(parameter.input.Search, parameter.input.DescriptorId, parameter.CloseServer, out var errors, out var documents))
			{
				if (documents != null && documents.ids != null)
				{
					parameter.output.DocumentList = new List<IDocument>();
					string[] ids = documents.ids;
					foreach (string id in ids)
					{
						parameter.output.DocumentList.Add(new Document
						{
							Id = id
						});
					}
					return true;
				}
				parameter.Errors = "Can't parse, because documents is null or documents.ids is null";
				return false;
			}
			parameter.Errors = errors;
			return false;
		}

		public bool GetDocument(IDocumentParameter parameter)
		{
			parameter.output.Binary = null;
			if (_client.GetDocument(parameter.input.Id, parameter.CloseServer, out var errors, out var binary))
			{
				parameter.output.Binary = Convert.FromBase64String(Encoding.Default.GetString(binary));
				return true;
			}
			parameter.Errors = errors;
			return false;
		}

		public bool GetDescriptors(IDescriptorListParameter parameter)
		{
			parameter.output.Descriptors = null;
			if (_client.GetDescriptors(parameter.CloseServer, out var errors, out var descriptor))
			{
				if (descriptor.ids != null && descriptor.names != null)
				{
					parameter.output.Descriptors = new List<IDescriptor>();
					for (int i = 0; i < descriptor.ids.Length; i++)
					{
						parameter.output.Descriptors.Add(new Descriptor
						{
							Id = descriptor.ids[i],
							Name = descriptor.names[i]
						});
					}
					return true;
				}
				parameter.Errors = "Can't parse, because Descriptor.ids is null, or Descriptor.names is null";
			}
			parameter.Errors = errors;
			return false;
		}

		public bool GetDocumentInfo(IDocumentInfoParameter parameter)
		{
			if (_client.GetDocumentInfo(parameter.document.Id, parameter.CloseServer, out var errors, out var infos))
			{
				if (infos != null && infos.keys != null && infos.values != null)
				{
					for (int i = 0; i < infos.keys.Length; i++)
					{
						if (!string.IsNullOrEmpty(infos.keys[i]))
						{
							if (parameter.document.Descriptors.ContainsKey(infos.keys[i]))
							{
								parameter.document.Descriptors[infos.keys[i]] = infos.values[i];
							}
							else
							{
								parameter.document.Descriptors.Add(infos.keys[i], infos.values[i]);
							}
						}
					}
					return true;
				}
				parameter.Errors = "Can't parse, because Descriptor.ids is null, or Descriptor.names is null";
			}
			parameter.Errors = errors;
			return false;
		}

		public void CloseContext()
		{
			if (_client != null)
			{
				_client.CloseServer();
			}
		}
	}
}
