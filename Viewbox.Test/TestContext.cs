using System.Diagnostics;
using System.Linq;
using System.Threading;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;
using Viewbox.Test.Mock;

namespace Viewbox.Test
{
    public class TestContext
    {
        private static TestContext _singleton;

        private TestContext()
        {
            HttpContextFactory.SetCurrentContext(new HttpContextFactoryMock());
            Stopwatch watch = new Stopwatch();
            ViewboxApplication.Init(new HttpApplicationMock());
            watch.Start();
            while (!ViewboxApplication.Initialized && watch.ElapsedMilliseconds < 30000)
            {
                Thread.Sleep(200);
            }
            watch.Stop();
            Assert.IsTrue(ViewboxApplication.Initialized);
            InitSession();
        }

        public TableObject TestTable { get; set; }
        public TableObject TestTable2 { get; set; }
        public TableObject TestTable3 { get; set; }
        public Column TestColumn { get; set; }
        public Column TestColumn2 { get; set; }
        public Column TestColumn3 { get; set; }
        public User TestUser { get; set; }
        public Role TestRole { get; set; }
        public Category TestCategory { get; set; }
        public Optimization TestOptimization { get; set; }
        public Parameter TestParameter { get; set; }

        public static TestContext GetInstance()
        {
            return _singleton ?? (_singleton = new TestContext());
        }

        public void InitSession()
        {
            Stopwatch watch = new Stopwatch();
            if (!ViewboxSession.IsInitialized)
            {
                ViewboxSession.Init();
                watch.Start();
                while (!ViewboxSession.IsInitialized && watch.ElapsedMilliseconds < 30000)
                {
                    Thread.Sleep(200);
                }
                watch.Stop();
            }
            Assert.IsTrue(ViewboxSession.IsInitialized);
            ViewboxSession.SetupObjects();
            if (ViewboxSession.TableObjects.Count == 0)
            {
                IssueListController issueListController = new IssueListController();
                issueListController.Index();
                TableListController tableListController = new TableListController();
                tableListController.Index();
                ViewListController viewListController = new ViewListController();
                viewListController.Index();
            }
            using (IDatabase conn = ViewboxApplication.Database.ConnectionManager.GetConnection())
            {
                var columns = conn.DbMapping.Load<Column>();
                if (TestTable == null)
                    TestTable = conn.DbMapping.Load<Issue>("type = " + (int) TableType.Issue).FirstOrDefault();
                if (TestTable == null)
                    return;
                if (TestColumn == null)
                    TestColumn = columns.FirstOrDefault(w => w.TableId == TestTable.Id);
                if (TestColumn == null)
                    return;
                if (TestTable2 == null)
                    TestTable2 =
                        conn.DbMapping.Load<Issue>("type = " + (int) TableType.Issue).FirstOrDefault(
                            w => w.Id != TestTable.Id);
                if (TestTable2 == null)
                    return;
                if (TestColumn2 == null)
                    TestColumn2 = columns.FirstOrDefault(w => w.TableId == TestTable2.Id);
                if (TestColumn2 == null)
                    return;
                if (TestTable3 == null)
                    TestTable3 = conn.DbMapping.Load<Table>("type = " + (int) TableType.Table).FirstOrDefault();
                if (TestTable3 == null)
                    return;

                if (TestColumn3 == null)
                    TestColumn3 = columns.FirstOrDefault(w => w.TableId == TestTable3.Id);
                if (TestColumn3 == null)
                    return;
                if (TestUser == null)
                    TestUser = conn.DbMapping.Load<User>().FirstOrDefault(w => w.UserName == "avendata_admin");
                if (TestUser == null)
                    return;
                if (TestUser == null || TestRole == null)
                {
                    var role = conn.DbMapping.Load<UserRoleMapping>().FirstOrDefault(w => w.UserId != TestUser.Id);
                    TestUser = conn.DbMapping.Load<User>().FirstOrDefault(w => w.Id == role.UserId);
                    if (TestUser == null)
                        return;
                    TestRole = conn.DbMapping.Load<Role>().FirstOrDefault(w => w.Id == role.RoleId);
                    if (TestRole == null)
                        return;
                }
                if (TestCategory == null)
                    TestCategory = conn.DbMapping.Load<Category>().FirstOrDefault();
                if (TestOptimization == null)
                    TestOptimization = conn.DbMapping.Load<Optimization>().FirstOrDefault();
                if (TestParameter == null)
                    TestParameter = conn.DbMapping.Load<Parameter>().FirstOrDefault();
            }
            if (TestTable != null)
            {
                if (!ViewboxSession.TableObjects.Contains(TestTable.Id))
                    ViewboxSession.AddTableObject(TestTable.Id);
                if (ViewboxSession.Columns.All(w => w.Table.Id != TestTable.Id))
                    ViewboxSession.SetupTableColumns(TestTable.Id);
            }
            if (TestTable2 != null)
            {
                if (!ViewboxSession.TableObjects.Contains(TestTable2.Id))
                    ViewboxSession.AddTableObject(TestTable2.Id);
                if (ViewboxSession.Columns.All(w => w.Table.Id != TestTable2.Id))
                    ViewboxSession.SetupTableColumns(TestTable2.Id);
            }
            if (TestTable3 != null)
            {
                if (!ViewboxSession.TableObjects.Contains(TestTable3.Id))
                    ViewboxSession.AddTableObject(TestTable3.Id);
                if (ViewboxSession.Columns.All(w => w.Table.Id != TestTable3.Id))
                    ViewboxSession.SetupTableColumns(TestTable3.Id);
            }
        }
    }
}