using NUnit.Framework;
using Rhino.Mocks;
using ViewboxAdmin.ViewModels;
using ViewboxAdmin.ViewModels.Collections;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    class CreateParameterValue_ViewModel_Test {

        private ICollectionModel _collectionModelMock;
        private ICollectionEdit_ViewModel _collectionEditViewModel;
        private CreateParameterValue_ViewModel SUT;


        [SetUp]
        public void SetUp() {
            _collectionModelMock = MockRepository.GenerateMock<ICollectionModel>();
            _collectionEditViewModel = MockRepository.GenerateMock<ICollectionEdit_ViewModel>();
            SUT = new CreateParameterValue_ViewModel(_collectionModelMock,_collectionEditViewModel);
        }

        [Test]
        public void Constructor_CollectionModel_Injection_Test() {
            Assert.AreEqual(_collectionModelMock,SUT.CollectionModel);
        }

        [Test]
        public void Constructor_ParentViewModel_Injection_Test() {
            Assert.AreEqual(_collectionEditViewModel,SUT.ParentViewModel);
        }

        [Test]
        public void Constructor_AddNewCollection_Is_Not_Null() {
            Assert.NotNull(SUT.AddNewCollection);
        }

        [Test]
        public void AddNewCollection_Clicked_By_User_Call_Parent() {
            //Act
            SUT.AddNewCollection.Execute(null);
            //Assert
            _collectionEditViewModel.AssertWasCalled(x=>x.CreateNewProfileValueCollection(_collectionModelMock));
        }

        [Test]
        public void AddNewCollection_Clicked_By_User_Triggers_Close_The_Window() {
            //Arrange
            bool isWindowCloseRequest = false;
            SUT.ParameterValuaCreationFinished += (o, e) => isWindowCloseRequest = true;
            //Act
            SUT.AddNewCollection.Execute(null);
            //Assert
            Assert.IsTrue(isWindowCloseRequest);
        }
    }
}
