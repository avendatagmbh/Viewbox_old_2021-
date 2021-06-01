using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SystemDb;
using Rhino.Mocks;
using NUnit.Framework;
using ViewboxAdmin.ViewModels;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    internal class EditText_ViewModel_Test {

        private EditText_ViewModel _edittextVM;
        private ISystemDb _systemdbMock;
        private IItemLoaderFactory _itemlosderfactoryMock;
        private IItemWrapperStructure _itemwrapperstructure;
        private ILanguage _languageMock;
        private ILanguageCollection _languageCollectionMock;
        private IItemLoader _itemLoaderMock;

        [SetUp]
        public void SetUp() {
            //set up test doubles
            _systemdbMock = MockRepository.GenerateMock<ISystemDb>();
            _itemlosderfactoryMock = MockRepository.GenerateMock<IItemLoaderFactory>();
            _itemwrapperstructure = MockRepository.GenerateMock<IItemWrapperStructure>();
            _languageMock = MockRepository.GenerateMock<ILanguage>();
            _itemLoaderMock = MockRepository.GenerateMock<IItemLoader>();
            _languageCollectionMock = MockRepository.GenerateMock<ILanguageCollection>();

            //Set up stub for displaylanguage 
            _systemdbMock.Stub(x => x.DefaultLanguage).Return(_languageMock);
            //set up stub for language list
            var langlist = new List<ILanguage> {_languageMock};
            _languageCollectionMock.Stub(x => x.GetEnumerator()).Return(langlist.GetEnumerator());
            _systemdbMock.Stub(x => x.Languages).Return(_languageCollectionMock);

            //set up stub for the create method
            _itemlosderfactoryMock.Stub(
                x => x.Create(Arg<TablesWithLocalizedTextEnum>.Is.Anything, Arg<ISystemDb>.Is.Anything)).Return(
                    _itemLoaderMock);
            //set up stub for the table loader
            _itemLoaderMock.Stub(
                x =>
                x.InitItems(Arg<ObservableCollection<IItemWrapperStructure>>.Is.Anything, Arg<ILanguage>.Is.Anything));
            _edittextVM = new EditText_ViewModel(_systemdbMock, _itemlosderfactoryMock);

        }

        [Test]
        public void Constructor_SystemDb_Injection() { Assert.AreEqual(_systemdbMock, _edittextVM.SystemDb); }

        [Test]
        public void Constructor_LoaderFactory_Inject_Test() { Assert.AreEqual(_itemlosderfactoryMock, _edittextVM.ItemLoaderAbstractFactory); }

        [Test]
        public void Constructor_Items_Init() { Assert.IsNotNull(_edittextVM.Items); }

        [Test]
        public void Constructor_Langugaes_Init() { Assert.IsNotNull(_edittextVM.Languages); }

        [Test]
        public void Constructor_SelectedLanguage_Init() { Assert.AreEqual(_languageMock, _edittextVM.SelectedLanguage); }

        [Test]
        public void Constructor_Languages_Init() {
            CollectionAssert.Contains(_edittextVM.Languages,_languageMock);
        }

        [Test]
        public void SelectedLanguage_Property_Get_Set() { _edittextVM.SelectedLanguage = _languageMock;
        Assert.AreEqual(_languageMock,_edittextVM.SelectedLanguage);}

        [Test]
        public void SelectedLanguage_OnPropertyChanged() { 
            //Arrange
            bool isfired = false;
            _edittextVM.PropertyChanged += (o, e) => { if (e.PropertyName == "SelectedLanguage") isfired = true; };
            //Act
            _edittextVM.SelectedLanguage = _languageMock;
            //Assert
            Assert.IsTrue(isfired);
        }

        [Test]
        public void SelectedLanguage_ReloadItems() {
            //Arrange
            var _lang = MockRepository.GenerateMock<ILanguage>();
            //Act
            _edittextVM.SelectedLanguage = _lang;
            //Assert
            _itemLoaderMock.AssertWasCalled(x => x.InitItems(Arg<ObservableCollection<IItemWrapperStructure>>.Is.Anything, Arg<ILanguage>.Matches(o=>o==_lang)));
        }


        [Test]
        public void TablesWithLocalizedTextCollectionEnum_Property_Get_Set() {
            //Arrange
            var expected = TablesWithLocalizedTextEnum.Tableobjects;
            //Act
            _edittextVM.TablesWithLocalizedText = expected;
            //Assert
            Assert.AreEqual(expected,_edittextVM.TablesWithLocalizedText);
        }

        [Test]
        public void TablesWithLocalizedTextEnum_OnPropertyChanged() {
            //Arrange
            bool isfired = false;
            _edittextVM.PropertyChanged +=
                (o, e) => { if (e.PropertyName == "TablesWithLocalizedText") isfired = true; };
            //Act
            _edittextVM.TablesWithLocalizedText = TablesWithLocalizedTextEnum.Collections;
            //Assert
            Assert.IsTrue(isfired);
        }

        [Test]
        public void TablesWithLocalizedTextCollection_Call_Set_The_State() {
            //Arrange
            var type = TablesWithLocalizedTextEnum.Parameter;
            //Act
            _edittextVM.TablesWithLocalizedText = type;
            //Assert
            _itemlosderfactoryMock.AssertWasCalled(x=>x.Create(type, _edittextVM.SystemDb));
        }

        [Test]
        public void TablesWithLocalizedTextCollection_ReLoadTAbles() {
            //Arrange
            var type = TablesWithLocalizedTextEnum.Parameter;
            //Act
            _edittextVM.TablesWithLocalizedText = type;
            //Assert
            _itemLoaderMock.AssertWasCalled(x => x.InitItems(Arg<ObservableCollection<IItemWrapperStructure>>.Is.Anything, Arg<ILanguage>.Is.Anything));
        }
    
}
}
