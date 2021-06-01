using System.Linq;
using System.Web.Mvc;
using SystemDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;
using Viewbox.Job;
using ViewboxDb;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class ExportControllerTest : BaseControllerTest<ExportController>
    {
        [TestMethod]
        public void GetTableCollectionTest()
        {
            ICategoryCollection result = Controller.GetTableCollection(TableType.View);
            Assert.IsNotNull(result);
            result = Controller.GetTableCollection(TableType.Table);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void IndexTest()
        {
            ActionResult result = Controller.Index();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ExportTableOverviewTest()
        {
            ActionResult result = Controller.ExportTableOverview(TableType.Table, Context.TestTable3.Id);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TableObjectsTest()
        {
            ActionResult result = Controller.TableObjects(TableType.Table);
            Assert.IsNotNull(result);
            result = Controller.TableObjects(TableType.View, -1, 0, true, "test");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ColumnsTest()
        {
            ActionResult result = Controller.Columns(Context.TestTable3.Id);
            Assert.IsNotNull(result);
            result = Controller.Columns(Context.TestTable3.Id, true, "test");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void StartTest()
        {
            ActionResult result = Controller.Start(Context.TestTable3.Id, ExportType.PDF);
            Assert.IsNotNull(result);
            result = Controller.Start(Context.TestTable3.Id, ExportType.Excel);
            Assert.IsNotNull(result);
            result = Controller.Start(Context.TestTable3.Id, ExportType.GDPdU);
            Assert.IsNotNull(result);
            result = Controller.Start(Context.TestTable3.Id, ExportType.HTML);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void StartPdfUserDefinedTest()
        {
            ActionResult result = Controller.StartPdfUserDefined(Context.TestTable3.Id,
                                                                 Export.UserDefinedExportProjects.Actebis);
            Assert.IsNotNull(result);
            result = Controller.StartPdfUserDefined(Context.TestTable3.Id,
                                                    Export.UserDefinedExportProjects.HofmeisterLieferschein);
            Assert.IsNotNull(result);
            result = Controller.StartPdfUserDefined(Context.TestTable3.Id,
                                                    Export.UserDefinedExportProjects.HofmeisterRechnung);
            Assert.IsNotNull(result);
            result = Controller.StartPdfUserDefined(Context.TestTable3.Id,
                                                    Export.UserDefinedExportProjects.HofmeisterSofortRechnung);
            Assert.IsNotNull(result);
            result = Controller.StartPdfUserDefined(Context.TestTable3.Id, Export.UserDefinedExportProjects.MKN);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void MassExportTest()
        {
            ActionResult result = Controller.MassExport("", ExportType.Excel, "test", false);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RefreshJobsTest()
        {
            ActionResult result = Controller.RefreshJobs("test");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ExportTypeSelectionTest()
        {
            ActionResult result = Controller.ExportTypeSelection("");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DownloadTest()
        {
            Export export = Export.List.FirstOrDefault();
            if (export != null)
            {
                ActionResult result = Controller.Download(export.Key);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void RunningJobsTest()
        {
            ActionResult result = Controller.RunningJobs();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CancelOtherUserJobTest()
        {
            Controller.Start(Context.TestTable3.Id, ExportType.PDF);
            Export export = Export.List.FirstOrDefault();
            if (export != null)
            {
                ActionResult result = Controller.CancelOtherUserJob(export.Key);
                Assert.IsNotNull(result);
            }
            Controller.Start(Context.TestTable.Id, ExportType.PDF);
            export = Export.List.FirstOrDefault();
            if (export != null)
            {
                ActionResult result = Controller.CancelOtherUserJob(export.Key, true);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void CancelJobTest()
        {
            Controller.Start(Context.TestTable3.Id, ExportType.PDF);
            Export export = Export.List.FirstOrDefault();
            if (export != null)
            {
                ActionResult result = Controller.CancelJob(export.Key);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void ExportJobsTest()
        {
            Controller.Start(Context.TestTable3.Id, ExportType.PDF);
            ActionResult result = Controller.ExportJobs("test");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DeleteExportTest()
        {
            Controller.Start(Context.TestTable3.Id, ExportType.PDF);
            Export export = Export.List.FirstOrDefault();
            if (export != null)
            {
                Controller.DeleteExport(export.Key);
            }
        }

        [TestMethod]
        public void AJobThatsRunningLongAndDoesNothingTest()
        {
            ActionResult result = Controller.AJobThatsRunningLongAndDoesNothing();
            Assert.IsNotNull(result);
        }

// GenerateCode
    }
}