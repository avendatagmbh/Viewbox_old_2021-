using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using ViewboxAdmin.ViewModels.Users;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    class UserUnitOfWork_Test {

        private IUserDataBaseMapper userDataBaseMapperMock;

        [SetUp]
        public void SetUp() {
            userDataBaseMapperMock = MockRepository.GenerateMock<IUserDataBaseMapper>();
        }

        private UserUnitOfWork CreateSUT(IUserDataBaseMapper userDataBaseMapper = null) {
            return new UserUnitOfWork(userDataBaseMapper);
        }

        [Test]
        public void ConstructorInit_Collections() { 
            //Act
            var SUT = CreateSUT();
            //Assert
            Assert.NotNull(SUT.newUsers);
            Assert.NotNull(SUT.dirtyUsers);
            Assert.NotNull(SUT.deletedUsers);
        }
        [Test]
        public void Constructor_Inject_UserDataBase() {
            //Act
            var SUT = CreateSUT(userDataBaseMapperMock);
            //Assert
            Assert.AreEqual(userDataBaseMapperMock,SUT.UserDataBaseMapper);
        }

        [Test]
        public void MarkAsNew_Add_Item_To_List() {
            //Arrange
            var SUT = CreateSUT();
            var user = new UserModel();
            //Act
            SUT.MarkAsNew(user);
            //Assert
            CollectionAssert.Contains(SUT.newUsers,user);
        }

        [Test]
        public void MarkAsDeleted_Add_Item_To_List() {
            //Arrange
            var SUT = CreateSUT();
            var user = new UserModel();
            //Act
            SUT.MarkAsDeleted(user);
            //Assert
            CollectionAssert.Contains(SUT.deletedUsers,user);
        }

        [Test]
        public void MarkAsDirty_Add_Item_To_Collection() {
            //Arrange
            var SUT = CreateSUT();
            var user = new UserModel();
            //Act
            SUT.MarkAsDirty(user);
            CollectionAssert.Contains(SUT.dirtyUsers,user);
        }

        [Test]
        public void MarkAsDirty_Not_Register_The_Same_Object_Twice() {
            //Arrange
            var SUT = CreateSUT();
            var user = new UserModel();
            //Act
            SUT.MarkAsDirty(user);
            SUT.MarkAsDirty(user);
            //Assert
            Assert.AreEqual(1,SUT.dirtyUsers.Count);
        }

        [Test]
        public void MarkAsDirty_Not_Register_Newly_Created_Elements() {
            //Arranghe
            var SUT = CreateSUT();
            var user = new UserModel();
            //Act
            SUT.MarkAsNew(user);
            SUT.MarkAsDirty(user);
            //Assert
            CollectionAssert.DoesNotContain(SUT.dirtyUsers,user);
        }

        [Test]
        public void Commit_Call_The_DeleteMethod() { 
            //Arrange
            var SUT = CreateSUT(userDataBaseMapperMock);
            //Act
            SUT.Commit();
            //Assert
            userDataBaseMapperMock.AssertWasCalled(x=>x.Delete(SUT.deletedUsers));
        }

        [Test]
        public void Commit_Call_The_Save_Method_With_New_Items() {
            //Arraneg
            var SUT = CreateSUT(userDataBaseMapperMock);
            //Act
            SUT.Commit();
            //Assert
            userDataBaseMapperMock.AssertWasCalled(x => x.Save(SUT.newUsers));
        }

        [Test]
        public void Commit_Call_The_Save_Method_With_Dirty_Items() {
            //Arrange
            var SUT = CreateSUT(userDataBaseMapperMock);
            //Act
            SUT.Commit();
            //Assert
            userDataBaseMapperMock.AssertWasCalled(x => x.Update(SUT.dirtyUsers));
        }

        [Test]
        public void Commit_Clear_Lists() {
            //Arrange
            var SUT = CreateSUT(userDataBaseMapperMock);
            var userDirty = new UserModel();
            var userNew = new UserModel();
            var userDeleted = new UserModel();
            //Act
            SUT.MarkAsDeleted(userDeleted);
            SUT.MarkAsDirty(userDeleted);
            SUT.MarkAsNew(userDeleted);
            SUT.Commit();
            CollectionAssert.IsEmpty(SUT.deletedUsers);
            CollectionAssert.IsEmpty(SUT.dirtyUsers);
            CollectionAssert.IsEmpty(SUT.newUsers);
        }
    }
}
