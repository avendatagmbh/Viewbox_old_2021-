using System;
using System.Collections.ObjectModel;
using Rhino.Mocks;
using NUnit.Framework;
using ViewboxAdmin.ViewModels;
using ViewboxAdmin.ViewModels.Users;


namespace ViewBoxAdmin_Test
{
    [TestFixture]
    internal class UserViewModel_Test {


        private ObservableCollection<UserModel> usersMock = new ObservableCollection<UserModel>();
        private IUnitOfWork<UserModel> unitOfWorkMock;

        [SetUp]
        public void SetUp() { unitOfWorkMock = MockRepository.GenerateMock<IUnitOfWork<UserModel>>(); }

        private UsersViewModel CreateSUT(params UserModel[] user) {
            usersMock = user != null
                            ? new ObservableCollection<UserModel>(user)
                            : new ObservableCollection<UserModel>();
            return new UsersViewModel(usersMock, unitOfWorkMock);
        }

        [Test]
        public void Constructor_Inject_UserCollection() {
            //Act
            var SUT = CreateSUT(null);
            //Assert
            Assert.AreEqual(usersMock, SUT.Users);
        }

        [Test]
        public void Constructor_Inject_UnitOfWork() {
            //Act
            var SUT = CreateSUT(null);
            //Assert
            Assert.AreEqual(unitOfWorkMock,SUT.UnitOfWork);
        }
    

    [TestCase("SelectedUser")]
        public void SelectedUser_PropertyChanged(string propertyName) {
            //Arrange
            bool isfired = false;
            var SUT = CreateSUT(null);
            SUT.PropertyChanged += (o, e) => { if (e.PropertyName == propertyName) isfired = true; };
            //Act
            SUT.SelectedUser = new UserModel();
            //Assert
            Assert.IsTrue(isfired);
        }

        [Test]
        public void DeleteUser_User_Click_Yes_Remove_User_From_Collection() {
            //Arrange
            var user1 = new UserModel();
            var user2 = new UserModel();
            var SUT = CreateSUT(user1, user2);
            //Act
            SUT.SelectedUser = user1;
            SUT.VerifyUserDelete += (o, e) => e.OnYesClick();
            SUT.DeleteUserCommand.Execute(null);
            //Assert
            CollectionAssert.DoesNotContain(SUT.Users,user1);
            CollectionAssert.Contains(SUT.Users,user2);
            
        }

        [Test]
        public void DeleteUser_User_Click_No_DO_Not_Remove_From_Collection() {
            //Arrange
            var user1 = new UserModel();
            var SUT = CreateSUT(user1);
            //Act
            SUT.SelectedUser = user1;
            SUT.VerifyUserDelete += (o, e) => e.OnNoClick();
            SUT.DeleteUserCommand.Execute(null);
            //Assert
            CollectionAssert.Contains(SUT.Users, user1);
        }

        [TestCase(12,"Before","After")]
        public void EditUser_User_Click_Yes_The_Selected_Item_Was_edited(int id, string oldname, string newname) {
            //Arrange
            var user = new UserModel();
            user.Id = id;
            user.UserName = oldname;
            var SUT = CreateSUT(user);
            int numberOfUser = SUT.Users.Count;
            //Act
            SUT.SelectedUser = user;
            SUT.CreateOrEditUser += (o, e) => {e.User.UserName = newname; e.Yes(); };
            SUT.EditUserCommand.Execute(null);
            //Assert
            Assert.AreEqual(id,SUT.SelectedUser.Id,"The id cannot change during editing");
            Assert.AreEqual(newname,SUT.SelectedUser.UserName,"The edited username should be present");
            Assert.AreEqual(numberOfUser,SUT.Users.Count,"the number of items in the collection should not change after editing");
        }

        [TestCase("DummyUserName")]
        public void NewUserCommand_Click_Yes_The_New_User_Goes_To_The_Collection(string username) {
            //Arrange
            var user = new UserModel();
            var SUT = CreateSUT(user);
            int numberofUsersBefore = SUT.Users.Count;
            //Act
            SUT.SelectedUser = user;
            SUT.CreateOrEditUser += (o, e) => { e.User.UserName = username; e.Yes();};
            SUT.NewUserCommand.Execute(null);
            //Assert
            Assert.AreEqual(++numberofUsersBefore,SUT.Users.Count);
            Assert.AreEqual(username,SUT.SelectedUser.UserName);
        }

        [Test]
        public void DeleteUserCommand_NotEnabled_If_SelectedItem_Null() {
            //Arrange
            var SUT = CreateSUT(null);
            SUT.SelectedUser = new UserModel();
            bool canexecute = SUT.DeleteUserCommand.CanExecute(null);
            Assert.IsTrue(canexecute,"The command should be able to execute here");
            //Act
            SUT.SelectedUser = null;
            canexecute = SUT.DeleteUserCommand.CanExecute(null);
            //Assert
            Assert.IsFalse(canexecute);
        }

        [Test]
        public void EditUserCommand_Disabled_Whenever_SelectedItem_Null() {
            //Arrange
            var SUT = CreateSUT(null);
            SUT.SelectedUser = new UserModel();
            bool canexecute = SUT.EditUserCommand.CanExecute(null);
            Assert.IsTrue(canexecute);
            //Act
            SUT.SelectedUser = null;
            canexecute = SUT.EditUserCommand.CanExecute(null);
            //Assert
            Assert.IsFalse(canexecute);
        }

        [Test]
        public void NewUserCommand_Is_Enabled_Always() { 
            //Arrange
            var SUT = CreateSUT(null);
            //Act
            SUT.SelectedUser = null;
            bool canexecute = SUT.NewUserCommand.CanExecute(null);
            //Assert 
            Assert.IsTrue(canexecute);
        }

        [Test]
        public void CommitUserCommand_Call_The_Unit_Of_Work_Method_Test() {
            //Arrange
            var SUT = CreateSUT(null);
            //Act
            SUT.CommitUserCommand.Execute(null);
            //Assert
            unitOfWorkMock.AssertWasCalled(x=>x.Commit());
        }

        [Test]
        public void CommitUserCommand_Failed_Is_Reported_To_UI() {
            //Arrange
            var SUT = CreateSUT(null);
            bool isReported = false;
            unitOfWorkMock.Stub(x => x.Commit()).Throw(new ArgumentException());
            SUT.ExceptionOccured += (o, e) => { if (e.GetException() != null) isReported = true; };
            //Act
            SUT.CommitUserCommand.Execute(null);
            //Assert
            Assert.IsTrue(isReported);
        }

        [Test]
        public void DeleteUserCommand_Call_UnitOfWork() {
            //Arrange
            var user = new UserModel();
            var SUT = CreateSUT(user);
            //Act
            SUT.SelectedUser = user;
                //user approve delete operation...
            SUT.VerifyUserDelete += (o, e) => e.OnYesClick();
            SUT.DeleteUserCommand.Execute(null);
            //Assert
            unitOfWorkMock.AssertWasCalled(x=>x.MarkAsDeleted(user));
        }

        [Test]
        public void NewUserCommand_Call_Unit_Of_Work() {
            //Arrange
            var SUT = CreateSUT(null);
            //Act
            SUT.CreateOrEditUser += (o, e) => {
                e.User.UserName = "valid"; e.Yes(); };
            SUT.NewUserCommand.Execute(null);
            //Assert
            unitOfWorkMock.AssertWasCalled(x=>x.MarkAsNew(Arg<UserModel>.Is.Anything));
        }

        [Test]
        public void EditUserCommand_Call_Unit_Of_Work() {
            //Arrange
            var user = new UserModel();
            var SUT = CreateSUT(user);
            //Act
            SUT.SelectedUser = user;
            SUT.CreateOrEditUser += (o, e) => e.Yes();
            SUT.EditUserCommand.Execute(null);
            //Assert
            unitOfWorkMock.AssertWasCalled(x=>x.MarkAsDirty(Arg<UserModel>.Is.Anything));
        }

        [Test]
        public void CreateNewUser_NotEnabled_With_Same_User_Name() {
            //Arrange
            string username = "UniqueKeyUserName";
            var existinguser = new UserModel();
            existinguser.UserName = username;
            var SUT = CreateSUT(existinguser);
            UserModel usercreated = null;
            //Act
            SUT.CreateOrEditUser += (o, e) => {
                e.User.UserName = username;
                usercreated = e.User;
                e.Yes();
            };
            SUT.NewUserCommand.Execute(null);
            //Assert
            CollectionAssert.DoesNotContain(SUT.Users,usercreated);
            unitOfWorkMock.AssertWasNotCalled(x => x.MarkAsNew(Arg<UserModel>.Is.Anything));
        }
    }
}
