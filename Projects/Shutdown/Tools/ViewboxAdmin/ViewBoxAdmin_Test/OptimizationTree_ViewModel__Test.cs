using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using Rhino.Mocks;
using NUnit.Framework;
using ViewboxAdmin.CustomEventArgs;
using ViewboxAdmin.Factories;
using ViewboxAdmin.ViewModels;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    class OptimizationTree_ViewModel__Test {

        private OptimizationTree_ViewModel _optimizationTreeVM;
        private ISystemDb _systemDb_Mock;
        private IOptimizationVM_Factory _optVM_Factory_Mock;
        private IOptimization _optimization;
        private IOptimizationCollection _optcollection;
        private IOptimizationGroup _optgroupMock;
        private IOptimization_ViewModel _optVM;
        [SetUp]
        public void TestSetup() {
            //setting up testdoubles
            _systemDb_Mock = MockRepository.GenerateMock<ISystemDb>();
            _optVM_Factory_Mock = MockRepository.GenerateMock<IOptimizationVM_Factory>();
            _optimization = MockRepository.GenerateMock<IOptimization>();
            _optcollection = MockRepository.GenerateMock<IOptimizationCollection>();
            _optgroupMock = MockRepository.GenerateMock<IOptimizationGroup>();
            _optVM = MockRepository.GenerateMock<IOptimization_ViewModel>();
        }

        

        private void CreateMinimalSUT() {
            // there is no optimization in systemdb
            _systemDb_Mock.Stub(x => x.Optimizations).Return(SetUpOptimizations(new List<IOptimization>()));
            SetUpAnOptimizationVMFactory(_optVM);
            _optimizationTreeVM = new OptimizationTree_ViewModel(_systemDb_Mock,_optVM_Factory_Mock);
        }

        private void CreateSUT(ISystemDb sysdb, IOptimizationVM_Factory optfactory) {
            _optimizationTreeVM = new OptimizationTree_ViewModel(sysdb,optfactory);
        }

        private void SetUpAnOptimizationVMFactory(IOptimization_ViewModel optVM) {
            _optVM_Factory_Mock.Stub(x => x.CreateRootElement(Arg<IOptimization>.Is.Anything)).Return(optVM); 
        }

        private IOptimizationCollection SetUpOptimizations(List<IOptimization> optlist) { 
            IOptimizationCollection optimizations = MockRepository.GenerateMock<IOptimizationCollection>();
            optimizations.Stub(x => x.GetEnumerator()).Return(optlist.GetEnumerator());
            return optimizations;
        }

        private IOptimization SetUpOptimization(OptimizationType opttype) { 
            IOptimization opt = MockRepository.GenerateMock<IOptimization>();
            opt.Stub(x => x.Group).Return(SetUpOptimizationGroupType(opttype));
            return opt;
        }

        private IOptimizationGroup SetUpOptimizationGroupType(OptimizationType optType) {
            IOptimizationGroup optgroup = MockRepository.GenerateMock<IOptimizationGroup>();
            optgroup.Stub(x => x.Type).Return(optType);
            return optgroup;
        }

        [Test]
        public void Constructor_SystemDb_Dependency_Injection_Test() {
            //Act
            CreateMinimalSUT();
            //Assert
            Assert.AreEqual(_systemDb_Mock,_optimizationTreeVM.SystemDb);
        }

        [Test]
        public void Constructor_Factory_Injection_Test() {
            //Act
            CreateMinimalSUT();
            //Assert
            Assert.AreEqual(_optVM_Factory_Mock,_optimizationTreeVM.OptimizationTreeCreator);
        }

        [Test]
        public void Constructor_Init_FirstGeneration() {
            //Act
            CreateMinimalSUT();
            //Assert
            Assert.IsNotNull(_optimizationTreeVM.FirstGeneration);
        }

        [Test]
        public void Constructor_Load_Only_SystemType_Optimizations_To_FirstGeneration() {
            var opt1 = SetUpOptimization(OptimizationType.System);
            var opt2 = SetUpOptimization(OptimizationType.System);
            var opt3 = SetUpOptimization(OptimizationType.SortColumn);
            int systemtype = 2;
            List<IOptimization> optlist = new List<IOptimization>(){opt1,opt2,opt3};
            _systemDb_Mock.Stub(x => x.Optimizations).Return(SetUpOptimizations(optlist));
            SetUpAnOptimizationVMFactory(_optVM);
            //Act
            CreateSUT(_systemDb_Mock,_optVM_Factory_Mock);
            //Assert
            Assert.AreEqual(systemtype,_optimizationTreeVM.FirstGeneration.Count,"The system type optimizations should be in the collection");
            _optVM_Factory_Mock.AssertWasCalled(x=>x.CreateRootElement(Arg<IOptimization>.Is.Anything),options=>options.Repeat.Times(systemtype));
            _optVM.AssertWasCalled(x => x.DeleteOptimization += Arg<EventHandler<OptimizationEventArgs>>.Is.Anything, options => options.Repeat.Times(systemtype));
        }

        [Test]
        public void DeleteOptimizationRequest_Test() {
            CreateSUTWithASystemTypeOptimization();
            IOptimization opt2delet = MockRepository.GenerateMock<IOptimization>();
            bool isfired = false;
            _optimizationTreeVM.DeleteOptimizationRequest += (o, e) => { isfired = true; };
            //Act
            _optVM.Raise(x=>x.DeleteOptimization+=null,null,new OptimizationEventArgs(opt2delet));
            //Assert
            Assert.IsTrue(isfired);
        }

        [Test]
        public void DeleteOptimizationRequest_UserEnable_The_SystemDB_Method_Is_Called() {
            CreateSUTWithASystemTypeOptimization();
            IOptimization opt2delet = MockRepository.GenerateMock<IOptimization>();
            _optimizationTreeVM.DeleteOptimizationRequest += (o, e) => e.OnYesClick();
            //Act
            _optVM.Raise(x => x.DeleteOptimization += null, null, new OptimizationEventArgs(opt2delet));
            //Assert
            _systemDb_Mock.AssertWasCalled(x=>x.RemoveOptimizationFromAllTables(Arg<IOptimization>.Matches(o=>o==opt2delet)),options=>options.Repeat.Once());
        }

        [Test]
        public void DeleteOptimizationRequest_UserNOTEnable_The_SystemDB_Method_Is_Called() {
            CreateSUTWithASystemTypeOptimization();
            IOptimization opt2delet = MockRepository.GenerateMock<IOptimization>();
            _optimizationTreeVM.DeleteOptimizationRequest += (o, e) => e.OnNoClick();
            //Act
            _optVM.Raise(x => x.DeleteOptimization += null, null, new OptimizationEventArgs(opt2delet));
            //Assert
            _systemDb_Mock.AssertWasNotCalled(x => x.RemoveOptimizationFromAllTables(Arg<IOptimization>.Is.Anything));
        }

        private void CreateSUTWithASystemTypeOptimization() {
            var opt1 = SetUpOptimization(OptimizationType.System);
            List<IOptimization> optlist = new List<IOptimization>() {opt1};
            _systemDb_Mock.Stub(x => x.Optimizations).Return(SetUpOptimizations(optlist));
            SetUpAnOptimizationVMFactory(_optVM);
            CreateSUT(_systemDb_Mock, _optVM_Factory_Mock);
        }

        [Test]
        public void DeleteOptimizationRequest_Throw_Exception_Will_Be_Reported() {
            //Arrange
            CreateSUTWithASystemTypeOptimization();
            IOptimization opt2delet = MockRepository.GenerateMock<IOptimization>();
            Exception expected_exception = new Exception();
            Exception actual_exception = null;
            _systemDb_Mock.Stub(x => x.RemoveOptimizationFromAllTables(Arg<IOptimization>.Is.Anything)).Throw(expected_exception);
            _optimizationTreeVM.ExceptionOccured += (o, e) => actual_exception = e.GetException();
            _optimizationTreeVM.DeleteOptimizationRequest += (o, e) => e.OnYesClick();
            //Act
            _optVM.Raise(x => x.DeleteOptimization += null, null, new OptimizationEventArgs(opt2delet));
            //Assert
            Assert.AreEqual(expected_exception,actual_exception);
        }
    }
}
