using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using NUnit.Framework;
using Rhino.Mocks;
using ViewboxAdmin.CustomEventArgs;
using ViewboxAdmin.ViewModels;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    class Optimization_ViewModel_Test {

        private IOptimization_ViewModel _optimization_VM;
        private IOptimization _optimizationMock;
        private IOptimization_ViewModel _optimizationVM_As_Ctor_Parameter_Mock;
        private IOptimizationCollection _optimizationCollectionMock;
        private IOptimizationCollection _emptyOptVollMock;

        [SetUp]
        public void SetUp() {
            //Set up test doubles
            _optimizationCollectionMock = MockRepository.GenerateMock<IOptimizationCollection>();
            _optimizationMock = MockRepository.GenerateMock<IOptimization>();
            _optimizationVM_As_Ctor_Parameter_Mock = MockRepository.GenerateMock<IOptimization_ViewModel>();
            _emptyOptVollMock = MockRepository.GenerateMock<IOptimizationCollection>();
            optlist = new List<IOptimization>() { };
        }

        private List<IOptimization> optlist = null;

        private void Create_Optimization_Test_Double(List<IOptimization> optimlist, string optvalue = "defaultvalue") {
            SetOptimization_Value(optvalue);
            OptimizationChildrenStub(optimlist);
        }

        private void SetOptimization_Value(string optimizationvalue) {
            _optimizationMock.Stub(x => x.Value).Return(optimizationvalue);
        }

        private void OptimizationChildrenStub(List<IOptimization> childrenlist) {
            _optimizationCollectionMock.Stub(x => x.GetEnumerator()).Return(childrenlist.GetEnumerator());
            _optimizationMock.Stub(x => x.Children).Return(_optimizationCollectionMock);
        }

        private void Create_SUT_With_One_Parameter_Ctor(IOptimization opt) {
            _optimization_VM = new Optimization_ViewModel(opt);
        }

        private void Create_SUT_With_Two_Parameter_Ctor(IOptimization opt, IOptimization_ViewModel optVM) {
            _optimization_VM = new Optimization_ViewModel(opt,optVM);
        }

        [Test]
        public void Constructor_With_1_Param_Optimization_Injection_Test() {
            //Arrange
            Create_Optimization_Test_Double(optlist);
            //Act
            Create_SUT_With_One_Parameter_Ctor(_optimizationMock);
            //Assert
            Assert.AreEqual(_optimizationMock,_optimization_VM.Optimization);
        }

        [Test]
        public void Constructor_With_1_Parameter_Parent_Null_Test() {
            //Arrange
            Create_Optimization_Test_Double(optlist);
            //Act
            Create_SUT_With_One_Parameter_Ctor(_optimizationMock);
            //Assert
            Assert.IsNull(_optimization_VM.Parent);
        }

        [Test]
        public void Constructor_With_2_Param_Name_OptimizationValue() {
            //Arrange
            var valuestring = "Whatever";
            Create_Optimization_Test_Double(optlist,valuestring);
            //Act
            Create_SUT_With_Two_Parameter_Ctor(_optimizationMock,_optimizationVM_As_Ctor_Parameter_Mock);
            //Assert
            Assert.AreEqual(valuestring,_optimization_VM.Name);
        }

        [Test]
        public void Constructor_Children_Is_Not_Null() {
            //Arrange
            Create_Optimization_Test_Double(optlist);
            //Act
            Create_SUT_With_Two_Parameter_Ctor(_optimizationMock,_optimizationVM_As_Ctor_Parameter_Mock);
            //Assert
            Assert.IsNotNull(_optimization_VM.Children);
        }

        [Test]
        public void Constructor_IsExpanded_Is_False_As_default() {
            //Arrange
            Create_Optimization_Test_Double(optlist);
            //Act
            Create_SUT_With_Two_Parameter_Ctor(_optimizationMock, _optimizationVM_As_Ctor_Parameter_Mock);
            //Assert
            Assert.IsFalse(_optimization_VM.IsExpanded);
        }

        [Test]
        public void Constructor_IsSelected_Is_False_As_Default() {
            //Arrange
            Create_Optimization_Test_Double(optlist);
            //Act
            Create_SUT_With_Two_Parameter_Ctor(_optimizationMock, _optimizationVM_As_Ctor_Parameter_Mock);
            //Assert
            Assert.IsFalse(_optimization_VM.IsSelected);
        }

        [Test]
        public void Constructor_DeleteOptimizationCommand_Is_Not_Null() {
            //Arrange
            Create_Optimization_Test_Double(optlist);
            //Act
            Create_SUT_With_Two_Parameter_Ctor(_optimizationMock, _optimizationVM_As_Ctor_Parameter_Mock);
            //Assert
            Assert.NotNull(_optimization_VM.DeleteOptimizationCommand);
        }

        [Test]
        public void Constructor_Optimization_Children_Filled() {
            //Arrange
            var optimizationList = new List<IOptimization>();
            var optimizationWithoutChildren = MockRepository.GenerateMock<IOptimization>();
            _emptyOptVollMock.Stub(x => x.GetEnumerator()).Return(new List<IOptimization>().GetEnumerator());
            //the children are empty...
            optimizationWithoutChildren.Stub(x => x.Children).Return(_emptyOptVollMock);
            //Adding optimizations to the list
            optimizationList.Add(optimizationWithoutChildren);
            optimizationList.Add(optimizationWithoutChildren);
            optimizationList.Add(optimizationWithoutChildren);
            Create_Optimization_Test_Double(optimizationList);
            //Act
            Create_SUT_With_Two_Parameter_Ctor(_optimizationMock,_optimizationVM_As_Ctor_Parameter_Mock);
            //Assert
            Assert.AreEqual(optimizationList.Count,_optimization_VM.Children.Count);
        }

        [Test]
        public void Children_CollectionChanged_Test() {
            //Arrange
            Create_Optimization_Test_Double(optlist);
            Create_SUT_With_Two_Parameter_Ctor(_optimizationMock, _optimizationVM_As_Ctor_Parameter_Mock);

            bool isfired = false;
            _optimization_VM.Children.CollectionChanged +=
                (o, e) => { if (e.NewItems.Contains(_optimization_VM)) isfired = true; };
            //Act
            _optimization_VM.Children.Add(_optimization_VM);
            //Assert
            Assert.IsTrue(isfired);
        }

        [Test]
        public void Parent_PropertyChange_Get_Set_Test() {
            //Arrange
            Create_Optimization_Test_Double(optlist);
            Create_SUT_With_Two_Parameter_Ctor(_optimizationMock, _optimizationVM_As_Ctor_Parameter_Mock);
            bool isfired = false;
            _optimization_VM.Parent = null;
            _optimization_VM.PropertyChanged += (o, e) => { if (e.PropertyName == "Parent") isfired = true; };
            //Act
            _optimization_VM.Parent = _optimization_VM;
            //Assert
            Assert.AreEqual(_optimization_VM,_optimization_VM.Parent);
            Assert.IsTrue(isfired);
        }

        [Test]
        public void Optimization_PropertyChange_Get_Set_Test() {
            //Arrange
            Create_Optimization_Test_Double(optlist);
            Create_SUT_With_Two_Parameter_Ctor(_optimizationMock, _optimizationVM_As_Ctor_Parameter_Mock);
            bool isfired = false;
            _optimization_VM.Optimization = null;
            _optimization_VM.PropertyChanged += (o, e) => { if (e.PropertyName == "Optimization") isfired = true; };
            //Act
            _optimization_VM.Optimization = _optimizationMock;
            //Assert
            Assert.AreEqual(_optimizationMock,_optimization_VM.Optimization);
            Assert.IsTrue(isfired);
        }

        [Test]
        public void IsExpanded_PropertyChange_Test() {
            //Arrange
            Create_Optimization_Test_Double(optlist);
            Create_SUT_With_Two_Parameter_Ctor(_optimizationMock, _optimizationVM_As_Ctor_Parameter_Mock);
            bool isfired = false;
            _optimization_VM.IsExpanded = false;
            _optimization_VM.PropertyChanged += (o, e) => { if (e.PropertyName == "IsExpanded") isfired = true; };
            //Act
            _optimization_VM.IsExpanded = true;
            //Assert
            Assert.IsTrue(isfired);
        }

        [Test]
        public void IsSelected_PropertyChange_Test() {
            //Arrange
            Create_Optimization_Test_Double(optlist);
            Create_SUT_With_Two_Parameter_Ctor(_optimizationMock, _optimizationVM_As_Ctor_Parameter_Mock);
            bool isfired = false;
            _optimization_VM.IsSelected = false;
            _optimization_VM.PropertyChanged += (o, e) => { if (e.PropertyName == "IsSelected") isfired = true; };
            //Act
            _optimization_VM.IsSelected = true;
            //Assert
            Assert.IsTrue(isfired);
        }

        [Test]
        public void Name_PropertyChange_Fire_Test() {
            //Arrange
            Create_Optimization_Test_Double(optlist);
            Create_SUT_With_Two_Parameter_Ctor(_optimizationMock, _optimizationVM_As_Ctor_Parameter_Mock);
            bool isfired = false;
            _optimization_VM.Name = null;
            _optimization_VM.PropertyChanged += (o, e) => { if (e.PropertyName == "Name") isfired = true; };
            //Act
            _optimization_VM.Name = "whatever";
            //Assert
            Assert.IsTrue(isfired);
        }

        [Test]
        public void DeleteOptimizationCommand_Report_The_Current_Optimization_In_A_CustomEventArg() {
            //Arrange
            Create_Optimization_Test_Double(optlist);
            Create_SUT_With_Two_Parameter_Ctor(_optimizationMock, _optimizationVM_As_Ctor_Parameter_Mock);
            OptimizationEventArgs eventarg = new OptimizationEventArgs(null); 
            _optimization_VM.DeleteOptimization += (o, e) => eventarg = e ;
            //Act
            _optimization_VM.DeleteOptimizationCommand.Execute(null);
            //Assert
            Assert.AreEqual(_optimizationMock,eventarg.Optimization);

        }


    
    }
}
