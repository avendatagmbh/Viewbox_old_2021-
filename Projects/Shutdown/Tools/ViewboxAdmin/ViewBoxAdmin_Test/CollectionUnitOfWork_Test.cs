using System;
using NUnit.Framework;
using Rhino.Mocks;
using ViewboxAdmin.ViewModels.Collections;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    class CollectionUnitOfWork_Test {

        private ICollectionsUnitOfWork SUT;
        private IParameterEditor _parametereditorMock;
        private ICollectionModel _collectionModelMock;
        private IParameterModel _parameterModelMock;

        [SetUp]
        public void SetUp() {
            //creating test doubles
            _collectionModelMock = MockRepository.GenerateMock<ICollectionModel>();
            _parameterModelMock = MockRepository.GenerateMock<IParameterModel>();
            _parametereditorMock = MockRepository.GenerateMock<IParameterEditor>();
            // creating the SUT
            SUT = new CollectionUnitOfWork(_parametereditorMock);
        }
        [Test]
        public void Constructor_Injection_Test() {
            //Assert
            Assert.AreEqual(_parametereditorMock,SUT.ParameterValueEditor);
        }
        [Test]
        public void Dictionary_Init_Test() {
            //Assert
            Assert.NotNull(SUT.NewItems,"the collection of new items are not initialized");
            Assert.NotNull(SUT.DeletedItems,"the collection of deleted items are not initialized");
            Assert.NotNull(SUT.DirtyItems,"the collection of dirty items are not initialized");
        }

        [Test]
        public void DebugMessage_PropChangeTest() {
            //Arrange
            bool isfired = false;
            //Act
            SUT.PropertyChanged += (o, e) => { if (e.PropertyName == "DebugMessage") isfired = true; };
            SUT.DebugMessage = "Whatever";
            //Assert
            Assert.IsTrue(isfired);
        }
        [Test]
        public void MarkAsDirty_Test() {
            //Act
            SUT.MarkAsDirty(_collectionModelMock);
            //Assert
            Assert.IsTrue(SUT.DirtyItems.Contains(_collectionModelMock));
        }

        [Test]
        public void MarkAsDeleted_Test() {
            //Act
            SUT.MarkAsDeleted(_collectionModelMock);
            //Assert
            Assert.IsTrue(SUT.DeletedItems.Contains(_collectionModelMock));
        }

        [Test]
        public void MarkAsNew_Test() {
            var t = Tuple.Create(_collectionModelMock, _parameterModelMock);
            //Act
            SUT.MarkAsNew(t);
            //Assert
            Assert.IsTrue(SUT.NewItems.Contains(t));
        }

        [Test]
        public void Commit_Test_Call_Delete() {
            //Act
            SUT.MarkAsDeleted(_collectionModelMock);
            SUT.Commit();
            //Assert
            _parametereditorMock.AssertWasCalled(x=>x.Delete(_collectionModelMock),o=>o.Repeat.Once());
        }

        [Test]
        public void Commit_Test_Call_Update() {
            //Act
            SUT.MarkAsDirty(_collectionModelMock);
            SUT.Commit();
            //Assert
            _parametereditorMock.AssertWasCalled(x=>x.Update(_collectionModelMock),o=>o.Repeat.Once());
        }
        [Test]
        public void Commit_Test_Create() {
            //Arrange
            var t = Tuple.Create(_collectionModelMock, _parameterModelMock);
            //Act
            SUT.MarkAsNew(t);
            SUT.Commit();
            //Assert
            _parametereditorMock.AssertWasCalled(x => x.CreateNew(t.Item1, t.Item2), o => o.Repeat.Once());
        }

        [Test]
        public void Commit_After_Items_Should_Be_Cleared() {
            //Act
            SUT.MarkAsDeleted(_collectionModelMock);
            SUT.Commit();
            //Assert
            Assert.IsTrue(SUT.DeletedItems.Count==0);
        }
        
        [Test]
        public void MarkAsDirty_If_Object_Marked_As_Deleted_Cannot_Go_To_Dirty() {
            //Act
            SUT.MarkAsDeleted(_collectionModelMock);
            SUT.MarkAsDirty(_collectionModelMock);
            //Assert
            Assert.IsFalse(SUT.DirtyItems.Contains(_collectionModelMock));
        }
        [Test]
        public void MarkAsDirty_Do_Not_Contain_The_Same_Object_Twice() {
            //Act
            SUT.MarkAsDirty(_collectionModelMock);
            SUT.MarkAsDirty(_collectionModelMock);
            //Assert
            Assert.IsTrue(SUT.DirtyItems.Count==1);
        }
        [Test]
        public void MarkAsDirty_If_Object_New_Dont_Register_As_Dirty() {
            //Arrange
            var t = Tuple.Create(_collectionModelMock, _parameterModelMock);
            //Act
            SUT.MarkAsNew(t);
            SUT.MarkAsDirty(_collectionModelMock);
            //Assert
            Assert.IsFalse(SUT.DirtyItems.Contains(_collectionModelMock));
        }
    }
}
