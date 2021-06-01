using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using SystemDb;
using Viewbox.Job.PdfExport.Basler;
using Viewbox.Job.PdfExport.General;
using ViewboxDb;

namespace Viewbox.Job.PdfExport
{
	public static class PdfExporterFactory
	{
		internal static IPdfExporter GetPdfExporter(IDataReader dataReader, Stream targetStream, List<IColumn> columns, ITableObject tableObject, CancellationToken cancellationToken, ILanguage language, IOptimization optimization, IUser user, TableObjectCollection tempTableObjects, bool hasGroupSubtotal = false)
		{
			string database = tableObject.Database.ToLower();
			string tableName = tableObject.TableName.ToLower();
			PdfExporterConfigElement exporter = PdfExporterConfig.GetExporter(database, tableName);
			Type exporterType = Type.GetType(exporter.Exporter) ?? typeof(LegacyPdfExporter);
			IIssue issue = tableObject as IIssue;
			if (issue != null && issue.Flag == 42)
			{
				exporterType = typeof(KontoauszuegePdfExport);
				columns = columns.OrderBy((IColumn c) => c.Ordinal).ToList();
			}
			else if (issue != null && issue.Flag == 43)
			{
				exporterType = typeof(ProvisionskontoPdfExport);
				columns = columns.OrderBy((IColumn c) => c.Ordinal).ToList();
			}
			IPdfExporter exporterInstance = Activator.CreateInstance(exporterType, cancellationToken) as IPdfExporter;
			if (exporterInstance != null)
			{
				exporterInstance.ExporterConfig = exporter;
				exporterInstance.DataReader = dataReader;
				exporterInstance.TargetStream = targetStream;
				exporterInstance.TempTableObjects = tempTableObjects;
				exporterInstance.HasGroupSubtotal = hasGroupSubtotal;
				if (exporterInstance is LegacyPdfExporter)
				{
					LegacyPdfExporter generalPdfExporter3 = exporterInstance as LegacyPdfExporter;
					generalPdfExporter3.Columns = columns;
					generalPdfExporter3.TableObject = tableObject;
					generalPdfExporter3.CancellationToken = cancellationToken;
					generalPdfExporter3.Language = language;
					generalPdfExporter3.Optimization = optimization;
					generalPdfExporter3.User = user;
				}
				else if (exporterInstance is KontoauszuegePdfExport)
				{
					KontoauszuegePdfExport generalPdfExporter2 = exporterInstance as KontoauszuegePdfExport;
					generalPdfExporter2.Columns = columns;
					generalPdfExporter2.TableObject = tableObject;
					generalPdfExporter2.CancellationToken = cancellationToken;
					generalPdfExporter2.Language = language;
					generalPdfExporter2.Optimization = optimization;
					generalPdfExporter2.User = user;
				}
				else if (exporterInstance is ProvisionskontoPdfExport)
				{
					ProvisionskontoPdfExport generalPdfExporter = exporterInstance as ProvisionskontoPdfExport;
					generalPdfExporter.Columns = columns;
					generalPdfExporter.TableObject = tableObject;
					generalPdfExporter.CancellationToken = cancellationToken;
					generalPdfExporter.Language = language;
					generalPdfExporter.Optimization = optimization;
					generalPdfExporter.User = user;
				}
			}
			return exporterInstance;
		}

		internal static IPdfExporter GetBelegPdfExporter(IDataReader dataReader, Stream targetStream, List<IColumn> columns, ITableObject tableObject, CancellationToken cancellationToken, ILanguage language, IOptimization optimization, IUser user, TableObjectCollection tempTableObjects)
		{
			string database = tableObject.Database.ToLower();
			string tableName = tableObject.TableName.ToLower();
			PdfExporterConfigElement exporter = PdfExporterConfig.GetExporter(database, tableName);
			BelegPdfExporter exporterInstance = new BelegPdfExporter(cancellationToken);
			if (exporterInstance != null)
			{
				exporterInstance.ExporterConfig = exporter;
				exporterInstance.DataReader = dataReader;
				exporterInstance.TargetStream = targetStream;
				exporterInstance.TempTableObjects = tempTableObjects;
				if (exporterInstance != null)
				{
					BelegPdfExporter generalPdfExporter = exporterInstance;
					generalPdfExporter.Columns = columns;
					generalPdfExporter.TableObject = tableObject;
					generalPdfExporter.CancellationToken = cancellationToken;
					generalPdfExporter.Language = language;
					generalPdfExporter.Optimization = optimization;
					generalPdfExporter.User = user;
				}
			}
			return exporterInstance;
		}
	}
}
