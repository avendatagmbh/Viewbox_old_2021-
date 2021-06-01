using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using eBalanceKit.Models;
using eBalanceKit.Structures;
using eBalanceKit.Windows;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.GlobalSearch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBase.Interfaces;

namespace eBalanceKitBusiness.Test
{
    
    
    /// <summary>
    ///This is a test class for GlobalSearcherTest and is intended
    ///to contain all GlobalSearcherTest Unit Tests
    ///</summary>
    [TestClass()]
    public class GlobalSearcherTest {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        public static void CopyAll(DirectoryInfo source, DirectoryInfo target) {
            if (source.FullName.ToLower() == target.FullName.ToLower()) {
                return;
            }

            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false) {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles()) {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            string sourceDirectory = @"C:\Users\mga\Documents\Visual Studio 2010\Projects\vs2010\Projects\eBalance\Taxonomy\Taxonomy\Taxonomy";
            //sourceDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location +
            //                  "..\\..\\..\\eBalance\\Taxonomy\\Taxonomy\\Taxonomy";
            string targetDirectory;// = @".\Taxonomy";
            targetDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof (Taxonomy.Taxonomy)).Location) + "\\" + "Taxonomy";

            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);
            Directory.CreateDirectory(targetDirectory);
            CopyAll(diSource, diTarget);

            sourceDirectory = @"C:\Users\mga\Documents\Visual Studio 2010\Projects\vs2010\Projects\eBalance\eBalanceKit\eBalanceKit\ResourceDictionaries";
            ////targetDirectory = Directory.GetCurrentDirectory() + "\\ResourceDictionaries";
            targetDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof (MainWindow)).Location) + "\\" + "ResourceDictionaries";

            diSource = new DirectoryInfo(sourceDirectory);
            Directory.CreateDirectory(targetDirectory);
            diTarget = new DirectoryInfo(targetDirectory);
            CopyAll(diSource, diTarget);
        }

        private Structures.DbMapping.System system;
        private User adminUser;
        private Company company;
        private Document document;
        private MainWindowModel mainWindowModel;
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        [TestInitialize]
        public void MyTestInitialize()
        {
            adminUser = TestHelper.LogonAsAdmin();

            #region [ Prepare ]

            system = TestHelper.CreateSystem();
            company = TestHelper.CreateCompany();
            document = TestHelper.CreateDocument();

            #endregion [ Prepare ]

            #region [ Init document ]
            
            TestHelper.InitDocument(document);

            #endregion [ Init document ]
            MainWindow main = new MainWindow();
            main.Init();
            mainWindowModel = new MainWindowModel(main);
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            try {
                if (UserManager.Instance.CurrentUser.UserName != adminUser.UserName)
                    TestHelper.LogonAsUser(adminUser);
            } catch {
                //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
            }
            try {
                if (document != null)
                    TestHelper.DeleteDocument(document);
            } catch {
                //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
            }
            try {
                if (company != null)
                    TestHelper.DeleteCompany(company);
            } catch (Exception) {
                //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
            }
            try {
                if (system != null)
                    TestHelper.DeleteSystem(system);
            } catch {
                //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
            }
        }
        #endregion

        /// <summary>
        ///A test for Search
        ///</summary>
        [TestMethod()]
        // Couldn't copy the resourceDictionaries, xaml's references are not found.
        // If we know where to copy these values it would be nicer.
        public void SearchTest() {
            /*TestHelper.CreateCompany();
            Document document = TestHelper.CreateDocument();
            MainWindow main = new MainWindow();
            main.Init();
            MainWindowModel mainWindowModel = new MainWindowModel(main);
            Action<INavigationTreeEntryBase> setSelectedNavigationTreeEntry = v => mainWindowModel.SelectedNavigationEntry = v as NavigationTreeEntry;
            INavigationTree navigationTree = mainWindowModel.NavigationTree;
            GlobalSearcher target = new GlobalSearcher(document, setSelectedNavigationTreeEntry, navigationTree);
            string searchString = "notes";
            bool searchInId = true;
            bool searchInLabel = true;
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            Task.Factory.StartNew(() => target.Search(searchString, searchInId, searchInLabel, token), token);
            Assert.IsTrue(target.ResultTreeRoots.Count > 0);
            //Assert.Inconclusive("A method that does not return a value cannot be verified.");*/
            Action<INavigationTreeEntryBase> setSelectedNavigationTreeEntry = v => mainWindowModel.SelectedNavigationEntry = v as NavigationTreeEntry;
            INavigationTree navigationTree = mainWindowModel.NavigationTree;
            AppConfig.AppUiCulture = CultureInfo.CreateSpecificCulture("en-EN");
            GlobalSearcher target = new GlobalSearcher(document, setSelectedNavigationTreeEntry, navigationTree);
            string searchString = "notes";
            bool searchInId = true;
            bool searchInLabel = true;
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            Task.Factory.StartNew(() => target.Search(searchString, searchInId, searchInLabel, token), token);
            Assert.IsTrue(target.ResultTreeRoots.Count > 0);
        }
    }
}
