using System.Windows;
using eBalanceKit.Models;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using eBalanceKitBusiness.Structures;
using System.Collections.Generic;
using eBalanceKitBusiness.Rights;
using System.Collections.ObjectModel;

namespace eBalanceKitBusiness.Test
{


    /// <summary>
    /// Unit tests for the User class
    /// </summary>
    /// <author>Gabor Bauer</author>
    /// <since>2012-04-12</since>
    [TestClass()]
    public class UserTest
    {
        #region [ Init ]

        private TestContext testContextInstance;

        public TestContext TestContext { get { return testContextInstance; } set { testContextInstance = value; } }
        
        [ClassInitialize()]
        public static void UserTestInitialize(TestContext testContext)
        {
            TestHelper.Init();
        }
        
        //Mit ClassCleanup führen Sie Code aus, nachdem alle Tests in einer Klasse ausgeführt wurden.
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen.
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        
        #endregion [ Init ]

        /// <summary>
        /// This test is for the purpose of thesting the User.AssignedCompanies property. 
        /// The User.AssignedCompaniesAsXml should change according to the User.AssignedCompanies property.
        ///</summary>
        [TestMethod()]
        public void UserTest_AssignedCompaniesTest()
        {
            TestHelper.LogonAsAdmin();

            #region [ Prepare ]

            User user = new User();
            string xml = user.AssignedCompaniesAsXml;
            ObservableCollection<Company> companies = new ObservableCollection<Company> {
                TestHelper.CreateCompany(),
                TestHelper.CreateCompany(),
                TestHelper.CreateCompany()
            };

            #endregion [ Prepare ]

            try
            {
                foreach (Company company in companies)
                {
                    user.AssignedCompanies.Add(new CompanyInfo(company));
                }
                xml = user.AssignedCompaniesAsXml;
                ObservableCollection<CompanyInfo> companiesReturned = user.AssignedCompanies;

                Assert.AreEqual(companies.Count, companiesReturned.Count,
                                "The CompanyList result is differs than the excepted!");

                for (int i = 0; i < companies.Count; i++)
                {
                    if (companies[i].Id != companiesReturned[i].Id)
                        Assert.Fail("The CompanyList result is differs than the excepted!");
                }

                while (user.AssignedCompanies.Count > 0)
                {
                    user.AssignedCompanies.RemoveAt(0);
                }
                xml = user.AssignedCompaniesAsXml;

                Assert.AreEqual(xml, user.SerializeToXml(new List<CompanyInfo>()));
            }
            catch (Exception) {

                throw;
            }
            #region [ Cleanup ]
            finally
            {
                foreach (Company company in companies) {
                    try {
                        TestHelper.DeleteCompany(company);
                    } catch {
                        //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                    }
                }
            }
            #endregion [ Cleanup ]
        }
        
        /// <summary>
        /// This test is for the purpose of testing the company administrator features of the rights management.
        /// In order to make a user as company administrator the IsCompanyAdmin has to set to true and the AssignedCompanies collection has to be filled with the appropriate values 
        /// the companies where the user is a company administrator. For company administrators the administered companies has to be visible and not for non company admin users. 
        ///</summary>
        [TestMethod()]
        public void UserTest_CompanyAdminTest() {

            User adminUser = TestHelper.LogonAsAdmin();
            for (int i = 1; i < 3; i++) {

                #region [ Prepare ]

                if (UserManager.Instance.CurrentUser == null || UserManager.Instance.CurrentUser.UserName != adminUser.UserName)
                    TestHelper.LogonAsUser(adminUser, TestHelper.adminUserPw);

                User testUser = TestHelper.CreateUser(i % 2 == 0);

                #endregion [ Prepare ]

                try {
                    ObservableCollection<Company> companies = new ObservableCollection<Company> {
                        TestHelper.CreateCompany(),
                        TestHelper.CreateCompany(),
                        TestHelper.CreateCompany(),
                        TestHelper.CreateCompany()
                    };

                    TestHelper.LogonAsUser(testUser);

                    try {
                        object test = testUser.AssignedCompanies;
                        testUser.AssignedCompanies.Add(new CompanyInfo(companies[0]));
                        testUser.AssignedCompanies.Add(new CompanyInfo(companies[2]));

                        if (testUser.IsCompanyAdmin) {
                            Assert.IsTrue(RightManager.RightDeducer.CompanyVisible(companies[0]));
                            Assert.IsTrue(RightManager.RightDeducer.CompanyVisible(companies[2]));
                            Assert.IsFalse(RightManager.RightDeducer.CompanyVisible(companies[1]));
                            Assert.IsFalse(RightManager.RightDeducer.CompanyVisible(companies[3]));

                            // create company and a report for the company then revoke the rights for the company and finally check whether reports should not be visible
                            // MainWindowModel.Companies FinancialYears and Documents should be null in this case

                        } else {
                            foreach (Company company in companies) {
                                Assert.IsFalse(RightManager.RightDeducer.CompanyVisible(company));
                            }
                        }
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
                        foreach (Company company in companies) {
                            try {
                                TestHelper.DeleteCompany(company);
                            } catch {
                                //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                            }
                        }
                    }
                    #endregion [ Cleanup ]
                } catch (Exception) {
                    throw;
                }
                #region [ Cleanup ]
                finally {
                    try {
                        TestHelper.DeleteUser(testUser);
                    } catch (Exception) {
                        //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                    }
                }
                #endregion [ Cleanup ]
            }
        }

        /// <summary>
        /// No companies allowed, but still reports appears and the company combobox on the main window is also filled with wrong value.
        /// if the user is the owner of the report and cannot see the company because the rights to the company is revoked from the user the report is still appears)
        ///</summary>
        [TestMethod()]
        public void UserTest_HasReportButCompanyNotAllowedTest() {
            
            User adminUser = TestHelper.LogonAsAdmin();
            User testUser = TestHelper.CreateUser(true);

            try
            {
                ObservableCollection<Company> companies = new ObservableCollection<Company> {
                    TestHelper.CreateCompany(),
                    TestHelper.CreateCompany(),
                    TestHelper.CreateCompany(),
                    TestHelper.CreateCompany()
                };

                try {
                    TestHelper.LogonAsUser(testUser);

                    object test = testUser.AssignedCompanies;
                    testUser.AssignedCompanies.Add(new CompanyInfo(companies[0]));
                    testUser.AssignedCompanies.Add(new CompanyInfo(companies[2]));

                    Window owner = new Window();
                    // Resource dictionary loading problem, will fix later
                    //MainWindowModel model = new MainWindowModel(owner);

                } catch (Exception) {
                    throw;
                }
                #region [ Cleanup ]
                finally {
                    try {
                        if (UserManager.Instance.CurrentUser.UserName != adminUser.UserName)
                            TestHelper.LogonAsUser(adminUser);
                    } catch
                    {
                        //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                    }
                    foreach (Company company in companies) {
                        try {
                            TestHelper.DeleteCompany(company);
                        } catch {
                            //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                        }
                    }
                }
                #endregion [ Cleanup ]
            }
            catch (Exception) {
                throw;
            }
            #region [ Cleanup ]
            finally {
                try {
                    TestHelper.DeleteUser(testUser);
                } catch (Exception) {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
            }
            #endregion [ Cleanup ]
        }
    }
}
