using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Rhino.Mocks;
using NUnit.Framework;
using ViewboxAdmin.ViewModels;
using ViewboxAdmin.ViewModels.Collections;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    class Parameters_ViewModel_Test {
        private IParameterModel _paramModelMock;
        private ObservableCollection<IParameterModel> _parametersModel;
        private ObservableCollection<LanguageTextModel> _languagetexts;
        private ICollectionsUnitOfWork _collectionUnitOfWork;
        private Parameters_ViewModel SUT;

        [SetUp]
        public void SetUp() {
            //set up test doubles 
            _paramModelMock = MockRepository.GenerateMock<IParameterModel>();
            _languagetexts = new ObservableCollection<LanguageTextModel>();
            _collectionUnitOfWork = MockRepository.GenerateMock<ICollectionsUnitOfWork>();
            _parametersModel = new ObservableCollection<IParameterModel>();
            _parametersModel.Add(_paramModelMock);
            // create SUT
            SUT = new Parameters_ViewModel(_parametersModel,_languagetexts,_collectionUnitOfWork);
        }

        [Test]
        public void Constructor_Parameters_Injection_Test() {
            Assert.AreEqual(_parametersModel, SUT.Parameters);
        }
        [Test]
        public void Constructor_Langugaes_Injection_Test() {
            Assert.AreEqual(_languagetexts, SUT.Languages);
        }
        [Test]
        public void Constructor_UoW_Injection_Test() {
            Assert.AreEqual(_collectionUnitOfWork,SUT.UnitOfWork);
        }
        [Test]
        public void Constructor_Command_Not_Null() {
            Assert.NotNull(SUT.CommitCommand);
        }
        [Test]
        public void CommitCommand_Calls_UnitOfWorks_Commit() {
            //Act
            SUT.CommitCommand.Execute(null);
            //Assert
            _collectionUnitOfWork.AssertWasCalled(x=>x.Commit());
        }

        [Test]
        public void SelectedParameters_PropChanged_Test() {
            //Arrange
            bool isfired = false;
            var parameter = MockRepository.GenerateMock<IParameterModel>();
            SUT.PropertyChanged += (o, e) => { if (e.PropertyName == "SelectedParameter") isfired = true; };
            //Act
            SUT.SelectedParameter = parameter;
            //Assert
            Assert.IsTrue(isfired);
        }

        [Test]
        public void SelectedParameters_Trigger_DataContextChange() {
            //Arrange
            CollectionEdit_ViewModel VM = null;
            SUT.DataContextChange += (o, e) => VM = e.ViewModel;
            //Act
            SUT.SelectedParameter = MockRepository.GenerateMock<IParameterModel>();
            //Assert
            Assert.NotNull(VM);
        }

    }
}
