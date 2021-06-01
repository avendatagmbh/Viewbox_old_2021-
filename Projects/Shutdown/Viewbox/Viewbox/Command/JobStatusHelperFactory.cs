using System;
using System.Web.Mvc;
using Viewbox.Job;

namespace Viewbox.Command
{
	public class JobStatusHelperFactory
	{
		public static IJobStatusHelper CreateJobStatusHelper(Base job, Controller controller)
		{
			Transformation transformation = job as Transformation;
			if (transformation != null)
			{
				return new TransformationHelper(transformation, controller);
			}
			OptimizationJob optjob = job as OptimizationJob;
			if (optjob != null)
			{
				return new OptimizationJobHelper(optjob, controller);
			}
			Export export = job as Export;
			if (export != null)
			{
				return new ExportJobHelper(export, controller);
			}
			PopulateIndexesJob indexesJob = job as PopulateIndexesJob;
			if (indexesJob != null)
			{
				return new IndexesJobHelper(indexesJob, controller);
			}
			RelationJob relation = job as RelationJob;
			if (relation != null)
			{
				return new RelationJobHelper(relation, controller);
			}
			GenerateTableJob genT = job as GenerateTableJob;
			if (genT != null)
			{
				return new GenerateTableJobHelper(genT, controller);
			}
			CreateArchiveFilesJob archF = job as CreateArchiveFilesJob;
			if (archF != null)
			{
				return new CreateArchiveFilesJobHelper(archF, controller);
			}
			DownloadJob afpConversionJob = job as DownloadJob;
			if (afpConversionJob != null)
			{
				return new DownloadJobHelper(afpConversionJob, controller);
			}
			throw new Exception("This type is not known by this factory");
		}
	}
}
