using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using ViewboxAdminBusiness.Manager;
using ViewboxAdmin_ViewModel.Structures.Config;

namespace Viewbox_Admin_ViewModel_Test
{
    [TestFixture]
    class ApplicationConfig_Test {

        private IApplicationConfig _appConfig = null;
        private IFileManager _filemanagerMock = null;
        [SetUp]
        public void SetUp() {
            _filemanagerMock = MockRepository.GenerateMock<IFileManager>();
            _appConfig = new ApplicationConfig(_filemanagerMock);
        }

        [Test]
        public void Constructor_ConfigDirectory_Hardcoded() {
            //Assert
            Assert.IsTrue(_appConfig.ConfigDirectory.Contains("\\AvenDATA\\ViewboxAdmin"));
        }

        [Test]
        public void LastProfile_Change_Property_Fired_Test() {
            //Arrange
            bool IsFired = false;
            _appConfig.PropertyChanged += (o, e) => { if (e.PropertyName == "LastProfile") IsFired = true; };
            //Act
            _appConfig.LastProfile = "Foo";
            //Assert
            Assert.IsTrue(IsFired);
        }

        [Test]
        public void LastUser_Change_Property_Fired_Test() {
            //Arrange
            bool IsFired = false;
            _appConfig.PropertyChanged += (o, e) => { if (e.PropertyName == "LastUser") IsFired = true; };
            //Act
            _appConfig.LastUser = "Foo";
            //Assert
            Assert.IsTrue(IsFired);
        }

        [Test]
        public void LastProfile_Change_Property_Not_Fired_With_The_Same_Value_Test() {
            //Arrange
            bool IsFired = false;
            _appConfig.LastProfile = "Foo";
            _appConfig.PropertyChanged += (o, e) => IsFired = true;
            //Act
            _appConfig.LastProfile = "Foo";
            //Assert
            Assert.IsFalse(IsFired);
        }

        [Test]
        public void LastUser_Change_Property_Not_Fired_With_The_Same_Value_Test() {
            //Arrange
            bool IsFired = false;
            _appConfig.LastUser = "Foo";
            _appConfig.PropertyChanged += (o, e) => IsFired = true;
            //Act
            _appConfig.LastUser = "Foo";
            //Assert
            Assert.IsFalse(IsFired);
        }

        [Test]
        public void The_Directory_Should_Be_Checked_If_Exist() {
            //Act
            _appConfig.ConfigDirectory="Foo";
            //Assert
            _filemanagerMock.AssertWasCalled(x=>x.DirectoryExist("Foo"));
        }




        [Test]
        public void Constructor_Should_Call_The_Filemanager() {
            //Assert
            _filemanagerMock.AssertWasCalled(x => x.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));
        }

        [Test]
        public void Constructor_Properties_Get_Initialized() {
            //Assert
            Assert.AreEqual(string.Empty,_appConfig.LastProfile);
            Assert.AreEqual(string.Empty,_appConfig.LastUser);
            Assert.AreEqual(ConfigLocation.Directory,_appConfig.ConfigLocationType);
            Assert.IsTrue(_appConfig.ConfigDirectory.Contains("\\AvenDATA\\ViewboxAdmin"));

        }

        [Test]
        public void ConfigDirectory_Dont_Create_Directory_If_Already_Exist_Test() {
            //Arrange
            _filemanagerMock.Stub(x => x.DirectoryExist(Arg<string>.Is.Anything)).Return(true); // mock that diretory exist already
            //Act
            _appConfig.ConfigDirectory = "foo";
            //Assert
            _filemanagerMock.AssertWasCalled(x => { x.CreateDirectory(Arg<string>.Is.Anything); }, options => options.Repeat.Once()); // it is called from constructor
        }

        [Test]
        public void ConfigDirectory_If_directory_Exists_Do_Create_Test() {
            //Arrange
            _filemanagerMock.Stub(x => x.DirectoryExist(Arg<string>.Is.Anything)).Return(false);
            //Act
            _appConfig.ConfigDirectory = "foo";
            //Assert
            _filemanagerMock.AssertWasCalled(x => x.CreateDirectory(Arg<string>.Is.Anything), options => options.Repeat.Times(2)); // it is called in the constructor too !!!
        }

        [Test]
        public void ConfigDirectory_Set_Trim_Test() {
            _appConfig.ConfigDirectory = "   space   ";
            Assert.AreEqual("space",_appConfig.ConfigDirectory);
        }

       

    }
}
