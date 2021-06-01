using NUnit.Framework;

namespace SystemDb_Test
{
    /// <summary>
    ///   checking if all affected datatables are listed in the dictionary... see the constant below...
    /// </summary>
    [TestFixture]
    internal class MergeMetaDatabases_TestBase_Test
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            SUT = new MergeMetaDatabases_TestBase();
        }

        #endregion

        private MergeMetaDatabases_TestBase SUT;
        private readonly int NumberOfTablesOn_EntityRelations_Created_By_Benjamin_Stollin = 29;

        [Test]
        public void Check_Consistency_Of_MetaDataBase_And_Dictionary_()
        {
            Assert.IsTrue(SUT.tablenameType.Count == NumberOfTablesOn_EntityRelations_Created_By_Benjamin_Stollin,
                          "all affected tables should be listed in the current dictionary");
        }
    }
}