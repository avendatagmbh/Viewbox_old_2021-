using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SystemDb;
using Rhino.Mocks;
using NUnit.Framework;
using ViewboxAdmin.Manager;
using ViewboxAdmin.Models.ProfileRelated;
using ViewboxAdmin.ViewModels;
using ViewboxAdmin.ViewModels.MergeDataBase;
using ViewboxAdminBusiness.Manager;
using ViewboxAdmin_ViewModel.Structures.Config;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    class Merger_ViewModel_Test {

        private Merger_ViewModel mergerVM;
        private IProfileModelRepository _modelrepoMock;
        private IProfileManager _profilemanagerMock;
        private IMergeDataBase _mergedatabaseMock;

        private IProfileModel _profilemodelMock_1;
        private IProfileModel _profilemodelMock_2;
        private IProfile _profileMock_1;
        private IProfile _profileMock_2;
        private ISystemDb _systemDbMock_1;
        private ISystemDb _systemDbMock_2;


        [SetUp]
        public void SetUp() {
            //for constructor
            _modelrepoMock = MockRepository.GenerateMock<IProfileModelRepository>();
            _profilemanagerMock = MockRepository.GenerateMock<IProfileManager>();
            _mergedatabaseMock = MockRepository.GenerateMock<IMergeDataBase>();
            //for methods
            _profilemodelMock_1 = MockRepository.GenerateMock<IProfileModel>();
            _profilemodelMock_2 = MockRepository.GenerateMock<IProfileModel>();
            _profileMock_1 = MockRepository.GenerateMock<IProfile>();
            _profileMock_2 = MockRepository.GenerateMock<IProfile>();
            _systemDbMock_1 = MockRepository.GenerateMock<ISystemDb>();
            _systemDbMock_2 = MockRepository.GenerateMock<ISystemDb>();
            //instantiate the SUT
            mergerVM = new Merger_ViewModel(_modelrepoMock,_profilemanagerMock,_mergedatabaseMock);
        }

        [Test]
        public void Constructor_ModelRepo_Inject_Test() {
            Assert.AreEqual(_modelrepoMock,mergerVM.ProfileModelRepository);
        }

        [Test]
        public void Constructor_ProfileManager_Inject_Test() {
            Assert.AreEqual(_profilemanagerMock,mergerVM.ProfileManager);
        }
        [Test]
        public void Constructor_MergeDataBaseMock_Test() {
            Assert.AreEqual(_mergedatabaseMock,mergerVM.MergeDataBaseStrategy);
        }
        [Test]
        public void MergeDataBases_Without_SystemDBs_Dont_Call_The_Worker_Method_Test() {
            //Act
            //mergerVM.MergeDataBases();
            mergerVM.MergeCommand.Execute(null);
            //Assert
            _mergedatabaseMock.AssertWasNotCalled(x=>x.MergeDataBases(Arg<ISystemDb>.Is.Anything,Arg<ISystemDb>.Is.Anything));
        }

        [Test]
        public void FirstProfile_Property_GetSet_Test() {
            //Arrange
            GetProfileModelMockFromRepository_1();
            //Act
            mergerVM.FirstProfile = _profileMock_1;
            //Assert
            Assert.AreEqual(_profileMock_1,mergerVM.FirstProfile);
        }

        [Test]
        public void FirstProfile_Property_PropertyChange_Fired_Test() {
            //Arrange
            bool isfired = false;
            GetProfileModelMockFromRepository_1();
            mergerVM.PropertyChanged += (o, e) => { if (e.PropertyName == "FirstProfile") isfired = true; };
            //Act
            mergerVM.FirstProfile = _profileMock_1;
            //Assert
            Assert.IsTrue(isfired);
        }

        private void GetProfileModelMockFromRepository_1() { _modelrepoMock.Stub(x => x.GetModel(_profileMock_1)).Return(_profilemodelMock_1); }
        private void GetProfileModelMockFromRepository_2() { _modelrepoMock.Stub(x => x.GetModel(_profileMock_2)).Return(_profilemodelMock_2); }

        [Test]
        public void FirstProfile_Property_PropertyChange_Not_Fired_With_The_Same_Value() {
            //Arrange
            bool isfired = false;
            GetProfileModelMockFromRepository_1();
            mergerVM.FirstProfile = _profileMock_1;
            mergerVM.PropertyChanged += (o, e) => { if (e.PropertyName == "FirstProfile") isfired = true; };
            //Act
            mergerVM.FirstProfile = _profileMock_1;
            //Assert
            Assert.IsFalse(isfired);
        }

        [Test]
        public void FirstProfile_Property_Load_Test_SuccessFull() {
            //Arrange
            SetUpSystemDbMockForProfileModel_1();
            mergerVM.FirstProfile = _profileMock_1;
            //Act
            _profilemodelMock_1.Raise(x=>x.FinishedProfileLoadingEvent+=null,null,new ErrorEventArgs(null));
            //Assert
            Assert.AreEqual(_systemDbMock_1,mergerVM.SystemDb1);
        }

        [Test]
        public void FirstProfile_Property_Load_Test_Failed() {
            //Arrange
            SetUpSystemDbMockForProfileModel_1();
            mergerVM.FirstProfile = _profileMock_1;
            Exception e = new Exception("just for test...");
            //Act
            _profilemodelMock_1.Raise(x => x.FinishedProfileLoadingEvent += null, null, new ErrorEventArgs(e));
            //Assert
            Assert.AreEqual(null, mergerVM.SystemDb1);
        }

        [Test]
        public void FirstProfile_Property_Load_Failed_DebugMessage_Added_Test() {
            //Arrange
            string error = "This a test error";
            SetUpSystemDbMockForProfileModel_1();
            mergerVM.FirstProfile = _profileMock_1;
            Exception e = new Exception(error);
            //Act
            _profilemodelMock_1.Raise(x => x.FinishedProfileLoadingEvent += null, null, new ErrorEventArgs(e));
            //Assert
            Assert.IsTrue(mergerVM.DebugText.Contains(error));
        }

        private void SetUpSystemDbMockForProfileModel_1() {
            //Arrange
            _profileMock_1.Stub(x => x.SystemDb).Return(_systemDbMock_1);
            _profilemodelMock_1.Stub(x => x.Profile).Return(_profileMock_1);
            _modelrepoMock.Stub(x => x.GetModel(_profileMock_1)).Return(_profilemodelMock_1);
        }

        private void SetUpSystemDbMockForProfileModel_2() {
            //Arrange
            _profileMock_2.Stub(x => x.SystemDb).Return(_systemDbMock_2);
            _profilemodelMock_2.Stub(x => x.Profile).Return(_profileMock_2);
            _modelrepoMock.Stub(x => x.GetModel(_profileMock_2)).Return(_profilemodelMock_2);
        }







        [Test]
        public void SecondProfile_Property_GetSet_Test() {
            //Arrange
            GetProfileModelMockFromRepository_2();
            //Act
            mergerVM.SecondProfile = _profileMock_2;
            //Assert
            Assert.AreEqual(_profileMock_2, mergerVM.SecondProfile);
        }

        [Test]
        public void SecondProfile_Property_PropertyChange_Fired_Test() {
            //Arrange
            bool isfired = false;
            GetProfileModelMockFromRepository_2();
            mergerVM.PropertyChanged += (o, e) => { if (e.PropertyName == "SecondProfile") isfired = true; };
            //Act
            mergerVM.SecondProfile = _profileMock_2;
            //Assert
            Assert.IsTrue(isfired);
        }

        

        [Test]
        public void SecondProfile_Property_PropertyChange_Not_Fired_With_The_Same_Value() {
            //Arrange
            bool isfired = false;
            GetProfileModelMockFromRepository_2();
            mergerVM.SecondProfile = _profileMock_2;
            mergerVM.PropertyChanged += (o, e) => { if (e.PropertyName == "SecondProfile") isfired = true; };
            //Act
            mergerVM.SecondProfile = _profileMock_2;
            //Assert
            Assert.IsFalse(isfired);
        }

        [Test]
        public void SecondProfile_Property_Load_Test_SuccessFull() {
            //Arrange
            SetUpSystemDbMockForProfileModel_2();
            mergerVM.SecondProfile = _profileMock_2;
            //Act
            _profilemodelMock_2.Raise(x => x.FinishedProfileLoadingEvent += null, null, new ErrorEventArgs(null));
            //Assert
            Assert.AreEqual(_systemDbMock_2, mergerVM.SystemDb2);
        }

        [Test]
        public void SecondProfile_Property_Load_Test_Failed() {
            //Arrange
            SetUpSystemDbMockForProfileModel_2();
            mergerVM.SecondProfile = _profileMock_2;
            Exception e = new Exception("just for test...");
            //Act
            _profilemodelMock_2.Raise(x => x.FinishedProfileLoadingEvent += null, null, new ErrorEventArgs(e));
            //Assert
            Assert.AreEqual(null, mergerVM.SystemDb2);
        }

        [Test]
        public void SecondProfile_Property_Load_Failed_DebugMessage_Added_Test() {
            //Arrange
            string error = "This a test error";
            SetUpSystemDbMockForProfileModel_2();
            mergerVM.SecondProfile = _profileMock_2;
            Exception e = new Exception(error);
            //Act
            _profilemodelMock_2.Raise(x => x.FinishedProfileLoadingEvent += null, null, new ErrorEventArgs(e));
            //Assert
            Assert.IsTrue(mergerVM.DebugText.Contains(error));
        }



        // async call...
        //[Test]
        //public void MergeDataBases_With_Two_Valid_SystemDb_Call_The_Worker_Method() {
        //    //Arrange
        //    SetUpSystemDbMockForProfileModel_1();
        //    SetUpSystemDbMockForProfileModel_2();
        //    mergerVM.SecondProfile = _profileMock_2;
        //    mergerVM.FirstProfile = _profileMock_1;
        //    _profilemodelMock_2.Raise(x => x.FinishedProfileLoadingEvent += null, null, new ErrorEventArgs(null));
        //    _profilemodelMock_1.Raise(x => x.FinishedProfileLoadingEvent += null, null, new ErrorEventArgs(null));
        //    Assert.IsNotNull(mergerVM.SystemDb1);
        //    Assert.IsNotNull(mergerVM.SystemDb1);
        //    //Act
        //    mergerVM.MergeDataBases();
        //    //Assert
        //    _mergedatabaseMock.AssertWasCalled(x => x.MergeDataBases(Arg<ISystemDb>.Is.Anything, Arg<ISystemDb>.Is.Anything));
        //}
        [Test]
        public void ProfileModel_Property_Get_Set_Test() {
            mergerVM.ProfileModel1 = _profilemodelMock_1;
            mergerVM.ProfileModel2 = _profilemodelMock_2;
            Assert.AreEqual(_profilemodelMock_1, mergerVM.ProfileModel1);
            Assert.AreEqual(_profilemodelMock_2, mergerVM.ProfileModel2);
        }
        [Test]
        public void ProfileModel1_PropertyChanged_Test() {
            bool isfired = false;
            mergerVM.PropertyChanged += (o, e) => { if (e.PropertyName == "ProfileModel1") isfired = true; };
            mergerVM.ProfileModel1 = _profilemodelMock_1;
            Assert.IsTrue(isfired);
        }

        [Test]
        public void ProfileModel2_PropertyChanged_Test() {
            bool isfired = false;
            mergerVM.PropertyChanged += (o, e) => { if (e.PropertyName == "ProfileModel2") isfired = true; };
            mergerVM.ProfileModel2 = _profilemodelMock_2;
            Assert.IsTrue(isfired);
        }




    }
}
