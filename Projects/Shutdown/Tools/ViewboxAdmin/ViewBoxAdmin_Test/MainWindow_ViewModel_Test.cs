using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SystemDb;
using Rhino.Mocks;
using NUnit.Framework;
using Utils;
using ViewboxAdmin;
using ViewboxAdmin.Manager;
using ViewboxAdmin.Models;
using ViewboxAdmin.Models.ProfileRelated;
using ViewboxAdmin.Structures;
using ViewboxAdmin.ViewModels;
using ViewboxAdminBusiness.Manager;
using ViewboxAdmin_ViewModel.Structures.Config;
using Autofac;


namespace ViewBoxAdmin_Test
{
    [TestFixture]
    class MainWindow_ViewModel_Test {
        private IProfileManager _profilemanagerMock;
        private IProfileModelRepository _profilemodelrepoMock;
        private INavigationEntriesDataContext _navtreeDataContextMock;
        private INavigationTree _navigationTree;
        private IProfileModel _profileModelMock;
        private MainWindow_ViewModel MainVM;
        private IProfile _profileMock;
        private IDispatcher _dispatcherMock;
        private ISystemDb _systemDbMock;
        private IIoCResolver _componentContext;
        private ObservableCollectionAsync<IProfile> _profiles = new ObservableCollectionAsync<IProfile>(); 
            [SetUp]
        public void SetUp() { 
            //set up test doubles
            _systemDbMock = MockRepository.GenerateMock<ISystemDb>();
            _dispatcherMock = MockRepository.GenerateMock<IDispatcher>();
            _profileModelMock = MockRepository.GenerateMock<IProfileModel>();
            _profilemodelrepoMock = MockRepository.GenerateMock<IProfileModelRepository>();
            _profileMock = MockRepository.GenerateMock<IProfile>();
            _componentContext = MockRepository.GenerateMock<IIoCResolver>();
            _profilemanagerMock = MockRepository.GenerateMock<IProfileManager>();
            _navigationTree = MockRepository.GenerateMock<INavigationTree>();
            _navtreeDataContextMock = MockRepository.GenerateMock<INavigationEntriesDataContext>();
            // this dispatcher stub simply execute its argument action... in real code this interface marshalling the action to the UI thread.
            _dispatcherMock.Stub(x => x.Invoke(Arg<Action>.Is.Anything)).WhenCalled(p => ((Action)p.Arguments[0]).Invoke());
            // test double for the navigation tree
            _navtreeDataContextMock.Stub(x => x.NavigationTree).Return(_navigationTree);
            
           
            
        }

        private MainWindow_ViewModel CreateSUT() {
            _profilemanagerMock.Stub(x => x.Profiles).Return(_profiles);
            return new MainWindow_ViewModel(_profilemanagerMock, _profilemodelrepoMock, _navtreeDataContextMock,
                                            _dispatcherMock, _componentContext);
        }

        [Test]
        public void Constructor_ProfileManager_Injection() {
            var MainVM = CreateSUT();
            //Assert
            Assert.AreEqual(_profilemanagerMock,MainVM.Profilemananger);
        }

        [Test]
        public void Constructor_ProfileModelREp_Injection() {
            var MainVM = CreateSUT();
            Assert.AreEqual(_profilemodelrepoMock,MainVM.ProfileModelRepository);
        }

        [Test]
        public void Constructor_Navigation_Tree_Injection() {
            var MainVM = CreateSUT();
            //Assert
            Assert.AreEqual(_navtreeDataContextMock,MainVM.NavigationTreeElementDataContext);
        }

        [Test]
        public void Constructor_Inject_Dispatcher() {
            var MainVM = CreateSUT();
            //Assert
            Assert.AreEqual(_dispatcherMock,MainVM.Dispatcher);
        }

        [Test]
        public void Constructor_Inject_DI_Container() {
            var MainVM = CreateSUT();
            //Assert
            Assert.AreEqual(_componentContext,MainVM.Container);
        }

        [Test]
        public void Constructor_Inject_NavigationTree() {
            var MainVM = CreateSUT();
            //Assert
            Assert.AreEqual(_navigationTree,MainVM.NavigationTree);
        }

        [Test]
        public void Constructor_Inject_Profiles() {
            //Act
            var MainVM = CreateSUT();
            //Assert
            Assert.AreEqual(_profiles, MainVM.Profiles);
        }

        [Test]
        public void UpgradeDataBase_Call_Upgrade_Method() {
            //Arrange
            MainWindow_ViewModel MainVM = CreateSUT();
            IProfile profile = MockRepository.GenerateMock<IProfile>();
            _profilemodelrepoMock.Stub(x => x.GetModel(profile)).Return(MockRepository.GenerateMock<IProfileModel>());
            //Act
            MainVM.SelectedProfile = profile;
            MainVM.UpGradeDatabase();
            //Assert
            profile.AssertWasCalled(x=>x.UpgradeDatabase());
        }
        
        [Test]
        public void UpdateDataContextTest() {
            //Arrange
            MainWindow_ViewModel MainVM = CreateSUT();
            IProfile profile = MockRepository.GenerateMock<IProfile>();
            IProfileModel profileModel = MockRepository.GenerateMock<IProfileModel>();
            _profilemodelrepoMock.Stub(x => x.GetModel(profile)).Return(profileModel);
            //Act
            MainVM.SelectedProfile = profile;
                //profile loaded succesfully
            RaiseProfileLoadingFinished(profileModel,null);
            //Assert
            _navtreeDataContextMock.AssertWasCalled(x => x[Arg<string>.Is.Equal("DeleteSystem")]=Arg<object>.Is.Anything);
            _navtreeDataContextMock.AssertWasCalled(x => x[Arg<string>.Is.Equal("EditText")] = Arg<object>.Is.Anything);
            _navtreeDataContextMock.AssertWasCalled(x => x[Arg<string>.Is.Equal("Merge")] = Arg<object>.Is.Anything);
            _navtreeDataContextMock.AssertWasCalled(x => x[Arg<string>.Is.Equal("Optimizations")] = Arg<object>.Is.Anything);
            _navtreeDataContextMock.AssertWasCalled(x => x[Arg<string>.Is.Equal("Parameters")] = Arg<object>.Is.Anything);
            _navtreeDataContextMock.AssertWasCalled(x => x[Arg<string>.Is.Equal("Users")] = Arg<object>.Is.Anything);
            _navtreeDataContextMock.AssertWasCalled(x => x[Arg<string>.Is.Equal("Roles")] = Arg<object>.Is.Anything);
        }

        [TestCase("CurrentUser")]
        public void CurrentUser_NotifyPropertyChanged(string propertyName) {
            //Arrange
            bool isfired = false;
            var SUT = CreateSUT();
            SUT.PropertyChanged += (o, e) => { if (e.PropertyName == propertyName) isfired = true; };
            //Assert
            SUT.CurrentUser = "dummyuser";
            //Act
            Assert.IsTrue(isfired);
        }

        private void RaiseProfileLoadingFinished(IProfileModel profilemodel,Exception exception) {
            System.IO.ErrorEventArgs erroreventargs = new System.IO.ErrorEventArgs(exception);
            profilemodel.Raise(x => x.FinishedProfileLoadingEvent += null, null, erroreventargs);
        }

        [Test]
        public void SetProfile_Set_The_LastProfile_Test() {
            //Arrange
            var SUT = CreateSUT();
            IProfile testprofile = MockRepository.GenerateMock<IProfile>();
            MockGetLastProfile(testprofile);
            MockGetProfileModel(testprofile,_profileModelMock);
            //Act
            SUT.SetProfile();
            //Assert
            Assert.AreEqual(testprofile,SUT.SelectedProfile);
        }

        private void MockGetProfileModel(IProfile profile, IProfileModel profileModel) {
            _profilemodelrepoMock.Stub(x => x.GetModel(profile)).Return(profileModel);
        }

        private void MockGetLastProfile(IProfile profile) {
            _profilemanagerMock.Stub(x => x.GetLastProfile()).Return(profile);
        }

    }
}
