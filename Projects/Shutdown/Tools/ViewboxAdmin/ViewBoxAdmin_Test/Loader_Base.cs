using System.Collections.ObjectModel;
using SystemDb;
using NUnit.Framework;
using Rhino.Mocks;
using ViewboxAdmin.ViewModels;

namespace ViewBoxAdmin_Test {
    internal class Loader_Base {
        protected IItemLoader _itemloader;
        protected ISystemDb _systemDbMock;
        protected ILanguage _languageMock;
        protected ObservableCollection<IItemWrapperStructure> _dummycollection;

        
        public virtual void SetUp() {
            _systemDbMock = MockRepository.GenerateMock<ISystemDb>();
            _dummycollection = new ObservableCollection<IItemWrapperStructure>();
            _itemloader = new TableLoader(_systemDbMock);
        }
    }
}