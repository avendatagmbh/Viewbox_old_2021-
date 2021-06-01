using SystemDb;
using NUnit.Framework;
using ViewboxAdmin.ViewModels.LocalizedTextLoaders;
using Rhino.Mocks;

namespace ViewBoxAdmin_Test
{
    class ConcreteLoaderBase:LoaderBase {
        public ConcreteLoaderBase(ISystemDb sysdb):base(sysdb) {
                
        }
    }
    [TestFixture]
    class LoaderBase_Test {
        private ConcreteLoaderBase _concreteLoader;
        private ISystemDb _systemdbMock;
        [SetUp]
        public void SetUp() {
            _systemdbMock = MockRepository.GenerateMock<ISystemDb>();
            _concreteLoader = new ConcreteLoaderBase(_systemdbMock);
        }
        [Test]
        public void Constructor_SystemDb_Injection() {
            Assert.AreEqual(_systemdbMock,_concreteLoader.SystemDb);
        }
    }
}
