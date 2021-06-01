using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using SystemDb.Upgrader;
using DbAccess;
using DbAccess.Structures;
using NUnit.Framework;
using Rhino.Mocks;
using ViewboxAdmin_ViewModel.Structures.Config;

namespace Viewbox_Admin_ViewModel_Test
{
    [TestFixture]
    internal class Profile_Test {
        private IProfile _profile;
        private ISystemDb _systemdbMock;
        private IDbConfig _dbconfigMock;
        private IDatabaseOutOfDateInformation _dboutofdateinfoMock;


        [SetUp]
        public void SetUp() {
           
            _systemdbMock = MockRepository.GenerateMock<ISystemDb>();
            _dbconfigMock = MockRepository.GenerateMock<IDbConfig>();
            _profile = new Profile(_systemdbMock,_dbconfigMock);
            _dboutofdateinfoMock = MockRepository.GenerateMock<IDatabaseOutOfDateInformation>();
        }

        [Test]
        public void TestInfrastructure() { Assert.IsTrue(true); }

        [Test]
        public void Constructor_DB_Is_Not_Null() { Assert.NotNull(_profile.SystemDb); }

        [Test]
        public void Name_Property_Change_Fired_Test() {
            //Arrange
            bool isFired = false;
            _profile.PropertyChanged += (o, e) => { if (e.PropertyName == "Name") isFired = true; };
            //Act
            _profile.Name = "foooooo";
            //Assert
            Assert.IsTrue(isFired);
        }

        [Test]
        public void Description_Property_Change_Fired_Test() {
            //Arrange
            bool isFired = false;
            _profile.PropertyChanged += (o, e) => { if (e.PropertyName == "Description") isFired = true; };
            //Act
            _profile.Description = "descript";
            //Assert
            Assert.IsTrue(isFired);
        }

        [Test]
        public void Loaded_Property_Change_Fired_Test() {
            //Arrange
            bool isFired = false;
            _profile.PropertyChanged += (o, e) => { if (e.PropertyName == "Loaded") isFired = true; };
            //Act
            _profile.Loaded = true;
            //Assert
            Assert.IsTrue(isFired);
        }

        [Test]
        public void Loaded_Property_Change_Fires_Displaystring_Too_Test() {
            //Arrange
            bool isFired = false;
            _profile.PropertyChanged += (o, e) => { if (e.PropertyName == "DisplayString") isFired = true; };
            //Act
            _profile.Loaded = true;
            //Assert
            Assert.IsTrue(isFired);
        }

        [Test]
        public void IsLoading_Property_Change_Fired_Test() {
            //Arrange
            bool isFired = false;
            _profile.PropertyChanged += (o, e) => { if (e.PropertyName == "IsLoading") isFired = true; };
            //Act
            _profile.IsLoading = true;
            //Assert
            Assert.IsTrue(isFired);
        }

        [Test]
        public void IsLoading_Property_Change_Fires_Displaystring_Too_Test() {
            //Arrange
            bool isFired = false;
            _profile.PropertyChanged += (o, e) => { if (e.PropertyName == "DisplayString") isFired = true; };
            //Act
            _profile.IsLoading = true;
            //Assert
            Assert.IsTrue(isFired);
        }

        [Test]
        public void Name_Property_Change_NOT_Fire_If_The_Same_value_Is_Set_Test() {
            //Arrange
            bool isFired = false;
            string value = "whatever";
            _profile.Name = value;
            _profile.PropertyChanged += (o, e) => { if (e.PropertyName == "Name") isFired = true; };
            //Act
            _profile.Name = value;
            //Assert
            Assert.IsFalse(isFired);
        }

        [Test]
        public void Description_Property_Change_NOT_Fired_If_the_Same_Value_Is_Set_Test() {
            //Arrange
            bool isFired = false;
            string value = "etwas";
            _profile.Description = value;
            _profile.PropertyChanged += (o, e) => { if (e.PropertyName == "Description") isFired = true; };
            //Act
            _profile.Description = value;
            //Assert
            Assert.IsFalse(isFired);
        }

        [Test]
        public void Loaded_Property_Change_NOT_Fired_If_the_Same_value_Set_Test() {
            //Arrange
            bool isFired = false;
            bool value = true;
            _profile.Loaded = value;
            _profile.PropertyChanged += (o, e) => { if (e.PropertyName == "Loaded") isFired = true; };
            //Act
            _profile.Loaded = value;
            //Assert
            Assert.IsFalse(isFired);
        }

        [Test]
        public void Loaded_Property_Change_NOT_Fired_If_The_Same_Value_Set_Displaystring_Too_Test() {
            //Arrange
            bool isFired = false;
            bool value = true;
            _profile.Loaded = value;
            _profile.PropertyChanged += (o, e) => { if (e.PropertyName == "DisplayString") isFired = true; };
            //Act
            _profile.Loaded = value;
            //Assert
            Assert.IsFalse(isFired);
        }

        [Test]
        public void IsLoading_Property_Change_NOT_Fired_If_The_Same_Value_Set_Test() {
            //Arrange
            bool isFired = false;
            bool value = true;
            _profile.IsLoading = value;
            _profile.PropertyChanged += (o, e) => { if (e.PropertyName == "IsLoading") isFired = true; };
            //Act
            _profile.IsLoading = value;
            //Assert
            Assert.IsFalse(isFired);
        }

        [Test]
        public void IsLoading_Property_Change_NOT_Fired_If_The_Same_Value_Set_Displaystring_Too_Test() {
            //Arrange
            bool isFired = false;
            bool value = true;
            _profile.IsLoading = value;
            _profile.PropertyChanged += (o, e) => { if (e.PropertyName == "DisplayString") isFired = true; };
            //Act
            _profile.IsLoading = value;
            //Assert
            Assert.IsFalse(isFired);
        }

        [Test]
        public void Name_InvalidFileName_Should_Throw_Argument_Exception() {
            //Assert
            Assert.Catch<ArgumentException>(() => _profile.Name = "*");
            Assert.Catch<ArgumentException>(() => _profile.Name = "/");
            Assert.Catch<ArgumentException>(() => _profile.Name = "normalpart_...*");
            Assert.DoesNotThrow(()=>_profile.Name = "normalfilename");
        }

        [Test]
        public void IsLoading_DisplayString_Should_Contain_IsLoading_String() {
            //Act
            _profile.IsLoading = true;
            //Assert
            Assert.IsTrue(_profile.DisplayString.Contains("loading"));
        }
         
        [Test]
        public void DisplayString_Should_Contain_DB_Not_Up_To_date_String() {
            //Arrange
            _profile.IsLoading = false;
            //Act
            _profile.DatabaseOutOfDateInformation = _dboutofdateinfoMock;
            //Assert
            Assert.IsTrue(_profile.DisplayString.Contains("up-to-date"));
        }

        [Test]
        public void DataBaseOutOfDate_Property_Fires_Event() {
            //Arrange
            bool isFired = false;
            _profile.PropertyChanged += (o, e) => { if (e.PropertyName == "DatabaseOutOfDateInformation") isFired = true; };
            //Act
            _profile.DatabaseOutOfDateInformation = _dboutofdateinfoMock;
            //Assert
            Assert.IsTrue(isFired);
        }

        [Test]
        public void DataBaseOutOfDate_Property_Fires_display_String_Event() {
            //Arrange
            bool isFired = false;
            _profile.PropertyChanged += (o, e) => { if (e.PropertyName == "DisplayString") isFired = true; };
            //Act
            _profile.DatabaseOutOfDateInformation = _dboutofdateinfoMock;
            //Assert
            Assert.IsTrue(isFired);
        }

        [Test]
        public void Load_If_Loading_Connect_Is_not_Called() {
            //Arrange
            _profile.IsLoading = true;
            //Act
            _profile.Load();
            //Assert
            _systemdbMock.AssertWasNotCalled(x=>x.Connect(Arg<string>.Is.Anything,Arg<string>.Is.Anything,Arg<int>.Is.Anything));
        }

        [Test]
        public void Load_If_Loaded_Connect_Is_not_Called() {
            //Arrange
            _profile.Loaded = true;
            //Act
            _profile.Load();
            //Assert
            _systemdbMock.AssertWasNotCalled(x => x.Connect(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<int>.Is.Anything));
        }

        [Test]
        public void Load_If_Loading_Connect_Is_Called() {
            //Arrange
            _profile.IsLoading = false;
            _profile.Loaded = false;
            //Act
            _profile.Load();
            //Assert
            _systemdbMock.AssertWasCalled(x => x.Connect(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<int>.Is.Anything));
        }

        [Test]
        public void Load_If_Loading_Connect_is_called_With_Correct_Params() {
            //Arrange
            _profile.IsLoading = false;
            _profile.Loaded = false;
            _dbconfigMock.Stub(x=>x.DbType).Return("yxz");
            _dbconfigMock.Stub(x=>x.ConnectionString).Return("connectio");
            //Act
            _profile.Load();
            //Assert
            _systemdbMock.AssertWasCalled(x => x.Connect(Arg<string>.Matches(o=> o.Equals(_dbconfigMock.DbType)),Arg<string>.Matches(u=>u.Equals(_dbconfigMock.ConnectionString)),Arg<int>.Is.Anything));
        }

        [Test]
        public void Load_If_Database_Out_Of_Date_Loaded_Should_Be_False() {
            //Arrange
            _profile.IsLoading = false;
            _profile.Loaded = false;
            _systemdbMock.Stub(x => x.DatabaseOutOfDateInformation).Return(_dboutofdateinfoMock);
            //Act
            _profile.Load();
            //Assert
            Assert.IsFalse(_profile.Loaded);
        }

        //[Test]
        //public void Load_If_Database_NOT_Out_Of_Date_Loaded_Should_Be_False() {
        //    //Arrange
        //    _profile.IsLoading = false;
        //    _profile.Loaded = false;
        //    _systemdbMock.Stub(x => x.DatabaseOutOfDateInformation).Return(null);
        //    //Act
        //    _profile.Load();
        //    //Assert
        //    Assert.IsTrue(_profile.Loaded);
        //}

        //[Test]
        //public void Load_If_Connect_throws_exception() {
        //    //Arrange
        //    _profile.IsLoading = false;
        //    _profile.Loaded = false;
        //    Exception e = new Exception();
        //    _systemdbMock.Stub(x => x.DatabaseOutOfDateInformation).Throw(e);
        //    //Act
        //    Assert.Catch(() => _profile.Load());
        //    //Assert
        //    Assert.IsFalse(_profile.Loaded);
        //}
        [Test]
        public void UpgradeDataBase_Called_Test() {
            //Arrange
            _profile.DatabaseOutOfDateInformation = _dboutofdateinfoMock;
            //Act
            _profile.UpgradeDatabase();
            //Assert
            _systemdbMock.AssertWasCalled(x => x.UpgradeDatabase(Arg<IDatabase>.Is.Anything));
            
        }

        [Test]
        public void UpgradeDataBase_NOT_Called_Test() {
            //Arrange
            _profile.DatabaseOutOfDateInformation = null;
            //Act
            _profile.UpgradeDatabase();
            //Assert
            _systemdbMock.AssertWasNotCalled(x => x.UpgradeDatabase(Arg<IDatabase>.Is.Anything));

        }

        [Test]
        public void Description_get_set_Test() {
            _profile.Description = "whatever";
            Assert.AreEqual("whatever",_profile.Description);
        }

        [Test]
        public void DisplayStringContainsName_test() { 
            _profile.Name = "whatever";
            _profile.IsLoading = false;
            _profile.DatabaseOutOfDateInformation = null;
            Assert.IsTrue(_profile.DisplayString==_profile.Name);
        }

        [Test]
        public void IsLoading_After_LoadEnded_False() {
            
            _profile.Load();
            _systemdbMock.Raise(x=>x.LoadingFinished+=null);
            Assert.IsFalse(_profile.IsLoading);
        }
    




}
}
