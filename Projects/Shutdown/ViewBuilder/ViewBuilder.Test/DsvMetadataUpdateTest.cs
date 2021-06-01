using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViewBuilderBusiness.MetadataUpdate;
using System.IO;

namespace ViewBuilder.Test
{
    [TestClass]
    public class DsvMetadataUpdateTest : TestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            LoadProfile("DsvMetadataUpdateTestConfig.xml");
        }

        [TestMethod]
		public void GetTableInformationFromCustomerDbTest()
        {
	        var dsvMetadataUpdate = new MetadataConsistencyInspector(ProfileConfig);
	        var tableDifferences = dsvMetadataUpdate.CompareTableInformation();
			
			Console.WriteLine(string.Join(Environment.NewLine, tableDifferences));

			Assert.IsNotNull(tableDifferences);
			Assert.IsTrue(tableDifferences.Any());
        }
    }
}
