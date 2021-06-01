using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using ViewboxAdminBusiness.Manager;
using ViewboxAdmin_ViewModel.Structures.Config;


namespace Viewbox_Admin_ViewModel_Test
{
    [TestFixture]
    internal class ProfileManager_Test {
        private IProfileManager _profinfoManager;
        private IApplicationManager _appmanagerMock;
        private IApplicationConfig _applicationconfigMock;
        private IFileManager _filemanangerMock;
        private IProfile _profileMock;
        private IXmlProfileManager _xmlprofileMock;

        [SetUp]
        public void SetUp() {
            //create mock objects
            _appmanagerMock = MockRepository.GenerateMock<IApplicationManager>();
            _applicationconfigMock = MockRepository.GenerateMock<IApplicationConfig>();
            _filemanangerMock = MockRepository.GenerateMock<IFileManager>();
            _profileMock = MockRepository.GenerateMock<IProfile>();
            _xmlprofileMock = MockRepository.GenerateMock<IXmlProfileManager>();

            _appmanagerMock.Stub(x => x.ApplicationConfig).Return(_applicationconfigMock);
            _appmanagerMock.ApplicationConfig = _applicationconfigMock;

            //create the actual test class
            _profinfoManager = new ProfileManager(_appmanagerMock, _filemanangerMock, _xmlprofileMock);
        }

        [Test]
        public void InfrastructureTest() {
            Assert.AreEqual(true, true);
        }

        [Test]
        public void Constructor_The_Profile_Collection_Is_Not_Null_Object() {
            Assert.IsNotNull(_profinfoManager.Profiles);
        }

        [Test]
        public void Constructor_Appmanager_Dependency_Injection_Through_Constructor_Test() {
            Assert.AreEqual(_appmanagerMock, _profinfoManager.ApplicationManager);
        }

        public void Constructor_FileManager_DependencyInjection_Through_Constructor_Test() {
            Assert.AreEqual(_filemanangerMock, _profinfoManager.FileManager);
        }


        [Test]
        public void Constructor_ProfileDirectory_Should_Be_Under_the_Config_Directory_Test() {
            _applicationconfigMock.Stub(x => x.ConfigDirectory).Return("Basepath");
            Assert.IsTrue(_profinfoManager.ProfileDirectory.Contains("Basepath"));
        }

        [Test]
        public void Save_If_ConfigLocation_Is_Directory_Directory_Existence_Checked() {
            //Arrange
            _applicationconfigMock.Stub(x => x.ConfigLocationType).Return(ConfigLocation.Directory);
            //Act
            _profinfoManager.Save(_profileMock);
            //Assert
            _filemanangerMock.AssertWasCalled(x => x.DirectoryExist(Arg<string>.Is.Anything));
        }

        [Test]
        public void Save_If_Profile_Diretory_Not_Exist_Than_Create_One() {
            //Arrange
            _applicationconfigMock.Stub(x => x.ConfigLocationType).Return(ConfigLocation.Directory);
            _filemanangerMock.Stub(x => x.DirectoryExist(Arg<string>.Is.Anything)).Return(false);
            //Act
            _profinfoManager.Save(_profileMock);
            //Assert
            _filemanangerMock.AssertWasCalled(x => x.CreateDirectory(Arg<string>.Is.Anything));
        }

        [Test]
        public void Save_If_Profile_Diretory_Exist_Than_NOT_Create_One() {
            //Arrange
            _applicationconfigMock.Stub(x => x.ConfigLocationType).Return(ConfigLocation.Directory);
            _filemanangerMock.Stub(x => x.DirectoryExist(Arg<string>.Is.Anything)).Return(true);
            //Act
            _profinfoManager.Save(_profileMock);
            //Assert
            _filemanangerMock.AssertWasNotCalled(x => x.CreateDirectory(Arg<string>.Is.Anything));
        }

        [Test]
        public void Save_SaveToXml_Is_Called_If_One_Wants_To_Save_Test() {
            //Arraneg
            _applicationconfigMock.Stub(x => x.ConfigLocationType).Return(ConfigLocation.Directory);
            //Act
            _profinfoManager.Save(_profileMock);
            //Assert
            _xmlprofileMock.AssertWasCalled(x => x.SaveProfileToXml(Arg<IProfile>.Is.Anything, Arg<string>.Is.Anything));
        }

        [Test]
        public void Save_SaveToXml_Was_Called_With_The_Correct_Filepath_Parameters() {
            //Arrange
            _applicationconfigMock.Stub(x => x.ConfigLocationType).Return(ConfigLocation.Directory);
            //Act
            _profinfoManager.Save(_profileMock);
            //Assert
            _xmlprofileMock.AssertWasCalled(
                x =>
                x.SaveProfileToXml(Arg<IProfile>.Is.Anything,
                                   Arg<string>.Matches(o => o.Contains(_profinfoManager.ProfileDirectory))));
        }

        [Test]
        public void Save_SaveToXml_Was_Called_With_The_Correct_Filepath_Parameters_2() {
            //Arraneg
            _applicationconfigMock.Stub(x => x.ConfigLocationType).Return(ConfigLocation.Directory);
            _profileMock.Stub(x => x.Name).Return("name");
            //Act
            _profinfoManager.Save(_profileMock);
            //Assert
            _xmlprofileMock.AssertWasCalled(
                x => x.SaveProfileToXml(Arg<IProfile>.Is.Anything, Arg<string>.Matches(o => o.Contains("name"))));
        }

        [Test]
        public void Save_SaveToXml_Is_Called_With_Correct_Profile_Test() {
            //Arraneg
            _applicationconfigMock.Stub(x => x.ConfigLocationType).Return(ConfigLocation.Directory);
            //Act
            _profinfoManager.Save(_profileMock);
            //Assert
            _xmlprofileMock.AssertWasCalled(
                x => x.SaveProfileToXml(Arg<IProfile>.Matches(o => o == _profileMock), Arg<string>.Is.Anything));
        }

        [Test]
        public void Profiles_After_Saving_The_Item_It_Is_In_The_Collection() {
            _profinfoManager.Save(_profileMock);
            
            int expect = 1;
            int elementincollection = _profinfoManager.Profiles.Count;
            //Assert
            Assert.AreEqual(expect,elementincollection);
        }

        [Test]
        public void Profiles_GetProfile_From_Collection() {
            _profileMock.Stub(x => x.Name).Return("NaMe");
            //Act
            _profinfoManager.Profiles.Add(_profileMock);
            IProfile actual = _profinfoManager.GetProfile("Name");
            //Assert
            Assert.AreEqual(_profileMock,actual);
        }

        [Test]
        public void Profiles_GetProfile_From_Collection_If_Not_Exist() {
            //Arrange
            _profileMock.Stub(x => x.Name).Return("Foo");
            //Act
            _profinfoManager.Profiles.Add(_profileMock);
            //Assert
            IProfile actual = _profinfoManager.GetProfile("Whatever");
            Assert.IsNull(actual);
        }




        [Test]
        public void Open_OpenXml_IS_called_Test() {
            //Arrange
            _applicationconfigMock.Stub(x => x.ConfigLocationType).Return(ConfigLocation.Directory);
            //Act
            _profinfoManager.Open("profile");
            //Assert
            _xmlprofileMock.AssertWasCalled(x => x.LoadProfileFromFile(ref Arg<IProfile>.Ref(Rhino.Mocks.Constraints.Is.Anything(), _profileMock).Dummy,
                                 Arg<string>.Is.Anything)); 
        }

        [Test]
        public void Open_OpenFromXml_Was_Called_with_The_correct_filepath_Parameters() {
            //Arrange
            _applicationconfigMock.Stub(x => x.ConfigLocationType).Return(ConfigLocation.Directory);
            //Act
            _profinfoManager.Open("profile");
            //Assert
            _xmlprofileMock.AssertWasCalled(x => x.LoadProfileFromFile(ref Arg<IProfile>.Ref(Rhino.Mocks.Constraints.Is.Anything(), _profileMock).Dummy,
                                 Arg<string>.Matches(o=>o.Contains(_profinfoManager.ProfileDirectory)))); ;
        }

        [Test]
        public void Open_OpenFromXml_Was_Called_with_The_correct_filepath_Parameters_2() {
            //Arrange
            _applicationconfigMock.Stub(x => x.ConfigLocationType).Return(ConfigLocation.Directory);
            //Act
            _profinfoManager.Open("profile");
            //Assert
            _xmlprofileMock.AssertWasCalled(x => x.LoadProfileFromFile(ref Arg<IProfile>.Ref(Rhino.Mocks.Constraints.Is.Anything(),_profileMock).Dummy,
                Arg<string>.Matches(o => o.Contains("profile" + ".xml"))));
        }

        

        [Test]
        public void DeleteProfile_FileExist_Was_Called_Correct_Path_Test() {
            //Arrange
            _profileMock.Stub(x => x.Name).Return("fooo");
            //Act
            _profinfoManager.DeleteProfile(_profileMock);
            //Assert
            _filemanangerMock.AssertWasCalled(
                x =>
                x.FileExist(Arg<string>.Matches(o => o.Contains(_profinfoManager.ProfileDirectory) && o.Contains("fooo"))));
        }

        [Test]
        public void Open_The_Profile_from_Xml_Was_Returned_By_Save_Method() {
            //Arrange
            _xmlprofileMock.Stub(
                x =>
                x.LoadProfileFromFile(ref Arg<IProfile>.Ref(Rhino.Mocks.Constraints.Is.Anything(), _profileMock).Dummy,
                                      Arg<string>.Is.Anything));
            //Act
            IProfile profile = _profinfoManager.Open("whatever");
            //Assert
            Assert.AreEqual(_profileMock,profile);

        }

        [Test]
        public void Open_If_the_fileOperation_Throw_An_Exception_It_Will_ReThrown() {
            //Arrange
            Exception e = new Exception();
            _xmlprofileMock.Stub(
                x =>
                x.LoadProfileFromFile(ref Arg<IProfile>.Ref(Rhino.Mocks.Constraints.Is.Anything(), _profileMock).Dummy,
                                      Arg<string>.Is.Anything)).Throw(e);
            //Act
            
            //Assert
            Assert.Catch<Exception>(() => { IProfile prof = _profinfoManager.Open("whatever"); });


        }

        [Test]
        public void DeleteProfile_Call_file_Deletion_If_file_exists_Test() {
            //Arrange
            _filemanangerMock.Stub(x => x.FileExist(Arg<string>.Is.Anything)).Return(true);
            //Act
            _profinfoManager.DeleteProfile(_profileMock);
            //Assert
            _filemanangerMock.AssertWasCalled(x => x.Delete(Arg<string>.Is.Anything));
        }

        [Test]
        public void DeleteProfile_NOT_Call_file_Deletion_If_file_NOT_exists_Test() {
            //Arrange
            _filemanangerMock.Stub(x => x.FileExist(Arg<string>.Is.Anything)).Return(false);
            //Act
            _profinfoManager.DeleteProfile(_profileMock);
            //Assert
            _filemanangerMock.AssertWasNotCalled(x => x.Delete(Arg<string>.Is.Anything));
        }

        [Test]
        public void DeleteProfile_Check_Profile_get_removed_From_Collection() {
            //Arrange
            _profinfoManager.Profiles.Add(_profileMock);
            //Act
            _profinfoManager.DeleteProfile(_profileMock);
            //Assert
            Assert.IsFalse(_profinfoManager.Profiles.Contains(_profileMock));
        }

        [Test]
        public void LoadProfiles_After_LoadProfile_The_Collection_All_Old_Items_Are_Removed_From_Collection() {
            //Arrange
            _profinfoManager.Profiles.Add(_profileMock);
            //Act
            _profinfoManager.LoadProfiles();
            //Assert
            Assert.IsFalse(_profinfoManager.Profiles.Contains(_profileMock));
        }

        [Test]
        public void LoadProfiles_If_Init_Called_Is_Directory_Directory_Existence_Checked() {
            //Arrange
            _applicationconfigMock.Stub(x => x.ConfigLocationType).Return(ConfigLocation.Directory);
            //Act
            _profinfoManager.LoadProfiles();
            //Assert
            _filemanangerMock.AssertWasCalled(x => x.DirectoryExist(_profinfoManager.ProfileDirectory));
        }
    

        [Test]
        public void LoadProfiles_Check_File_CollectionWas_Called() {
            //Arrange
            var _fileinfo = MockRepository.GenerateMock<FileInfo>();
            _fileinfo.Stub(x => x.Name).Return("filename");
            _applicationconfigMock.Stub(x => x.ConfigLocationType).Return(ConfigLocation.Directory);
            _filemanangerMock.Stub(x => x.DirectoryExist(Arg<string>.Is.Anything)).Return(true);
            _filemanangerMock.Stub(x => x.GetFileInfo(_profinfoManager.ProfileDirectory)).Return(new List<FileInfo>(){_fileinfo});
            //Act
            _profinfoManager.LoadProfiles();
            //Assert
            _filemanangerMock.AssertWasCalled(x => x.GetFileInfo(_profinfoManager.ProfileDirectory));
            
        }

        [Test]
        public void LoadProfiles_Check_File_Was_Load_Correctly() {
            //Arrange
            var _fileinfo = MockRepository.GenerateMock<FileInfo>();
            _fileinfo.Stub(x => x.Name).Return("JustAFakeName.xml");
            _profileMock.Stub(x => x.Name).Return("JustAFakeName");
            _xmlprofileMock.Stub(x => x.LoadProfileFromFile(ref Arg<IProfile>.Ref(Rhino.Mocks.Constraints.Is.Anything(),_profileMock).Dummy,
                                 Arg<string>.Is.Anything));
            _applicationconfigMock.Stub(x => x.ConfigLocationType).Return(ConfigLocation.Directory);
            _filemanangerMock.Stub(x => x.DirectoryExist(Arg<string>.Is.Anything)).Return(true);
            _filemanangerMock.Stub(x => x.GetFileInfo(_profinfoManager.ProfileDirectory)).Return(new List<FileInfo>() { _fileinfo });
            //Act
            _profinfoManager.LoadProfiles();
            //Assert
            Assert.AreEqual(1,_profinfoManager.Profiles.Count);
            IProfile prof = _profinfoManager.Profiles[0];
            Assert.AreEqual("JustAFakeName",prof.Name);

        }
        [Test]
        public void Save_Profiles_Adding_Element_CollectionChanged_Fires() {
            bool isfired = false;
            _profinfoManager.Profiles.CollectionChanged += (s, e) => { if (e.NewItems[0] == _profileMock) isfired = true; };
            _profinfoManager.Save(_profileMock);
            Assert.IsTrue(isfired);

            
        }

}
}
