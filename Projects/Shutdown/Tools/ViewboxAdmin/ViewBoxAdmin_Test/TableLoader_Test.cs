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
    class TableLoader_Test : Loader_Base {


        private ITableObjectCollection _tableobjectcollectionMock;
        


        [SetUp]
        public override void SetUp() {
            base.SetUp();
            _tableobjectcollectionMock = MockRepository.GenerateMock<ITableObjectCollection>();
        }

        [Test]
        public void InitItem_Objects_List_Was_Accessed() {
            //Arrange
            _tableobjectcollectionMock.Stub(x => x.GetEnumerator()).Return(new List<ITableObject>().GetEnumerator());
            _systemDbMock.Stub(x => x.Objects).Return(_tableobjectcollectionMock);
            //Act
            _itemloader.InitItems(_dummycollection,_languageMock);
            //Assert
            _systemDbMock.AssertWasCalled(x => x.Objects);
        }
        
    }
}
