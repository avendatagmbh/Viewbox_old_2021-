using System.Linq;
using System.Web.Mvc;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;
using Viewbox.Models;
using ViewboxDb;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class UserManagementControllerTest : BaseControllerTest<UserManagementController>
    {
        [TestMethod]
        public void IndexTest()
        {
            ActionResult result = Controller.Index();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TableVisibilityCopyTest()
        {
            ActionResult result = Controller.TableVisibilityCopy();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreateTableSettingsCopyDialogTest()
        {
            ActionResult result = Controller.CreateTableSettingsCopyDialog();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreateTableTypeRightSwitchDialogTest()
        {
            ActionResult result = Controller.CreateTableTypeRightSwitchDialog("Table");
            Assert.IsNotNull(result);
            result = Controller.CreateTableTypeRightSwitchDialog("View");
            Assert.IsNotNull(result);
            result = Controller.CreateTableTypeRightSwitchDialog("Issue");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TableVisibilityCopy1Test()
        {
            ActionResult result = Controller.TableVisibilityCopy1();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void get_UserOptionsLangTest()
        {
            //Controller.get_UserOptionsLang();
        }

        private int GetTestUser()
        {
            using (IDatabase conn = ViewboxApplication.Database.ConnectionManager.GetConnection())
            {
                User user = conn.DbMapping.Load<User>().FirstOrDefault(w => w.UserName == "unittest");
                return user == null ? 0 : user.Id;
            }
        }

        [TestMethod]
        public void CreateUserTest()
        {
            int testUser = GetTestUser();
            ActionResult result = null;
            if (testUser != 0)
            {
                result = Controller.DeleteUser(testUser);
                Assert.IsNotNull(result);
            }
            result = Controller.CreateUser();
            Assert.IsNotNull(result);
            result = Controller.CreateUser("unittest", "unittest", "unittest", "unittest", "unittest@unittest.com", "");
            Assert.IsNotNull(result);
            ViewboxApplication.Database.SystemDb.PerformChanges();
            testUser = GetTestUser();
            Assert.AreNotEqual(testUser, 0);
            result = Controller.DeleteUser(testUser);
            Assert.IsNotNull(result);
            ViewboxApplication.Database.SystemDb.PerformChanges();
        }

        [TestMethod]
        public void UpdateUserTest()
        {
            // In CreateUserTest
        }

        [TestMethod]
        public void DeleteUserTest()
        {
            // In CreateUserTest
        }

        [TestMethod]
        public void CreateRoleDialogModelTest()
        {
            DialogModel result = UserManagementController.CreateRoleDialogModel();
            Assert.IsNotNull(result);
            //Controller.CreateRoleDialogModel();
        }

        [TestMethod]
        public void CreateRoleDialogTest()
        {
            ActionResult result = Controller.CreateRoleDialog();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreateOptimizationDeleteDialogTest()
        {
            ActionResult result = Controller.CreateOptimizationDeleteDialog();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreateInfoDialogModelTest()
        {
            DialogModel result = Controller.CreateInfoDialogModel();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreateInfoDialogTest()
        {
            ActionResult result = Controller.CreateInfoDialog();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreateInfoTest()
        {
            ActionResult result = Controller.CreateInfo();
            Assert.IsNotNull(result);
        }

        private int GetTestRole()
        {
            using (IDatabase conn = ViewboxApplication.Database.ConnectionManager.GetConnection())
            {
                Role role = conn.DbMapping.Load<Role>().FirstOrDefault(w => w.Name == "unittest_role");
                return role == null ? 0 : role.Id;
            }
        }

        [TestMethod]
        public void CreateRoleTest()
        {
            ActionResult result;
            int role = GetTestRole();
            if (role != 0)
            {
                result = Controller.DeleteRole(role);
            }
            result = Controller.CreateRole("unittest_role", "");
            Assert.IsNotNull(result);
            role = GetTestRole();
            Assert.AreNotEqual(role, 0);
            if (role != 0)
            {
                result = Controller.DeleteRole(role);
                Assert.IsNotNull(result);
            }
            ViewboxApplication.Database.SystemDb.PerformChanges();
        }

        [TestMethod]
        public void UpdateRoleTest()
        {
            // In CreateRoleTest
        }

        [TestMethod]
        public void DeleteRoleTest()
        {
            // In CreateRoleTest
        }

        [TestMethod]
        public void SwitchModeTest()
        {
            ActionResult result = Controller.SwitchMode(Context.TestUser.Id, CredentialType.User);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void UpdateOptimizationRightTest()
        {
            if (Context.TestOptimization == null)
                return;
            ActionResult result = Controller.SwitchMode(Context.TestUser.Id, CredentialType.User);
            Assert.IsNull(result);
            result = Controller.UpdateOptimizationRight(Context.TestOptimization.Id, RightType.Read, "");
            Assert.IsNotNull(result);
            result = Controller.UpdateOptimizationRight(Context.TestOptimization.Id, RightType.None, "");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UpdateCategoryRightTest()
        {
            if (Context.TestCategory == null)
                return;
            ActionResult result = Controller.SwitchMode(Context.TestUser.Id, CredentialType.User);
            Assert.IsNull(result);
            result = Controller.UpdateCategoryRight(Context.TestCategory.Id, RightType.Read, TableType.Issue);
            Assert.IsNotNull(result);
            result = Controller.UpdateCategoryRight(Context.TestCategory.Id, RightType.None, TableType.Issue);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UpdateTableTypeRightNavigationBarTest()
        {
            ActionResult result = Controller.UpdateTableTypeRightNavigationBar(RightType.Inherit, TableType.Table);
            Assert.IsNotNull(result);
            result = Controller.UpdateTableTypeRightNavigationBar(RightType.None, TableType.Table);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UpdateTableTypeRightJobTest()
        {
            ActionResult result = Controller.SwitchMode(Context.TestUser.Id, CredentialType.User);
            Assert.IsNull(result);
            result = Controller.UpdateTableTypeRightJob(Context.TestUser.Id, RightType.Inherit, TableType.Issue);
            Assert.IsNotNull(result);
            Controller.UpdateTableTypeRightJob(Context.TestUser.Id, RightType.None, TableType.Issue);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UpdateTableTypeRightTest()
        {
            ActionResult result = Controller.SwitchMode(Context.TestUser.Id, CredentialType.User);
            Assert.IsNull(result);
            result = Controller.UpdateTableTypeRight(Context.TestUser.Id, RightType.Inherit, TableType.Issue);
            Assert.IsNotNull(result);
            Controller.UpdateTableTypeRight(Context.TestUser.Id, RightType.None, TableType.Issue);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UpdateTableObjectRightTest()
        {
            ActionResult result = Controller.SwitchMode(Context.TestUser.Id, CredentialType.User);
            Assert.IsNull(result);
            result = Controller.UpdateTableObjectRight(Context.TestTable.Id, RightType.Read);
            Assert.IsNotNull(result);
            result = Controller.UpdateTableObjectRight(Context.TestTable.Id, RightType.None);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UpdateReportListObjectRightTest()
        {
            ActionResult result = Controller.SwitchMode(Context.TestUser.Id, CredentialType.User);
            Assert.IsNull(result);
            result = Controller.UpdateReportListObjectRight(Context.TestTable.Id, RightType.Read);
            Assert.IsNotNull(result);
            result = Controller.UpdateReportListObjectRight(Context.TestTable.Id, RightType.None);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UpdateColumnRightTest()
        {
            ActionResult result = Controller.SwitchMode(Context.TestUser.Id, CredentialType.User);
            Assert.IsNull(result);
            result = Controller.UpdateColumnRight(Context.TestColumn.Id, RightType.None);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UpdateLogRightsTest()
        {
            ActionResult result = Controller.SwitchMode(Context.TestUser.Id, CredentialType.User);
            Assert.IsNull(result);
            result = Controller.UpdateLogRights(Context.TestUser.Id, RightType.Write);
            Assert.IsNotNull(result);
            result = Controller.UpdateLogRights(Context.TestUser.Id, RightType.None);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UserAddRoleTest()
        {
            ActionResult result = Controller.SwitchMode(Context.TestUser.Id, CredentialType.User);
            Assert.IsNull(result);
            using (IDatabase conn = ViewboxApplication.Database.ConnectionManager.GetConnection())
            {
                var userRole = conn.DbMapping.Load<UserRoleMapping>().Where(w => w.UserId == Context.TestUser.Id);
                var role = conn.DbMapping.Load<Role>().FirstOrDefault(w => userRole.All(d => d.RoleId != w.Id));
                result = Controller.UserAddRole(Context.TestUser.Id, role.Id);
                Assert.IsNotNull(result);
                result = Controller.UserRemoveRole(Context.TestUser.Id, role.Id);
                Assert.IsNotNull(result);
            }
        }

        /*
		[TestMethod]
        public void UserRemoveRoleTest()
        {
            // In UserAddRoleTest
		}
        */

        [TestMethod]
        public void CheckPasswordTest()
        {
            ActionResult result = Controller.CheckPassword("test");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ImportUserFromADDialogTest()
        {
            ActionResult result = Controller.ImportUserFromADDialog();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetADUserListPartialTest()
        {
            /*ActionResult result = Controller.GetADUserListPartial("localhost", "test");
            Assert.IsNotNull(result);*/
        }

        [TestMethod]
        public void ImportUsersFromADTest()
        {
            ActionResult result = Controller.ImportUsersFromAD(new ADUserCollection());
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetEmptyFilterSecurityAdviseTest()
        {
            ActionResult result = Controller.GetEmptyFilterSecurityAdvise();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ShowErrorMessageTest()
        {
            ActionResult result = Controller.ShowErrorMessage("error");
            Assert.IsNotNull(result);
        }

// GenerateCode
    }
}