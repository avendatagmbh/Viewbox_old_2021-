using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using eBalanceKitBusiness.Import;

namespace eBalanceKitBusiness.Test
{
    [TestClass]
    public class BalanceListImporterTest
    {

        /// <summary>
        ///A test for NormalizeValue
        ///</summary>
        [TestMethod]
        public void NormalizeValueTest()
        {
            BalanceListImporter target = new BalanceListImporter(null, null);
            string tmpValue = "-1.500.000";
            string expected = "-1500000";
            string actual = target.NormalizeValue(tmpValue);
            Assert.AreEqual(expected, actual);
            CultureInfo englishCulture = CultureInfo.CreateSpecificCulture("en-US");
            Assert.AreEqual(Convert.ToDecimal(actual, englishCulture), new decimal(-1500000d));
            tmpValue = "1,23";
            expected = "1.23";
            actual = target.NormalizeValue(tmpValue);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(Convert.ToDecimal(actual, englishCulture), new decimal(1.23d));
            tmpValue = "1.23";
            expected = "1.23";
            actual = target.NormalizeValue(tmpValue);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(Convert.ToDecimal(actual, englishCulture), new decimal(1.23d));
            tmpValue = "1,2";
            expected = "1.2";
            actual = target.NormalizeValue(tmpValue);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(Convert.ToDecimal(actual, englishCulture), new decimal(1.2d));
            tmpValue = "1";
            expected = "1";
            actual = target.NormalizeValue(tmpValue);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(Convert.ToDecimal(actual, englishCulture), new decimal(1d));
            tmpValue = "100.522";
            expected = "100522";
            actual = target.NormalizeValue(tmpValue);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(Convert.ToDecimal(actual, englishCulture), new decimal(100522d));
            tmpValue = "100,522";
            expected = "100522";
            actual = target.NormalizeValue(tmpValue);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(Convert.ToDecimal(actual, englishCulture), new decimal(100522d));
            tmpValue = "100.522,12";
            expected = "100522.12";
            actual = target.NormalizeValue(tmpValue);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(Convert.ToDecimal(actual, englishCulture), new decimal(100522.12d));
            tmpValue = "100.522,1";
            expected = "100522.1";
            actual = target.NormalizeValue(tmpValue);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(Convert.ToDecimal(actual, englishCulture), new decimal(100522.1d));
            tmpValue = "100.522.300,100";
            expected = "100522300.100";
            actual = target.NormalizeValue(tmpValue);
            Assert.AreEqual(expected, actual, "100.522.300,100");
            Assert.AreEqual(Convert.ToDecimal(actual, englishCulture), new decimal(100522300.100d));
            tmpValue = "100.522,100";
            expected = "100522.100";
            actual = target.NormalizeValue(tmpValue);
            Assert.AreEqual(expected, actual, "100.522,100");
            Assert.AreEqual(Convert.ToDecimal(actual, englishCulture), new decimal(100522.100d));
            tmpValue = "100,522,300.100";
            expected = "100522300.100";
            actual = target.NormalizeValue(tmpValue);
            Assert.AreEqual(expected, actual, "100,522,300.100");
            Assert.AreEqual(Convert.ToDecimal(actual, englishCulture), new decimal(100522300.100d));
            tmpValue = "100,522.100";
            expected = "100522.100";
            actual = target.NormalizeValue(tmpValue);
            Assert.AreEqual(expected, actual, "100,522.100");
            Assert.AreEqual(Convert.ToDecimal(actual, englishCulture), new decimal(100522.100d));
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
