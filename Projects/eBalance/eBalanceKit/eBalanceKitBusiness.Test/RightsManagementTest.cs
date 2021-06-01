using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping.Rights;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Test
{
    /// <summary>
    /// Unit tests for rights management
    /// </summary>
    /// <author>Gabor Bauer</author>
    /// <since>2012-04-18</since>
    [TestClass]
    public class RightsManagementTest
    {
        #region [ Init ]

        private TestContext testContextInstance;
        
        public TestContext TestContext { get { return testContextInstance; } set { testContextInstance = value; } }

        [ClassInitialize()]
        public static void RightsManagementTestInitialize(TestContext testContext)
        {
            TestHelper.Init();
        }

        #endregion [ Init ]

        /// <summary>
        /// Basic test case for checking User.AllowUserManagement and User.EditableUsers collection without manipulating rights
        /// </summary>
        [TestMethod]
        public void Rights_AllowUserManagementTest()
        {
            User adminUser = TestHelper.LogonAsAdmin();
            User companyAdmin = null;
            List<User> plainUsers = new List<User>();
            Company company = null;

            try
            {
                #region [ Prepare ]

                company = TestHelper.CreateCompany();
                companyAdmin = TestHelper.CreateUser(true);

                for (int i = 1; i < 11; i++)
                {
                    plainUsers.Add(TestHelper.CreateUser(false));
                }
                
                int numUsers = 10;
                bool adminFound = false;
                bool companyAdminFound = false;

                #endregion [ Prepare ]

                foreach (User user in UserManager.Instance.Users)
                {
                    if (user.Id == adminUser.Id)
                    {
                        // admin
                        adminFound = true;
                        Assert.IsTrue(user.IsAdmin, TestResource.UserShouldBeAdmin);
                        Assert.IsTrue(!user.IsCompanyAdmin, TestResource.UserShouldNotBeCompanyAdmin);
                        Assert.IsTrue(user.AllowUserManagement, TestResource.AllowUserManagementShouldBeTrue);

                        // check all editalbe users
                        foreach (User editableUser in UserManager.Instance.EditableUsers) {
                            Assert.IsTrue(CheckEditableUser(adminUser, editableUser));
                        }
                    }
                    else if (user.Id == companyAdmin.Id)
                    {
                        // company admin
                        companyAdminFound = true;
                        Assert.IsTrue(!user.IsAdmin, TestResource.UserShouldNotBeAnAdministrator);
                        Assert.IsTrue(user.IsCompanyAdmin, TestResource.UserShouldBeCompanyAdmin);
                        // AllowUserManagement is false because no companies were assigned
                        Assert.IsTrue(!user.AllowUserManagement);
                        user.AssignedCompanies.Add(new CompanyInfo(company));
                        // now AllowUserManagement shold be true
                        Assert.IsTrue(user.AllowUserManagement, TestResource.AllowUserManagementShouldBeTrue);

                        // check it has all editable users it should have (plainUsers)
                        foreach (User editableUser in plainUsers) {
                            Assert.IsTrue(CheckEditableUser(companyAdmin, editableUser));
                        }
                    }
                    else
                    {
                        if (plainUsers.Any(u => u.Id == user.Id))
                        {
                            // previously created non admin and non company admin users
                            numUsers -= 1;
                            Assert.IsTrue(!user.IsAdmin, TestResource.UserShouldNotBeAnAdministrator);
                            Assert.IsTrue(!user.IsCompanyAdmin, TestResource.UserShouldNotBeAnAdministrator);
                            Assert.IsTrue(!user.AllowUserManagement, TestResource.UserShouldNotBeAnAdministrator);
                            Assert.IsTrue(user.AssignedCompanies.Count == 0, TestResource.UserShouldNotHaveAssignedCompanies);
                            Assert.IsTrue(user.EditableRoles.Count == 0, TestResource.UserShouldNotHaveAssignedCompanies);
                            Assert.IsTrue(user.EditableUsers.Count == 0, TestResource.UserShouldNotHaveAssignedCompanies);
                        }
                    }
                }
                Assert.IsTrue(numUsers == 0);
                Assert.IsTrue(adminFound);
                Assert.IsTrue(companyAdminFound);
            }
            catch (Exception) {
                throw;
            }
            #region [ Cleanup ]
            finally {
                try {
                    if (UserManager.Instance.CurrentUser.UserName != adminUser.UserName)
                        TestHelper.LogonAsUser(adminUser);
                } catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
                try {
                    if (companyAdmin != null)
                        TestHelper.DeleteUser(companyAdmin);
                } catch
                {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
                foreach (User user in plainUsers) {
                    try {
                        TestHelper.DeleteUser(user);
                    } catch {
                        //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                    }
                }
                if (company != null) {
                    try {
                        TestHelper.DeleteCompany(company);
                    } catch (Exception) {
                        //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                    }
                }
            }
            #endregion [ Cleanup ]
        }

        /// <summary>
        /// Advanced test case for checking company administrator user's User.EditableUsers collection with manipulating rights and company assignments
        /// </summary>
        [TestMethod]
        public void Rights_EditableUsersTest() {

            Structures.DbMapping.System system = null;
            User adminUser = TestHelper.LogonAsAdmin();
            User companyAdmin = null;
            List<User> plainUsers = new List<User>();
            List<Company> companies = new List<Company>();
            List<Company> administeredCompanies = new List<Company>();
            List<Document> documents = new List<Document>();
            List<Role> roles = new List<Role>();
            
            try {
                
                #region [ Prepare ]
                
                system = TestHelper.CreateSystem();

                companyAdmin = TestHelper.CreateUser(true);
                for (int i = 1; i < 5; i++)
                {
                    plainUsers.Add(TestHelper.CreateUser(false));
                }
                for (int i = 1; i < 5; i++) {
                    companies.Add(TestHelper.CreateCompany());
                }

                #endregion [ Prepare ]

                Company company1 = companies[0];
                User user1 = plainUsers[0];
                Company company2 = companies[1];
                User user2 = plainUsers[1];
                Company company3 = companies[2];
                User user3 = plainUsers[2];
                Company company4 = companies[3];
                User user4 = plainUsers[3];

                // company admin is only able to see roles and users who has rights to one of the companies of AssignedCompanies, if a role or user has rights to 
                // an another company (CompanyManager.NotAllowedCompanies) then the company admin sholud not see the role or user
                // the same for user with assigned roles which role is not allowed according to the above, then the user itself should be also not allowed to see
                administeredCompanies.Add(company1);
                companyAdmin.AssignedCompanies.Add(new CompanyInfo(company1));
                TestHelper.LogonAsUser(user1);
                TestHelper.AssignUserToCompany(company1);
                documents.Add(TestHelper.CreateDocument());

                #region [ Test scenario 1 ]
                // user2 gets right to company1 => user2 still appears EditableUsers collection of company admin
                Right actualRight = RoleManager.GetUserRole(user2).GetRight(company1);
                actualRight.Read = true;
                RoleManager.GetUserRole(user2).GetRight(company1).SetRights(actualRight);
                RoleManager.GetUserRole(user2).Save();
                TestHelper.LogonAsUser(companyAdmin);
                Assert.IsTrue(CheckEditableUser(companyAdmin, user1), "test scenario 1/1 failed");
                Assert.IsTrue(CheckEditableUser(companyAdmin, user2), "test scenario 1/2 failed");
                #endregion [ Test scenario 1 ]

                #region [ Test scenario 2 ]
                // user2 gets right to company2 => user2 no longer appears in the EditableUsers collection of company admin
                Right otherRight = RoleManager.GetUserRole(user2).GetRight(company2);
                otherRight.Read = true;
                RoleManager.GetUserRole(user2).GetRight(company2).SetRights(otherRight);
                RoleManager.GetUserRole(user2).Save();
                TestHelper.LogonAsUser(companyAdmin);
                Assert.IsTrue(CheckEditableUser(companyAdmin, user1), "test scenario 2/1 failed");
                Assert.IsTrue(CheckEditableUser(companyAdmin, user2, false), "test scenario 2/2 failed");
                #endregion [ Test scenario 2 ]

                #region [ Test scenario 3 ]
                // user3 gets right to company1 => user3 still appears EditableUsers collection of company admin
                Right right3 = RoleManager.GetUserRole(user3).GetRight(company1);
                right3.Read = true;
                RoleManager.GetUserRole(user3).GetRight(company1).SetRights(right3);
                RoleManager.GetUserRole(user3).Save();
                TestHelper.LogonAsUser(companyAdmin);
                Assert.IsTrue(CheckEditableUser(companyAdmin, user3), "test scenario 3/1 failed");

                // user3 gets right to company3 => user3 no longer appears in the EditableUsers collection of company admin
                TestHelper.LogonAsUser(user3);
                TestHelper.AssignUserToCompany(company3);
                Document document3 = TestHelper.CreateDocument();
                documents.Add(document3);
                Right right4 = RoleManager.GetUserRole(user3).GetRight(document3);
                right4.Read = true;
                RoleManager.GetUserRole(user3).GetRight(company3).SetRights(right4);
                RoleManager.GetUserRole(user3).Save();
                TestHelper.LogonAsUser(companyAdmin);
                Assert.IsTrue(CheckEditableUser(companyAdmin, user3, false), "test scenario 3/2 failed");
                #endregion [ Test scenario 3 ]

                #region [ Test scenario 4 ]
                // user4 gets right to company1 => user4 still appears EditableUsers collection of company admin
                Right right5 = RoleManager.GetUserRole(user4).GetRight(company1);
                right5.Read = true;
                RoleManager.GetUserRole(user4).GetRight(company1).SetRights(right5);
                RoleManager.GetUserRole(user4).Save();
                TestHelper.LogonAsUser(companyAdmin);
                Assert.IsTrue(CheckEditableUser(companyAdmin, user4), "test scenario 4/1 failed");

                // user4 gets assigned with role1 which has rights to company4 => user4 no longer appears in the EditableUsers collection of company admin
                Role role1 = new Role(new DbRole()) {
                    DbRole = {
                        DoDbUpdate = false,
                        UserId = user4.Id,
                        Name = "role_" + Guid.NewGuid().ToString("N"),
                        Comment = null
                    }
                };
                role1.DbRole.DoDbUpdate = true;
                role1.DbRole.Save();
                roles.Add(role1);
                RoleManager.Roles.Add(role1);
                Right right6 = RoleManager.GetRole(role1.DbRole.Id).GetRight(company4);
                right6.Read = true;
                RoleManager.GetRole(role1.DbRole.Id).GetRight(company4).SetRights(right6);
                RoleManager.GetRole(role1.DbRole.Id).Save();
                //TODO: this is a workaround calling it twice, some crayz thing happens, will investigate it later
                TestHelper.LogonAsUser(companyAdmin);
                TestHelper.LogonAsUser(companyAdmin);
                Assert.IsTrue(CheckEditableUser(companyAdmin, user4, false), "test scenario 4/2 failed");
                #endregion [ Test scenario 4 ]

            } catch (Exception)
            {
                throw;
            }
            #region [ Cleanup ]
            finally {
                try {
                    if (UserManager.Instance.CurrentUser.UserName != adminUser.UserName)
                        TestHelper.LogonAsUser(adminUser);
                } catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
                try {
                    if (companyAdmin != null)
                        TestHelper.DeleteUser(companyAdmin);
                } catch
                {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
                foreach (Document document in documents) {
                    try {
                        TestHelper.DeleteDocument(document);
                    } catch {
                        //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                    }
                }
                foreach (User user in plainUsers) {
                    try {
                        TestHelper.DeleteUser(user);
                    } catch {
                        //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                    }
                }
                foreach (Company company in companies) {
                    try {
                        TestHelper.DeleteCompany(company);
                    } catch (Exception) {
                        //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                    }
                }
                foreach (Role role in roles) {
                    try {
                        TestHelper.DeleteRole(role);
                    } catch (Exception) {
                        //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                    }
                }
                try
                {
                    if (system != null)
                        TestHelper.DeleteSystem(system);
                }
                catch
                {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
            }
            #endregion [ Cleanup ]   
        }

        /// <summary>
        /// Advanced test case for checking tooltip data for effective rights
        /// </summary>
        [TestMethod]
        public void Rights_ToolTipTest() {
            string role1Name = "role1_" + Guid.NewGuid().ToString("N");
            string role2Name = "role2_" + Guid.NewGuid().ToString("N");
            Structures.DbMapping.System system = null;
            User adminUser = TestHelper.LogonAsAdmin();
            List<User> plainUsers = new List<User>();
            List<Company> companies = new List<Company>();
            List<Document> documents = new List<Document>();
            List<Role> roles = new List<Role>();

            try {

                TestHelper.InitSystem();

                #region [ Prepare ]
                
                system = TestHelper.CreateSystem();

                for (int i = 1; i < 3; i++) {
                    plainUsers.Add(TestHelper.CreateUser(false));
                }
                for (int i = 1; i < 4; i++) {
                    companies.Add(TestHelper.CreateCompany());
                }
                
                #endregion [ Prepare ]

                Company company1 = companies[0];
                User user1 = plainUsers[0];
                Company company2 = companies[1];
                User user2 = plainUsers[1];
                Company company3 = companies[2];
                company3.Owner = user1;

                // document1
                documents.Add(TestHelper.CreateDocument(company1));
                Document document1 = documents[0];
                document1.FinancialYear = company1.VisibleFinancialYears[0];
                // document2
                Right user1CompanyRight = RoleManager.GetUserRole(user1).GetRight(company2);
                user1CompanyRight.Read = true;
                RoleManager.GetUserRole(user1).GetRight(company2).SetRights(user1CompanyRight);
                RoleManager.GetUserRole(user1).Save();
                documents.Add(TestHelper.CreateDocument(company2));
                Document document2 = documents[1];
                document2.Owner = user1;
                document2.FinancialYear = company2.VisibleFinancialYears[0];
                                
                EffectiveUserRightTreeNode effectiveRightsAdmin = RightManager.GetEffectiveUserRightTree(adminUser);
                // root node rights are not set except read, write and grant (for admin)
                Assert.IsFalse(IsRightNotSet(effectiveRightsAdmin, DbRight.RightTypes.Read), "Case 0/1");
                Assert.IsFalse(IsRightNotSet(effectiveRightsAdmin, DbRight.RightTypes.Write), "Case 0/2");
                Assert.IsTrue(IsRightNotSet(effectiveRightsAdmin, DbRight.RightTypes.Export), "Case 0/3");
                Assert.IsFalse(IsRightNotSet(effectiveRightsAdmin, DbRight.RightTypes.Grant), "Case 0/4");
                Assert.IsTrue(IsRightNotSet(effectiveRightsAdmin, DbRight.RightTypes.Send), "Case 0/5");

                #region [ Check admin's owner and other rights ]

                // Company
                EffectiveUserRightTreeNode nodeCompany = GetNode(effectiveRightsAdmin, company1);
                // the user has owner rights for read and write and not for the others
                Assert.IsTrue(HasAdminRight(nodeCompany, DbRight.RightTypes.Read, adminUser), "Case 1/1");
                Assert.IsTrue(HasAdminRight(nodeCompany, DbRight.RightTypes.Write, adminUser), "Case 1/2");
                Assert.IsTrue(IsGranted(nodeCompany, DbRight.RightTypes.Export, adminUser), "Case 1/3");
                Assert.IsTrue(HasAdminRight(nodeCompany, DbRight.RightTypes.Grant, adminUser), "Case 1/4");
                Assert.IsTrue(IsGranted(nodeCompany, DbRight.RightTypes.Send, adminUser), "Case 1/5");
                // company rights are not inherited (explicitly set or owner)
                Assert.IsFalse(HasInheritedRight(nodeCompany, DbRight.RightTypes.Read, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 2/1");
                Assert.IsFalse(HasInheritedRight(nodeCompany, DbRight.RightTypes.Write, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 2/2");
                Assert.IsFalse(HasInheritedRight(nodeCompany, DbRight.RightTypes.Export, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 2/3");
                Assert.IsFalse(HasInheritedRight(nodeCompany, DbRight.RightTypes.Grant, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 2/4");
                Assert.IsFalse(HasInheritedRight(nodeCompany, DbRight.RightTypes.Send, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 2/5");
                // company rights are explicitly set
                Assert.IsFalse(IsRightNotSet(nodeCompany, DbRight.RightTypes.Read), "Case 3/1");
                Assert.IsFalse(IsRightNotSet(nodeCompany, DbRight.RightTypes.Write), "Case 3/2");
                Assert.IsFalse(IsRightNotSet(nodeCompany, DbRight.RightTypes.Export), "Case 3/3");
                Assert.IsFalse(IsRightNotSet(nodeCompany, DbRight.RightTypes.Grant), "Case 3/4");
                Assert.IsFalse(IsRightNotSet(nodeCompany, DbRight.RightTypes.Send), "Case 3/5");
                Assert.IsFalse(IsGranted(nodeCompany, DbRight.RightTypes.Read, adminUser), "Case 4/1");
                Assert.IsFalse(IsGranted(nodeCompany, DbRight.RightTypes.Write, adminUser), "Case 4/2");
                Assert.IsFalse(IsDenied(nodeCompany, DbRight.RightTypes.Read, adminUser), "Case 4/3");
                Assert.IsFalse(IsDenied(nodeCompany, DbRight.RightTypes.Write, adminUser), "Case 4/4");

                // own Document
                EffectiveUserRightTreeNode nodeDocument = GetNode(effectiveRightsAdmin, company1, document1.FinancialYear, document1);
                //Assert.IsTrue(HasInheritedRight(nodeDocument, DbRight.RightTypes.Read, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 5/1");
                //Assert.IsTrue(HasInheritedRight(nodeDocument, DbRight.RightTypes.Write, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 5/2");
                Assert.IsTrue(HasAdminRight(nodeDocument, DbRight.RightTypes.Read, adminUser), "Case 5/1");
                Assert.IsTrue(HasAdminRight(nodeDocument, DbRight.RightTypes.Write, adminUser), "Case 5/2");
                Assert.IsTrue(IsGranted(nodeDocument, DbRight.RightTypes.Export, adminUser), "Case 5/3");
                Assert.IsTrue(HasAdminRight(nodeDocument, DbRight.RightTypes.Grant, adminUser), "Case 5/4");
                Assert.IsTrue(IsGranted(nodeDocument, DbRight.RightTypes.Send, adminUser), "Case 5/5");
                try {
                    HasOwnerRight(nodeDocument, DbRight.RightTypes.Count, adminUser);
                    Assert.Fail("DbRight.RightTypes.Count is implemented, please modify the test case!");
                } catch (NotImplementedException) {
                    // expected
                }
                // document rights are not inherited (explicitly set or admin)
                Assert.IsFalse(HasInheritedRight(nodeDocument, DbRight.RightTypes.Read, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 6/1");
                Assert.IsFalse(HasInheritedRight(nodeDocument, DbRight.RightTypes.Write, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 6/2");
                Assert.IsFalse(HasInheritedRight(nodeDocument, DbRight.RightTypes.Export, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 6/3");
                Assert.IsFalse(HasInheritedRight(nodeDocument, DbRight.RightTypes.Grant, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 6/4");
                Assert.IsFalse(HasInheritedRight(nodeDocument, DbRight.RightTypes.Send, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 6/5");
                // document rights are explicitly set
                Assert.IsFalse(IsRightNotSet(nodeDocument, DbRight.RightTypes.Read), "Case 7/1");
                Assert.IsFalse(IsRightNotSet(nodeDocument, DbRight.RightTypes.Write), "Case 7/2");
                Assert.IsFalse(IsRightNotSet(nodeDocument, DbRight.RightTypes.Export), "Case 7/3");
                Assert.IsFalse(IsRightNotSet(nodeDocument, DbRight.RightTypes.Grant), "Case 7/4");
                Assert.IsFalse(IsRightNotSet(nodeDocument, DbRight.RightTypes.Send), "Case 7/5");
                Assert.IsFalse(IsGranted(nodeDocument, DbRight.RightTypes.Read, adminUser), "Case 8/1");
                Assert.IsFalse(IsGranted(nodeDocument, DbRight.RightTypes.Write, adminUser), "Case 8/2");
                Assert.IsFalse(IsDenied(nodeDocument, DbRight.RightTypes.Read, adminUser), "Case 8/3");
                Assert.IsFalse(IsDenied(nodeDocument, DbRight.RightTypes.Write, adminUser), "Case 8/4");
                
                // not own Document
                effectiveRightsAdmin = RightManager.GetEffectiveUserRightTree(adminUser);
                EffectiveUserRightTreeNode nodeDocument2 = GetNode(effectiveRightsAdmin, company2, document2.FinancialYear, document2);
                // the user has no owner rights for read and write and for the others
                Assert.IsFalse(HasOwnerRight(nodeDocument2, DbRight.RightTypes.Read, adminUser), "Case 9/1");
                Assert.IsFalse(HasOwnerRight(nodeDocument2, DbRight.RightTypes.Write, adminUser), "Case 9/2");
                Assert.IsFalse(HasOwnerRight(nodeDocument2, DbRight.RightTypes.Export, adminUser), "Case 9/3");
                Assert.IsFalse(HasOwnerRight(nodeDocument2, DbRight.RightTypes.Grant, adminUser), "Case 9/4");
                Assert.IsFalse(HasOwnerRight(nodeDocument2, DbRight.RightTypes.Send, adminUser), "Case 9/5");
                // document rights are not inherited (explicitly set or owner)
                Assert.IsFalse(HasInheritedRight(nodeDocument2, DbRight.RightTypes.Read, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 10/1");
                Assert.IsFalse(HasInheritedRight(nodeDocument2, DbRight.RightTypes.Write, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 10/2");
                Assert.IsFalse(HasInheritedRight(nodeDocument2, DbRight.RightTypes.Export, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 10/3");
                Assert.IsFalse(HasInheritedRight(nodeDocument2, DbRight.RightTypes.Grant, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 10/4");
                Assert.IsFalse(HasInheritedRight(nodeDocument2, DbRight.RightTypes.Send, adminUser, effectiveRightsAdmin.Descriptor.Target), "Case 10/5");
                // document rights explicitly set
                Assert.IsFalse(IsRightNotSet(nodeDocument2, DbRight.RightTypes.Read), "Case 11/1");
                Assert.IsFalse(IsRightNotSet(nodeDocument2, DbRight.RightTypes.Write), "Case 11/2");
                Assert.IsFalse(IsRightNotSet(nodeDocument2, DbRight.RightTypes.Export), "Case 11/3");
                Assert.IsFalse(IsRightNotSet(nodeDocument2, DbRight.RightTypes.Grant), "Case 11/4");
                Assert.IsFalse(IsRightNotSet(nodeDocument2, DbRight.RightTypes.Send), "Case 11/5");
                Assert.IsFalse(IsGranted(nodeDocument2, DbRight.RightTypes.Read, adminUser), "Case 12/1");
                Assert.IsFalse(IsGranted(nodeDocument2, DbRight.RightTypes.Write, adminUser), "Case 12/2");
                Assert.IsFalse(IsDenied(nodeDocument2, DbRight.RightTypes.Read, adminUser), "Case 12/3");
                Assert.IsFalse(IsDenied(nodeDocument2, DbRight.RightTypes.Write, adminUser), "Case 12/4");

                #endregion [ Check admin's owner and other rights ]

                #region [ user1 rights ]
                
                EffectiveUserRightTreeNode effectiveRightsU1 = RightManager.GetEffectiveUserRightTree(user1);
                // own Document
                EffectiveUserRightTreeNode nodeDocU1 = GetNode(effectiveRightsU1, company2, document2.FinancialYear, document2);
                Assert.IsTrue(HasOwnerRight(nodeDocU1, DbRight.RightTypes.Read, user1), "Case 13/1");
                Assert.IsTrue(HasOwnerRight(nodeDocU1, DbRight.RightTypes.Write, user1), "Case 13/2");
                Assert.IsTrue(HasOwnerRight(nodeDocU1, DbRight.RightTypes.Export, user1), "Case 13/3");
                Assert.IsTrue(HasOwnerRight(nodeDocU1, DbRight.RightTypes.Grant, user1), "Case 13/4");
                Assert.IsTrue(HasOwnerRight(nodeDocU1, DbRight.RightTypes.Send, user1), "Case 13/5");
                try {
                    HasOwnerRight(nodeDocument, DbRight.RightTypes.Count, user1);
                    Assert.Fail("DbRight.RightTypes.Count is implemented, please modify the test case!");
                } catch (NotImplementedException) {
                    // expected
                }
                // document rights inherited
                Assert.IsFalse(HasInheritedRight(nodeDocU1, DbRight.RightTypes.Read, user1, effectiveRightsU1.Descriptor.Target), "Case 14/1");
                Assert.IsFalse(HasInheritedRight(nodeDocU1, DbRight.RightTypes.Write, user1, effectiveRightsU1.Descriptor.Target), "Case 14/2");
                Assert.IsFalse(HasInheritedRight(nodeDocU1, DbRight.RightTypes.Export, user1, effectiveRightsU1.Descriptor.Target), "Case 14/3");
                Assert.IsFalse(HasInheritedRight(nodeDocU1, DbRight.RightTypes.Grant, user1, effectiveRightsU1.Descriptor.Target), "Case 14/4");
                Assert.IsFalse(HasInheritedRight(nodeDocU1, DbRight.RightTypes.Send, user1, effectiveRightsU1.Descriptor.Target), "Case 14/5");
                // document rights explicitly set
                Assert.IsFalse(IsRightNotSet(nodeDocU1, DbRight.RightTypes.Read), "Case 15/1");
                Assert.IsFalse(IsRightNotSet(nodeDocU1, DbRight.RightTypes.Write), "Case 15/2");
                Assert.IsFalse(IsRightNotSet(nodeDocU1, DbRight.RightTypes.Export), "Case 15/3");
                Assert.IsFalse(IsRightNotSet(nodeDocU1, DbRight.RightTypes.Grant), "Case 15/4");
                Assert.IsFalse(IsRightNotSet(nodeDocU1, DbRight.RightTypes.Send), "Case 15/5");
                Assert.IsFalse(IsGranted(nodeDocU1, DbRight.RightTypes.Read, user1), "Case 16/1");
                Assert.IsFalse(IsGranted(nodeDocU1, DbRight.RightTypes.Write, user1), "Case 16/2");
                Assert.IsFalse(IsDenied(nodeDocU1, DbRight.RightTypes.Read, user1), "Case 16/3");
                Assert.IsFalse(IsDenied(nodeDocU1, DbRight.RightTypes.Write, user1), "Case 16/4");
                
                // not own Document
                EffectiveUserRightTreeNode nodeDoc1U1 = GetNode(effectiveRightsU1, company1, document1.FinancialYear, document1);
                Assert.IsFalse(HasOwnerRight(nodeDoc1U1, DbRight.RightTypes.Read, user1), "Case 17/1");
                Assert.IsFalse(HasOwnerRight(nodeDoc1U1, DbRight.RightTypes.Write, user1), "Case 17/2");
                Assert.IsFalse(HasOwnerRight(nodeDoc1U1, DbRight.RightTypes.Export, user1), "Case 17/3");
                Assert.IsFalse(HasOwnerRight(nodeDoc1U1, DbRight.RightTypes.Grant, user1), "Case 17/4");
                Assert.IsFalse(HasOwnerRight(nodeDoc1U1, DbRight.RightTypes.Send, user1), "Case 17/5");
                // document rights inherited
                Assert.IsTrue(HasInheritedRight(nodeDoc1U1, DbRight.RightTypes.Read, user1, effectiveRightsU1.Descriptor.Target), "Case 18/1");
                Assert.IsTrue(HasInheritedRight(nodeDoc1U1, DbRight.RightTypes.Write, user1, effectiveRightsU1.Descriptor.Target), "Case 18/2");
                Assert.IsTrue(HasInheritedRight(nodeDoc1U1, DbRight.RightTypes.Export, user1, effectiveRightsU1.Descriptor.Target), "Case 18/3");
                Assert.IsTrue(HasInheritedRight(nodeDoc1U1, DbRight.RightTypes.Grant, user1, effectiveRightsU1.Descriptor.Target), "Case 18/4");
                Assert.IsTrue(HasInheritedRight(nodeDoc1U1, DbRight.RightTypes.Send, user1, effectiveRightsU1.Descriptor.Target), "Case 18/5");
                // document rights explicitly set
                Assert.IsFalse(IsRightNotSet(nodeDoc1U1, DbRight.RightTypes.Read), "Case 19/1");
                Assert.IsFalse(IsRightNotSet(nodeDoc1U1, DbRight.RightTypes.Write), "Case 19/2");
                Assert.IsFalse(IsRightNotSet(nodeDoc1U1, DbRight.RightTypes.Export), "Case 19/3");
                Assert.IsFalse(IsRightNotSet(nodeDoc1U1, DbRight.RightTypes.Grant), "Case 19/4");
                Assert.IsFalse(IsRightNotSet(nodeDoc1U1, DbRight.RightTypes.Send), "Case 19/5");
                Assert.IsFalse(IsGranted(nodeDoc1U1, DbRight.RightTypes.Read, user1), "Case 20/1");
                Assert.IsFalse(IsGranted(nodeDoc1U1, DbRight.RightTypes.Write, user1), "Case 20/2");
                Assert.IsFalse(IsDenied(nodeDoc1U1, DbRight.RightTypes.Read, user1), "Case 20/3");
                Assert.IsFalse(IsDenied(nodeDoc1U1, DbRight.RightTypes.Write, user1), "Case 20/4");

                // not own Company
                EffectiveUserRightTreeNode nodeCompany2 = GetNode(effectiveRightsU1, company2);
                // the user has owner rights for read and write and not for the others
                Assert.IsFalse(HasOwnerRight(nodeCompany2, DbRight.RightTypes.Read, user1), "Case 21/1");
                Assert.IsFalse(HasOwnerRight(nodeCompany2, DbRight.RightTypes.Write, user1), "Case 21/2");
                Assert.IsFalse(HasOwnerRight(nodeCompany2, DbRight.RightTypes.Export, user1), "Case 21/3");
                Assert.IsFalse(HasOwnerRight(nodeCompany2, DbRight.RightTypes.Grant, user1), "Case 21/4");
                Assert.IsFalse(HasOwnerRight(nodeCompany2, DbRight.RightTypes.Send, user1), "Case 21/5");
                // company rights are inherited except Read because it is explicitly set
                Assert.IsFalse(HasInheritedRight(nodeCompany2, DbRight.RightTypes.Read, user1, effectiveRightsU1.Descriptor.Target), "Case 22/1");
                Assert.IsTrue(HasInheritedRight(nodeCompany2, DbRight.RightTypes.Write, user1, effectiveRightsU1.Descriptor.Target), "Case 22/2");
                Assert.IsTrue(HasInheritedRight(nodeCompany2, DbRight.RightTypes.Export, user1, effectiveRightsU1.Descriptor.Target), "Case 22/3");
                Assert.IsTrue(HasInheritedRight(nodeCompany2, DbRight.RightTypes.Grant, user1, effectiveRightsU1.Descriptor.Target), "Case 22/4");
                Assert.IsTrue(HasInheritedRight(nodeCompany2, DbRight.RightTypes.Send, user1, effectiveRightsU1.Descriptor.Target), "Case 22/5");
                // company rights are explicitly set or inherited
                Assert.IsFalse(IsRightNotSet(nodeCompany2, DbRight.RightTypes.Read), "Case 23/1");
                Assert.IsFalse(IsRightNotSet(nodeCompany2, DbRight.RightTypes.Write), "Case 23/2");
                Assert.IsFalse(IsRightNotSet(nodeCompany2, DbRight.RightTypes.Export), "Case 23/3");
                Assert.IsFalse(IsRightNotSet(nodeCompany2, DbRight.RightTypes.Grant), "Case 23/4");
                Assert.IsFalse(IsRightNotSet(nodeCompany2, DbRight.RightTypes.Send), "Case 23/5");
                Assert.IsTrue(IsGranted(nodeCompany2, DbRight.RightTypes.Read, user1), "Case 24/1");
                Assert.IsFalse(IsDenied(nodeCompany2, DbRight.RightTypes.Read, user1), "Case 24/3");
                Assert.IsFalse(IsDenied(nodeCompany2, DbRight.RightTypes.Write, user1), "Case 24/4");

                // own Company
                EffectiveUserRightTreeNode nodeCompany3 = GetNode(effectiveRightsU1, company3);
                // the user has owner rights for read and write and not for the others
                Assert.IsTrue(HasOwnerRight(nodeCompany3, DbRight.RightTypes.Read, user1), "Case 25/1");
                Assert.IsTrue(HasOwnerRight(nodeCompany3, DbRight.RightTypes.Write, user1), "Case 25/2");
                Assert.IsTrue(HasOwnerRight(nodeCompany3, DbRight.RightTypes.Export, user1), "Case 25/3");
                Assert.IsTrue(HasOwnerRight(nodeCompany3, DbRight.RightTypes.Grant, user1), "Case 25/4");
                Assert.IsTrue(HasOwnerRight(nodeCompany3, DbRight.RightTypes.Send, user1), "Case 25/5");
                // company rights are not inherited (explicitly set or owner)
                Assert.IsFalse(HasInheritedRight(nodeCompany3, DbRight.RightTypes.Read, user1, effectiveRightsU1.Descriptor.Target), "Case 26/1");
                Assert.IsFalse(HasInheritedRight(nodeCompany3, DbRight.RightTypes.Write, user1, effectiveRightsU1.Descriptor.Target), "Case 26/2");
                Assert.IsFalse(HasInheritedRight(nodeCompany3, DbRight.RightTypes.Export, user1, effectiveRightsU1.Descriptor.Target), "Case 26/3");
                Assert.IsFalse(HasInheritedRight(nodeCompany3, DbRight.RightTypes.Grant, user1, effectiveRightsU1.Descriptor.Target), "Case 26/4");
                Assert.IsFalse(HasInheritedRight(nodeCompany3, DbRight.RightTypes.Send, user1, effectiveRightsU1.Descriptor.Target), "Case 26/5");
                // company rights are explicitly set
                Assert.IsFalse(IsRightNotSet(nodeCompany3, DbRight.RightTypes.Read), "Case 27/1");
                Assert.IsFalse(IsRightNotSet(nodeCompany3, DbRight.RightTypes.Write), "Case 27/2");
                Assert.IsFalse(IsRightNotSet(nodeCompany3, DbRight.RightTypes.Export), "Case 27/3");
                Assert.IsFalse(IsRightNotSet(nodeCompany3, DbRight.RightTypes.Grant), "Case 27/4");
                Assert.IsFalse(IsRightNotSet(nodeCompany3, DbRight.RightTypes.Send), "Case 27/5");
                Assert.IsFalse(IsGranted(nodeCompany3, DbRight.RightTypes.Read, user1), "Case 28/1");
                Assert.IsFalse(IsGranted(nodeCompany3, DbRight.RightTypes.Write, user1), "Case 28/2");
                Assert.IsFalse(IsDenied(nodeCompany3, DbRight.RightTypes.Read, user1), "Case 28/3");
                Assert.IsFalse(IsDenied(nodeCompany3, DbRight.RightTypes.Write, user1), "Case 28/4");

                #endregion [ user1 rights ]

                #region [ user2 rights ]

                // document3
                documents.Add(TestHelper.CreateDocument(company3));
                Document document3 = documents[0];
                document3.FinancialYear = company3.VisibleFinancialYears[0];

                EffectiveUserRightTreeNode effectiveRightsU2 = RightManager.GetEffectiveUserRightTree(user2);
                EffectiveUserRightTreeNode nodeCompany4 = GetNode(effectiveRightsU2, company3);
                // the user has owner rights for read and write and not for the others
                Assert.IsFalse(HasOwnerRight(nodeCompany4, DbRight.RightTypes.Read, user2), "Case 29/1");
                Assert.IsFalse(HasOwnerRight(nodeCompany4, DbRight.RightTypes.Write, user2), "Case 29/2");
                Assert.IsFalse(HasOwnerRight(nodeCompany4, DbRight.RightTypes.Export, user2), "Case 29/3");
                Assert.IsFalse(HasOwnerRight(nodeCompany4, DbRight.RightTypes.Grant, user2), "Case 29/4");
                Assert.IsFalse(HasOwnerRight(nodeCompany4, DbRight.RightTypes.Send, user2), "Case 29/5");
                // company rights are inherited except Read because it is explicitly set
                Assert.IsTrue(HasInheritedRight(nodeCompany4, DbRight.RightTypes.Read, user2, effectiveRightsU2.Descriptor.Target), "Case 30/1");
                Assert.IsTrue(HasInheritedRight(nodeCompany4, DbRight.RightTypes.Write, user2, effectiveRightsU2.Descriptor.Target), "Case 30/2");
                Assert.IsTrue(HasInheritedRight(nodeCompany4, DbRight.RightTypes.Export, user2, effectiveRightsU2.Descriptor.Target), "Case 30/3");
                Assert.IsTrue(HasInheritedRight(nodeCompany4, DbRight.RightTypes.Grant, user2, effectiveRightsU2.Descriptor.Target), "Case 30/4");
                Assert.IsTrue(HasInheritedRight(nodeCompany4, DbRight.RightTypes.Send, user2, effectiveRightsU2.Descriptor.Target), "Case 30/5");
                // company rights are explicitly set or inherited
                Assert.IsFalse(IsRightNotSet(nodeCompany4, DbRight.RightTypes.Read), "Case 31/1");
                Assert.IsFalse(IsRightNotSet(nodeCompany4, DbRight.RightTypes.Write), "Case 31/2");
                Assert.IsFalse(IsRightNotSet(nodeCompany4, DbRight.RightTypes.Export), "Case 31/3");
                Assert.IsFalse(IsRightNotSet(nodeCompany4, DbRight.RightTypes.Grant), "Case 31/4");
                Assert.IsFalse(IsRightNotSet(nodeCompany4, DbRight.RightTypes.Send), "Case 31/5");
                Assert.IsFalse(IsGranted(nodeCompany4, DbRight.RightTypes.Read, user2), "Case 31/1");
                Assert.IsFalse(IsGranted(nodeCompany4, DbRight.RightTypes.Write, user2), "Case 31/2");
                Assert.IsFalse(IsDenied(nodeCompany4, DbRight.RightTypes.Read, user2), "Case 31/3");
                Assert.IsFalse(IsDenied(nodeCompany4, DbRight.RightTypes.Write, user2), "Case 31/4");
                
                // explicitly set
                // set send by userrole
                Right user2CompanyRight = RoleManager.GetUserRole(user2).GetRight(company3);
                user2CompanyRight.Send = true;
                RoleManager.GetUserRole(user2).GetRight(company3).SetRights(user2CompanyRight);
                RoleManager.GetUserRole(user2).Save();

                effectiveRightsU2 = RightManager.GetEffectiveUserRightTree(user2);
                nodeCompany4 = GetNode(effectiveRightsU2, company3);

                Assert.IsFalse(HasOwnerRight(nodeCompany4, DbRight.RightTypes.Send, user2), "Case 32/1");
                Assert.IsFalse(HasInheritedRight(nodeCompany4, DbRight.RightTypes.Send, user2, effectiveRightsU2.Descriptor.Target), "Case 32/1");
                Assert.IsFalse(IsRightNotSet(nodeCompany4, DbRight.RightTypes.Send), "Case 32/3");
                Assert.IsTrue(IsGranted(nodeCompany4, DbRight.RightTypes.Send, user2), "Case 32/4");
                Assert.IsFalse(IsDenied(nodeCompany4, DbRight.RightTypes.Send, user2), "Case 32/5");
                EffectiveUserRightTreeNode nodeDocument3 = GetNode(effectiveRightsU2, company3, document3.FinancialYear, document3);
                Assert.IsTrue(HasInheritedRight(nodeDocument3, DbRight.RightTypes.Send, user2, company3), "Case 32/6");
                Assert.IsFalse(IsGranted(nodeDocument3, DbRight.RightTypes.Send, user2), "Case 32/7");

                // set Export by role1
                // create new roles
                Role role1 = new Role(new DbRole() { Name = role1Name });
                Right user2CompanyRight2 = role1.GetRight(company3);
                user2CompanyRight2.Export = true;
                role1.GetRight(company3).SetRights(user2CompanyRight2);
                RoleManager.AddRole(role1);
                user2.AssignedRoles.Add(role1);
                role1.Save();
                roles.Add(role1);

                effectiveRightsU2 = RightManager.GetEffectiveUserRightTree(user2);
                nodeCompany4 = GetNode(effectiveRightsU2, company3);

                //Assert.IsFalse(HasOwnerRight(nodeCompany4, DbRight.RightTypes.Export, user2), "Case 33/1");
                //Assert.IsFalse(HasInheritedRight(nodeCompany4, DbRight.RightTypes.Export, user2, effectiveRightsU2.Descriptor.Target), "Case 33/2");
                //Assert.IsFalse(IsRightNotSet(nodeCompany4, DbRight.RightTypes.Export), "Case 33/3");
                //Assert.IsTrue(IsGranted(nodeCompany4, DbRight.RightTypes.Export, user2), "Case 33/4");
                //Assert.IsFalse(IsDenied(nodeCompany4, DbRight.RightTypes.Export, user2), "Case 33/5");
                //nodeDocument3 = GetNode(effectiveRightsU2, company3, document3.FinancialYear, document3);
                //Assert.IsTrue(HasInheritedRight(nodeDocument3, DbRight.RightTypes.Export, user2, company3), "Case 33/6");
                //Assert.IsFalse(IsGranted(nodeDocument3, DbRight.RightTypes.Export, user2), "Case 33/7");

                #endregion [ user2 rights ]

            } catch (Exception) {
                throw;
            }
            #region [ Cleanup ]
            finally {
                try {
                    if (UserManager.Instance.CurrentUser.UserName != adminUser.UserName)
                        TestHelper.LogonAsUser(adminUser);
                } catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
                foreach (Document document in documents) {
                    try {
                        TestHelper.DeleteDocument(document);
                    } catch {
                        //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                    }
                }
                foreach (User user in plainUsers) {
                    try {
                        TestHelper.DeleteUser(user);
                    } catch {
                        //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                    }
                }
                foreach (Company company in companies) {
                    try {
                        TestHelper.DeleteCompany(company);
                    } catch (Exception) {
                        //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                    }
                }
                foreach (Role role in roles) {
                    try {
                        TestHelper.DeleteRole(role);
                    } catch (Exception) {
                        //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                    }
                }
                try {
                    if (system != null)
                        TestHelper.DeleteSystem(system);
                } catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
            }
            #endregion [ Cleanup ]   
        }

        

        #region [ Helper methods ]

        private bool IsGranted(EffectiveUserRightTreeNode node, DbRight.RightTypes rightType, object grantedBy) {
            if (node == null)
                return false;
            string tooltip = node.GetToolTipForType(rightType);
            if (grantedBy is User)
                return tooltip == string.Format(ResourcesCommon.GrantedUser, (grantedBy as User).UserName);
            else if (grantedBy is Role)
                return tooltip == string.Format(ResourcesCommon.Granted, (grantedBy as Role).DbRole.Name);
            return false;
        }

        private bool IsDenied(EffectiveUserRightTreeNode node, DbRight.RightTypes rightType, object grantedBy) {
            if (node == null)
                return false;
            string tooltip = node.GetToolTipForType(rightType);
            if (grantedBy is User)
                return tooltip == string.Format(ResourcesCommon.DeniedUser, (grantedBy as User).UserName);
            else if (grantedBy is Role)
                return tooltip == string.Format(ResourcesCommon.Denied, (grantedBy as Role).DbRole.Name);
            return false;
        }

        private bool IsRightNotSet(EffectiveUserRightTreeNode node, DbRight.RightTypes rightType) {
            if (node == null)
                return false;
            string tooltip = node.GetToolTipForType(rightType);
            return tooltip == ResourcesCommon.NotSet;
        }

        private bool HasAdminRight(EffectiveUserRightTreeNode node, DbRight.RightTypes rightType, User user) {
            if (node == null)
                return false;
            string tooltip = node.GetToolTipForType(rightType);
            if (node.Descriptor.Target is Company)
                return tooltip == ResourcesCommon.AllowedDueToAdminRights;
            if (node.Descriptor.Target is Document)
                return tooltip == ResourcesCommon.AllowedDueToAdminRights;
            return false;
        }

        private bool HasOwnerRight(EffectiveUserRightTreeNode node, DbRight.RightTypes rightType, User user) {
            if (node == null)
                return false;
            string tooltip = node.GetToolTipForType(rightType);
            if (node.Descriptor.Target is Company)
                return tooltip == string.Format(ResourcesCommon.GrantOwnerCompany, user.UserName);
            if (node.Descriptor.Target is Document)
                return tooltip == string.Format(ResourcesCommon.GrantOwnerDocument, user.UserName);
            return false;
        }
        
        private bool HasInheritedRight(EffectiveUserRightTreeNode node, DbRight.RightTypes rightType, User user, object parent) {
            if (node == null)
                return false;
            string tooltip = node.GetToolTipForType(rightType);
            string parentName = null;
            if (parent != null) {

                if (parent is Right) parentName = EffectiveRightDescriptor.GetRightContentTypeVerbal(parent as Right);
                else if (parent is Company) parentName = (parent as Company).Name;
                else if (parent is FinancialYear) parentName = (parent as FinancialYear).FYear.ToString(LocalisationUtils.GermanCulture);
                else if (parent is Document) parentName = (parent as Document).Name;
                else parentName = parent.ToString();
            }
            return tooltip == string.Format(ResourcesCommon.InheritedFrom, parentName);
        }

        private EffectiveUserRightTreeNode GetNode(EffectiveUserRightTreeNode effectiveRights, Company company = null, FinancialYear financialYear = null, Document document = null) { 
            
            if (company == null)
                return effectiveRights;

            if (effectiveRights.HeaderString == ResourcesCommon.RightTreeNodeAllCaption) {
                foreach (IRightTreeNode treeNodeCompany in effectiveRights.Children) {
                    if (treeNodeCompany.HeaderString == company.Name) {
                        if (financialYear == null) return treeNodeCompany as EffectiveUserRightTreeNode;
                        foreach (IRightTreeNode treeNodeFY in treeNodeCompany.Children) {
                            if (treeNodeFY.HeaderString == financialYear.FYear.ToString()) {
                                if (document == null) return treeNodeFY as EffectiveUserRightTreeNode;
                                foreach (IRightTreeNode treeNodeDocument in treeNodeFY.Children) {
                                    if (treeNodeDocument.HeaderString == document.Name) {
                                        return treeNodeDocument as EffectiveUserRightTreeNode;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private bool CheckEditableUser(User companyAdmin, User user, bool isEditableUser = true) {
            return isEditableUser == companyAdmin.EditableUsers.Any(u => u.Id == user.Id);
        }

        #endregion [ Helper methods ]
    }
}
