using System.Collections.Generic;
using SystemDb;
using DbAccess;
using NUnit.Framework;
using Rhino.Mocks;

namespace SystemDb_Test
{
    [TestFixture]
    internal class Merge_Test
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            //generate test doubles
            _databaseMock = MockRepository.GenerateMock<IDatabase>();
            _dbmappingMock = MockRepository.GenerateMock<IDbMapping>();
            _databaseMock.Stub(x => x.DbMapping).Return(_dbmappingMock);
            //create sut
            SUT = new MapBusinessObjectList(_databaseMock);
        }

        #endregion

        private MapBusinessObjectList SUT;
        private IDatabase _databaseMock;
        private IDbMapping _dbmappingMock;
        private List<int> mytestList;

        private void FillUpListWithElement(int numberofElements)
        {
            mytestList = new List<int>();
            for (int i = 0; i < numberofElements; i++)
            {
                mytestList.Add(i);
            }
        }

        [Test]
        public void Constructor_Inject_Database_Test()
        {
            Assert.AreEqual(_databaseMock, SUT.Database);
        }

        [Test]
        public void MapCollection_Call_Save_Method_As_Many_Times_As_Many_Items_Are_In_The_collection()
        {
            //Arrange
            int numberofitems = 223;
            FillUpListWithElement(numberofitems);
            //Act
            SUT.MapCollection(mytestList);
            //Assert
            _dbmappingMock.AssertWasCalled(x => x.Save(Arg<string>.Is.Anything), o => o.Repeat.Times(numberofitems));
        }

        [Test]
        public void MapCollection_Open_Close_DataBase_Once_And_Only_Once()
        {
            //Arrange
            int numberofItemsInList = 12;
            FillUpListWithElement(numberofItemsInList);
            //Act
            SUT.MapCollection(mytestList);
            //Assert
            _databaseMock.AssertWasCalled(x => x.Open(), o => o.Repeat.Once());
            _databaseMock.AssertWasCalled(x => x.Dispose(), o => o.Repeat.Once());
        }
    }
}