using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using SystemDb;
using NUnit.Framework;
using Rhino.Mocks;
using ViewboxAdmin;
using ViewboxAdmin.ViewModels;
using ViewboxAdmin.ViewModels.Roles;
using ViewboxAdmin.ViewModels.Users;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    class RoleEditViewModel_Test {

        private ObservableCollection<RoleModel> rolesStub;
        private ObservableCollection<UserModel> userStub;
        private IUnitOfWork<RoleModel> unitOfWorkMock;
        private IMessageBoxProvider messageBoxMock;
        private RoleModel roleMock;

        [SetUp]
        public void SetUp() {
            rolesStub = new ObservableCollection<RoleModel>();
            userStub = new ObservableCollection<UserModel>();
            unitOfWorkMock = MockRepository.GenerateMock<IUnitOfWork<RoleModel>>();
            messageBoxMock = MockRepository.GenerateMock<IMessageBoxProvider>();
            roleMock = new RoleModel();
        }

        private RoleEditViewModel CreateSUT() {
            return new RoleEditViewModel(userStub,rolesStub,unitOfWorkMock,messageBoxMock);
        }

        [Test]
        public void Constructor_Inject_UserCollection() {
            var SUT = CreateSUT();
            Assert.AreEqual(userStub,SUT.Users);
        }

        [Test]
        public void Constructor_Inject_Roles() {
            var SUT = CreateSUT();
            Assert.AreEqual(rolesStub, SUT.Roles);
        }

        [Test]
        public void Constructor_Inject_UnitOfWotk_Test() { 
            var SUT = CreateSUT();
            Assert.AreEqual(unitOfWorkMock, SUT.UnitOfWork);
        }
        [Test]
        public void Constructor_Inject_MessageBoxProvider() { 
            var SUT = CreateSUT();
        Assert.AreEqual(messageBoxMock,SUT.MessageBoxProvider);
        }

        [Test]
        public void DeleteCommand_Enabled_If_SelectedItem_Not_Null_Diabled_Anyway() { 
            var SUT = CreateSUT();
            SUT.SelectedRole = null;
            Assert.IsFalse(SUT.DeleteCommand.CanExecute(null));
        }

        [Test]
        public void DeleteCommand_Enabled_If_Selected_Item_NOT_Null() { 
            //Arraneg
            var SUT = CreateSUT();
            //Act
            SUT.SelectedRole = new RoleModel();
            //Assert
            Assert.IsTrue(SUT.DeleteCommand.CanExecute(null));
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        public void EditCommand_Enabled_If_SelectedRole_NOt_Null(bool isNull, bool isenabled) {
            var SUT = CreateSUT();
            //Act
            SUT.SelectedRole = NullOrNotNullRoleMOdel(isNull);
            //Assert
            Assert.AreEqual(isenabled, SUT.EditCommand.CanExecute(null));
        }

        [TestCase(MessageBoxResult.Yes,Result = false)]
        [TestCase(MessageBoxResult.No,Result = true)]
        public bool DeleteCommand_Remove_RoleModel_If_User_Enable(MessageBoxResult result) {
            //Arrange
            var role = new RoleModel();
            rolesStub.Add(role);
            var SUT = CreateSUT();
            //Act
            SUT.SelectedRole = role;
            messageBoxMock.Stub(x => x.Show(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<MessageBoxButton>.Is.Equal(MessageBoxButton.YesNo), Arg<MessageBoxImage>.Is.Anything, Arg<MessageBoxResult>.Is.Anything)).Return(result);
            SUT.DeleteCommand.Execute(null);
            //Assert
            return SUT.Roles.Contains(role);
        }

        [Test]
        public void DeleteCommand_Call_Unit_Of_Work() { 
            //Arrange
            var role = new RoleModel();
            rolesStub.Add(role);
            var SUT = CreateSUT();
            //Act
            SUT.SelectedRole = role;
            setMessageBoxProoviderMock(MessageBoxResult.Yes);
            SUT.DeleteCommand.Execute(null);
            //Assert
            unitOfWorkMock.AssertWasCalled(x=>x.MarkAsDeleted(role));
        }



        private void setMessageBoxProoviderMock(MessageBoxResult result) {
            messageBoxMock.Stub(x => x.Show(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<MessageBoxButton>.Is.Anything, Arg<MessageBoxImage>.Is.Anything, Arg<MessageBoxResult>.Is.Anything)).Return(result);
        }

        private RoleModel NullOrNotNullRoleMOdel(bool isnull) {
            if (isnull) return null;
            return new RoleModel();
        }

        [Test]
        public void CommitCommand_Call_UOW() { 
            var SUT = CreateSUT();
            //Act
            SUT.CommitCommand.Execute(null);
            //Assert
            unitOfWorkMock.AssertWasCalled(x=>x.Commit());
        }

        [TestCase("testname1",SpecialRights.Grant)]
        [TestCase("testname2", SpecialRights.None)]
        [TestCase("testname3", SpecialRights.Super)]
        public void EditCommand_UserMake_Edit_ANd_Click_Save_Test(string name, SpecialRights rights) {
            RoleModel roletoedit = new RoleModel();
            rolesStub.Add(roletoedit);
            var SUT = CreateSUT();
            //Act
            SUT.SelectedRole = roletoedit;
            SUT.EditOrCreate += (yes, no, role) => {
                role.Name = name; role.Flags = rights; yes();
            };
            SUT.EditCommand.Execute(null);
            //Assert
            Assert.AreEqual(name, SUT.SelectedRole.Name);
            Assert.AreEqual(rights,SUT.SelectedRole.Flags);
            unitOfWorkMock.AssertWasCalled(x=>x.MarkAsDirty(SUT.SelectedRole));
        }

        [Test]
        public void NewCommand_New_Role_Goes_To_Collection_And_Got_selected_ANd_Call_Unit_Of_Work() {
            //Arrange
            var SUT = CreateSUT();
            SUT.EditOrCreate += (yes, no, role) => yes();
            //Act
            SUT.NewCommand.Execute(null);
            //Assert
            CollectionAssert.IsNotEmpty(SUT.Roles, "The role collection cannot be empty after creating a new role");
            Assert.IsNotNull(SUT.SelectedRole,"The newly created role, got selected");
            unitOfWorkMock.AssertWasCalled(x => x.MarkAsNew(SUT.SelectedRole));
        }

        [Test]
        public void NewCommand_User_Click_No_Nothing_Happens() {
            //Arrange
            var SUT = CreateSUT();
            SUT.EditOrCreate += (yes, no, role) => no();
            //Act
            SUT.NewCommand.Execute(null);
            //Assert
            CollectionAssert.IsEmpty(SUT.Roles, "The role collection should be empty after creating a new role, because the user clicked CANCEL");
            Assert.IsNull(SUT.SelectedRole, "The newly created role, do not get selected, because user click cancel");
            unitOfWorkMock.AssertWasNotCalled(x => x.MarkAsNew(SUT.SelectedRole));
        }

        void SUT_EditOrCreate(Action yes, Action no, RoleModel role) {
            yes();
        }


    }
}
