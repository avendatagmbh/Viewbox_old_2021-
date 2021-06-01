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
    class ApplicationConfigManager_Test {

        private ApplicationConfigManager _applicationConfigManager;
        private IFileManager _filemanager;
        private IXmlConfigManager _xmlconfigmanager;
        private IApplicationConfig _applicationconfig;

        [SetUp]
        public void SetUp() {
            _filemanager = MockRepository.GenerateMock<IFileManager>();
            _xmlconfigmanager = MockRepository.GenerateMock<IXmlConfigManager>();
            _applicationconfig = MockRepository.GenerateMock<IApplicationConfig>();
            
            _applicationConfigManager = new ApplicationConfigManager(_filemanager,_xmlconfigmanager);
        }

        [Test]
        public void Constructor_The_Filemanager_And_XmlManager_Is_Not_Null_Test() {
            //Act
            Assert.NotNull(_applicationConfigManager.FileManager);
            Assert.NotNull(_applicationConfigManager.XmlManager);
        }

        [Test]
        public void ConfigFileName_Test() {
            string expected = "viewboxadmin.xml";
            string actual = _applicationConfigManager.ConfigFileName;
            Assert.AreEqual(expected,actual);
        }

        [Test]
        public void ConfigDirectory_Test() {
            //Arrange
            _filemanager.Stub(x => x.GetFolderPath(Environment.SpecialFolder.ApplicationData)).Return(
                 Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            //Act
            string actual = _applicationConfigManager.ConfigDirectory;
            //Assert
            Assert.IsTrue(actual.Contains("\\AvenDATA\\ViewboxAdmin"));
        }

        [Test]
        public void ConfigDirectory_SpecialFolderPath_Was_Called_From_Config_Directory_Test() {
            //Act
            string configdirectory = _applicationConfigManager.ConfigDirectory;
            //Assert
            _filemanager.AssertWasCalled(x=>x.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        }

        [Test]
        public void Load_Should_Open_A_File_Test() {
            //Act
            _applicationConfigManager.Load();
            //Assert
            _filemanager.AssertWasCalled(x=>x.FileExist(Arg<string>.Is.Anything));
        }

        [Test]
        public void Load_Should_Open_A_Path_Which_Contains_File_And_Directory_Test() {
            //Act
            _applicationConfigManager.Load();
            //Assert
            _filemanager.AssertWasCalled(x => x.FileExist(Arg<string>.Matches(a=>a.Contains(_applicationConfigManager.ConfigDirectory) && a.Contains(_applicationConfigManager.ConfigFileName))));
        }

        [Test]
        public void ConfigDirectory_Should_Check_If_The_Directory_Exist_Test() {
            //Act
            string s = _applicationConfigManager.ConfigDirectory;
            //Assert
            _filemanager.AssertWasCalled(x=> x.DirectoryExist(Arg<string>.Is.Anything));
        }

        [Test]
        public void ConfigDirectory_If_Directory_Doesnt_Exist_It_should_Create_One_Test() {
            //Arrange
            _filemanager.Stub(x => x.DirectoryExist(Arg<string>.Is.Anything)).Return(false);
            //Act
            string configdiretory = _applicationConfigManager.ConfigDirectory;
            //Assert
            _filemanager.AssertWasCalled(x=>x.CreateDirectory(Arg<string>.Is.Anything));
        }

        [Test]
        public void ConfigDirectory_If_Directory_Exist_It_Should_Not_create_One_Test() {
            //Arrange
            _filemanager.Stub(x => x.DirectoryExist(Arg<string>.Is.Anything)).Return(true);
            //Act
            string s = _applicationConfigManager.ConfigDirectory;
            //Assert
            _filemanager.AssertWasNotCalled(x => x.CreateDirectory(Arg<string>.Is.Anything));
        }

        [Test]
        public void Load_Check_XmlLoader_Was_Called_Test() {
            //Arrange
            _filemanager.Stub(x => x.FileExist(Arg<string>.Is.Anything)).Return(true);
            //Act
            _applicationConfigManager.Load();
            //Assert
            _xmlconfigmanager.AssertWasCalled(x=>x.LoadApplicationConfigurationDataFromXml(Arg<string>.Is.Anything));
        }

        [Test]
        public void Load_Check_XmlLoader_Was_Not_Called_If_configfile_not_Exist_Test() {
            //Arrange
            _filemanager.Stub(x => x.FileExist(Arg<string>.Is.Anything)).Return(false);
            //Act
            _applicationConfigManager.Load();
            //Assert
            _xmlconfigmanager.AssertWasNotCalled(x => x.LoadApplicationConfigurationDataFromXml(Arg<string>.Is.Anything));
        }

        [Test]
        public void Load_AfterCallingLoad_The_Appconfig_Is_Loaded_Test() {
            //Arrange
            _filemanager.Stub(x => x.FileExist(Arg<string>.Is.Anything)).Return(true);
            _applicationconfig.Stub(x => x.LastProfile).Return("PROFILE");
            _applicationconfig.Stub(x => x.LastUser).Return("USER");
            _xmlconfigmanager.Stub(x => x.LoadApplicationConfigurationDataFromXml(Arg<string>.Is.Anything)).Return(
                _applicationconfig);
            //Act
            _applicationConfigManager.Load();
            //Assert
            Assert.AreEqual(_applicationconfig,_applicationConfigManager.ApplicationConfig);
            Assert.AreEqual("PROFILE",_applicationConfigManager.ApplicationConfig.LastProfile);
            Assert.AreEqual("USER", _applicationConfigManager.ApplicationConfig.LastUser);
        }

        [Test]
        public void Load_Calling_Load_Should_Contain_File_Path_Info_Test() {
            //Arrange
            _filemanager.Stub(x => x.FileExist(Arg<string>.Is.Anything)).Return(true);
            //Act
            _applicationConfigManager.Load();
            //Assert
            _xmlconfigmanager.AssertWasCalled(
                x =>
                x.LoadApplicationConfigurationDataFromXml(Arg<string>.Matches(a => a.Contains(_applicationConfigManager.ConfigDirectory) && a.Contains(_applicationConfigManager.ConfigFileName) )));
        }

        [Test]
        public void Save_Should_Call_XMLWriter_Save_Method_Test() {
            //Act
            _applicationConfigManager.Save();
            //Assert
            _xmlconfigmanager.AssertWasCalled(x=>x.SaveApplicationConfigurationDataToXml(Arg<IApplicationConfig>.Is.Anything,Arg<string>.Is.Anything));
        }

        [Test]
        public void Save_Should_Call_XMLWriter_Save_With_Correct_Path() {
            //Act
            _applicationConfigManager.Save();
            //Assert
            _xmlconfigmanager.AssertWasCalled(x => x.SaveApplicationConfigurationDataToXml(Arg<IApplicationConfig>.Is.Anything, Arg<string>.Matches(a=>a.Contains(_applicationConfigManager.ConfigDirectory) && a.Contains(_applicationConfigManager.ConfigFileName))));
        }

        [Test]
        public void Save_Should_Call_XMLWriter_Save_With_The_Current_Config() {
            //Arrange
            _applicationConfigManager.ApplicationConfig = _applicationconfig;
            //Act
            _applicationConfigManager.Save();
            //Assert
            _xmlconfigmanager.AssertWasCalled(x => x.SaveApplicationConfigurationDataToXml(Arg<IApplicationConfig>.Matches(a=>a==_applicationConfigManager.ApplicationConfig), Arg<string>.Is.Anything));
        }
            
    }
}
