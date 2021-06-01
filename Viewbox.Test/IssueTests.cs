using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using SystemDb.Internal;
using DbAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;
using Viewbox.Job;
using Viewbox.Models;

namespace Viewbox.Test
{
    /// <summary>
    ///   Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class IssueTests : BaseTest
    {
        #region Properties

        private const int WaitSeconds = 60;
        private List<Parameter> parameters;

        #endregion Properties

        #region TestMethods

        [TestMethod]
        public void ExecuteSingleIssueTest()
        {
            int issueId = 19681;
            StringBuilder builder = new StringBuilder();
            using (IDatabase conn = ViewboxApplication.Database.ConnectionManager.GetConnection())
            {
                List<IssueExtension> extensions = conn.DbMapping.Load<IssueExtension>("ref_id = " + issueId);
                parameters = conn.DbMapping.Load<Parameter>("issue_id = " + issueId);
                foreach (IssueExtension extension in extensions)
                {
                    string msg = ExecuteIssue(extension);
                    builder.Append(msg).Append(String.IsNullOrEmpty(msg) ? "" : Environment.NewLine);
                }
            }
            Assert.IsTrue(builder.Length == 0, builder.ToString());
        }

        [TestMethod]
        public void ExecuteIssueTest()
        {
            StringBuilder builder = new StringBuilder();
            using (IDatabase conn = ViewboxApplication.Database.ConnectionManager.GetConnection())
            {
                List<IssueExtension> extensions = conn.DbMapping.Load<IssueExtension>();
                parameters = conn.DbMapping.Load<Parameter>();
                foreach (IssueExtension extension in extensions)
                {
                    string msg = ExecuteIssue(extension);
                    builder.Append(msg).Append(String.IsNullOrEmpty(msg) ? "" : Environment.NewLine);
                }
            }
            Assert.IsTrue(builder.Length == 0, builder.ToString());
        }

        #endregion TestMethods

        #region HelperMethods

        private string ExecuteIssue(IssueExtension extension)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Error while executing issue. (ref_id = ").Append(extension.RefId).Append(") - ");
            try
            {
                DataGridController controller = new DataGridController();
                CommandController commandController = new CommandController();
                PartialViewResult result =
                    controller.ExecuteIssue(extension.RefId, PrepareParameters(extension, controller)) as
                    PartialViewResult;
                if (result == null)
                    return builder.Append(" ExecuteIssue returned null").ToString();
                DialogModel model = result.Model as DialogModel;
                if (model == null)
                    return builder.Append(" Model is null").ToString();
                Transformation transformation = Transformation.Find(model.Key);
                if (transformation == null)
                    return builder.Append(" Can't find transformation").ToString();
                ViewResult tableResult = null;
                DataGridModel dataGridModel = null;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                while (watch.ElapsedMilliseconds < WaitSeconds*1000)
                {
                    IEnumerable<Base> jobs = ViewboxSession.GetFinishedJobs();
                    Base job = jobs.FirstOrDefault(w => w.Key == model.Key);

                    if (job != null)
                    {
                        transformation = job as Transformation;
                        if (transformation == null)
                            return builder.Append(" Can't find transformation after executed ").ToString();
                        Thread.Sleep(200);
                        if (transformation.Status == Base.JobStatus.Crashed)
                        {
                            Exception exc = Janitor.GetError(transformation.Key);
                            return
                                builder.Append(" Transformation crashed: ").Append(exc == null ? "" : exc.Message).
                                    ToString();
                        }
                        Exception e = Janitor.GetError(transformation.Key);
                        if (e != null)
                            return builder.Append(" Exception occured: ").Append(e.Message).ToString();

                        if (transformation.Status != Base.JobStatus.Finished)
                            return builder.Append(" Transformation have not finished successfully ").ToString();

                        if (transformation.TransformationObject == null)
                            return builder.Append(" Can't find transformation object ").ToString();
                        ViewboxSession.SetupObjects();
                        tableResult = controller.Index(transformation.TransformationObject.Table.Id) as ViewResult;
                        if (tableResult == null)
                            return builder.Append(" Can't find tableResult ").ToString();

                        dataGridModel = tableResult.Model as DataGridModel;
                        if (dataGridModel == null)
                            return builder.Append(" Can't find dataGridModel ").ToString();

                        if (dataGridModel.RowsCount == 0)
                            return builder.Append(" No rows returned ").ToString();
                        return "";
                    }
                }
                watch.Stop();
                if (tableResult != null && dataGridModel != null && dataGridModel.RowsCount > 0 &&
                    (dataGridModel.DataTable == null || dataGridModel.DataTable.Rows.Count == 0))
                    return builder.Append(" Rowcount in model is greater than 0 but no rows returned ").ToString();
                return builder.Append(" Executing time more than ").Append(WaitSeconds).Append(" s").ToString();
            }
            catch (Exception ex)
            {
                return builder.Append(" Exception: ").Append(ex.Message).ToString();
            }
        }

        private List<string> PrepareParameters(IssueExtension extension, DataGridController controller)
        {
            List<string> result = new List<string>();
            foreach (Parameter parameter in parameters.Where(w => w.IssueId == extension.RefId))
            {
                if (
                    String.IsNullOrEmpty(controller.ValidateIssueParameterInput(extension.RefId, parameter.Id,
                                                                                parameter.Default ?? "")))
                    result.Add(parameter.Default ?? "");
            }
            return result;
        }

        #endregion HelperMethods
    }
}