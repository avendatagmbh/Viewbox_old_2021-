using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using ViewboxAdmin;
using ViewboxAdmin.CustomEventArgs;
using ViewboxAdmin.ViewModels;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    public class SystemDelete_ViewModel_Test {

        private SystemDeleteViewModel _sysdelVM;
        private ISystemDb _dbMock;
        private IDeleteSystemStrategy _deletestrategyMock;
        private IOptimization _optMock;
        private IOptimizationCollection _optCollectionMock;

        [SetUp]
        public void SetUp() {
            //mocker classes
            _dbMock = MockRepository.GenerateMock<ISystemDb>();
            _deletestrategyMock = MockRepository.GenerateMock<IDeleteSystemStrategy>();
            _optMock = MockRepository.GenerateMock<IOptimization>();
            _optCollectionMock = MockRepository.GenerateMock<IOptimizationCollection>();
            SetUpOptimizationMock();
            //create the SUT
            _sysdelVM = new SystemDeleteViewModel(_dbMock, _deletestrategyMock);
        }
        
        [Test]
        public void InfrastructureTest() {
            Assert.IsTrue(true);
        }

        [Test]
        public void Constructor_SysDb_Is_Injected_Test() {
            //Assert
            Assert.AreEqual(_dbMock,_sysdelVM.SystemDBModel);
        }

        [Test]
        public void Constructor_ObservableCollection_Is_Not_Null() {
            Assert.NotNull(_sysdelVM.Optimizations);
        }

        [Test]
        public void Constructor_Delete_Strategy_Is_Injected() {
            Assert.NotNull(_sysdelVM.DeleteSystemStrategy);
        }

        [Test]
        public void Constructor_The_Clear_Debug_Window_Command_Is_Not_Null() {
            Assert.NotNull(_sysdelVM.ClearDebugWindowCommand);
        }

        [Test]
        public void Constructor_The_Deleet_System_Command_Is_Not_Null() {
            Assert.NotNull(_sysdelVM.DeleteSystemCommand);
        }

        [Test]
        public void ClearDebugWindow_The_DebugString_Should_Be_Empty() {
            //Arrange
            _sysdelVM.DebugText = "Debug message";
            //Act
            _sysdelVM.ClearDebugWindowCommand.Execute(new object());
            string s = _sysdelVM.DebugText;
            //Assert
            Assert.IsTrue(string.IsNullOrEmpty(s));
            
        }

        //[Test]
        //public void DeleteSystemCommand_The_DeleteStrategy_Should_Be_Called_With_Correct_Id() {
        //    //Arrange
        //    int system2delete = 12;
        //    IOptimization optmock = MockRepository.GenerateMock<IOptimization>();
        //    optmock.Stub(x => x.Id).Return(system2delete);
        //    _sysdelVM.Selected = optmock;
        //    //Act
        //    _sysdelVM.DeleteSystemCommand.Execute(new object());
        //    //Assert
        //    _deletestrategyMock.AssertWasCalled(x=>x.DeleteSystemFromMetaDataBase(Arg<int>.Matches(o=>o==system2delete)));
        //}

        [Test]
        public void DeleteSystemCommand_With_Null_The_Worker_Method_Was_Not_Called() {
            //Arrange
            int system2delete = 12;
            IOptimization optmock = MockRepository.GenerateMock<IOptimization>();
            optmock.Stub(x => x.Id).Return(system2delete);
            _sysdelVM.IsWorking = false;
            _sysdelVM.Selected = null;
            //Act
            _sysdelVM.DeleteSystemCommand.Execute(new object());
            //Assert
            _deletestrategyMock.AssertWasNotCalled(x => x.DeleteSystemFromMetaDataBase(Arg<int>.Is.Anything));
            
        }

        [Test]
        public void DeleteSystemCommand_If_The_Delete_Process_Started_One_Can_Execute_The_Command() {
            //Arrange
            _sysdelVM.IsWorking = true;
            //Act
            bool canexecute = _sysdelVM.DeleteSystemCommand.CanExecute(new object());
            //Assert
            Assert.IsFalse(canexecute);
        }

        [Test]
        public void DeleteSystemCommand_One_Cannot_Execute_The_Command() {
            //Arrange
            _sysdelVM.IsWorking = false;
            //Act
            bool canexecute = _sysdelVM.DeleteSystemCommand.CanExecute(new object());
            //Assert
            Assert.IsTrue(canexecute);
        }

        [Test]
        public void DeleteSystemCommand_Is_Ongoing_The_Delete_Command_Cannot_Be_Called_Again() {
            //Arrange
            _sysdelVM.IsWorking = true;
            _sysdelVM.Selected = _optMock;
            _optMock.Stub(x => x.Id).Return(345);
            //Act
            _sysdelVM.DeleteSystemCommand.Execute(new object());
            //Assert
            _deletestrategyMock.AssertWasNotCalled(x=>x.DeleteSystemFromMetaDataBase(Arg<int>.Is.Anything));
        }
        [Test]
        public void IsWorking_Should_Be_False_By_default() {
            //Arrange
            Assert.IsFalse(_sysdelVM.IsWorking);
        }

        [Test]
        public void DebugMessage_Property_Get_Set_Test() {
            //Arrange
            string text = "whatever it is";
            //Act
            _sysdelVM.DebugText = text;
            //Assert
            Assert.AreEqual(text,_sysdelVM.DebugText);
        }

        [Test]
        public void DebugMessage_OnPropertyChanged_Fires() {
            //Arrange
            bool isfired = false;
            _sysdelVM.PropertyChanged += (o, e) => { if (e.PropertyName == "DebugText") isfired = true; };
            //Act
            _sysdelVM.DebugText = "whatever";
            //Assert
            Assert.IsTrue(isfired);
        }

        [Test]
        public void Constructor_Subscribed_To_Start_Event() {
            //Assert
            _deletestrategyMock.AssertWasCalled(x => x.Started += Arg < EventHandler<EventArgs>>.Is.Anything);
        }

        [Test]
        public void Constructor_Subscribed_To_Crash_Event() {
            //Assert
            _deletestrategyMock.AssertWasCalled(x => x.Crashed += Arg<EventHandler<EventArgs>>.Is.Anything);
        }
        [Test]
        public void Constructor_Subscribed_To_Completed_Event() {
            //Assert
            _deletestrategyMock.AssertWasCalled(x => x.Completed += Arg<EventHandler<EventArgs>>.Is.Anything);
            
        }

        [Test]
        public void Constructor_Subscribed_To_Progress_Event() {
            //Assert
            _deletestrategyMock.AssertWasCalled(x => x.DebugEvent += Arg<EventHandler<DebugEventArgs>>.Is.Anything);
        }

        [Test]
        public void IsWorking_After_Start_Event_Fired_Should_Be_True() {
            //Act
            _deletestrategyMock.Raise(x=>x.Started+=null,this,null);
            //Assert
            Assert.IsTrue(_sysdelVM.IsWorking);
        }

        [Test]
        public void IsWorking_After_Completed_Event_Fired_Should_Be_False() {
            //Arrange
            SetUpOptimizationMock();
            //Act
            _deletestrategyMock.Raise(x => x.Completed += null, null, null);
            //Assert
            Assert.IsFalse(_sysdelVM.IsWorking);
        }

        private void SetUpOptimizationMock() {
            var _optsysMock = MockRepository.GenerateMock<IOptimization>();
            _optsysMock.Stub(x => x.Group.Type).Return(OptimizationType.System);
            var _optnonsysMock = MockRepository.GenerateMock<IOptimization>();
            _optnonsysMock.Stub(x => x.Group.Type).Return(OptimizationType.None);
            var optlist = new List<IOptimization>() {_optsysMock, _optsysMock, _optnonsysMock};
            _optCollectionMock.Stub(x => x.GetEnumerator()).Return(optlist.GetEnumerator());
            _dbMock.Stub(x => x.Optimizations).Return(_optCollectionMock);
        }

        [Test]
        public void IsWorking_After_Crashed_Event_Fired_Should_Be_True() {
            _deletestrategyMock.Raise(x => x.Crashed += null, this, null);
            //Assert
            Assert.IsFalse(_sysdelVM.IsWorking);
        }

        //[Test]
        //public void DebugText_Debugg_Event_Fired_Was_Obtained() {
        //    //Arrange
        //    DebugEventArgs dargs = new DebugEventArgs();
        //    string s = "This is debug message";
        //    dargs.DebugMessage = s;
        //    //Act
        //    _deletestrategyMock.Raise(x=>x.ProgressEvent+=null,null,dargs);
        //    //Assert
        //    Assert.IsTrue(_sysdelVM.DebugText.Contains(s));
        //}

        //[Test]
        //public void Init_The_System_Type_Opt_Goes_to_The_collection() {
        //    //Arrange
        //    _optMock.Stub(x => x.Group.Type).Return(OptimizationType.System);
        //    var optlist = new List<IOptimization>() {_optMock,_optMock,_optMock};
        //    _optCollectionMock.Stub(x => x.GetEnumerator()).Return(optlist.GetEnumerator());
        //    _dbMock.Stub(x => x.Optimizations).Return(_optCollectionMock);
        //    //Act
        //    _sysdelVM.Init();
        //    //Assert
        //    Assert.AreEqual(3,_sysdelVM.Optimizations.Count);
        //}

        [Test]
        public void Init_The_ONLY_System_Type_Opt_Goes_to_The_collection() {
            //Arrange
            //SetUpOptimizationMock();
            //Act
            //_sysdelVM.Init();
            //Assert
            Assert.AreEqual(2, _sysdelVM.Optimizations.Count);
        }

        [Test]
        public void Selected_If_There_Is_No_System_The_Selected_Should_Be_Null() {
            //Arrange
            var _optnonsysMock = MockRepository.GenerateMock<IOptimization>();
            _optnonsysMock.Stub(x => x.Group.Type).Return(OptimizationType.None);

            var optlist = new List<IOptimization>() { _optnonsysMock };
            _optCollectionMock.Stub(x => x.GetEnumerator()).Return(optlist.GetEnumerator());
            _dbMock.Stub(x => x.Optimizations).Return(_optCollectionMock);
            //Act
            _sysdelVM.LoadSystems();
            //Assert
            Assert.IsNull(_sysdelVM.Selected);
            
        }
        [Test]
        public void Selected_Property_Test() {
            //Act
            _sysdelVM.Selected = _optMock;
            //Assert
            Assert.AreEqual(_optMock, _sysdelVM.Selected);
        }

        [Test]
        public void Selected_OnProperty_Changed_Test() {
            //Arrange
            bool isfired = false;
            _sysdelVM.PropertyChanged += (o, e) => { if (e.PropertyName == "Selected") isfired = true; };
            //Act
            _sysdelVM.Selected = _optMock;
            //Assert
            Assert.IsTrue(isfired);
        }

        //[Test]
        //public void Optimizations_After_Operation_Completed_The_Optimization_Collection_Should_Be_Reloaded() {
        //    //Arrange
        //   SetUpOptimizationMock();
        //    //Act
        //    _sysdelVM.Init();
        //    Assert.AreEqual(2, _sysdelVM.Optimizations.Count);
        //    _deletestrategyMock.Raise(x=>x.Completed+=null,null,null);
        //    //Assert
        //    Assert.AreEqual(0,_sysdelVM.Optimizations.Count);

        //}

        [Test]
        public void Optimizations_CollectionChanged_Notification_Was_Thrown() {
            bool isfired = false;
            _sysdelVM.Optimizations.CollectionChanged += (o, e) => { isfired = true; };
            _sysdelVM.Optimizations.Add(_optMock);
            Assert.IsTrue(isfired);
        }

    }
}
