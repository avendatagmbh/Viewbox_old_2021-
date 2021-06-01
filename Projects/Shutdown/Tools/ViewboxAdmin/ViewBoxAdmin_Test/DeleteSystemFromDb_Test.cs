using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using NUnit.Framework;
using Rhino.Mocks;
using ViewboxAdmin.ViewModels;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    class DeleteSystemFromDb_Test {

        private IDeleteSystemStrategy _deletestrategy;
        private ISystemDb _systemDbMock;
        private IOptimizationCollection _optcollectionMock;
        private IOptimization _optimization;
        private IOptimizationGroup _optgroup;
        private IIssue _issueMock;
        private IIssueCollection _issueCollectionMock;
        private IParameterCollection _parameterCollectionMock;
        private IParameter _parameterMock;


        [SetUp]
        public void SetUp() {
            //systemDB mock
            _systemDbMock = MockRepository.GenerateMock<ISystemDb>();

            //in memory object mocks
            _optcollectionMock = MockRepository.GenerateMock<IOptimizationCollection>();
            _optimization = MockRepository.GenerateMock<IOptimization>();
            _optgroup = MockRepository.GenerateMock<IOptimizationGroup>();
            _issueMock = MockRepository.GenerateMock<IIssue>();
            _issueCollectionMock = MockRepository.GenerateMock<IIssueCollection>();
            _parameterCollectionMock = MockRepository.GenerateMock<IParameterCollection>();
            _parameterMock = MockRepository.GenerateMock<IParameter>();
            
            _deletestrategy = new DeleteSystemReporter(_systemDbMock);

        }

        [Test]
        public void Constructor_Dependency_Injection_Test() {
            Assert.NotNull(_deletestrategy.SystemDb);
        }

        [Test]
        public void Constructor_ThrowException_If_SystemDb_NULL() {
            Assert.Catch(() => new DeleteSystemReporter(null));
        }



        [Test]
        public void DeleteSystemFromMetaDataBase_If_Optimization_Not_Exist_Crash() {
            //Arrange
            _optcollectionMock.Stub(x => x[Arg<int>.Is.Anything]).Return(null);
            _systemDbMock.Stub(x => x.Optimizations).Return(_optcollectionMock);
            bool isfired = false;
            bool isdebugfired = false;
            string debugmessage = string.Empty;
            _deletestrategy.Crashed += ((o, e) => isfired = true);
            _deletestrategy.DebugEvent += ((o, e) => { isdebugfired = true;
                debugmessage = e.DebugMessage;
            });
            //Act
            _deletestrategy.DeleteSystemFromMetaDataBase(12);
            //Assert
            Assert.IsTrue(isfired);
            Assert.IsTrue(isdebugfired);
            Assert.IsFalse(string.IsNullOrEmpty(debugmessage));
        }


        [Test]
        public void DeleteSystemFromMetaDataBase_If_The_Opt_Group_Id_Is_not_correct_Crash() {
            //Arrange
            int optid = 77;
            SetUpOptimization(optid);
            bool isfired = false;
            _deletestrategy.Crashed += ((o, e) => isfired = true);
            //Act
            _deletestrategy.DeleteSystemFromMetaDataBase(optid);
            //Assert
            Assert.IsTrue(isfired);
        }

        private void SetUpOptimization(int optid) {
            _optgroup.Expect(x => x.Id).Return(3);
            _optimization.Expect(x => x.Group).Return(_optgroup);
            _optcollectionMock.Expect(x => x[optid]).Return(_optimization);
            _systemDbMock.Expect(x => x.Optimizations).Return(_optcollectionMock);
        }

        //[Test]
        //public void DeleteSystemFromMetaDataBase_The_Following_Delete_Methods_Should_Be_Called_Test() {
        //    // This is obviously a test smell... Too much work to set up this
        //    //Arrange
        //    int optid = 12;
        //    int tableid = 23;
        //    int columnid = 78;
        //    int paramid = 88;
        //    string valuestring = "DatabaseName";
        //    //setting up optimization mock
        //    SetUpAFullSystemDBMock(paramid, columnid, optid, valuestring, tableid);
        //    //Act
        //    _deletestrategy.DeleteSystemFromMetaDataBase(optid);
        //    //Assert
        //    //Table related elements
        //    _systemDbMock.AssertWasCalled(x => x.DeleteTableObject(tableid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteTableArchiveInfo(tableid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteTableRoles(tableid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteTableOriginalName(tableid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteTableSchemes(tableid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteOrderArea(tableid));
            
        //    _systemDbMock.AssertWasCalled(x => x.DeleteTableText(tableid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteTableUsers(tableid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteTableOrder(tableid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteUserTableSettings(tableid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteIssues(tableid));
        //    //column related elements
        //    _systemDbMock.AssertWasCalled(x => x.DeleteColumn(columnid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteRoleColumn(columnid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteColumnTexts(columnid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteUsersColumn(columnid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteColumnOrder(columnid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteUserColumnSettings(columnid));
        //    //Optimization related elements
        //    _systemDbMock.AssertWasCalled(x=>x.RemoveOptimizationFromAllTables(optid));
        //    //Delete relations 
        //    _systemDbMock.AssertWasCalled(x=>x.DeleteRelations(valuestring));
        //    //Delete 
        //    _systemDbMock.AssertWasCalled(x => x.DeleteParameters(paramid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteParameterText(paramid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteParameterCollection(paramid));

            
        //}

        private void SetUpAFullSystemDBMock(int paramid, int columnid, int optid, string valuestring, int tableid) {
            SetUpASystemTypeOptimization(optid, valuestring);
            //setting up tableobject mock
            var tableobjectMock = MockRepository.GenerateMock<ITableObject>();
            tableobjectMock.Stub(x => x.Database).Return(valuestring);
            tableobjectMock.Stub(x => x.Id).Return(tableid);
            var tablelist = new List<ITableObject>() {tableobjectMock};
            var tablecollMock = MockRepository.GenerateMock<ITableObjectCollection>();
            tablecollMock.Stub(x => x.GetEnumerator()).Return(tablelist.GetEnumerator());

            //Setting up issue mock
            _parameterMock.Stub(x => x.Id).Return(paramid);
            var parameterList = new List<IParameter>() {_parameterMock};
            _parameterCollectionMock.Stub(x => x.GetEnumerator()).Return(parameterList.GetEnumerator());
            _issueMock.Stub(x => x.Parameters).Return(_parameterCollectionMock);
            _issueMock.Stub(x => x.Id).Return(tableid);
            var issuelist = new List<IIssue>() {_issueMock};
            _issueCollectionMock.Stub(x => x.GetEnumerator()).Return(issuelist.GetEnumerator());
            _systemDbMock.Stub(x => x.Issues).Return(_issueCollectionMock);
            //setting up column mock
            var columnmock = MockRepository.GenerateMock<IColumn>();
            columnmock.Stub(x => x.Table).Return(tableobjectMock);
            columnmock.Stub(x => x.Id).Return(columnid);
            var columncollectionmock = MockRepository.GenerateMock<IFullColumnCollection>();
            var columnlist = new List<IColumn>() {columnmock};
            columncollectionmock.Stub(x => x.GetEnumerator()).Return(columnlist.GetEnumerator());
            _systemDbMock.Stub(x => x.Objects).Return(tablecollMock);
            _systemDbMock.Stub(x => x.Columns).Return(columncollectionmock);
        }

        private void SetUpASystemTypeOptimization(int optid, string valuestring) {
            _optgroup.Expect(x => x.Type).Return(OptimizationType.System);
            _optimization.Expect(x => x.Group).Return(_optgroup);
            _optimization.Expect(x => x.Id).Return(optid);
            _optimization.Stub(x => x.FindValue(OptimizationType.System)).Return(valuestring);
            _optcollectionMock.Expect(x => x[optid]).Return(_optimization);
            _systemDbMock.Expect(x => x.Optimizations).Return(_optcollectionMock);
        }

        //[Test]
        //public void DeleteSystemFromMetaDataBase_Parameter_Delete_Test() {
        //    // This is obviously a test smell... Too much work to set up this
        //    //Arrange
        //    int optid = 12;
        //    int tableid = 23;
        //    int columnid = 78;
        //    int paramid = 88;
        //    string valuestring = "DatabaseName";
        //    //setting up optimization mock
        //    SetUpIssueMock(paramid, columnid, optid, valuestring, tableid);
        //    //Act
        //    _deletestrategy.DeleteSystemFromMetaDataBase(optid);
        //    //Assert
        //    _systemDbMock.AssertWasCalled(x=>x.DeleteParameters(paramid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteParameterText(paramid));
        //    _systemDbMock.AssertWasCalled(x => x.DeleteParameterCollection(paramid));
        //}

        private void SetUpIssueMock(int paramid, int columnid, int optid, string valuestring, int tableid) {
            _optgroup.Expect(x => x.Type).Return(OptimizationType.System);
            _optimization.Expect(x => x.Group).Return(_optgroup);
            _optimization.Expect(x => x.Id).Return(optid);
            _optimization.Stub(x => x.FindValue(OptimizationType.System)).Return(valuestring);
            _optcollectionMock.Expect(x => x[optid]).Return(_optimization);
            _systemDbMock.Expect(x => x.Optimizations).Return(_optcollectionMock);
            //setting up tableobject mock
            var tableobjectMock = MockRepository.GenerateMock<ITableObject>();
            tableobjectMock.Stub(x => x.Database).Return(valuestring);
            tableobjectMock.Stub(x => x.Id).Return(tableid);

            var tablelist = new List<ITableObject>() {tableobjectMock};
            var tablecollMock = MockRepository.GenerateMock<ITableObjectCollection>();
            tablecollMock.Stub(x => x.GetEnumerator()).Return(tablelist.GetEnumerator());

            //Setting up issue mock
            _parameterMock.Stub(x => x.Id).Return(paramid);
            var parameterList = new List<IParameter>() {_parameterMock};
            _parameterCollectionMock.Stub(x => x.GetEnumerator()).Return(parameterList.GetEnumerator());
            _issueMock.Stub(x => x.Parameters).Return(_parameterCollectionMock);
            _issueMock.Stub(x => x.Id).Return(tableid);
            var issuelist = new List<IIssue>() {_issueMock};
            _issueCollectionMock.Stub(x => x.GetEnumerator()).Return(issuelist.GetEnumerator());
            _systemDbMock.Stub(x => x.Issues).Return(_issueCollectionMock);

            //setting up column mock
            var columnmock = MockRepository.GenerateMock<IColumn>();
            columnmock.Stub(x => x.Table).Return(tableobjectMock);
            columnmock.Stub(x => x.Id).Return(columnid);
            var columncollectionmock = MockRepository.GenerateMock<IFullColumnCollection>();
            var columnlist = new List<IColumn>() {columnmock};
            columncollectionmock.Stub(x => x.GetEnumerator()).Return(columnlist.GetEnumerator());
            _systemDbMock.Stub(x => x.Objects).Return(tablecollMock);
            _systemDbMock.Stub(x => x.Columns).Return(columncollectionmock);
        }
    }
}
