using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Rhino.Mocks;
using NUnit.Framework;
using ViewboxAdmin.ViewModels;
using ViewboxAdmin.ViewModels.Collections;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    class CollectionEdit_ViewModel_Test {

        private ObservableCollection<ICollectionModel> _collectionModelCollectionMock;
        private ObservableCollection<LanguageTextModel> _languageTextModelCollectionMock;
        private IParameters_ViewModel _parametersViewModelMock;
        private ICollectionModel _collectionModelMock;
        private CollectionEdit_ViewModel _SUT;

        [SetUp]
        public void SetUp() {
            //setting up test doubles
            _collectionModelCollectionMock = new ObservableCollection<ICollectionModel>();
            _languageTextModelCollectionMock = new ObservableCollection<LanguageTextModel>();
            _parametersViewModelMock = MockRepository.GenerateMock<IParameters_ViewModel>();
            _collectionModelMock = MockRepository.GenerateMock<ICollectionModel>();
            // creating SUT
            _SUT = new CollectionEdit_ViewModel(_collectionModelCollectionMock,_languageTextModelCollectionMock,_parametersViewModelMock);
        }

        [Test]
        public void Constructor_Injection_CollectionCollection_Test() {
            Assert.AreEqual(_collectionModelCollectionMock,_SUT.Collections);
        }

        [Test]
        public void Constructor_Injection_Languages_Test() {
            Assert.AreEqual(_languageTextModelCollectionMock,_SUT.LocalizedTexts);
        }

        [Test]
        public void Constructor_Injection_ParentVM_Test() {
            Assert.AreEqual(_parametersViewModelMock,_SUT.ParentViewModel);
        }

        [Test]
        public void Constructor_New_Command_Not_Null_Test() {
            Assert.NotNull(_SUT.NewCollectionRequest);
        }

        [Test]
        public void Constructor_DElete_Command_Not_Null_Test() {
            Assert.NotNull(_SUT.DeleteCollectionRequest);
        }

        [Test]
        public void SelectedItem_PropertyChanged() {
            //Arrange
            bool isfired = false;
            //Act
            _SUT.PropertyChanged += (o, e) => { if (e.PropertyName == "colletionModel") isfired = true; };
            _SUT.SelectedItem = _collectionModelMock;
            //Assert
            Assert.IsTrue(isfired);
        }

        [Test]
        public void SelectedItem_User_Edit_ParameterValues_Value_Field_It_Will_Call_Parents_Edit_Method() {
            //Arrange
            _collectionModelMock.Stub(x => x.Texts).Return(new TrulyObservableCollection<LanguageTextModel>());
            //Act
            _SUT.SelectedItem = _collectionModelMock;
            //Assert
            _collectionModelMock.Raise(x=>x.PropertyChanged+=null,null,new PropertyChangedEventArgs("Value"));
            _parametersViewModelMock.AssertWasCalled(x=>x.Edited(_collectionModelMock),o=>o.Repeat.Once());
        }

        [Test]
        public void SelectedItem_User_Edit_No_MultiSubscription() {
            //Arrange
            var coll1 = MockRepository.GenerateMock<ICollectionModel>();
            var coll2 = MockRepository.GenerateMock<ICollectionModel>();
            coll1.Stub(x => x.Texts).Return(new TrulyObservableCollection<LanguageTextModel>());
            coll2.Stub(x => x.Texts).Return(new TrulyObservableCollection<LanguageTextModel>());
            //Act
            _SUT.SelectedItem = coll1;
            _SUT.SelectedItem = coll2;
            _SUT.SelectedItem = coll1;
            //Assert
            coll1.Raise(x => x.PropertyChanged += null, null, new PropertyChangedEventArgs("Value"));
            _parametersViewModelMock.AssertWasCalled(x => x.Edited(coll1), o => o.Repeat.Once());
        }

        [Test]
        public void DeleteCollectionRequest_Is_Not_Enabled_When_There_Is_No_Selected_Item() {
            //Act
            _SUT.SelectedItem = null;
            //Assert
            Assert.IsFalse(_SUT.DeleteCollectionRequest.CanExecute(null));
        }

        [Test]
        public void DeleteCollectionRequest_Is_Enabled_When_There_Is_A_Selected_Item() {
            //Act
            _SUT.SelectedItem = _collectionModelMock;
            //Assert
            Assert.IsTrue(_SUT.DeleteCollectionRequest.CanExecute(null));
        }

        [Test]
        public void DeleteCollectionRequest_Is_Approved_By_User_Removed_From_Collection() {
            Create_SUT_With_Single_CollectionModel();
            _SUT.UserApproveRequest += (o, e) => e.OnYesClick();
            //Act
            _SUT.DeleteCollectionRequest.Execute(null);
            //Assert
            Assert.IsFalse(_SUT.Collections.Contains(_collectionModelMock));
        }

        [Test]
        public void DeleteCollectionRequest_Is_Approved_By_user_Call_Parent_ViewModel() {
            //Arrange
            Create_SUT_With_Single_CollectionModel();
            _SUT.UserApproveRequest += (o, e) => e.OnYesClick();
            //Act
            _SUT.DeleteCollectionRequest.Execute(null);
            //Assert
            _parametersViewModelMock.AssertWasCalled(x => x.Remove(_collectionModelMock));
        }

        [Test]
        public void DeleteCollectionRequest_Is_NOT_Approved_By_User_NOT_Removed_From_Collection() {
            //Arrange
            Create_SUT_With_Single_CollectionModel();
            _SUT.UserApproveRequest += (o, e) => e.OnNoClick();
            //Act
            _SUT.DeleteCollectionRequest.Execute(null);
            //Assert
            Assert.IsTrue(_SUT.Collections.Contains(_collectionModelMock));
        }

        [Test]
        public void NewCollectionRequest() {
            //Arrange
            CreateParameterValue_ViewModel VM = null;
            _SUT.UserNewCollectionRequest += (o, e) => VM = e.ViewModel;
            //Act
            _SUT.NewCollectionRequest.Execute(null);
            //Assert
            Assert.IsTrue(VM.CollectionModel.Value==null);
        }

        private void Create_SUT_With_Single_CollectionModel() {
            _collectionModelCollectionMock.Add(_collectionModelMock);
            _SUT = new CollectionEdit_ViewModel(_collectionModelCollectionMock, _languageTextModelCollectionMock,
                                                _parametersViewModelMock);
            _SUT.SelectedItem = _collectionModelMock;
        }
    }
}
