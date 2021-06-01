using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Mocks;
using NUnit.Framework;
using ViewboxAdmin.Models;
using ViewboxAdmin.ViewModels;
using ViewboxAdminBusiness.Manager;
using ViewboxAdmin_ViewModel.Structures.Config;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    class ProfileManagement_ViewModel_Test {
        //SUT
        private ProfileManagement_ViewModel profileVM;
        //Test dubles
        private IProfileManager _profileManager_Mock;
        private IMainWindow_ViewModel _mainVM_Mock;
        private IProfile _profile_Mock;
        [SetUp]
        public void SetUp() {
            //set up mocks
            _profile_Mock = MockRepository.GenerateMock<IProfile>();
            _profileManager_Mock = MockRepository.GenerateMock<IProfileManager>();
            _mainVM_Mock = MockRepository.GenerateMock<IMainWindow_ViewModel>();
            //set up SUT
            profileVM = new ProfileManagement_ViewModel(_profileManager_Mock,_mainVM_Mock);
        }

        [Test]
        public void Constructor_Profile_Manager_Injection() {
            Assert.AreEqual(_profileManager_Mock,profileVM.ProfileManager);
        }

        [Test]
        public void Constructor_MainWindowVm_Injection() {
            Assert.AreEqual(_mainVM_Mock,profileVM.MainWindowViewModel);
        }

        [Test]
        public void Constructor_DeleteProfileCommand_Init() {
            Assert.IsNotNull(profileVM.DeleteProfileCommand);
        }

        [Test]
        public void Constructor_CreateNewProfileCommand_Init() {
            Assert.IsNotNull(profileVM.CreateNewProfileCommand);
        }

        

        
        [Test]
        public void SelectedProfile_Property_Get_Set() {
            //Act
            profileVM.SelectedProfile = _profile_Mock;
        Assert.AreEqual(_profile_Mock,profileVM.SelectedProfile);}

        [Test]
        public void SelectedProfile_PropertyChanged() {
            //Arrange
            bool isfired = false;
            profileVM.PropertyChanged += (o, e) => { if (e.PropertyName == "SelectedProfile") isfired = true; };
            //Act
            profileVM.SelectedProfile = _profile_Mock;
            //Assert
            Assert.IsTrue(isfired);
        }

        

        [Test]
        public void DeleteProfileCommand_CanExecute_False() {
            //Arrange
            profileVM.SelectedProfile = null;
            //Act
            bool canexecute = profileVM.DeleteProfileCommand.CanExecute(null);
            //Assert
            Assert.IsFalse(canexecute);
        }

        [Test]
        public void DeleteProfileCommand_Command_CanExecute_True() {
            //Arrange
            profileVM.SelectedProfile = _profile_Mock;
            //Act
            bool canexecute = profileVM.DeleteProfileCommand.CanExecute(null);
            //Assert
            Assert.IsTrue(canexecute);
        }

        [Test]
        public void CreateNewProfileCommand_CanExecute_Always() {
            Assert.IsTrue(profileVM.CreateNewProfileCommand.CanExecute(null));
        }

        [Test]
        public void CreateNewProfileCommand_Provides_A_ViewModel() {
            //Arrange
            CreateNewProfile_ViewModel CVM = null;
            profileVM.AddNewProfile += (o, e) => { if (e != null) CVM = e.CreateNewProfileVM; };
            //Act
            profileVM.CreateNewProfileCommand.Execute(null);
            //Assert
            Assert.IsNotNull(CVM,"The returned ViewModel should be not null");
            Assert.AreEqual(profileVM,CVM.profileManagementViewModel,"The ViewModel was not set correctly to the child ViewModel");
        }

        [Test]
        public void DeleteProfileCommand_YES() {
            //Arrange
            profileVM.SelectedProfile = _profile_Mock;
            profileVM.DeleteItem += (o, e) => e.OnYesClick();
            //Act
            profileVM.DeleteProfileCommand.Execute(null);
            //Assert
            _profileManager_Mock.AssertWasCalled(x=>x.DeleteProfile(_profile_Mock));
        }

        [Test]
        public void DeleteProfileCommand_NO() {
            //Arrange
            profileVM.SelectedProfile = _profile_Mock;
            profileVM.DeleteItem += (o, e) => e.OnNoClick();
            //Act
            profileVM.DeleteProfileCommand.Execute(null);
            //Assert
            _profileManager_Mock.AssertWasNotCalled(x => x.DeleteProfile(_profile_Mock));
        }


    }
}
